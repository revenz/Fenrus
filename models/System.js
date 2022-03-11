const fs = require('fs');
const Globals = require('../Globals');

class SystemInstance 
{
    Version;
    Revision;
    AllowRegister = true;
    AllowGuest = true;
    _File = './data/system.json';
    SearchEngines = [];
    Properties = {};
    AuthStrategy;
    GuestDashboard;
    SystemGroups;

    constructor(){
    }    

    getIsConfigured() {
        if(!this.AuthStrategy){
            console.log('No AuthStrategy configured');
            return false;
        }
        return true;
    }
    
    getAuthStrategy(name){
        name = name || this.AuthStrategy;
        if(/^[a-zA-Z0-9\-]+$/.test(name) === false)
            return;
  
        let filename = `./strategies/${name}.js`
        if(fs.existsSync(filename) == false){
            console.log('Strategy does not exist: ' + filename);
            return;
        }
        let classType = require('.' + filename); // this needs another dot
        return new classType();
    }

    async load() {   
        let self = this;
        try{
            let file = self._File;
            if(fs.existsSync(file) == true) {     
                let json = fs.readFileSync(file, { encoding: 'utf-8'});
                if(json.charCodeAt(0) === 65279)
                    json = json.substring(1);
                let obj = JSON.parse(json);    
                Object.keys(obj).forEach(k => {
                    self[k] = obj[k];
                });         
            }  
            if(!this.Version || !this.GuestDashboard?.Name){
                await this.initDefaultConfig();
            }
        }
        catch(err)
        {
            console.log('error loading system settings', err);
        }
    }

    async initDefaultConfig()
    {        
        let defaultConfigFile = './defaultconfig.json';
        if(fs.existsSync(defaultConfigFile) == true) {   
            let json = fs.readFileSync(defaultConfigFile, { encoding: 'utf-8'});     

            if(json.trim() !== ''){
                if(json.charCodeAt(0) === 65279)
                    json = json.substring(1);
                let obj = JSON.parse(json);  

                if(!this.SearchEngines?.length)                
                    this.SearchEngines = obj.SearchEngines || [];

                if(!this.SystemGroups)
                    this.SystemGroups = obj.Groups || [];
                
                if(!this.GuestDashboard?.Name){
                    this.GuestDashboard = {
                        Uid: 'Guest',
                        Name: 'Guest',
                        Groups: obj.Groups.map(x => {
                            return {
                                Uid: x.Uid,
                                Name: x.Name,
                                Enabled: true
                            }
                        })
                    }
                }
                await this.save();     
            }  
        }
    }

    async save() {
        if(!this.Revision)
            this.Revision = 1;
        else
            ++this.Revision;
            
        let json = this.toJson();
        let self = this;
        return new Promise(function (resolve, reject) {
            fs.writeFile(self._File, json, (err, data) => {
                if(err)
                    reject();
                else
                    resolve();
            });
        });
    }

    getProperty(name) {
        if(this.Properties && this.Properties[name])
            return this.Properties[name];
        return null;
    }
    
    async setProperty(name, value) {
        if(!this.Properties)
            this.Properties = {};
        this.Properties[name] = value;
        await this.save();
    }

    toJson(noSecret) { 
        if(noSecret){
            return JSON.stringify({       
                AllowRegister: this.AllowRegister,
                AllowGuest: this.AllowGuest
            });
        }
        this.Version = Globals.getVersion();
        return JSON.stringify({             
            Version: this.Version,   
            Revision: this.Revision,
            AuthStrategy: this.AuthStrategy,
            AllowRegister: this.AllowRegister,
            AllowGuest: this.AllowGuest,
            SearchEngines: this.SearchEngines,
            Properties: this.Properties,
            GuestDashboard: this.GuestDashboard,
            SystemGroups: this.SystemGroups
        }, null, 2);
    }
}

class System {
    constructor() {
        throw new Error('Use System.getInstance()');
    }

    static getInstance(){
        if(!SystemInstance.instance){
            SystemInstance.instance = new SystemInstance();
        }
        return SystemInstance.instance;
    }
}


module.exports = System;