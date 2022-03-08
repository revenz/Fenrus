
let Utils = require('../helpers/utils');
let AppHelper = require('../helpers/appHelper');
let Globals = require("../Globals");
const Theme = require('../models/Theme');
const HomeRouter = require('./HomeRouter');

class Common
{
    getRouterArgs(req, customArgs)
    {
        console.log('customArgs.dashboard?.Theme', customArgs.dashboard?.Theme);
        if(customArgs.dashboard?.Theme) {
            req.theme = Theme.getTheme(customArgs.dashboard?.Theme);
        }
        let themeVariables = {};
        if(req.theme?.loadScript) {
            let instance = req.theme.loadScript();
            if(instance?.getVariables){
                themeVariables = instance.getVariables(req.settings.ThemeSettings ? req.settings.ThemeSettings[req.theme.Name] : null);
            }
        }
        let themeSettings = {};
        if(req.settings?.ThemeSettings && req.settings.ThemeSettings[req.theme.Name]){
            themeSettings = req.settings.ThemeSettings[req.theme.Name];
            // incase a new theme setting has been added
            for(let setting of req.theme.Settings)
            {
                if(themeSettings[setting.Name] === undefined)
                    themeSettings[setting.Name] = setting.Default;
            }
        }
        else if(req.theme?.Settings?.length){
            // need to get default settings
            for(let setting of req.theme.Settings){
                if(setting.Default !== null && setting.Default !== undefined)
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
            isHome:req.originalUrl === '/' || !req.originalUrl || req.originalUrl.startsWith('/dashboard'),
            pageUrl: req.originalUrl,
            isGuest: req.isGuest,
            isAdmin: req.user?.IsAdmin === true,
            theme: req.theme,
            dashboardTheme: customArgs.dashboard?.Theme,
            themeVariables: {},
            user: req.user, 
            settings: req.settings,
            themeVariables: themeVariables,
            themeSettings: themeSettings,
            Utils: new Utils(),
            version: Globals.getVersion(),
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