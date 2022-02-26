const express = require('express');
const common = require('./Common');
const Globals = require('../Globals');
const Theme = require('../models/Theme');
const FileHelper = require('../helpers/FileHelper');

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
    res.render('home', common.getRouterArgs(req, { 
        title: '', 
        themes:themes,
        themeVariables: themeVariables,
        themeSettings: themeSettings
    }));    
});

router.get('/about', async (req, res) => {
    
    res.render('about', common.getRouterArgs(req, { 
        title: 'About', 
        version: Globals.Version,
    }));      
});
  
module.exports = router;