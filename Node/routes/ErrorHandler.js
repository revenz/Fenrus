const Globals = require("../Globals");

class ErrorHandler {

    req;
    res;

    constructor(req, res){
        this.req = req;
        this.res = res;
    }

    catchError(codeBlock)
    {
        try
        {
            codeBlock();
        }
        catch(err) 
        {
            console.err(this.timeString(), err);
        }
    }

    async await(codeBlock){
        
        try
        {
            await codeBlock();
        }
        catch(err) 
        {
            console.error(this.timeString(), err);
            this.res.render('error', {
                version: Globals.getVersion(),
                error: err
            });
        }
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

module.exports = ErrorHandler;