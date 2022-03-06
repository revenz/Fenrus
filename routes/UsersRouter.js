const express = require('express');
const UserManager = require('../helpers/UserManager');
const common = require('./Common');
const System = require('../models/System');

class UsersRouter
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

    init()
    {
        this.router.get('/', (req, res) => {
            let userManager = UserManager.getInstance();         
            let system = System.getInstance();

            res.render('settings/users/list', common.getRouterArgs(req, { 
                title: 'Users',        
                allowRegister: system.AllowRegister,
                users: userManager.listUsers()
            }));
        });

        this.router.delete('/:uid', async (req, res) => {    
            let userManager = UserManager.getInstance();  
            await userManager.deleteUser(req.params.uid);
            res.sendStatus(200);
        });

        
        this.router.get('/set-allow-register/:allowed', async (req, res) => { 
            let allowed = req.params.allowed === 'true';
                      
            let system = System.getInstance();
            system.AllowRegister = allowed;
            await system.save();
            res.sendStatus(200);
        });

        this.router.get('/:uid/set-admin/:isAdmin', async (req, res) => { 
            let isAdmin = req.params.isAdmin === 'true';
            
            let userManager = UserManager.getInstance();  
            let user = userManager.getUserByUid(req.params.uid);
            if(!user){
                res.status(404).send('User not found').end();
                return;
            }
            user.IsAdmin = isAdmin;
            await userManager.save();
            res.sendStatus(200);
        });
    }
}

module.exports = UsersRouter;