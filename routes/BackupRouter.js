const express = require('express');
const common = require('./Common');
const fsPromises = require("fs/promises");
const ImageHelper = require('../helpers/ImageHelper');

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
        json = JSON.stringify(cloned, null, 2);
        res.writeHead(200, {
          'Content-Disposition': `attachment; filename="Fenrus-${new Date().toLocaleDateString()}.json"`,
          'Content-Type': 'application/json',
        })
        res.write(json);
        res.end();        
    }

    async import(req, res) {
        console.log('###### IMPORTING2!');
        let settings = await this.getSettings(req);        
        let model = req.body;
        if(!model.Revision)
        {
            res.status(400).send('Invalid data').end();
            return;
        }

        let imageHelper = new ImageHelper();

        if(model.BackgroundImageBase64)
        {
            console.log('#### IMPORT CUSTOM BACKGROUND');
            await fsPromises.writeFile('./wwwroot' + item.BackgroundImage, item.BackgroundImageBase64, { encoding: 'base64'});    
        }

        if(model?.Groups?.length)
        {
            for(let grp of model.Groups)
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
                }
            }
        }

        if(model?.SearchEngines?.length)
        {
            for(let item of model.SearchEngines)
            {
                if(item.IconBase64){
                    console.log('#### IMPORT CUSTOM ICON: ' + item.Name);
                    await fsPromises.writeFile('./wwwroot' + item.Icon, item.IconBase64, { encoding: 'base64'});                    
                    delete item.IconBase64;
                }
            }
        }

        console.log('#### IMPORT groups: ' + model.Groups?.length);
        console.log('#### IMPORT SearchEngines: ' + model.SearchEngines?.length);
        console.log('#### IMPORT Dashboards: ' + model.Dashboards?.length);
        settings.AccentColor = model.AccentColor;
        settings.Theme = model.Theme;
        settings.LinkTarget = model.LinkTarget;
        settings.ShowGroupTitles = model.ShowGroupTitles;
        settings.CollapseMenu = model.CollapseMenu;
        settings.Groups = model.Groups;
        settings.Dashboards = model.Dashboards;
        settings.ThemeSettings = model.ThemeSettings;
        settings.ShowSearch = model.ShowSearch;
        settings.SearchEngines = model.SearchEngines;
        settings.ShowStatusIndicators = model.ShowStatusIndicators;
        await settings.save();
        res.status(200).send('').end();
    }
}
  


module.exports = BackupRouter;