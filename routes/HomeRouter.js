const express = require('express');
const common = require('./Common');
const Globals = require('../Globals');
const Theme = require('../models/Theme');

const router = express.Router();

router.get('/', async (req, res) => {
    let themeVariables = {};
    if(req.theme?.loadScript) {
        let instance = req.theme.loadScript();
        if(instance?.getVariables){
            themeVariables = instance.getVariables(req.settings.ThemeSettings ? req.settings.ThemeSettings[req.theme.Name] : null);
        }
    }
    res.render('home', common.getRouterArgs(req, { 
        title: '', 
        themeVariables: themeVariables
    }));    
});

router.get('/about', async (req, res) => {
    
    res.render('about', common.getRouterArgs(req, { 
        title: 'About', 
        version: Globals.Version,
    }));      
});
  
module.exports = router;