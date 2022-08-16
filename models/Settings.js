const fs = require('fs');
const ImageHelper = require('../helpers/ImageHelper');
const Utils = require('../helpers/utils');
const System = require('./System');

class SettingsInstance {
    Revision = 0;
    LinkTarget = '_self';
    Theme = '';
    CollapseMenu = false;
    AccentColor = '#ff0090';
    ShowGroupTitles = true;
    BackgroundImage = '';
    Groups = [];
    Dashboards = [];
    ThemeSettings = {};
    ShowSearch = false;
    SearchEngines = [];    
    ShowStatusIndicators = true;

    _File = './data/config.json';
    _DefaultFile = './defaultconfig.json';

    uid = '';

    constructor(uid){
        this.uid = uid;
        this._File = `./data/configs/${uid}.json`;
    }
    load() {        
        console.log('loading!');
        let self = this;
        return new Promise(async (resolve, reject) => {
            try{
                if(this.loadSync())
                    resolve(self);                
            }catch(err) {}
            reject();
        });
    }
    
    loadSync() {        
        console.log('loading!');
        let self = this;
        let file = self._File;
        let save = false;
        console.log('checking for file: ' + file);
        if(fs.existsSync(file) == false) {
            // use default config
            let guest = Settings.getForGuest();
            self.Dashboards = [ {...guest.Dashboards[0] }];
            self.Dashboards[0].Name = 'Default';
            console.log('###################', guest.Dashboards[0].Name, self.Dashboards[0].Name);
            self.Dashboards[0].Uid = new Utils().newGuid();
            self.Dashboards[0].Enabled = true;
            self.Dashboards[0].BackgroundImage = '';
            self.BackgroundImage = guest.Dashboards[0].BackgroundImage;
            self.Theme = 'Default';
            self.AccentColor = guest.AccentColor;
            
            self.save();
            return true;
        }
        console.log('using config file: ' + file);
        
        const data = fs.readFileSync(file);
    
        let obj = JSON.parse(data);    
        Object.keys(obj).forEach(k => {
            self[k] = obj[k];
        });         
        if(self.Groups?.length) {
            for(let grp of self.Groups) {
                if(grp.Enabled === undefined){
                    grp.Enabled = true;
                    save = true;
                }
                if(!grp?.Items?.length)
                    continue;                                
                for(let item of grp.Items){
                    if(item.Enabled === undefined){
                        item.Enabled = true;
                        save = true;
                    }
                }
            }
        }    
        if(self.SearchEngines?.length) {
            for(let se of self.SearchEngines) {
                if(se.Enabled === undefined){
                    se.Enabled = true;
                    save = true;
                }
            }
        }


        if(!self.Dashboards?.length) {
            save = true;
            self.Dashboards = [ {
                Uid: new Utils().newGuid(),
                Name: 'Default',
                Enabled: true,
                Groups: self.Groups.map(x => { return {
                    Uid: x.Uid,
                    Name: x.Name,
                    Enabled: true
                }})
            }];                     
        }
        if(save)
            self.save();
        return true;
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
                CollapseMenu: this.CollapseMenu,
                BackgroundImage: this.BackgroundImage,
                ThemeSettings: this.ThemeSettings,
                ShowStatusIndicators: this.ShowStatusIndicators
            });
        }
        return JSON.stringify({                
            Revision: this.Revision,
            LinkTarget: this.LinkTarget,
            Theme: this.Theme,
            AccentColor: this.AccentColor,
            ShowGroupTitles: this.ShowGroupTitles,
            CollapseMenu: this.CollapseMenu,
            BackgroundImage: this.BackgroundImage,
            ThemeSettings: this.ThemeSettings,
            ShowStatusIndicators: this.ShowStatusIndicators,
            SearchEngines: this.SearchEngines,
            Dashboards: this.Dashboards,
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
        return SettingsInstance.findAppInstanceActual(this.Groups, uid);
    }

    static findAppInstanceActual(groups, uid) {
        for(let group of groups)
        {
            let item = SettingsInstance.findAppInstanceInGroup(group.Items, uid);
            if(item)
                return item;
        }
        return null;
    }


    static findAppInstanceInGroup(items, uid){
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
                    let item = SettingsInstance.findAppInstanceInGroup(item.Items, uid);
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

    static getForUserSync(uid) 
    {
        SettingsInstance.instances = SettingsInstance.instances || {};

        if(SettingsInstance.instances[uid])
            return SettingsInstance.instances[uid];

        let instance = new SettingsInstance(uid);
        instance.loadSync();
        SettingsInstance.instances[uid] = instance;
        return instance;
    }



    static getForGuest() 
    {
        SettingsInstance.instances = SettingsInstance.instances || {};

        if(SettingsInstance.instances['GUEST'])
            return SettingsInstance.instances['GUEST'];

        let system = System.getInstance();
        let settings = {};
        settings.Dashboards = [
            system.GuestDashboard
        ];
        settings.Theme = 'Default';
        settings.Groups = system.SystemGroups.filter(x => x.Enabled !== false);
        settings.AccentColor = '#ff0090';
        settings.findAppInstance = (uid) => {
            return SettingsInstance.findAppInstanceActual(settings.Groups, uid);
        };
        SettingsInstance.instances['GUEST'] = settings;
        return settings;
    }

    static clearUser(uid){
        if(!SettingsInstance.instances)
            return;
        delete SettingsInstance.instances[uid];
    }
}


module.exports = Settings;