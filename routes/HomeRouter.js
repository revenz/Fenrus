const express = require('express');
const common = require('./Common');
const Globals = require('../Globals');

const router = express.Router();

router.get('/', (req, res) => {
    res.render('home', common.getRouterArgs(req, { 
        title: '', 
    }));    
});

router.get('/about', async (req, res) => {
    
    res.render('about', common.getRouterArgs(req, { 
        title: 'About', 
        version: Globals.Version,
    }));      
});
  
module.exports = router;