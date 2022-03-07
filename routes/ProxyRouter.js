const express = require('express');
const Utils = require('../helpers/utils');
const System = require('../models/System');
var https = require('https')
var http = require('http')

class ProxyRouter{
    
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
        this.router.get('/:url', (req, res) => {

            let system = System.getInstance();
            if(!system.AllowGuest && req.isGuest)
                return res.sendStatus(401);

            let url = req.params.url.replace(/\-/g, '/');
            url = new Utils().base64Decode(url);
            if(!url)
                return res.sendStatus(404);

            let isHttps = url.toLowerCase().startsWith('https:');

            (isHttps ? https : http).get(url, (data) => {
 
                //remove express headers
                Object.keys(res.getHeaders()).forEach((headerName) => {
                    res.removeHeader(headerName);
                });
         
                // write status
                res.status = data.statusCode;
                // write headers
                Object.keys(data.headers).forEach((headerName) => {
                    res.setHeader(headerName, data.headers[headerName]);
                });

                // cache it
                res.setHeader('Cache-Control', 'public, max-age=3600');
         
                //Pipe data to response stream
                data.pipe(res);         
            });
        });
    }
}

module.exports = ProxyRouter;