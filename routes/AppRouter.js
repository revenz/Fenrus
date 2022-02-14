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

            return fetch(args.url, {
                headers: args.headers
            }).then(res => res.json()).catch(error => {
                console.log('error: ' + error);
            });
        }
    }
    return funcArgs;
}

router.get('/:appName/:uid/status', (req, res) => {    
    let app = req.app;    
    
    let func = require(`../apps/${app.Directory}/code.js`).status;
    if(!func){
        res.status(500).send("Invalid app");
        return;
    }

    let appInstance = req.appInstance;   
    let funcArgs = getAppArgs(appInstance);
    func(funcArgs).then((html) =>{
        res.send(html);
    }).catch(error => {
        res.status(500).send(error);
    })
});

router.get('/:appName/:icon', (req, res) => {    
    let app = req.app;
    let file = `../apps/${app.Directory}/${app.Icon}`;
    console.log('Icon file: ' + file);
    res.sendFile(path.resolve(__dirname, file));
});

router.post('/:appName/test', (req, res) => {
    let app = req.app;
    let func = require(`../apps/${app.Directory}/code.js`).test;
    if(!func){
        res.status(500).send("Invalid app");
        return;
    }

    let appInstance = req.body.AppInstance; 
    let funcArgs = getAppArgs(appInstance);
    func(funcArgs).then(() =>{
        res.status(200).send('');
    }).catch(error => {
        res.status(500).send(error);
    })

});

router.param('appName', (req, res, next, appName) => {
    let app = AppHelper.getInstance().get(appName);
    
    if(!app){
        res.status(404).send("App not found");
        return;
    }
    req.app = app;
    next();
});

router.param('uid', (req, res, next, uid) => {
    
    let appInstance = Settings.getInstance().findAppInstance(uid);
    if(!appInstance){
        res.status(404).send("App instance not found: " + uid);
        return;
    }
    req.appInstance = appInstance;
    next();
})


module.exports = router;