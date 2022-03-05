const express = require('express');
const Utils = require('../helpers/utils');
const common = require('./Common');
const System = require('../models/System');

const router = express.Router();

router.get('/', async (req, res) => {
    console.log('test');

    let isAdmin = req.user?.IsAdmin === true;
    let dashboards = [...(req.settings.Dashboards || [])];

    dashboards.sort((a,b) => {
        if(a.Name === 'Default')
            return -1;
        if(b.Name === 'Default')
            return 1;
        return a.Name.localeCompare(b.Name);
    });

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
    let dashboard;
    let system = System.getInstance();
    if(uid === 'Guest'){
        if(req.user?.IsAdmin !== true)
            return res.sendStatus(401);
        dashboard = {
            Uid: 'Guest',
            Name: 'Guest',
            Groups: system.GuestDashboard?.Groups || []
        };
    }
    else {
        dashboard = isNew ? {
            Uid: 'new',
            Name: 'New Dashboard',
            Groups: []
        } : req.settings.Dashboards?.find(x => x.Uid === uid);
    }

    if(!dashboard){
        return res.sendStatus(404);
    }

    let groups = getGroups(req.settings, system);

    // filter out any missing groups, incase they have been deleted
    let groupUids = groups.map(x => x.Uid);
    let groupCount = dashboard.Groups.length;
    dashboard.Groups = dashboard.Groups.filter(x => groupUids.indexOf(x.Uid) >= 0);
    if(groupCount != dashboard.Groups.length){
        // save it, so we remove the deleted groups
        if(uid !== 'Guest')
            req.settings.save();
    }
    
    res.render('dashboards/editor', common.getRouterArgs(req, { 
        title: 'Dashboards',
        model: {
            dashboard: dashboard,
            groups: groups
        }
    }));  
});

function getGroups(settings, system){
    
    let groups = settings.Groups?.filter(x => x.Enabled)?.map(x => {
        return { 
            Uid: x.Uid,
            Name: x.Name,
            IsSystem:false
        }
    }) || [];
    groups = groups.concat(system?.SystemGroups?.filter(x => x.Enabled)?.map(x => {
        return { 
            Uid: x.Uid,
            Name: x.Name,
            IsSystem:true
        }
    }) || []);

    groups.sort((a, b) => {
        if(a.IsSystem === b.IsSystem)
            return a.Name.localeCompare(b.Name);
        return a.IsSystem ? -1 : 1;
    })
    
    return groups;
}
  

router.post('/:uid', async (req, res) => {

    let name = req.body.Name.trim();
    let uid = req.params.uid;
    if(uid === 'Guest'){
        if(req.user?.IsAdmin !== true)
            return res.sendStatus(401);
        let system = System.getInstance();

        system.GuestDashboard.Groups = req.body.Groups || [];
        await system.save();
        return res.sendStatus(200);
    }

    let settings = req.settings;
    // check for duplicate names
    let dupName = settings.Dashboards.find(x => x.Uid != uid && x.Name?.toLowerCase() == name.toLowerCase());
    if(dupName)
        return res.status(400).send('Duplicate name');


    // get existing 
    let existing = settings.Dashboards.find(x => x.Uid === uid)

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
    let uid = req.params.uid;
    if(uid === 'Guest')
        return res.status(400).send("Cannot delete 'Guest' dashboard");

    let dashboard = req.settings.Dashboards.find(x => x.Uid === uid);
    if(!dashboard)
        return res.sendStatus(200); // already gone

    if(dashboard.Name === 'Default')
        return res.status(400).send('Cannot delete the \'Default\' dashboard.');

    req.settings.Dashboards = req.settings.Dashboards.filter(x => x.Uid != dashboard.Uid);
    await req.settings.save();
    res.sendStatus(200);
});

router.post('/:uid/status/:enabled', async (req, res) => {
    let uid = req.params.uid;
    if(uid === 'Guest')
        return res.status(400).send("Cannot change 'Guest' dashboard enabled state");

    let enabled = req.params.enabled !== false && req.params.enabled !== 'false';

    let settings = req.settings;
    let dashboard = settings.Dashboards.find(x => x.Uid === req.params.uid);
    if(!dashboard)
        return req.sendStatus(200); // silent fail

    dashboard.Enabled = enabled;
    await settings.save();
    res.sendStatus(200);
});
  

module.exports = router;