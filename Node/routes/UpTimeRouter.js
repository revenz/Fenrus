const express = require('express');
const common = require('./Common');
const Settings = require('../models/Settings');
const AppHelper = require('../helpers/appHelper');
const System = require('../models/System');
const Utils = require('../helpers/utils');
const fsPromises = require('fs.promises');
const UpTimeService = require('../services/UpTimeService');
const fsExists = require('fs.promises.exists');
const path = require('path');

class UpTimeRouter{
    
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
        this.router.get('/', async (req, res) => await this.list(req, res));
        this.router.get('/:uid', async (req, res) => await this.getAppUptime(req, res));
    }

    getDirectory(req)  {
        let userUid = req.user.Uid;
        return `./data/uptime/${userUid}`;
    }

    async getUpTimeFiles(req)
    {
        let dir = this.getDirectory(req);
        try{
            let files =await fsPromises.readdir(dir);
            let results = [];
            for(let f of files){
                console.log('file', f);
                let fn = f.substring(f.replace(/\\/g, '/').lastIndexOf('/') + 1);
                fn = fn.replace('.json', '');
                results.push(fn);
            }
            return results;
        }
        catch(err) 
        {
            console.log('err', err);
            return [];
        }
    }

    async list(req, res) 
    {
        let args = common.getRouterArgs(req, 
        { 
            title: 'Up Time'
        });
        args.settings = await this.getSettings(req);

        var files = await this.getUpTimeFiles(req);
        let upService = new UpTimeService();
        var apps = await upService.getAppsForUser(req.user.Uid);
        var appsWithStats = [];
        for(let app of apps)
        {
            if(files.indexOf(app.Uid) >= 0)
            {
                app.IsUp = upService.getLastIsUp(req.user.Uid, app.Url);
                appsWithStats.push(app);
            }
        }
        appsWithStats.sort((a, b) => a.Name.localeCompare(b.Name));
        console.log('items', appsWithStats);

        args.data = {
            typeName: 'UpTime',
            title: 'Up-Time',
            description: 'This page shows the up-time for monitored sites and applications',
            icon: 'icon-puzzle-piece',
            baseUrl: '/settings/uptime',
            items: appsWithStats
        };
        
        res.render('settings/uptime/list', common.getRouterArgs(req, args)); 
    }


    async getAppUptime(req, res) {
        let appUid = req.params.uid;
        if(/^[a-fA-F0-9\-]{36}$/.test(appUid) !== true){
            console.log('did not match: ' + appUid);
            res.json([]).end();
            return;
        }

        let dir = this.getDirectory(req);
        let file = dir + '/' + appUid + '.json';
        if(await fsExists(file) !== true)
        {
            console.log('file did not exist: ' + appUid);
            res.json([]).end();
            return;
        }
        res.sendFile(path.join(__dirname, '..', file));                
    }


}
  


module.exports = UpTimeRouter;