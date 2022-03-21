const express = require('express');
const fetch = require('node-fetch');

let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');
const path = require('path');
const ChartHelper = require('../helpers/ChartHelper');
const fsExists = require('fs.promises.exists');
const Globals = require('../Globals');
const FenrusRouter = require('./FenrusRouter');

class AppRouter extends FenrusRouter {
    
    router;
    instances = {};

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
    
    init() 
    {        
        this.router.get('/:appName/app.css', async(req, res) => await this.safeAsync('getAppCss', req, res));
        
        this.router.post('/:appName/test', async(req, res) => await this.safeAsync('test', req, res));

        this.router.get('/:appName/www/:resource', async(req, res) => await this.safeAsync('getResource', req, res));

        this.router.get('/:appName/:uid/status', async(req, res) => await this.safeAsync('getStatus', req, res));

        this.router.get('/:appName/:icon', async(req, res) => await this.safeAsync('getIcon', req, res));


        this.router.param('appName', (req, res, next, appName) => {
            let app = AppHelper.getInstance().get(appName);
            
            if(!app){
                res.status(404).send("App not found").end();
                return;
            }
            req.app = app;
            next();
        });

        this.router.param('uid', (req, res, next, uid) => {
            
            let appInstance = req.settings.findAppInstance(uid);
            if(!appInstance){
                res.status(404).send("App instance not found: " + uid).end();
                return;
            }
            req.appInstance = appInstance;
            next();
        })
    }

    async test(req, res) 
    {
        let app = req.app;
        let appInstance = req.body.AppInstance;   
        
        let instance = this.getInstance(app, appInstance);
        if(!instance.funcArgs)
            instance.funcArgs = this.getAppArgs(appInstance, req.settings);
        let funcArgs = instance.funcArgs;
        funcArgs.url = appInstance.ApiUrl || appInstance.Url;
        funcArgs.properties = appInstance.Properties;

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
    }

    async getAppCss(req, res)
    {
        let app = req.app;
        let file = `../apps/${app.Directory}/app.css`;
        if(await fsExists(file) === false)
            return res.sendStatus(404);
        res.setHeader('Cache-Control', 'public, max-age=' + (31 * 24 * 60 * 60)); // cache header
        res.sendFile(path.resolve(__dirname, file));
    }


