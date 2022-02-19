const fs = require('fs');

class SystemConfigInstance {

    Revision = 0;

    AdminAddresses = [];

    _File = './data/system.json';

    constructor(){}
    
    load() {        
        let self = this;
        return new Promise(function (resolve, reject) {
            let save = false;
            if(fs.existsSync(self._File) == false) {
                console.log('no config files found');
                self.save();
                return;
            }            
            fs.readFile(self._File, (err, data) => {
                if(err) {
                    console.log(err);
                    reject(err);
                }
                else             
                {
                    let obj = JSON.parse(data);    
                    Object.keys(obj).forEach(k => {
                        self[k] = obj[k];
                    });         
                    if(save)
                        self.save();
                    resolve(self);                   
                }
            });
        });
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


    toJson(noGroups) { 
        return JSON.stringify({                
            AdminAddresses: this.AdminAddresses
        }, null, 2);
    }

}

class SystemConfig {
    constructor() {
        throw new Error('Use SystemConfig.getInstance()');
    }

    static getInstance(){
        if(!SystemConfigInstance.instance){
            SystemConfigInstance.instance = new SystemConfigInstance();
        }
        return SystemConfigInstance.instance;
    }
}


module.exports = SystemConfig;