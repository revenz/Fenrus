const express = require('express');
let SystemConfig = require('../models/SystemConfig');

const router = express.Router();

router.get('/', (req, res) => {
    res.render('system', 
    { 
        title: 'System',
        settings: SystemConfig.getInstance()
    });    
});
  

router.post('/', async (req, res) => {

    let model = req.body;
    if(!model){
        res.status(400).send('Invalid data').end();
        return;
    }

    let instance = req.settings;
    Object.keys(model).forEach(k => {
        instance[k] = model[k];
    })
    await instance.save();
    res.status(200).send('').end();
});
  

module.exports = router;