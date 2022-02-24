const express = require('express');
const FileHelper = require('../helpers/FileHelper');
const System = require('../models/System');
const common = require('./Common');

const router = express.Router();

router.get('/', async (req, res) => {

    let themes = await FileHelper.getDirectories('./wwwroot/themes');

    var system = System.getInstance();

    res.render('system', common.getRouterArgs(req, { 
        title: 'System',
        system: system,
        themes: themes
    }));    
});
  

router.post('/', async (req, res) => {

    let model = req.body;
    if(!model){
        res.status(400).send('Invalid data').end();
        return;
    }

    var system = System.getInstance();
    // explicitly set these variables for security
    system.AllowRegister = model.AllowRegister === true;
    
    await system.save();
    res.status(200).send('').end();
});
  

module.exports = router;