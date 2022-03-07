const express = require('express');
const common = require('./Common');
const Globals = require('../Globals');
const FileHelper = require('../helpers/FileHelper');
const System = require('../models/System');
const Utils = require('../helpers/utils');

class HomeRouter {

    router;
    themes;

    constructor()
    {
        this.router = express.Router();
        this.themes = FileHelper.getDirectoriesSync('./wwwroot/themes') 
        this.init();
    }

    get()
    {
        return this.router;
    }

    init()
    {
        this.router.get('/', async(req, res) => await this.home(req, res));

        this.router.get('/dashboard/:uid', async(req, res) => await this.dashboard(req, res));

        this.router.get('/about', async(req, res) => await this.about(req, res));
    }

    async about(req, res) 
    {
        res.render('about', common.getRouterArgs(req, { 
            title: 'About', 
            version: Globals.getVersion(),
        }));  
    }

    async home(req, res) 
    {        
        let dashboardInstance = this.getDefaultDashboard(req, res);
        console.log('dashboardInstance', dashboardInstance);
        return this.renderDashboard(req, res, dashboardInstance, false);
    }

    getDefaultDashboard(req, res){
        let settings = req.settings;
        let dashboardUid = req.isGuest ? 'Guest' : req.cookies?.dashboard || 'Default';
        let dashboardInstance = settings.Dashboards?.find(x => x.Uid === dashboardUid && x.Enabled !== false);
        if(!dashboardInstance)
            dashboardInstance = settings.Dashboards.find(x => x.Enabled !== false) || req.settings.Dashboards[0];
        return dashboardInstance;
    }

    async dashboard(req, res){
        let dashboardUid = req.params.uid;
        let dashboardInstance;
        let inline = req.query.inline === 'true';
        if(dashboardUid === 'Default')
        {
            dashboardInstance = this.getDefaultDashboard(req, res);
        }
        else
        {
            let settings = req.settings;
            dashboardInstance = settings.Dashboards?.find(x => x.Uid === dashboardUid && x.Enabled !== false);
            if(!dashboardInstance)
                return res.sendStatus(404);
        }
        return this.renderDashboard(req, res, dashboardInstance, inline);
    }

    async renderDashboard(req, res, dashboardInstance, inline)
    {
        let system = System.getInstance();
        let settings = req.settings;
        let dashboard = { Groups: [], Name: dashboardInstance?.Name, BackgroundImage: dashboardInstance.BackgroundImage };
        for(let grp of dashboardInstance?.Groups || []){
            if(grp.Enabled === false)
                continue;

            let actualGroup = settings.Groups.find(x => x.Uid === grp.Uid);
            if(!actualGroup)
            {
                // check for a system group
                actualGroup = system.SystemGroups.find(x => x.Uid == grp.Uid);
            }
            if(!actualGroup || actualGroup.Enabled === false)
                continue;
            dashboard.Groups.push(actualGroup);
        }

        let searchEngines = req.isGuest ? [] : req.settings.SearchEngines.filter(x => x.Enabled != false) || [];
        if(system.SearchEngines?.length)
            searchEngines = searchEngines.concat(system.SearchEngines.filter(x => x.Enabled != false));

        let dashboards = settings.Dashboards.filter(x => x.Enabled !== false).map(x => {
            return {
                Uid: x.Uid,
                Name: x.Name
            };
        });

        dashboards.sort((a, b) => {
            return a.Name.localeCompare(b.Name);
        })
        
        res.render(inline ? 'dashboard' : 'home', common.getRouterArgs(req, { 
            title: '', 
            dashboardInstanceUid: new Utils().newGuid(),            
            dashboard: dashboard,
            themes:this.themes,
            dashboards: dashboards,
            searchEngines: searchEngines
        }));    

    }
}
  
module.exports = HomeRouter;
