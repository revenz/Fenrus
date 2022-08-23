const express = require('express');
const common = require('./Common');
const fsPromises = require("fs/promises");
const ImageHelper = require('../helpers/ImageHelper');
const AdminMiddleware = require('../middleware/AdminMiddleware');
const UserManager = require('../helpers/UserManager');
const Settings = require('../models/Settings');
const System = require('../models/System');

class BackupRouter{
    
    router;

    constructor()
    {
        this.router = express.Router();
        this.init();
    }

    get()
    {
        return this.router;
    }

    async getSettings(req){
        return req.settings;
    }

    init() {        
        this.router.get('/', async (req, res) => await this.index(req, res));
        this.router.get('/export', async (req, res) => await this.export(req, res));
        this.router.post('/import', async (req, res) => await this.import(req, res));
        
        this.router.use(AdminMiddleware);
        this.router.get('/export-config', async (req, res) => await this.exportConfig(req, res));
        this.router.post('/import-config', async (req, res) => await this.importConfig(req, res));
    }

    async index(req, res) 
    {
        let args = common.getRouterArgs(req, 
        { 
            title: 'Backup'
        });
        args.settings = await this.getSettings(req);

        args.data = {
            typeName: 'Backup',
            title: 'Backup',
            description: 'This page allows you to schedule and take backups of your configuration',
            icon: 'icon-files-o',
            baseUrl: '/settings/backup'
        };
        
        res.render('settings/backup/editor', common.getRouterArgs(req, args)); 
    }

    async export(req, res) {
        console.log('##### exporting !!!!');
        let settings = await this.getSettings(req);
        let data = await this.exportSettings(settings);
        console.log('data', data);
        let json = JSON.stringify(data, null, 2);
        res.writeHead(200, {
          'Content-Disposition': `attachment; filename="Fenrus-${new Date().toLocaleDateString()}.json"`,
          'Content-Type': 'application/json',
        })
        res.write(json);
        res.end();        
    }

    async exportForUser(uid) {
        let settings = await Settings.getForUser(uid);
        return await this.exportSettings(settings);
    }

    async exportSettings(settings)
    {
        if(!settings)
            return {};
        let json = settings.toJson();
        let cloned = JSON.parse(json);
        if(cloned.BackgroundImage)
        {     
            console.log('#### BACKGROUND IMAGE: ' + cloned.BackgroundImage)
            cloned.BackgroundImageBase64 = await fsPromises.readFile('./wwwroot/' + cloned.BackgroundImage, {encoding: 'base64'});
        }
        if(cloned.Groups?.length)
        {
            for(let grp of cloned.Groups)
            {
                if(!grp.Items?.length)
                    continue;
                for(let item of grp.Items){
                    if(item.Icon && item.Icon.indexOf('apps') < 0){
                        item.IconBase64 = await fsPromises.readFile('./wwwroot/' + item.Icon, {encoding: 'base64'});
                    }
                }
            }
        }
        if(cloned.SearchEngines?.length)
        {
            for(let item of cloned.SearchEngines)
            {
                if(item.Icon){
                    item.IconBase64 = await fsPromises.readFile('./wwwroot/' + item.Icon, {encoding: 'base64'});
                }
            }
        }
        return cloned;
    }

    async import(req, res) {
        let settings = await this.getSettings(req);        
        let model = req.body;
        if(!model.Revision)
        {
            res.status(400).send('Invalid data').end();
            return;
        }
        await this.importConfig(settings, model);

        res.status(200).send('').end();
    }

    async importForUser(uid, config)
    {
        let settings = await Settings.getForUser(uid);
        await this.importSetting(settings, config);
    }

