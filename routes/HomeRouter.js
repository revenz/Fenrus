const express = require('express');
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');
const fsPromises = require("fs/promises");
const fs = require("fs");

const router = express.Router();

router.get('/', (req, res) => {
    res.render('home', 
    { 
        title: '', 
        Utils: new Utils(),
        user: req.user, 
        settings: req.settings,
        AppHelper: AppHelper.getInstance()
    });    
});

var version;  

router.get('/about', async (req, res) => {
    
    if(!version){
        if(fs.existsSync('./version.txt')){            
            version = await fsPromises.readFile('./version.txt', { encoding: 'utf-8'});
            if(version){
                version = version.trim();
            }
        }

        if(!version)
            version = 'UNKNOWN';
    }
    res.render('about', 
    { 
        title: 'About', 
        user: req.user, 
        settings: req.settings,
        version: version,
        AppHelper: AppHelper.getInstance()
    });    
});
  
module.exports = router;