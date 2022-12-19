const express = require('express');
const Utils = require('../helpers/utils');
const common = require('./Common');
const System = require('../models/System');
const ImageHelper = require('../helpers/ImageHelper');
const FileHelper = require('../helpers/FileHelper');

const router = express.Router();

router.get('/', async (req, res) => {
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
            description: 'This page lets you create Dashboards.  Having more than one dashboard will let you switch between them on the home screen.\n\nTo add items to your Dashboard, first create a Group containing the items you wish to add.  Then you can add that new Group to a Dashboard.',
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
    let description;
    if(uid === 'Guest'){
        if(req.user?.IsAdmin !== true)
            return res.sendStatus(401);
        description = 'This page lets you configure the Guest dashboard, the dashboard that will be shown to users if they have not signed in.\n\nYou can disable this dashboard if you wish for users to login before using Fenrus.\n\nThis is also the default dashboard set for new users.';
        dashboard = {
            Uid: 'Guest',
            Name: 'Guest',
            Theme: system.GuestDashboard?.Theme || 'Default',
            BackgroundImage: system.GuestDashboard?.BackgroundImage,
            Groups: system.GuestDashboard?.Groups || [],
            Enabled: system.AllowGuest
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

    let groups = getGroups(req.settings, system, uid === 'Guest');
    let themes = await FileHelper.getDirectories('./wwwroot/themes');

    // filter out any missing groups, incase they have been deleted
    let groupUids = groups.map(x => x.Uid);
    let groupCount = dashboard.Groups.length;
    dashboard.Groups = dashboard.Groups.filter(x => groupUids.indexOf(x.Uid) >= 0);
    if(groupCount != dashboard.Groups.length){
        // save it, so we remove the deleted groups
        if(uid !== 'Guest')
            req.settings.save();
    }

    if(!dashboard.AccentColor)
        dashboard.AccentColor = req.settings.AccentColor;
    
    res.render('settings/dashboards/editor', common.getRouterArgs(req, { 
        title: 'Dashboards',
        description: description,
        themes: themes,
        data: {}, // needed for generic list
        model: {
            dashboard: dashboard,
            groups: groups
        }
    }));  
});

function getGroups(settings, system, isGuest){
    
    let groups = isGuest ? [] : settings.Groups?.filter(x => x.Enabled)?.map(x => {
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

        let guestBackground = await new ImageHelper().saveImageIfBase64(req.body.BackgroundImage, 'backgrounds');
        system.GuestDashboard.BackgroundImage = guestBackground;
        system.GuestDashboard.Theme = req.body.Theme || 'Default';
        system.GuestDashboard.Groups = req.body.Groups || [];
        system.AllowGuest = req.body.Enabled;
        await system.save();
        return res.sendStatus(200);
    }

    let settings = req.settings;
    // check for duplicate names
    let dupName = settings.Dashboards.find(x => x.Uid != uid && x.Name?.toLowerCase() == name.toLowerCase());
    if(dupName)
        return res.status(400).send('Duplicate name');

    // get existing 
    let dashboard = settings.Dashboards.find(x => x.Uid === uid)

    if(!dashboard) {
        if(name === 'Guest' || name === 'Default')
            return res.status(400).send(`The name '${name}' is a reserved name.`);
        dashboard = {
            Uid: new Utils().newGuid(),
            Enabled: true,
            Name: name
        };
        settings.Dashboards.push(dashboard);
    }
    else
    {
        if(dashboard.Name !== 'Guest' && dashboard.Name !== 'Default')
        {
            if(name === 'Guest' || name === 'Default')
                return res.status(400).send(`The name '${name}' is a reserved name.`);

        }
    }
    dashboard.Name = name;
    dashboard.Theme = req.body.Theme;
    dashboard.Groups = req.body.Groups || [];
    dashboard.BackgroundImage = await new ImageHelper().saveImageIfBase64(req.body.BackgroundImage, 'backgrounds');

    if(req.body.AccentColor?.toLowerCase() === req.settings.AccentColor.toLowerCase())
        dashboard.AccentColor = '';
    else
        dashboard.AccentColor = req.body.AccentColor;
        
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

router.post('/:uid/move-group/:groupUid/:up', async (req, res) => {
    let dashboardUid = req.params.uid;
    let groupUid = req.params.groupUid;
    let up = req.params.up === 'true';

    let settings = req.settings;
    let dashboard = settings.Dashboards.find(x => x.Uid === dashboardUid);
    if(!dashboard)
        return res.sendStatus(200); // silent fail

    let index = dashboard.Groups.findIndex(x => x.Uid === groupUid);

    if(up === false && index >= dashboard.Groups.length - 1)
        return res.sendStatus(200); // already at bottom
    if(up  && index <= 0)
        return res.sendStatus(200); // already at top

    // moving up means actually move lower in the index where topmost is 0
    let dest = index + (up ? -1 : 1);

    // swap the items
    var a = dashboard.Groups[index];
    dashboard.Groups[index] = dashboard.Groups[dest];
    dashboard.Groups[dest] = a;

    await settings.save();
    res.sendStatus(200);
});  


router.post('/:uid/remove-group/:groupUid', async (req, res) => {
    let dashboardUid = req.params.uid;
    let groupUid = req.params.groupUid;

    let settings = req.settings;
    let dashboard = settings.Dashboards.find(x => x.Uid === dashboardUid);
    if(!dashboard)
        return res.sendStatus(200); // silent fail

    let index = dashboard.Groups.findIndex(x => x.Uid === groupUid);

    if(index < 0)
        return res.sendStatus(200); // silent fail
    dashboard.Groups.splice(index, 1);
    await settings.save();
    res.sendStatus(200);
});  

module.exports = router;