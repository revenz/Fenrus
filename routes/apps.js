const express = require('express');
const fetch = require('node-fetch');

let Settings = require('../models/settings');
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');
const path = require('path');

const router = express.Router();

router.get('/apps/:appName/:uid/status', (req, res) => {
    
    let app = AppHelper.getInstance().get(req.params['appName']);
    if(!app){
        res.status(404).send("App not found");
        return;
    }
    let appInstance = Settings.getInstance().findAppInstance(req.params['uid']);
    if(!appInstance){
        res.status(404).send("App instance not found: " + req.params['uid']);
        return;
    }

    let func = require(`../apps/${app.Directory}/code.js`).status;
    if(!func){
        res.status(500).send("Invalid app");
        return;
    }


    let url = appInstance.Url;
    let funcArgs = {
        url: url,
        properties: appInstance.Properties,
        Utils: new Utils(),
        liveStats: (items) => {            
            let html = '<ul class="livestats">';
            for (let item of items) {

                html += `<li><span class="title">${htmlEncode(item[0])}</span><span class="value">${htmlEncode(item[1])}</span></li>`;
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

            console.log('about to fetch: ' + args.url);            
            return fetch(args.url, {
                headers: args.headers
            }).then(res => res.json()).catch(error => {
                res.status(500).send("Error: " + error);
                return;
            });
        }
    }
    func(funcArgs).then((html) =>{
        res.send(html);
    }).catch(error => {
        res.status(500).send(error);
    })
});

router.get('/apps/:appName/:icon', (req, res) => {
    
    let app = AppHelper.getInstance().get(req.params['appName']);
    if(!app || app.Icon != req.params['icon']){
        res.status(404).send("Invalid app icon");
        return;
    }

    let file = `../apps/${app.Directory}/${app.Icon}`;
    res.sendFile(path.resolve(__dirname, file));
});


function htmlEncode(text) {
    if(text === undefined) 
        return '';
    if(typeof(text) !== 'string')
        text = '' + text;
    return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\"/g, "&#34;").replace(/\'/g, "&#39;");
}

module.exports = router;