    async importSetting(settings, config)
    {
        console.log('import config');
        let imageHelper = new ImageHelper();

        if(config.BackgroundImageBase64)
        {
            console.log('#### IMPORT CUSTOM BACKGROUND');
            await fsPromises.writeFile('./wwwroot' + item.BackgroundImage, item.BackgroundImageBase64, { encoding: 'base64'});    
        }

        if(config?.Groups?.length)
        {
            for(let grp of config.Groups)
            {
                if(!grp?.Items?.length)
                    continue;
                for(let item of grp.Items)
                {
                    if(item.IconBase64){
                        console.log('#### IMPORT CUSTOM ICON: ' + item.Name);
                        await fsPromises.writeFile('./wwwroot' + item.Icon, item.IconBase64, { encoding: 'base64'});    
                        delete item.IconBase64;
                    }
                    if(item.Properties){
                        let props = Object.keys(item.Properties);
                        for(let p of props){
                            let value = item.Properties[p];
                            if(!value)
                                continue;
                            
                        }
                    }
                }
            }
        }

        if(config?.SearchEngines?.length)
        {
            for(let item of config.SearchEngines)
            {
                if(item.IconBase64){
                    console.log('#### IMPORT CUSTOM ICON: ' + item.Name);
                    await fsPromises.writeFile('./wwwroot' + item.Icon, item.IconBase64, { encoding: 'base64'});                    
                    delete item.IconBase64;
                }
            }
        }

        console.log('#### IMPORT groups: ' + config.Groups?.length);
        console.log('#### IMPORT SearchEngines: ' + config.SearchEngines?.length);
        console.log('#### IMPORT Dashboards: ' + config.Dashboards?.length);
        settings.AccentColor = config.AccentColor;
        settings.Theme = config.Theme;
        settings.LinkTarget = config.LinkTarget;
        settings.ShowGroupTitles = config.ShowGroupTitles;
        settings.CollapseMenu = config.CollapseMenu;
        settings.Groups = config.Groups;
        settings.Dashboards = config.Dashboards;
        settings.ThemeSettings = config.ThemeSettings;
        settings.ShowSearch = config.ShowSearch;
        settings.SearchEngines = config.SearchEngines;
        settings.ShowStatusIndicators = config.ShowStatusIndicators;
        await settings.save();
    }

    
    /**
     * Exports the entire Fenrus configuration, including users and system settings
     */
    async exportConfig(req, res) {
        let users = UserManager.getInstance().Users;
        let system = System.getInstance();
        let json = system.toJson();
        let config = JSON.parse(json);
        delete config.Revision;

        config.Users = [];
        for(let user of users)
        {
            user.Config = await this.exportForUser(user.Uid);
            config.Users.push(user);
        }
        json = JSON.stringify(config, null, 2);
        res.writeHead(200, {
          'Content-Disposition': `attachment; filename="Fenrus-${new Date().toLocaleDateString()}.json"`,
          'Content-Type': 'application/json',
        })
        res.write(json);
        res.end();
    }

    /**
     * Imports the entire Fenrus configuration, including users and system settings
     */
    async importConfig(req, res) {   
        console.log('### importing config', res);
        let model = req.body;
        if(!model){
            return res.status(400).send('').end();
        }
        console.log(model);
        let system = System.getInstance();
        system.AuthStrategy = model.AuthStrategy;
        system.AllowRegister = model.AllowRegister;
        system.AllowGuest = model.AllowGuest;
        let userManager = UserManager.getInstance();

        if(model.SearchEngines)
            system.SearchEngines = model.SearchEngines;
        if(model.Docker)
            system.Docker = model.Docker;
        if(model.Properties)
            system.Properties = model.Properties;
        if(model.GuestDashboard)
            system.GuestDashboard = model.GuestDashboard;
        if(model.SystemGroups)
            system.SystemGroups = model.SystemGroups;

        if(model.Users)
        {
            var users = [];
            for(let user of model.Users)
            {
                if(!user?.Uid || !user?.Config)
                    continue;

                users.push({
                    Uid: user.Uid,
                    Username: user.Username,
                    Password: user.Password,
                    IsAdmin: user.IsAdmin
                });
                console.log('restoring user', user.Uid, user.Name);
                await this.importForUser(user.Uid, user.Config);
            }
            userManager.Users = users;
            await userManager.save();
        }
        console.log('### Backup restored');
        res.status(200).end();
    }
}
  


module.exports = BackupRouter;