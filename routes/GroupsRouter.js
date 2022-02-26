const express = require('express');
const common = require('./Common');
const Settings = require('../models/Settings');

class GroupsRouter{
    
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

    init() {
        
        this.router.get('/', async (req, res) => {
            let args = common.getRouterArgs(req, 
            { 
                title: 'Groups',
                guestSettings: this.isGuest
            });
            args.settings = await this.getSettings(req);
            res.render('groups', args);
        });
        

        this.router.post('/order', async (req, res) => {

            let model = req.body;
            if(!model){
                res.status(400).send('Invalid data');
                return;
            }
            let uids = req.body.uids;
            let settings = await this.getSettings(req);

            settings.Groups = settings.Groups.sort((a, b) => {
                let aIndex = uids.indexOf(a.Uid);
                let bIndex = uids.indexOf(b.Uid);
                if(aIndex == -1 && bIndex == -1){
                    // not in the list
                    aIndex = settings.Groups.indexOf(a);
                    bIndex = settings.Groups.indexOf(b);
                    return aIndex - bIndex;
                }
                if(bIndex < 0)
                    return -1;
                if(aIndex < 0)
                    return 1;
                return aIndex - bIndex;
            });

            settings.save();

            res.status(200).send('').end();
        });
    }
}
  


module.exports = GroupsRouter;