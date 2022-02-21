const jwt = require('jsonwebtoken');
const Settings = require('../models/settings');
const GlobalSettings = require('../models/globalsettings');

module.exports = async (req, res, next) => {
    var token = req.cookies?.jwt_auth;
    if (!token) {
        res.status(401).redirect('/login?error=401');
        return;
    } 
       

    try{
        let decode = jwt.verify(token, (await GlobalSettings.get()).JWTSecret);
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