const fs = require('fs');
const ImageHelper = require('../helpers/ImageHelper');

class SettingsInstance {

    Revision = 0;
    LinkTarget = '_self';
    Theme = '';
    AccentColor = '#ff0090';
    ShowGroupTitles = true;
    BackgroundImage = '';
    Groups = [];

    _File = './data/config.json';
    _DefaultFile = './defaultconfig.json';

    uid = '';

    constructor(uid){
        this.uid = uid;
        this._File = `./data/configs/${uid}.json`;
    }
    
    load() {        
        let self = this;
        return new Promise(function (resolve, reject) {
            let file = self._File;
            let save = false;
            console.log('checking for file: ' + file);
            if(fs.existsSync(file) == false) {
                file = self._DefaultFile;
                console.log('checking for file: ' + file);
                if(fs.existsSync(file) == false) {
                    console.log('no config files found');
                    self.save();
                    return;
                }
                save = true;
            }
            console.log('using config file: ' + file);
            
            fs.readFile(file, (err, data) => {
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
        ++this.Revision;
        if(/^\themes/.test(this.BackgroundImage))
            this.BackgroundImage = '';
        
        this.BackgroundImage = await new ImageHelper().saveImageIfBase64(this.BackgroundImage, 'backgrounds');

        console.log('Saving settings, revision: ' + this.Revision);
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

    addGroup(group) {
        if(!group)
            return;
        this.Groups.push(group);
        this.save();
    }

    toJson(noGroups) { 
        if(noGroups){
            return JSON.stringify({                
                Revision: this.Revision,
                LinkTarget: this.LinkTarget,
                Theme: this.Theme,
                AccentColor: this.AccentColor,
                ShowGroupTitles: this.ShowGroupTitles,
                BackgroundImage: this.BackgroundImage,
            });
        }
        return JSON.stringify({                
            Revision: this.Revision,
            LinkTarget: this.LinkTarget,
            Theme: this.Theme,
            AccentColor: this.AccentColor,
            ShowGroupTitles: this.ShowGroupTitles,
            BackgroundImage: this.BackgroundImage,
            Groups: this.Groups
        }, null, 2);
    }

    findGroupInstance(uid, groups) {
        if(!groups) groups = this.Groups;
        for(let group of groups){
            if(group.Uid === uid)
                return group;

            if(!group.Items?.length)
                continue;
            
            for(let item of group.Items){
                if(item._Type === 'DashboardGroup'){
                    let result = this.findGroupInstance(uid, item);
                    if(result)
                        return result;
                }
            }
        }
        return null;
    }

    
    findAppInstance(uid) {
        for(let group of this.Groups)
        {
            let item = this.findAppInstanceInGroup(group.Items, uid);
            if(item)
                return item;
        }
        return null;
    }

    findAppInstanceInGroup(items, uid){
        if(!items) return null;
        for(let item of items){
            if(item._Type === 'DashboardApp')
            {
                if(item.Uid === uid)
                    return item;
            }
            else if(item._Type === 'DashboardGroup'){
                if(item.Items?.length)
                {
                    let item = this.findAppInstanceInGroup(item.Items, uid);
                    if(item)
                        return item;
                }
            }
        }
        return null;
    }
}

class Settings {
    constructor() {
        throw new Error('Use Settings.getForUser()');
    }

    static async getForUser(uid) 
    {
        SettingsInstance.instances = SettingsInstance.instances || {};

        if(SettingsInstance.instances[uid])
            return SettingsInstance.instances[uid];

        let instance = new SettingsInstance(uid);
        await instance.load();
        SettingsInstance.instances[uid] = instance;
        return instance;
    }
}


module.exports = Settings;