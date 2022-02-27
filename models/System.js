const fs = require('fs');
const crypto = require('crypto');

class SystemInstance {
    
    JwtSecret = '';
    AllowRegister = true;
    AllowGuest = true;
    _File = './data/system.json';
    SearchEngines = [];

    constructor(){
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
            if(!self.JwtSecret){
                self.JwtSecret = crypto.randomBytes(256).toString('base64');
                self.AllowRegister = true;
                self.save();
            }
        }
        catch(err)
        {
            console.log('error loading system settings', err);
        }
    }

    async save() {
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

    toJson(noSecret) { 
        if(noSecret){
            return JSON.stringify({       
                AllowRegister: this.AllowRegister,
                AllowGuest: this.AllowGuest
            });
        }
        return JSON.stringify({                
            JwtSecret: this.JwtSecret,
            AllowRegister: this.AllowRegister,
            AllowGuest: this.AllowGuest,
            SearchEngines: this.SearchEngines
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