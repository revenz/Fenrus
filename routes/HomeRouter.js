const express = require('express');
const common = require('./Common');
const Globals = require('../Globals');
const FileHelper = require('../helpers/FileHelper');
const System = require('../models/System');

const router = express.Router();

let themes = FileHelper.getDirectoriesSync('./wwwroot/themes');


router.get('/', async (req, res) => {    
    
    let settings = req.settings;
    let dashboardUid = req.isGuest ? 'Guest' : req.cookies?.dashboard || 'Default';
    let dashboardInstance = settings.Dashboards?.find(x => x.Uid === dashboardUid && x.Enabled !== false);
    if(!dashboardInstance)
        dashboardInstance = settings.Dashboards.find(x => x.Enabled !== false) || req.settings.Dashboards[0];

    let dashboard = { Groups: []};
    for(let grp of dashboardInstance?.Groups || []){
        if(grp.Enabled === false)
            continue;

        let actualGroup = settings.Groups.find(x => x.Uid === grp.Uid);
        if(!actualGroup || actualGroup.Enabled === false)
            continue;
        dashboard.Groups.push(actualGroup);
    }

    let searchEngines = req.isGuest ? [] : req.settings.SearchEngines.filter(x => x.Enabled != false) || [];
    let system = System.getInstance();
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
    
    res.render('home', common.getRouterArgs(req, { 
        title: '', 
        dashboard: dashboard,
        themes:themes,
        dashboards: dashboards,
        searchEngines: searchEngines
    }));    
});

router.get('/about', async (req, res) => {
    
    res.render('about', common.getRouterArgs(req, { 
        title: 'About', 
        version: Globals.Version,
    }));      
});


  
module.exports = router;