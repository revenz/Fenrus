const express = require('express');
const UserManager = require('../helpers/UserManager');
const common = require('./Common');

const router = express.Router();

router.get('/', (req, res) => {
    let userManager = UserManager.getInstance();    

    res.render('users', common.getRouterArgs(req, { 
        title: 'Users',        
        users: userManager.listUsers()
    }));
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