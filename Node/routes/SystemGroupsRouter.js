const express = require('express');
const common = require('./Common');
const AppHelper = require('../helpers/appHelper');
const System = require('../models/System');
const Utils = require('../helpers/utils');
const adminMiddleware = require('../middleware/AdminMiddleware');
const ImageHelper = require('../helpers/ImageHelper');

class SystemGroupsRouter
{    
    router;

    constructor()
    {
        this.router = express.Router();
        this.init();
    }

    get()
    {
        return this.router;
    }

    init() {

        this.router.use(adminMiddleware);
        
        this.router.get('/', async (req, res) => {

            let args = common.getRouterArgs(req, 
            { 
                title: 'System Groups'
            });

            let system = System.getInstance();
            let groups = system.SystemGroups?.map(x => { 
                            return {
                                Uid: x.Uid,
                                Name: x.Name,
                                Enabled: x.Enabled,
                                IsSystem: true
                            }
                         }) || [];

            args.data = {
                typeName: 'System Group',
                title: 'System Groups',
                description: 'This page lets you configure groups that will be available to all users and will be available to use on the Guest dashboard.\n\nIf you disable a group here that group will become unavailable to all users using it.',
                icon: 'icon-puzzle-piece',
                baseUrl: '/settings/system/groups',
                items: groups,
                IsAdmin: true
            };
            
            res.render('settings/list', common.getRouterArgs(req, args)); 
        });

        this.router.post('/:uid/status/:enabled', async (req, res) => {
            let enabled = req.params.enabled !== false && req.params.enabled !== 'false';
        
            let system = System.getInstance();
            let group = system.SystemGroups.find(x => x.Uid === req.params.uid);
            if(!group)
                return req.sendStatus(200); // silent fail
        
            group.Enabled = enabled;
            await system.save();
            res.sendStatus(200);
        });
        

        this.router.delete('/:uid', async (req, res) => {
            let system = System.getInstance();
            let group = system.SystemGroups.find(x => x.Uid === req.params.uid);
            if(!group)
                return res.sendStatus(200); // already gone

            system.SystemGroups = system.SystemGroups.filter(x => x.Uid != group.Uid);

            // basic remove from group
            if(system.GuestDashboard?.Groups?.length)
            {
                system.GuestDashboard.Groups = system.GuestDashboard.Groups.filter(x => x.Uid != group.Uid);
            }

            await system.save();
            res.sendStatus(200);
        });

        this.router.get('/:uid', async (req, res) => {   
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
                group.AccentColor = req.settings.AccentColor || '#ff0090';
            
            let systemGroups = system.SystemGroups.filter(x => x.Uid != group.Uid);
            let groups = req.settings.Groups.filter(x => x.Uid != group.Uid);
            
            group.IsSystem = true;
            let apps = AppHelper.getInstance().getList();
            res.render('settings/groups/editor', common.getRouterArgs(req, { 
                title: 'System Group',
                isSystem:true,
                apps: apps,
                model:group,
                systemGroups: systemGroups,
                docker: [],
                groups: groups
            }));   
        });
        
        this.router.post('/:uid', async (req, res) => {    

            let uid = req.params.uid;
            let system = System.getInstance();

            let name = req.body.Name.trim();
            let group = system.SystemGroups?.find(x => x.Uid === uid);

            // check for duplicate names
            let dupName = system.SystemGroups.find(x => x.Uid != uid && x.Name?.toLowerCase() == name.toLowerCase());
            if(dupName)
                return res.status(400).send('Duplicate name');

            let addToDashboard = false;
            if(!group)
            {
                // new group                    
                group = {
                    Uid: new Utils().newGuid(),
                    Enabled: true
                };
                system.SystemGroups.push(group);
                addToDashboard = req.body.AddToDashboard === true;  
            }

            // get existing 
            group.Name = name;
            group._Type = 'DashboardGroup';
            group.HideGroupTitle = req.body.HideGroupTitle;
            group.Items = req.body.Items || [];
            for(let item of group.Items)
            {
                if(!item.Icon && item._Type === 'DashboardLink')
                    item.Icon = await new ImageHelper().downloadFavIcon(item.Url, item.Uid);
                else
                    item.Icon = await new ImageHelper().saveImageIfBase64(item.Icon, 'icons', item.Uid);
            }

            if(req.body.AccentColor.toLowerCase() === req.settings.AccentColor.toLowerCase())
                group.AccentColor = '';
            else
                group.AccentColor = req.body.AccentColor;

            if(addToDashboard) {
                if(!system.GuestDashboard)
                    system.GuestDashboard = { Uid: 'Guest', Name: 'Name', Groups: []};
                if(!system.GuestDashboard.Groups)
                    system.GuestDashboard.Groups = [];

                system.GuestDashboard.Groups.push({
                    Uid: group.Uid,
                    Name: group.Name,
                    Enabled: true
                });
            }

            await system.save();
            return res.sendStatus(200);                     
        });
    }
}
  


module.exports = SystemGroupsRouter;