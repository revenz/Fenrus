const express = require('express');
const Utils = require('../helpers/utils');
const System = require('../models/System');
var https = require('https');
var http = require('http');
const FenrusRouter = require('./FenrusRouter');
const urlParse = require('url').parse;

class ProxyRouter extends FenrusRouter {
    
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

    init() {
        this.router.get('/:url', async(req, res) => await this.safeAsync('proxyResource', req, res));
    }
        
    async proxyResource(req, res)
    {
        let system = System.getInstance();
        if(!system.AllowGuest && req.isGuest)
            return res.sendStatus(401);

        let url = req.params.url.replace(/\-/g, '/');
        url = new Utils().base64Decode(url);
        if(!url)
            return res.sendStatus(404);

        let isHttps = url.toLowerCase().startsWith('https:');
        var q = urlParse(url, true);
        let options = {
            host: q.hostname,
            port: q.port || (isHttps ? 443 : 80),
            path:  q.path,
            timeout: 5000
        };
        (isHttps ? https : http).get( options, (data) => 
        {
            try
            {
                //remove express headers
                Object.keys(res.getHeaders()).forEach((headerName) => {
                    res.removeHeader(headerName);
                });
            
                // write status
                res.status(data.statusCode);
                // write headers
                Object.keys(data.headers).forEach((headerName) => {
                    res.setHeader(headerName, data.headers[headerName]);
                });

                // cache it
                if(res.statusCode >= 200 && res.statusCode <= 299)
                    res.setHeader('Cache-Control', 'public, max-age=' + (31 * 24 * 60 * 60));                
            
                //Pipe data to response stream
                data.pipe(res);      
            }
            catch(err) 
            {                
                console.log('error proxying object', err);
                try{
                    res.sendStatus(500);
                }
                catch(err2){} // can happen if closed
            }
        });
    }
}

module.exports = ProxyRouter;