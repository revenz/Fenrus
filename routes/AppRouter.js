const express = require('express');
const fetch = require('node-fetch');

let Settings = require('../models/settings');
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');
const path = require('path');

const router = express.Router();

let instances = {};

function getInstance(app, appInstance){    
    if(instances[appInstance.Uid])
        return instances[appInstance.Uid];

    let classType = require(`../apps/${app.Directory}/code.js`);
    instances[appInstance.Uid] = new classType();
    return instances[appInstance.Uid];
}

function getAppArgs(appInstance){
    let url = appInstance.Url;
    let utils = new Utils();
    let settings = Settings.getInstance();
    let funcArgs = {
        url: url,
        properties: appInstance.Properties,
        Utils: utils,
        linkTarget: settings.LinkTarget,
        liveStats: (items) => {            
            let html = '<ul class="livestats">';
            for (let item of items) {
                
                for(let i=0;i<item.length;i++){
                    if(/^:html:/.test(item[i]) == false)
                        item[i] = utils.htmlEncode(item[i]);
                    else
                        item[i] = item[i].substring(6);
                }

                html += `<li><span class="title">${item[0]}</span><span class="value">${item[1]}</span></li>`;
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
    let appInstance = req.appInstance;   

    let instance = getInstance(app, appInstance);

    let funcArgs = getAppArgs(appInstance);
    try
    {
        let result = await instance.status(funcArgs);        
        res.send(result || '').end();
    }
    catch(err){}
});

router.get('/:appName/app.css', (req, res) => {    
    let app = req.app;
    let file = `../apps/${app.Directory}/app.css`;
    res.setHeader('Cache-Control', 'public, max-age=3600'); // cache header
    res.sendFile(path.resolve(__dirname, file));
});

router.get('/:appName/:icon', (req, res) => {    
    let app = req.app;
    let file = `../apps/${app.Directory}/${app.Icon}`;
    res.setHeader('Cache-Control', 'public, max-age=3600'); // cache header
    res.sendFile(path.resolve(__dirname, file));
});


router.post('/:appName/test', async (req, res) => {
    let app = req.app;
    let appInstance = req.appInstance;   
    
    let instance = getInstance(app, appInstance);
    
    let funcArgs = getAppArgs(appInstance);
    let msg = '';
    try
    {
        let result = await instance.test(funcArgs);   
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