const express = require('express');
const AppHelper = require('../helpers/appHelper');
const Utils = require('../helpers/utils');
let ImageHelper = require('../helpers/ImageHelper');

const router = express.Router();

router.post('/move', (req, res) => {
    let settings = req.settings;
    let sourceGroup = settings.findGroupInstance(req.body.SourceUid);
    if(!sourceGroup){
        res.status(404).send('Group not found');
        return;    
    }

    let itemUid = req.body.ItemUid;
    let item = sourceGroup.Items.filter(x => x.Uid === itemUid)[0];
    if(!item){
        res.status(404).send('Item not found in group');
        return;    
    }

    let destGroup = settings.findGroupInstance(req.body.DestinationUid);
    if(!destGroup){
        res.status(404).send('Destination not found');
        return;    
    }

    sourceGroup.Items = sourceGroup.Items.filter(x => x != item);
    destGroup.Items.push(item);;
    settings.save();
    res.status(200).send('');
});

router.route('/:uid')
     .get((req, res) => {
        let settings = req.settings;
        let group = req.group;
        let apps = AppHelper.getInstance().getList();
        res.render('group', 
        { 
            title: 'Group',   
            model: group,     
            user: req.user,
            settings: settings,
            apps: apps,
            Utils: new Utils()
        });    
    })
    .post(async (req, res) => {
        let model = req.body;
        if(!model.Name || (model._Type !== 'DashboardGroup' && model._Type))
        {
            res.status(404).send('Invalid group');
            return;
        }
        model._Type = 'DashboardGroup';
        
        let imageHelper = new ImageHelper();
        for(let item of model.Items || [])
        {
            console.log('saving item', item);
            if(item._Type === 'DashboardLink')
            {
                if(!item.Icon)
                    item.Icon = await imageHelper.downloadFavIcon(item.Url, item.Uid);
                else
                    item.Icon = await imageHelper.saveImageIfBase64(item.Icon, 'icons', item.Uid);
            }
            else if(/data:/.test(item.Icon))                
                item.Icon = await imageHelper.saveImageIfBase64(item.Icon, 'icons', item.Uid);
            else if(/^\/apps\//.test(item.Icon))
                item.Icon = ''; // default icon reset it
        }

        let settings = req.settings;
        if(req.isNew){
            // add to the list
            settings.addGroup(model);
        }else{
            // update it
            let group = req.group;
            Object.keys(model).forEach(x => {
                group[x] = model[x];
            })
            settings.save();
        }
        res.status(200).send('').end();
    })
    .delete((req, res) => {
        if(req.isNew === false)
        {
            let uid = req.uid;        
            let settings = req.settings;
            settings.Groups = settings.Groups.filter(x => x.Uid !== uid);
            settings.save();
        }
        res.status(200).send('').end();
    });

router.param('uid', (req, res, next, uid) => {
    if(!uid){
        res.status(400).send('no uid specified').end();
        return;    
    }

    let settings = req.settings;
    let group = settings.findGroupInstance(uid);
    req.isNew = !group;
    if(!group){
        // its a new group        
        group = { 
            Uid: new Utils().newGuid(),
            Items: [],
            Name: '',
            Width: 1,
            Height: 1
        };
    }
    req.uid = uid;
    req.settings = settings;
    req.group = group;
    next();
});

module.exports = router;