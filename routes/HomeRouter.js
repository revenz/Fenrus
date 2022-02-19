const express = require('express');
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');

const router = express.Router();

router.get('/', (req, res) => {
    console.log('home settings', req.settings);
    res.render('home', 
    { 
        title: 'Home Page', 
        Utils: new Utils(),
        settings: req.settings,
        AppHelper: AppHelper.getInstance()
    });    
});
  

module.exports = router;