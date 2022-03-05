
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');
let Globals = require("../Globals");

class Common
{
    getRouterArgs(req, customArgs)
    {

        let themeVariables = {};
        if(req.theme?.loadScript) {
            let instance = req.theme.loadScript();
            if(instance?.getVariables){
                themeVariables = instance.getVariables(req.settings.ThemeSettings ? req.settings.ThemeSettings[req.theme.Name] : null);
            }
        }
        let themeSettings = {};
        if(req.settings.ThemeSettings && req.settings.ThemeSettings[req.theme.Name])
            themeSettings = req.settings.ThemeSettings[req.theme.Name];
        else if(req.theme.Settings?.length){
            // need to get default settings
            console.log(';theme.Settings', req.theme.Settings);
            for(let setting of req.theme.Settings){
                if(setting.Default)
                    themeSettings[setting.Name] = setting.Default;
                else if(setting.Type === 'Integer')
                    themeSettings[setting.Name] = 0;
                else if(setting.Type === 'Boolean')
                    themeSettings[setting.Name] = false;
                else if(setting.Type === 'String')
                    themeSettings[setting.Name] = '';
            }
        }

        let args = {         
            isHome:req.originalUrl === '/' || !req.originalUrl,
            pageUrl: req.originalUrl,
            isGuest: req.isGuest,
            isAdmin: req.user?.IsAdmin === true,
            theme: req.theme,
            themeVariables: {},
            user: req.user, 
            settings: req.settings,
            themeVariables: themeVariables,
            themeSettings: themeSettings,
            Utils: new Utils(),
            version: Globals.Version,
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