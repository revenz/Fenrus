const express = require('express');
const AppHelper = require('../helpers/appHelper');
const Utils = require('../helpers/utils');
let ImageHelper = require('../helpers/ImageHelper');
const common = require('./Common');
const Settings = require('../models/Settings');

class GroupRouter{
    
    isGuest;
    router;

    constructor(isGuest)
    {
        this.isGuest = isGuest;
        this.router = express.Router();
        this.init();
    }

    get()
    {
        return this.router;
    }

    async getSettings(req){
        if(!this.isGuest)
            return req.settings;
        // get the guest settings        
        return await Settings.getForGuest();
    }

    init() 
    {
        this.router.post('/move', async (req, res) => {
            let settings = await this.getSettings(req);
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

        this.router.post('/:uid/status/:enabled', (req, res) => {
            let enabled = req.params.enabled !== false && req.params.enabled !== 'false';
            req.group.Enabled = enabled;
            let settings = req.settings;
            settings.save();
            res.status(200).send('');
        });

        this.router.route('/:uid')
            .get((req, res) => {
                let group = req.group;
                let apps = AppHelper.getInstance().getList();
                res.render('group', common.getRouterArgs(req, 
                { 
                    title: 'Group',   
                    model: group,
                    guestSettings: this.isGuest,
                    apps: apps            
                }));    
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

        this.router.param('uid', async (req, res, next, uid) => {
            if(!uid){
                res.status(400).send('no uid specified').end();
                return;    
            }

            let settings = await this.getSettings(req);
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
    }
}

module.exports = GroupRouter;