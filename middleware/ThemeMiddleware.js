const Theme = require('../models/Theme');

module.exports = (req, res, next) => {
    if(req.method !== 'GET')
    {
        next();
        return;
    }
    if(/^\/(js|images|fonts)/.test(req.url)){
        next();
        return;
    }

    req.theme = Theme.getTheme(req.settings.Theme);
    next();
};