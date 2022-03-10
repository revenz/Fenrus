const express = require('express');
const common = require('./Common');
const Theme = require('../models/Theme')

const FenrusRouter = require('./FenrusRouter');

class ThemeRouter extends FenrusRouter 
{
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

    init() 
    {  
        this.router.get('/', async(req, res) => await this.safeAsync('index', req, res));
        
        this.router.post('/', async (req, res) => await this.safeAsync('save', req, res));
    }

    async index(req, res) 
    {
        let theme = await Theme.getTheme(req.theme.Name);
        if(!theme || !theme.Settings?.length)
        {
            res.status(404).redirect('/');
            return;
        }

        let themeModel = {};
        if(req.settings.ThemeSettings && req.settings.ThemeSettings[theme.Name])
            themeModel = req.settings.ThemeSettings[theme.Name];

        res.render('theme', common.getRouterArgs(req, { 
            title: 'Theme',
            model: theme,
            themeModel: themeModel
        }));  
    }

    async save(req, res) 
    { 
        let model = req.body;
        if(!model){
            res.status(400).send('Invalid data').end();
            return;
        }

        if(!req.settings.ThemeSettings)
            req.settings.ThemeSettings = {};

        req.settings.ThemeSettings[req.settings.Theme] = model;
        await req.settings.save();
        
        res.status(200).send('').end();
    }
}
    

module.exports = ThemeRouter;