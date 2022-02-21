const express = require('express');
const jwt = require('jsonwebtoken');
const UserManager = require('../helpers/UserManager');
const Settings = require('../models/Settings');
const System = require('../models/System');

const router = express.Router();
router.get('/', (req, res) => {
    
    var system = System.getInstance();
    res.render('login', 
    { 
        title: 'Login',
        allowRegister: system.AllowRegister
    });
});

router.post('/', async (req, res) => {
    const username = req.body.username;
    const password = req.body.password;
    const register = req.body.register;

    var system = System.getInstance();
    if(!system.AllowRegister && register)
        register = false; // dont allow registrations

    if(!username || !password)
    {        
        res.json({
            success: false,
            error: 'Invalid username or password'
        });
        return;
    }

    let manager = UserManager.getInstance();
    let user;
    if(register)
        user = await manager.register(username, password);
    else
        user = await manager.validate(username, password);

    if(!user || typeof(user) === 'string'){
        console.log('user', user, register);
        res.json({
            success: false,
            error: user || (register ? 'Failed to register' : 'Invalid username or password')
        });
        return;
    }

    if(register === false)
    {
        // clear their cached settings if loaded
        Settings.clearUser(user.Uid);
    }

    // The jwt.sign method are used
    // to create token
    const token = jwt.sign(
        JSON.stringify(user),
        system.JwtSecret
    );

    let maxAge = 31 * 24 * 60 * 60 * 1000 // milliseconds, 31 days
    
    res.cookie("jwt_auth", token, {
        secure: false,
        httpOnly: true,
        maxAge: maxAge 
    });

    // Pass the data or token in response
    res.json({
        success: true,
        token: token
    });
});
  

module.exports = router;