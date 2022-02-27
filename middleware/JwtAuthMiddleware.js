const jwt = require('jsonwebtoken');
const Settings = require('../models/Settings');
const System = require('../models/System');

module.exports = async (req, res, next) => {
    var token = req.cookies?.jwt_auth;
    let system = System.getInstance();
    console.log('system.AllowGuest', system.AllowGuest);
    if (!token) {
        if(!system.AllowGuest)
            res.status(401).redirect('/login?error=401');
        else
        {
            req.isGuest = true;
            req.settings = await Settings.getForGuest();
            next();
        }
        return;
    } 
       

    try
    {
        const decode = jwt.verify(token, system.JwtSecret);
        if(typeof(decode) === 'string')
            decode = JSON.parse(decode);

        if(/^[a-zA-Z0-9\-]+$/.test(decode.Uid) === false)
            throw 'Invalid UID';

        req.user = decode;
        req.settings = await Settings.getForUser(req.user.Uid);
    }
    catch(err)
    {
        if(!system.AllowGuest)
        {
            res.status(401).redirect('/login?error=401');
            return;
        }

        req.isGuest = true;
        req.settings = await Settings.getForGuest();
    }
    next();
}