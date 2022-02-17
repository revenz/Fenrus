const express = require('express');
const fetch = require('node-fetch');

let Settings = require('../models/settings');
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');
const path = require('path');

const router = express.Router();

function getAppArgs(appInstance){
    let url = appInstance.Url;
    let utils = new Utils();
    let funcArgs = {
        url: url,
        properties: appInstance.Properties,
        Utils: utils,
        liveStats: (items) => {            
            let html = '<ul class="livestats">';
            for (let item of items) {

                html += `<li><span class="title">${utils.htmlEncode(item[0])}</span><span class="value">${utils.htmlEncode(item[1])}</span></li>`;
            }
            html += '</ul>';
            return html;
        },        
        fetch: (args) => {
            if(typeof(args) === 'string')
                args = { url: args };
            
            if (!args.url.startsWith('http')) {
                if (url.endsWith('/') == false)
                    args.url = url + '/' + args.url;
                else
                    args.url = url + args.url;
            }
            if (!args.headers)
                args.headers = { 'Accept': 'application/json' };
            else if (!args.headers['Accept'])
                args.headers['Accept'] = 'application/json';
            console.log(`REQUEST [${args.method || 'GET'}]: ${args.url}`);
            return fetch(args.url, {
                headers: args.headers,
                method: args.method,
                body: args.body
            }).then(res => res.json()).catch(error => {
                console.log('error: ' + error);
            });
        }
    }
    return funcArgs;
}

router.get('/:appName/:uid/status', async (req, res) => {    
    let app = req.app;    
    
    let func = require(`../apps/${app.Directory}/code.js`).status;
    if(!func){
        res.status(500).send("Invalid app").end();
        return;
    }

    let appInstance = req.appInstance;   
    let funcArgs = getAppArgs(appInstance);
    try
    {
        let result = await func(funcArgs);        
        res.send(result || '').end();
    }
    catch(err){}
});

router.get('/:appName/:icon', (req, res) => {    
    let app = req.app;
    let file = `../apps/${app.Directory}/${app.Icon}`;
    console.log('Icon file: ' + file);
    res.setHeader('Cache-Control', 'public, max-age=3600'); // cache header
    res.sendFile(path.resolve(__dirname, file));
});

router.post('/:appName/test', async (req, res) => {
    let app = req.app;
    let func = require(`../apps/${app.Directory}/code.js`).test;
    if(!func){
        res.status(500).send("Invalid app").end();
        return;
    }

    let appInstance = req.body.AppInstance; 
    let funcArgs = getAppArgs(appInstance);
    let msg = '';
    try
    {
        let result = await func(funcArgs);   
        console.log('test result', result);
        if(result){
            res.status(200).send('').end();
            return;
        }        
    }
    catch(err){ msg = err; }
    res.status(500).send(msg).end();
});

router.param('appName', (req, res, next, appName) => {
    let app = AppHelper.getInstance().get(appName);
    
    if(!app){
        res.status(404).send("App not found").end();
        return;
    }
    req.app = app;
    next();
});

router.param('uid', (req, res, next, uid) => {
    
    let appInstance = Settings.getInstance().findAppInstance(uid);
    if(!appInstance){
        res.status(404).send("App instance not found: " + uid).end();
        return;
    }
    req.appInstance = appInstance;
    next();
})


module.exports = router;