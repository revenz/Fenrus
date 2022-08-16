const express = require('express');
const common = require('./Common');
const Globals = require('../Globals');
const FileHelper = require('../helpers/FileHelper');
const System = require('../models/System');
const Utils = require('../helpers/utils');
const FenrusRouter = require('./FenrusRouter');
const UpTimeService = require('../services/UpTimeService');

class HomeRouter extends FenrusRouter {

    router;
    themes;

    constructor()
    {
        super();

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
        this.router.get('/', async(req, res) => await this.safeAsync('home', req, res));

        this.router.get('/dashboard/:uid', async(req, res) => await this.safeAsync('dashboard', req, res));

        this.router.get('/about', async(req, res) => await this.safeAsync('about', req, res));
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
        if(req.isGuest)
        {
            let system = System.getInstance();
            res.setHeader('ETag', 'guest-' + system.Revision);
        }else {
            res.setHeader('ETag', req.settings.uid + '-' + req.settings.Revision);
        }
        //res.setHeader('ETag', settings.Uid + '-' + settings.Revision);
        let dashboardInstance = this.getDefaultDashboard(req, res);
        return this.renderDashboard(req, res, dashboardInstance, false);
    }

    getDefaultDashboard(req, res)
    {
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
        let dashboard = { 
            Uid: dashboardInstance.Uid, 
            Name: dashboardInstance.Name, 
            Theme: dashboardInstance.Theme,
            AccentColor: dashboardInstance.AccentColor,
            BackgroundImage: dashboardInstance.BackgroundImage,
            Groups: [], 
        };
        let upService = new UpTimeService();
        let areUps = {};
        if(req.user)
        {
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
                for(let item of actualGroup.Items)
                {
                    if(item?._Type !== 'DashboardApp' && item?._Type !== 'DashboardLink')
                        continue;
                    let lastUp = upService.getLastIsUp(req.user.Uid, item.ApiUrl || item.Url);
                    areUps[item.Uid] = lastUp;
                }
                dashboard.Groups.push(actualGroup);
            }
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
        });
    
        res.render(inline ? 'dashboard' : 'home', common.getRouterArgs(req, { 
            title: '', 
            dashboardInstanceUid: new Utils().newGuid(),            
            dashboard: dashboard,
            themes:this.themes,
            dashboards: dashboards,
            searchEngines: searchEngines,
            isUp: areUps
        }));    

    }
}
  
module.exports = HomeRouter;
