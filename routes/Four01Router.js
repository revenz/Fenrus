const express = require('express');
const System = require('../models/System');
const Globals = require('../Globals');
const e = require('express');

class Four01Router{
    
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
        
        this.router.get('/', (req, res) => {                

            let system = System.getInstance();

            let msg = req.query.msg;
            console.log('msg', msg);
            if(msg === 'no-register'){
                msg = "User registration is currently disabled.   Please contact the administrator.";
            }
            else if(msg === 'unauthorized'){
                msg = "You are not authorized to use this site";
            }


            let redirectUrl = '';
            let loginUrl = '';
            
            if(system.AuthStrategy === 'OAuthStrategy')
                loginUrl = '/logout';
            else if(system.AllowGuest)
                redirectUrl = '/';
            else 
                loginUrl = '/login';
            
            res.render('401', {
                message: msg,
                version: Globals.Version,
                redirectUrl: redirectUrl,
                loginUrl: loginUrl
            });
        });
    }
}
  


module.exports = Four01Router;