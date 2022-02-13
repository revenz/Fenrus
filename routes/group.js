const express = require('express');
const AppHelper = require('../helpers/appHelper');
const Utils = require('../helpers/utils');
let Settings = require('../models/settings');

const router = express.Router();

router.get('/group/:uid', (req, res) => {
    let uid = req.params['uid'];
    let settings = Settings.getInstance();
    let group = settings.findGroupInstance(uid);
    if(!group){
        // its a new group
        group = { 
            Uid: uid,
            Items: [],
            Name: '',
            Width: 1,
            Height: 1
        };
    }
    let apps = AppHelper.getInstance().getList();
    res.render('group', 
    { 
        title: 'Group',   
        model: group,     
        settings: settings,
        apps: apps,
        Utils: new Utils()
    });    
});


module.exports = router;