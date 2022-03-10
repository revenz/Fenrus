const express = require('express');
const FileHelper = require('../helpers/FileHelper');
const common = require('./Common');
const routerDashboard = require('./DashboardRouter');
const GroupsRouter = require('./GroupsRouter');
const SystemGroupsRouter = require('./SystemGroupsRouter');
const SearchEngineRouter = require('./SearchEngineRouter');
const UsersRouter = require('./UsersRouter');
const AdminMiddleware = require('../middleware/AdminMiddleware');
const FenrusRouter = require('./FenrusRouter');

class SettingsRouter extends FenrusRouter 
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
        this.router.get('/about', (req, res) => this.safe('about', req, res));
        this.router.post('/', async(req, res) => await this.safeAsync('save', req, res));
                
        this.router.use('/dashboards', routerDashboard);
        this.router.use('/groups', new GroupsRouter().get());
        this.router.use('/search-engines', new SearchEngineRouter().get());

        this.router.use(AdminMiddleware);
        this.router.use('/system/groups', new SystemGroupsRouter().get());
        this.router.use('/system/search-engines', new SearchEngineRouter(true).get());
        this.router.use('/users', new UsersRouter().get());
    }

    async index(req, res) 
    {       
        let themes = await FileHelper.getDirectories('./wwwroot/themes');

        res.render('settings/general/editor', common.getRouterArgs(req, { 
            title: 'Settings',
            themes: themes
        }));    
    }

    about(req, res) {        
        res.render('settings/about/editor', common.getRouterArgs(req, { 
            title: 'About'
        }));    
    }

    async save(req, res)
    {
        let model = req.body;
        if(!model){
            res.status(400).send('Invalid data').end();
            return;
        }

        let instance = req.settings;
        Object.keys(model).forEach(k => {
            instance[k] = model[k];
        })
        await instance.save();
        res.status(200).send('').end();
    }

}  

module.exports = SettingsRouter;
