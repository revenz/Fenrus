const express = require('express');
const common = require('./Common');
const Globals = require('../Globals');
const FileHelper = require('../helpers/FileHelper');
const System = require('../models/System');

const router = express.Router();

let themes = FileHelper.getDirectoriesSync('./wwwroot/themes');


router.get('/', async (req, res) => {    
    let themeVariables = {};
    if(req.theme?.loadScript) {
        let instance = req.theme.loadScript();
        if(instance?.getVariables){
            themeVariables = instance.getVariables(req.settings.ThemeSettings ? req.settings.ThemeSettings[req.theme.Name] : null);
        }
    }
    let themeSettings = {};
    if(req.settings.ThemeSettings && req.settings.ThemeSettings[req.theme.Name])
        themeSettings = req.settings.ThemeSettings[req.theme.Name];
    else if(req.theme.Settings?.length){
        // need to get default settings
        console.log(';theme.Settings', req.theme.Settings);
        for(let setting of req.theme.Settings){
            if(setting.Default)
                themeSettings[setting.Name] = setting.Default;
            else if(setting.Type === 'Integer')
                themeSettings[setting.Name] = 0;
            else if(setting.Type === 'Boolean')
                themeSettings[setting.Name] = false;
            else if(setting.Type === 'String')
                themeSettings[setting.Name] = '';
        }
    }

    let searchEngines = req.isGuest ? [] : req.settings.SearchEngines.filter(x => x.Enabled != false) || [];
    let system = System.getInstance();
    if(system.SearchEngines?.length)
        searchEngines = searchEngines.concat(system.SearchEngines.filter(x => x.Enabled != false));
    
    res.render('home', common.getRouterArgs(req, { 
        title: '', 
        themes:themes,
        themeVariables: themeVariables,
        themeSettings: themeSettings,
        searchEngines: searchEngines
        // [
        //     { Name: 'DuckDuckGo', Icon: '/search-engines/duckduckgo.jpg', Url: 'https://duckduckgo.com/?q=%s', IsDefault: true },
        //     { Name: 'Google', Icon: '/search-engines/google.png', Url: 'https://www.google.com/search?q=%s', Shortcut: 'g' },
        //     { Name: 'MightyApe', Icon: '/apps/MightyApe/icon.png', Url: 'https://www.mightyape.co.nz/search?q=%s', Shortcut: 'ma' },
        // ]
    }));    
});

router.get('/about', async (req, res) => {
    
    res.render('about', common.getRouterArgs(req, { 
        title: 'About', 
        version: Globals.Version,
    }));      
});

router.get('/logout', (req, res) => {    
    res.clearCookie("jwt_auth");
    
    var system = System.getInstance();
    if(system.AllowGuest)
        res.redirect('/').end();
    else
        res.redirect('/login').end();
});
  
module.exports = router;