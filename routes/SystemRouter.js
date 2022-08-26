const express = require('express');
const FileHelper = require('../helpers/FileHelper');
const System = require('../models/System');
const common = require('./Common');
const HttpHelper = require('../helpers/HttpHelper');
const Utils = require('../helpers/utils');
const StreamZip = require('node-stream-zip');
const fs = require('fs');

class SystemRouter
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
        this.router.get('/', async (req, res) => {

            let themes = await FileHelper.getDirectories('./wwwroot/themes');

            var system = System.getInstance();

            res.render('system', common.getRouterArgs(req, { 
                title: 'System',
                system: system,
                themes: themes
            }));    
        });
        

        this.router.post('/', async (req, res) => {

            let model = req.body;
            if(!model){
                res.status(400).send('Invalid data').end();
                return;
            }

            var system = System.getInstance();
            // explicitly set these variables for security
            system.AllowRegister = model.AllowRegister === true;
            system.AllowGuest = model.AllowGuest === true;
            
            await system.save();
            res.status(200).send('').end();
        });

        this.router.post('/update-apps', async(req, res) => this.updateApps(req, res));
    }

    async updateApps(req, res)
    {
        console.log('Updating Applications');
        const dir = __dirname + '/../temp';
        if(fs.existsSync(dir) == false){
            console.log('Creating temporary directory');
            fs.mkdirSync(dir, {recursive: true});
        }
        
        let appsUrl = 'https://github.com/revenz/Fenrus/raw/master/apps.zip';
        let zipfile =  dir + '/' + new Utils().newGuid();
        try
        {
            await HttpHelper.download(appsUrl, zipfile);
            
            const zip = new StreamZip.async({ file: zipfile});
            const count = await zip.extract(null, './apps');
            console.log(`Extracted ${count} entries`);
            await zip.close();

            console.log('Successfully updated applications');
            res.status(200).send(`Updated ${count} applications`).end();
        }
        catch(err)
        {
            console.log('Failed to update applications: ' + err);
            res.status(500).send('' + err).end();
        }
    }
}

module.exports = SystemRouter;