const express = require('express');
const Utils = require('../helpers/utils');
const common = require('./Common');

const router = express.Router();

router.get('/', async (req, res) => {
    console.log('test');

    let isAdmin = req.user?.isAdmin === true;
    let dashboards = req.settings.Dashboards || [];
    if(isAdmin)
    {
        // get guest dashboards
    }

    res.render('settings/list', common.getRouterArgs(req, { 
        title: 'Dashboards',
        data: {
            typeName: 'Dashboard',
            title: 'Dashboards',
            icon: 'icon-home',
            baseUrl: '/settings/dashboards',
            items: dashboards
        },
    }));    
});
router.get('/:uid', async (req, res) => {    
    let uid = req.params.uid;
    let isNew = uid === 'new';
    let dashboard = isNew ? {
        Uid: 'new',
        Name: 'New Dashboard',
        Groups: []
    } : req.settings.Dashboards?.find(x => x.Uid === uid);

    if(!dashboard){
        return res.sendStatus(404);
    }
    
    res.render('dashboards/editor', common.getRouterArgs(req, { 
        title: 'Dashboards',
        model: {
            dashboard: dashboard,
            groups: req.settings.Groups.filter(x =>{ return {
                Uid: x.Uid,
                Name: x.Name
            }})
        }
    }));  
});
  

router.post('/:uid', async (req, res) => {

    let name = req.body.Name.trim();

    let settings = req.settings;
    // check for duplicate names
    let dupName = settings.Dashboards.find(x => x.Uid != req.params.uid && x.Name?.toLowerCase() == name.toLowerCase());
    if(dupName)
        return res.status(400).send('Duplicate name');


    // get existing 
    let existing = settings.Dashboards.find(x => x.Uid === req.params.uid)

    if(!existing) {
        if(name === 'Guest' || name === 'Default')
            return res.status(400).send(`The name '${name}' is a reserved name.`);
        let dashboard = {
            Uid: new Utils().newGuid(),
            Name: name,
            Enabled: true,
            Groups: req.body.Groups || []
        };
        settings.Dashboards.push(dashboard);
    }
    else
    {
        if(existing.Name !== 'Guest' && existing.Name !== 'Default')
        {
            if(name === 'Guest' || name === 'Default')
                return res.status(400).send(`The name '${name}' is a reserved name.`);

            existing.Name = name;
        }
        existing.Groups = req.body.Groups || [];
    }
    await settings.save();
    return res.sendStatus(200);
});


router.delete('/:uid', async (req, res) => {
    let dashboard = req.settings.Dashboards.find(x => x.Uid === req.params.uid);
    if(!dashboard)
        return res.sendStatus(200); // already gone

    if(dashboard.Name === 'Default')
        return res.status(400).send('Cannot delete the \'Default\' dashboard.');
    else if(dashboard.Name === 'Guest')
        return res.status(400).send('Cannot delete the \'Guest\' dashboard.');

    req.settings.Dashboards = req.settings.Dashboards.filter(x => x.Uid != dashboard.Uid);
    await req.settings.save();
    res.sendStatus(200);
});
  

module.exports = router;