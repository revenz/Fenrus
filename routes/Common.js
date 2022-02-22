
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');

class Common
{
    getRouterArgs(req, customArgs)
    {
        let args = {            
            theme: req.theme,
            user: req.user, 
            settings: req.settings,
            Utils: new Utils(),
            AppHelper: AppHelper.getInstance()
        }

        if(customArgs){
            Object.keys(customArgs).forEach(x =>{
                args[x] = customArgs[x];
            });
        }

        return args;
    }
}

module.exports = new Common();