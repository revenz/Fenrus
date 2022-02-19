const e = require('express');
const express = require('express');
const fs = require('fs');
const jwt = require('jsonwebtoken');
const UserManager = require('../helpers/UserManager');

const router = express.Router();
router.get('/', (req, res) => {
    res.render('login', 
    { 
        title: 'Login'
    });
});

router.post('/', async (req, res) => {
    const username = req.body.username;
    const password = req.body.password;
    const register = req.body.register;
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
 
    // The jwt.sign method are used
    // to create token
    const token = jwt.sign(
        user,
        'secret--todo---change-this'
    );
        
    res.cookie("jwt_auth", token, {
        secure: true,//process.env.NODE_ENV !== "development",
        httpOnly: true,
        maxAge: 31 * 24 * 60 * 60 // seconds, 31 days
    });

    // Pass the data or token in response
    res.json({
        success: true,
        token: token
    });
});
  

module.exports = router;