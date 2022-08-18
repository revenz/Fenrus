const express = require('express');
const common = require('./Common');
const System = require('../models/System');
const Utils = require('../helpers/utils');
const FenrusRouter = require('./FenrusRouter');

class DockerRouter extends FenrusRouter {
    
    router;

    constructor()
    {
        super();
        this.router = express.Router();
        this.init();
    }

    get()
    {
        return this.router;
    }

    async getSettings(req){            
        return await System.getInstance();
    }

    init() {
        
        this.router.get('/', async(req, res) => await this.safeAsync('index', req, res));
  

        this.router.route('/:uid')
            .get((req, res) => this.safe('getItem', req, res))
            .post(async(req, res) => await this.safeAsync('save', req, res))
            .delete(async(req, res) => await this.safeAsync('delete', req, res));

        
        this.router.param('uid', async (req, res, next, uid) => {
            try
            {
                if(!uid){
                    res.status(400).send('no uid specified').end();
                    return;    
                }
                let settings = await this.getSettings(req);
                let docker = (settings.Docker || []).filter(x => x.Uid === uid);
                docker = docker?.length ? docker[0] : null;
                req.isNew = !docker;
                if(!docker){
                    // its new        
                    docker = { 
                        Uid: uid,
                        Name: '',
                        Address: '',
                        Port: 2375
                    };
                }
                req.uid = uid;
                req.model = settings;
                req.docker = docker;
                next();
            }
            catch(err) {
                this.handleError(res, err);
            }
        });
    }

    async index(req, res) 
    {        
        let settings = await this.getSettings(req);
        let data = [...(settings.Docker || [])];
    
        data.sort((a,b) => {
            return a.Name.localeCompare(b.Name);
        });
        
        res.render('settings/list', common.getRouterArgs(req, { 
            title: 'Docker',
            data: {
                typeName: 'Docker',
                title: 'Docker',
                description: 'This page lets you configure Docker instances which can be used in Apps and Links to open terminals into.',
                icon: 'icon-microchip',
                baseUrl: '/settings/system/docker',
                noEnabled: true,
                items: data
            },
        })); 
    }

    getItem(req, res) {
        res.render('settings/docker/editor', common.getRouterArgs(req, 
        { 
            title: 'Docker',   
            model: req.docker
        }));    
    }

    async save(req, res) 
    {            
        if(req.isNew)
        {
            if(!req.model.Docker)
                req.model.Docker = [];
            req.model.Docker.push({
                Uid: new Utils().newGuid(),
                Name: req.body.Name,
                Address: req.body.Address,
                Port: req.body.Port
            });
        }
        else
        {
            req.docker.Name = req.body.Name;
            req.docker.Address = req.body.Address;
            req.docker.Port = req.body.Port;
        }
        req.model.save();
        res.status(200).send('');
    }

    async delete(req, res) {
        if(req.isNew === false)
        {
            let uid = req.uid;        
            let settings = req.model;
            settings.Docker = settings.Docker.filter(x => x.Uid !== uid);
            await settings.save();
        }
        res.status(200).send('');
    }
}
  


module.exports = DockerRouter;