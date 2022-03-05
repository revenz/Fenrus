const express = require('express');
const common = require('./Common');
const Settings = require('../models/Settings');
const AppHelper = require('../helpers/appHelper');

class GroupsRouter{
    
    isSystem;
    router;

    constructor(isSystem)
    {
        this.isSystem = isSystem;
        this.router = express.Router();
        this.init();
    }

    get()
    {
        return this.router;
    }

    async getSettings(req){
        if(!this.isSystem)
            return req.settings;
        // get the guest settings        
        return await Settings.getForGuest();
    }

    init() {
        
        this.router.get('/', async (req, res) => {

            let args = common.getRouterArgs(req, 
            { 
                title: 'Groups'
            });
            args.settings = await this.getSettings(req);
            args.data = {
                typeName: 'Group',
                title: 'Groups',
                icon: 'icon-bars',
                baseUrl: '/settings/groups',
                items: args.settings.Groups
            };
            
            res.render('settings/list', common.getRouterArgs(req, args)); 
        });

        this.router.post('/:uid/status/:enabled', async (req, res) => {
            let enabled = req.params.enabled !== false && req.params.enabled !== 'false';
        
            let settings = await this.getSettings(req);
            let group = settings.Groups.find(x => x.Uid === req.params.uid);
            if(!group)
                return req.sendStatus(200); // silent fail
        
            group.Enabled = enabled;
            await settings.save();
            res.sendStatus(200);
        });
        

        this.router.delete('/:uid', async (req, res) => {
            let settings = await this.getSettings(req);
            let group = settings.Groups.find(x => x.Uid === req.params.uid);
            if(!group)
                return res.sendStatus(200); // already gone

            settings.Groups = settings.Groups.filter(x => x.Uid != group.Uid);

            // basic remove from group
            for(let db of settings.Dashboards || [])
            {
                db.Groups = db.Groups.filter(x => x.Uid != group.Uid);
            }

            await settings.save();
            res.sendStatus(200);
        });

        this.router.get('/:uid', async (req, res) => {    
            let uid = req.params.uid;
            let isNew = uid === 'new';
            let group = isNew ? {
                Uid: 'new',
                Name: 'New Group',
                Items: []
            } : req.settings.Groups?.find(x => x.Uid === uid);
        
            if(!group){
                return res.sendStatus(404);
            }
            
            let apps = AppHelper.getInstance().getList();
            res.render('groups/editor', common.getRouterArgs(req, { 
                title: 'Group',
                apps: apps,
                model:group
            }));  
        });
    }
}
  


module.exports = GroupsRouter;