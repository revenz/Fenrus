const jwt = require('jsonwebtoken');
let Settings = require('../models/settings');

module.exports = async (req, res, next) => {
    var token = req.cookies?.jwt_auth;
    if (!token) {
        res.status(401).redirect('/login?error=401');
        return;
    } 
       

    try{
        const decode = jwt.verify(token, 'secret--todo---change-this');
        if(typeof(decode) === 'string')
            decode = JSON.parse(decode);

        if(/^[a-zA-Z0-9\-]+$/.test(decode.Uid) === false)
            throw 'Invalid UID';

        req.user = decode;
        req.settings = await Settings.getForUser(req.user.Uid);
    }
    catch(err)
    {
        res.status(401).redirect('/login?error=401');
        return;
    }
    next();
}