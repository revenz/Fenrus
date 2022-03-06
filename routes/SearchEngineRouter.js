const express = require('express');
const common = require('./Common');
let ImageHelper = require('../helpers/ImageHelper');
const System = require('../models/System');
const Utils = require('../helpers/utils');

class SearchEngineRouter{
    
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
        // get the system settings settings                  
        return await System.getInstance();
    }

    init() {
        
        this.router.get('/', async (req, res) => {
            let settings = await this.getSettings(req);
            let data = [...(settings.SearchEngines || [])];
        
            data.sort((a,b) => {
                return a.Name.localeCompare(b.Name);
            });
            
            res.render('settings/list', common.getRouterArgs(req, { 
                title: 'Dashboards',
                data: {
                    typeName: (this.isSystem ? 'System ' : '') + 'Search Engine',
                    title: (this.isSystem ? 'System ' : '') + 'Search Engines',
                    description: this.isSystem  ? 
                        'This page lets you configure system wide search engines that will be available to all users and guests on the system.' : 
                        'This page lets you configure search engines that can be used on the home screen.\n\nIf you do not configure any and have no system search engines configured, the search box will not appear on the home screen.\n\nTo switch to an non-default search engine on the home screen type in the search engine Shortcut followed by a space.',
                    icon: 'icon-search',
                    baseUrl: '/settings/' + (this.isSystem ? 'system/' : '') + 'search-engines',
                    items: data
                },
            })); 
        });

        this.router.post('/:uid/default/:isDefault', async(req, res) => {
            if(req.isNew === false)
            {              
                for(let se of req.model.SearchEngines)
                {
                    se.IsDefault = false;
                }      
                req.searchEngine.IsDefault = req.params.isDefault === 'true';
                req.model.save();
            }
            res.status(200).send('');
        });        

        this.router.post('/:uid/status/:enabled', async(req, res) => {
            if(req.isNew === false)
            {            
                req.searchEngine.Enabled = req.params.enabled === 'true';
                req.model.save();
            }
            res.status(200).send('');
        });        

        this.router.route('/:uid')
            .get((req, res) => {
                res.render('settings/search-engines/editor', common.getRouterArgs(req, 
                { 
                    title: 'Search Engine',   
                    model: req.searchEngine,
                    isSystem: this.isSystem
                }));    
            })
            .post(async (req, res) => {

                let icon;
                if(!req.body.Icon)
                    icon = await new ImageHelper().downloadFavIcon(req.body.Url, req.uid);
                else
                    icon = await new ImageHelper().saveImageIfBase64(req.body.Icon, 'icons', req.uid);
                    
                let shortcut = (req.body.Shortcut || req.body.Name).replace(/\s/g, '').toLowerCase();

                if(req.isNew)
                {
                    if(!req.model.SearchEngines)
                        req.model.SearchEngines = [];
                    req.model.SearchEngines.push({
                        Uid: new Utils().newGuid(),
                        Name: req.body.Name,
                        Url: req.body.Url,
                        Shortcut: shortcut,
                        Enabled: true,
                        IsDefault: false,
                        Icon: icon
                    });
                }
                else
                {
                    req.searchEngine.Name = req.body.Name;
                    req.searchEngine.Url = req.body.Url;
                    req.searchEngine.Shortcut = shortcut;
                    req.searchEngine.Icon = icon;
                }
                req.model.save();
                res.status(200).send('');
            })
            .delete(async (req, res) => {
                if(req.isNew === false)
                {
                    let uid = req.uid;        
                    let settings = req.model;
                    settings.SearchEngines = settings.SearchEngines.filter(x => x.Uid !== uid);
                    await settings.save();
                }

                res.status(200).send('');
            });

        
        this.router.param('uid', async (req, res, next, uid) => {
            if(!uid){
                res.status(400).send('no uid specified').end();
                return;    
            }
            let settings = await this.getSettings(req);
            let searchEngine = settings.SearchEngines.filter(x => x.Uid === uid);
            console.log('searchEngine', searchEngine);
            searchEngine = searchEngine?.length ? searchEngine[0] : null;
            req.isNew = !searchEngine;
            if(!searchEngine){
                // its new        
                searchEngine = { 
                    Uid: uid,
                    Icon: '',
                    Name: '',
                    Url: 'https://www.google.com/search?q=%s',                    
                    IsDefault: false,
                    IsSystem: this.isSystem
                };
            }
            req.uid = uid;
            req.model = settings;
            req.searchEngine = searchEngine;
            next();
        });
    }
}
  


module.exports = SearchEngineRouter;