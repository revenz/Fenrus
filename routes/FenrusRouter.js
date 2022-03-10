const req = require('express/lib/request');
const Globals = require('../Globals');

class FenrusRouter {

    constructor() {
        if (this.constructor == FenrusRouter)
            throw new Error("Abstract classes can't be instantiated.");
    }

    async safe(funcName, req, res)
    {        
        try
        {
            this[funcName](req, res);
        }
        catch(err) 
        {
            console.error(this.timeString(), err);
            this.renderError(res, err);
        }
    }
    
    async safeAsync(funcName, req, res)
    {        
        try
        {
            await this[funcName](req, res);
        }
        catch(err) 
        {
            console.error(this.timeString(), err);
            this.renderError(res, err);
        }
    }

    renderError(res, err) 
    {
        res.render('error', {
            version: Globals.getVersion(),
            error: err
        });
    }
    
    handleError(res, err)
    {
        console.error(this.timeString(), err);
        this.renderError(res, err);
    }
    
    timeString() {
        let date = new Date();
        let hour = date.getHours();
        let min = date.getMinutes();
        let sec = date.getSeconds();
        let ms = date.getMilliseconds();
        return String(hour).padStart(2, '0')  + ':' +
               String(min).padStart(2, '0')  + ':' +
               String(sec).padStart(2, '0')  + '.' +
               String(ms).padStart(3, '0');
    }  
}
  
module.exports = FenrusRouter;