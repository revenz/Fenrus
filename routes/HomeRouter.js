const express = require('express');
let Settings = require('../models/settings');
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');

const router = express.Router();

router.get('/', (req, res) => {
    res.render('home', 
    { 
        title: 'Home Page', 
        Utils: new Utils(),
        settings: Settings.getInstance(),
        AppHelper: AppHelper.getInstance()
    });    
});
  

module.exports = router;