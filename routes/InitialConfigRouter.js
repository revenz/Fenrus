const express = require('express');
const common = require('./Common');
const System = require('../models/System');

class InitialConfigRouter{
    
    router;
    callback;

    constructor(callback)
    {
        this.router = express.Router();
        this.callback = callback;
        this.init();
    }

    get()
    {
        return this.router;
    }

    init() {      
        
        this.router.use((req, res, next) => {
            let system = System.getInstance();
            if(system.getIsConfigured()){
                res.status(401).redirect('/');
                return;
            }
            next();
        });
        
        this.router.get('/', async (req, res) => {
            let args = common.getRouterArgs(req, 
            { 
                title: 'Initial Configuration'
            });
            res.render('initial-config', args);
        });

        this.router.post('/', async(req, res) => {
            console.log('initial config post!');
            let system = System.getInstance();
            let strategyName = req.body.Strategy;
            let strategy = system.getAuthStrategy(strategyName);
            console.log('strategyInstance', strategy);
            if(!strategy)
            {
                res.status(400).send('Invalid strategy');
                return;
            }

            let success = await strategy.saveInitialConfig(req.body[req.body.Strategy], req, res);
            if(success){
                system.AuthStrategy = strategyName;
                await system.save();
                this.callback(strategy);
                res.sendStatus(200);            }
        });    
    }
}
  


module.exports = InitialConfigRouter;