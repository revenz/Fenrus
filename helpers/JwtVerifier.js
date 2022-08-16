const jwt = require('jsonwebtoken');
const Settings = require('../models/Settings');
const System = require('../models/System');


class JwtVerifier {
    verify(token) {        
        let system = System.getInstance();
        console.log('###### system.JwtSecret', system.Properties.JwtSecret);
        const decode = jwt.verify(token, system.Properties.JwtSecret);
        if(typeof(decode) === 'string')
            decode = JSON.parse(decode);

        if(/^[a-zA-Z0-9\-]+$/.test(decode.Uid) === false)
            throw 'Invalid UID';

        return decode;
    }
}

module.exports = JwtVerifier;
