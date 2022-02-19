const express = require('express');
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');

const router = express.Router();

router.get('/', (req, res) => {
    console.log('############## user', req.user);
    res.render('home', 
    { 
        title: 'Home Page', 
        Utils: new Utils(),
        user: req.user, 
        settings: req.settings,
        AppHelper: AppHelper.getInstance()
    });    
});
  

module.exports = router;