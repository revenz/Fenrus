const express = require('express');
const FileHelper = require('../helpers/FileHelper');
const common = require('./Common');
const routerDashboard = require('./DashboardRouter');
const GroupsRouter = require('./GroupsRouter');
const router = express.Router();

// router.get('/', async (req, res) => {

//     let themes = await FileHelper.getDirectories('./wwwroot/themes');

//     res.render('settings', common.getRouterArgs(req, { 
//         title: 'Settings',
//         themes: themes
//     }));    
// });
  
router.get('/', async (req, res) => {

    let themes = await FileHelper.getDirectories('./wwwroot/themes');

    res.render('settings/general', common.getRouterArgs(req, { 
        title: 'Settings',
        themes: themes
    }));    
});
  

router.post('/', async (req, res) => {

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
});

router.use('/dashboards', routerDashboard);
router.use('/groups', new GroupsRouter(false).get());
  

module.exports = router;