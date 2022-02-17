const express = require('express');
let Settings = require('../models/settings');

const router = express.Router();

router.get('/', (req, res) => {
    res.render('settings', 
    { 
        title: 'Settings',
        settings: Settings.getInstance()
    });    
});
  

router.post('/', async (req, res) => {

    let model = req.body;
    if(!model){
        res.status(400).send('Invalid data').end();
        return;
    }

    let instance = Settings.getInstance();
    Object.keys(model).forEach(k => {
        instance[k] = model[k];
    })
    await instance.save();
    res.status(200).send('').end();
});
  

module.exports = router;