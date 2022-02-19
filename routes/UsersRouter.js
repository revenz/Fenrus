const express = require('express');
const Utils = require('../helpers/utils');
const UserManager = require('../helpers/UserManager');

const router = express.Router();

router.use(function (req, res, next) {
    if(req.user.IsAdmin === false){
        res.status(401).redirect('/');
        return;
    }
    next();
});

router.get('/', (req, res) => {
    let userManager = UserManager.getInstance();    

    res.render('users', 
    { 
        title: 'Users',        
        user: req.user,
        users: userManager.listUsers(),
        settings: req.settings,
        Utils: new Utils()
    });
});

router.delete('/:uid', async (req, res) => {    
    let userManager = UserManager.getInstance();  
    await userManager.deleteUser(req.params.uid);
    res.status(200).send('').end();
});

router.get('/:uid/set-admin/:isAdmin', async (req, res) => { 
    let isAdmin = req.params.isAdmin === 'true';
    
    let userManager = UserManager.getInstance();  
    let user = userManager.getUserByUid(req.params.uid);
    if(!user){
        res.status(404).send('User not found').end();
        return;
    }
    user.IsAdmin = isAdmin;
    await userManager.save();
    res.status(200).send('').end();
});

module.exports = router;