    async getResource(req, res) 
    {    
        let resource = req.params.resource;
        if(/^[\w\d-_'"]+\.(png|jp(e)?g|css|svg|ico|gif)$/.test(resource) == false)
            return res.sendStatus(404);

        let app = req.app;
        let file = `../apps/${app.Directory}/${resource}`;
        file = path.join(__dirname, file);
        if(await fsExists(file) === false)
            return res.sendStatus(404);
        res.sendFile(file);    
    }

    async getIcon(req, res) 
    {
        let app = req.app;
        let file = `../apps/${app.Directory}/${app.Icon}`;
        res.setHeader('Cache-Control', 'public, max-age=' + (31 * 24 * 60 * 60)); // cache header
        res.sendFile(path.resolve(__dirname, file));
    }

    async getStatus(req, res) 
    {
        let app = req.app;  
        try
        {      
            let appInstance = req.appInstance;   

            let instance = this.getInstance(app, appInstance);
            if(!instance.funcArgs)
                instance.funcArgs = this.getAppArgs(appInstance, req.settings);
            let funcArgs = instance.funcArgs;
            funcArgs.url = appInstance.ApiUrl || appInstance.Url;
            funcArgs.properties = appInstance.Properties;
            funcArgs.changeIcon = (icon) => {
                res.setHeader('x-icon', funcArgs.Utils.base64Encode(icon));
            };
            funcArgs.setStatusIndicator = (indicator) => {
                indicator = (indicator || '').toLowerCase();
                if(indicator.startsWith('pause'))
                    indicator = '/common/status-icons/paused.png';
                else if(indicator.startsWith('record'))
                    indicator = '/common/status-icons/recording.png';
                else if(indicator.startsWith('stop'))
                    indicator = '/common/status-icons/stop.png';
                else if(indicator.startsWith('update'))
                    indicator = '/common/status-icons/update.png';

                res.setHeader('x-status-indicator', indicator === '' ? indicator : funcArgs.Utils.base64Encode(indicator));
            };

            let result = await instance.status(funcArgs);    
            if(!result || typeof(result) === 'string')
                res.send(result || '').end();
            else{
                res.type("image/png");
                res.status(201); // tell the UI its an image
                //res.Headers = { ContentType: 'image/png' };
                res.write(result);
                res.end();
            }
        }
        catch(err)
        { 
            console.log('error in app: ' + app.Name, err);
            res.send('').end();
        }
    }

    getInstance(app, appInstance){    
        if(!appInstance){
            // this can be null if testing an app that has not yet been added        
            let classType = require(`../apps/${app.Directory}/code.js`);
            return new classType();
        }
        if(this.instances[appInstance.Uid])
            return this.instances[appInstance.Uid];

        let classType = require(`../apps/${app.Directory}/code.js`);
        this.instances[appInstance.Uid] = new classType();
        return this.instances[appInstance.Uid];
    }

    getChartHelper(appInstance){
        let w = 1, h = 1;
        // this should move into a theme somehow, since it will be theme depednant
        if(appInstance.Size === 'medium')
            w = 2, h = 2;
        else if(appInstance.Size === 'large')
            w = 6, h = 2;
        else if(appInstance.Size === 'x-large')
            w = 4, h = 4;
        else if(appInstance.Size === 'xx-large')
            w = 6, h = 6;
        let unit = 3.75 * 14; // 1rem 
        let chartHelper = new ChartHelper(w * unit, h * unit);
        
        let chart = {};
        Object.getOwnPropertyNames(ChartHelper.prototype).forEach(x => {
            if(x === 'render' || x === 'constructor')
                return;
            chart[x] = async (config) => { 
                return await chartHelper[x](config); 
            }
        })
        return chart;
    }

    barInfo(args, items)
    {    
        let html = ':bar-info:';
        for(let item of items){
            if(isNaN(item.percent) === false)
            {
                html += `<div class="bar-info" ${ item.tooltip ? ('title="' + args.Utils.htmlEncode(item.tooltip) + '"') : '' }>` +
                            (item.icon ? `<div class="bar-icon"><img src="${item.icon}" /></div>` : '') +
                            '<div class="bar">' +
                                `<div class="fill" style="width:${item.percent}%"></div>` +
                                '<div class="labels">' +
                                    `<span class="info-label">${args.Utils.htmlEncode(item.label)}</span>` +
                                    `<span class="fill-label">${item.percent.toFixed(1)} %</span>` +
                                '</div>' + 
                            '</div>' + 
                        '</div>';
            }
            else if(item.value)
            {
                html += '<div class="bar-info-label-value">' +                         
                        `<span class="label">${item.label}</span>` +
                        `<span class="value">${item.value}</span>` +
                        '</div>';
            }
        }        
        return html;
    }

    getAppArgs(appInstance, settings){
        let url = appInstance.ApiUrl || appInstance.Url;

        let utils = new Utils();

        let chartHelper = this.getChartHelper(appInstance);

        let funcArgs = {
            url: url,
            version: Globals.getVersion(),
            properties: appInstance.Properties,
            Utils: utils,
            linkTarget: settings.LinkTarget,
            appIcon: appInstance.Icon,
            size: appInstance.Size,
            chart: chartHelper,        
            proxy: (url) => {
                return '/proxy/' + utils.base64Encode(url).replace(/\//g, '-');
            },
            liveStats: (items) => {            
                let html = '<ul class="livestats">';
                for (let item of items) {
                    
                    for(let i=0;i<item.length;i++){
                        if(/^:html:/.test(item[i]) == false)
                            item[i] = utils.htmlEncode(item[i]);
                        else
                            item[i] = item[i].substring(6);
                    }
                    if(item.length ===  1)
                    {
                        // special case, this is doing a span
                        html += `<li><span class="title span">${item[0]}</span></li>`;
                    } else {
                        html += `<li><span class="title">${item[0]}</span><span class="value">${item[1]}</span></li>`;
                    }
                }
                html += '</ul>';
                return html;
            },      
            barInfo: (items) => this.barInfo(funcArgs, items),
            carousel: (items) => {
                let id = utils.newGuid().replaceAll('-', '');
                let html = `:carousel:${id}:<div class="carousel" id="${id}">`;
                let controls = '<div class="controls" onclick="event.stopImmediatePropagation();return false;">';
                let count = 0;
                for(let item of items){
                    let itemId = utils.newGuid();
                    html += `<div class="item ${count === 0 ? 'visible initial' : ''}" id="${id}-${count}">`;
                    html += item
                    html += '</div>';
                    controls += `<a href="#${itemId}" class="${count === 0 ? 'selected' : ''}"></a>`;
                    ++count;
                }
                controls += '</div>';
                html += controls;
                html += '</div>';
                return html;
            },
            prepareFetch: (args) => {
                if(typeof(args) === 'string')
                    args = { url: args };
                if (!args.url.startsWith('http')) {
                    if (url.endsWith('/') == false)
                        args.url = funcArgs.url + '/' + args.url;
                    else
                        args.url = funcArgs.url + args.url;
                }
                if (!args.headers)
                    args.headers = { 'Accept': 'application/json' };
                else if (!args.headers['Accept'])
                    args.headers['Accept'] = 'application/json';
                console.log(`[${args.method || 'GET'}] => ${args.url}`);
                return args;
            },
            fetchResponse: (args) => {
                args = funcArgs.prepareFetch(args);

                let controller = new AbortController();
                let timeoutId = setTimeout(() => {
                    controller.abort();
                    console.log('Aborted call as it exceeded timeout: ' + args.url);
                }, Math.min(Math.max(args.timeout || 3000, 3000), 10000));

                return fetch(args.url, {
                    headers: args.headers,
                    method: args.method,
                    body: args.body,                    
                    signal: controller.signal
                }).catch(error => {
                    if(timeoutId)
                        clearTimeout(timeoutId);
                    timeoutId = null;
                    console.log('error: ' + error);
                });
            },
            fetch: (args) => {
                args = funcArgs.prepareFetch(args);

                let controller = new AbortController();
                let timeoutId = setTimeout(() => {
                    controller.abort();
                    console.log('Aborted call as it exceeded timeout: ' + args.url);
                }, Math.min(Math.max(args.timeout || 3000, 3000), 10000));

                return fetch(args.url, {
                    headers: args.headers,
                    method: args.method,
                    body: args.body,                    
                    signal: controller.signal
                }).then(res => {
                    clearTimeout(timeoutId);
                    timeoutId = null;
                    if(args.headers['Accept'].includes('json'))
                        return res.json();
                    else if(args.headers['Accept'].includes('text'))
                        return res.text();
                    else
                        return res;
                }).catch(error => {
                    if(timeoutId)
                        clearTimeout(timeoutId);
                    timeoutId = null;
                    console.log('error: ' + error);
                });
            }
        }
        return funcArgs;
    }
}

module.exports = AppRouter;
