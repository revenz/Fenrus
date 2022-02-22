const express = require('express');
const fsPromises = require("fs/promises");
const fs = require("fs");
const common = require('./Common');

const router = express.Router();

router.get('/', (req, res) => {
    res.render('home', common.getRouterArgs(req, { 
        title: '', 
    }));    
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
    res.render('about', common.getRouterArgs(req, { 
        title: 'About', 
        version: version,
    }));      
});
  
module.exports = router;