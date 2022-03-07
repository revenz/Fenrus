const express = require('express');
const common = require('./Common');
const Settings = require('../models/Settings');
const AppHelper = require('../helpers/appHelper');
const System = require('../models/System');
const Utils = require('../helpers/utils');

class GroupsRouter{
    
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

    async getSettings(req){
        return req.settings;
    }

    init() {
        
        this.router.get('/', async (req, res) => await this.list(req, res));

        this.router.post('/:uid/status/:enabled', async (req, res) => await this.setState(req, res));        

        this.router.delete('/:uid', async (req, res) => await this.deleteItem(req, res));

        this.router.get('/:uid', async (req, res) => await this.getItem(req, res));
        
        this.router.post('/:uid', async (req, res) => await this.save(req, res));
        
        this.router.post('/copy-item/:uid', async (req, res) => await this.copyItem(req, res));
    }

    async list(req, res) 
    {
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

        args.data = {
            typeName: 'Group',
            title: 'Groups',
            description: 'This page lets you create groups which can be used on Dashboards.\n\nA group will not appear by itself, it must be added to a dashboard.',
            icon: 'icon-puzzle-piece',
            baseUrl: '/settings/groups',
            items: groups
        };
        
        res.render('settings/list', common.getRouterArgs(req, args)); 
    }

    async setState(req, res) {
        let enabled = req.params.enabled !== false && req.params.enabled !== 'false';
    
        let settings = await this.getSettings(req);
        let group = settings.Groups.find(x => x.Uid === req.params.uid);
        if(!group)
            return req.sendStatus(200); // silent fail
    
        group.Enabled = enabled;
        await settings.save();
        res.sendStatus(200);        
    }

    async deleteItem(req, res) {
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
    }

    async getItem(req, res) {   
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

        let dashboards = (settings.Dashboards || []).filter(x => x.Enabled !== false).map(x => {
            return {
                Uid: x.Uid,
                Name: x.Name
            }
        });
        dashboards.sort((a,b) => {
            a.Name.localeCompare(b.Name);
        })

        let systemGroups = [];
        if(req.user.IsAdmin)
        {
            let system = System.getInstance();
            systemGroups = system.SystemGroups.filter(x => x.Uid != group.Uid);
        }
        let groups = req.settings.Groups.filter(x => x.Uid != group.Uid);
        
        let apps = AppHelper.getInstance().getList();
        res.render('settings/groups/editor', common.getRouterArgs(req, { 
            title: 'Group',
            apps: apps,
            dashboards: dashboards,
            model:group,
            systemGroups: systemGroups,
            groups: groups
        }));  
    }

    async save(req, res) 
    {  
        let uid = req.params.uid;

        let name = req.body.Name.trim();
        let settings = await this.getSettings(req);

        // check for duplicate names
        let dupName = settings.Groups.find(x => x.Uid != uid && x.Name?.toLowerCase() == name.toLowerCase());
        if(dupName)
            return res.status(400).send('Duplicate name');

        // get existing 
        let group = settings.Groups.find(x => x.Uid === uid);

        let addToDashboard = false;

        if(!group) {
            group = {
                Uid: new Utils().newGuid(),
                Enabled: true
            };
            addToDashboard = req.body.AddToDashboard === true;
            settings.Groups.push(group);
        }
        group.Name = name;
        group._Type = 'DashboardGroup';
        group.HideGroupTitle = req.body.HideGroupTitle;
        group.Items = req.body.Items || [];
        if(req.body.AccentColor.toLowerCase() === settings.AccentColor.toLowerCase())
            group.AccentColor = '';
        else
            group.AccentColor = req.body.AccentColor;

        if(addToDashboard) {
            let defaultDashboard = settings.Dashboards?.find(x => x.Name === 'Default');
            if(defaultDashboard){
                if(!defaultDashboard.Groups)
                    defaultDashboard.Groups = [];
                defaultDashboard.Groups.push({
                    Uid: group.Uid,
                    Name: group.Name,
                    Enabled: true
                });
            }
        }

        await settings.save();
        return res.sendStatus(200); 
    }

    async copyItem(req, res) 
    {
        let item = req.body;
        item.Uid = new Utils().newGuid();
        if(!item.Name)
            return res.status(400).send('Invalid data');

        let destination = req.params.uid;
        let group = req.settings.Groups.find(x => x.Uid === destination);
        if(group){
            if(!group.Items)
                group.Items = [];
            group.Items.push(item);
            await req.settings.save();
            return res.sendStatus(200);
        }

        if(req.user.IsAdmin == false)
            return res.status(400).send('Unknown group');
        
        let system = System.getInstance();
        group = system.SystemGroups.find(x => x.Uid === destination);
        if(!group)
            return res.status(400).send('Unknown group');

        if(!group.Items)
            group.Items = [];
        group.Items.push(item);
        await system.save();
        return res.sendStatus(200);
        
    }
}
  


module.exports = GroupsRouter;