const fs = require('fs');
const crypto = require('crypto');

class GlobalSettings {
    JWTSecret = null;

    _File = './data/settings.json';
    _DefaultFile = './defaultsettings.json';

    constructor() {
        // Generates the JWT secret for the first time, if one exists this will be discarded after load
        this.JWTSecret = crypto.randomBytes(256).toString('base64');
    }

    load() {
        let self = this;
        return new Promise(function (resolve, reject) {
            let file = self._File;
            let save = false;
            console.log('[GlobalSettings] checking for file: ' + file);
            if (fs.existsSync(file) == false) {
                file = self._DefaultFile;
                console.log('[GlobalSettings] checking for file: ' + file);
                if (fs.existsSync(file) == false) {
                    console.log('no config files found');
                    self.save();
                    return;
                }
                save = true;
            }
            console.log('[GlobalSettings] using config file: ' + file);

            fs.readFile(file, (err, data) => {
                if (err) {
                    console.log(err);
                    reject(err);
                } else {
                    let obj = JSON.parse(data);
                    Object.keys(obj).forEach(k => {
                        self[k] = obj[k];
                    });
                    if (save)
                        self.save();
                    resolve(self);
                }
            });
        });
    }

    async save() {
        console.log('Saving global settings');
        let json = this.toJson();
        let self = this;
        return new Promise(function (resolve, reject) {
            fs.writeFile(self._File, json, (err, data) => {
                if (err)
                    reject();
                else
                    resolve();
            });
        });
    }

    toJson() {
        return JSON.stringify({
            JWTSecret: this.JWTSecret
        }, null, 2);
    }

    static async get() {
        if (GlobalSettings.instance) {
            return GlobalSettings.instance;
        }

        let instance = new GlobalSettings();
        await instance.load();
        GlobalSettings.instance = instance;
        return instance;
    }
}

module.exports = GlobalSettings;