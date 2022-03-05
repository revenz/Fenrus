const express = require('express');
const common = require('./Common');
const Settings = require('../models/Settings');
const AppHelper = require('../helpers/appHelper');
const System = require('../models/System');
const Utils = require('../helpers/utils');

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
            let groups = (args.settings.Groups || []).map(x => { 
                return {
                    Uid: x.Uid,
                    Name: x.Name,
                    Enabled: x.Enabled,
                    IsSystem: false
                }
            });

            if(req.user.IsAdmin === true){
                let system = System.getInstance();
                if(system.SystemGroups?.length){
                    groups = system.SystemGroups.map(x => { 
                        return {
                            Uid: x.Uid,
                            Name: 'System: ' + x.Name,
                            Enabled: x.Enabled,
                            IsSystem: true
                        }
                    }).concat(groups);
                }
            }

            console.log('groups', groups);

            args.data = {
                typeName: 'Group',
                title: 'Groups',
                icon: 'icon-bars',
                baseUrl: '/settings/groups',
                items: groups
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

        this.router.get('/system/:uid', async (req, res) => {   
            if(req.user?.IsAdmin !== true)
                return res.sendStatus(401);

            let uid = req.params.uid;
            
            let system = System.getInstance();
            let isNew = uid === 'new';
            let group = isNew ? {
                Uid: 'new',
                IsSystem: true,
                Name: 'New Group',
                Items: []
            } : system.SystemGroups?.find(x => x.Uid === uid);
        
            if(!group){
                return res.sendStatus(404);
            }

            if(!group.AccentColor)
                group.AccentColor = settings.AccentColor || '#ff0090';
            
            let apps = AppHelper.getInstance().getList();
            res.render('groups/editor', common.getRouterArgs(req, { 
                title: 'Group',
                isSystem: true,
                apps: apps,
                model:group
            }));   
        });

        this.router.get('/:uid', async (req, res) => {    
            let uid = req.params.uid;
            let settings = await this.getSettings(req);
            let isNew = uid === 'new';
            let group = isNew ? {
                Uid: 'new',
                Name: 'New Group',
                Items: []
            } : settings.Groups?.find(x => x.Uid === uid);
        
            if(!group){
                return res.sendStatus(404);
            }

            if(!group.AccentColor)
                group.AccentColor = settings.AccentColor || '#ff0090';
            
            let apps = AppHelper.getInstance().getList();
            res.render('groups/editor', common.getRouterArgs(req, { 
                title: 'Group',
                apps: apps,
                model:group
            }));  
        });

        
        this.router.post('/:uid', async (req, res) => {    

            let uid = req.params.uid;
            let system = System.getInstance();

            let name = req.body.Name.trim();
            let systemGroup = system.SystemGroups?.find(x => x.Uid === uid);
            let settings = await this.getSettings(req);

            if(systemGroup){
                // check for duplicate names
                let dupName = system.SystemGroups.find(x => x.Uid != uid && x.Name?.toLowerCase() == name.toLowerCase());
                if(dupName)
                    return res.status(400).send('Duplicate name');

                // get existing 
                systemGroup.Name = name;
                systemGroup.HideGroupTitle = req.body.HideGroupTitle;
                systemGroup.Items = req.body.Items || [];
                if(req.body.AccentColor.toLowerCase() === settings.AccentColor.toLowerCase())
                    systemGroup.AccentColor = '';
                else
                    systemGroup.AccentColor = req.body.AccentColor;
                await system.save();
                return res.sendStatus(200); 
            }
            else{
                // check for duplicate names
                let dupName = settings.Groups.find(x => x.Uid != uid && x.Name?.toLowerCase() == name.toLowerCase());
                if(dupName)
                    return res.status(400).send('Duplicate name');


                // get existing 
                let group = settings.Groups.find(x => x.Uid === uid)

                if(!group) {
                    group = {
                        Uid: new Utils().newGuid(),
                        Enabled: true
                    };
                    settings.Groups.push(group);
                }
                group.Name = name;
                group.HideGroupTitle = req.body.HideGroupTitle;
                group.Items = req.body.Items || [];
                if(req.body.AccentColor.toLowerCase() === settings.AccentColor.toLowerCase())
                    group.AccentColor = '';
                else
                    group.AccentColor = req.body.AccentColor;
                await settings.save();
                return res.sendStatus(200); 
            }
        });
    }
}
  


module.exports = GroupsRouter;