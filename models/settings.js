const { promiseImpl } = require('ejs');
const fs = require('fs');

class SettingsInstance {

    Revision = 0;
    LinkTarget = '_self';
    Theme = '';
    AccentColor = '#ff0090';
    ShowGroupTitles = true;
    BackgroundImage = '';
    Groups = [];

    _File = '../data/config.json';

    constructor(){}
    
    load() {        
        let self = this;
        return new Promise(function (resolve, reject) {
            if(fs.existsSync(this._File) == false) {
                this.save();
            }
            else
            {
                fs.readFile(this._File, (err, data) => {
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
                        resolve(self);                   
                    }
                });
            }
        });
    }

    save() {
        ++this.Revision;
        let json = this.toJson();
        return new Promise(function (resolve, reject) {
            fs.writeFile(this._File, json, (err, data) => {

            });
        });
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
        });
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
        throw new Error('Use Settings.getInstance()');
    }

    static getInstance(){
        if(!SettingsInstance.instance){
            SettingsInstance.instance = new SettingsInstance();
        }
        return SettingsInstance.instance;
    }
}


module.exports = Settings;