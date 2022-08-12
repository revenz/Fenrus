const UserManager = require('../helpers/UserManager');
const Settings = require('../models/Settings');
const isReachable = require('is-reachable');

class UpTimeService 
{
    checking = false;
    checkedUrls = {};

    constructor(){        
    }

    log(){
        let args = arguments;
        if(typeof(args[0]) === 'string')
            args[0] = 'UpTimeService: '+ args[0];
        else
            args = ['UpTimeService'].concat(args);
        
        console.log(...args);
    }

    async check()
    {
        if(this.checking)
            return;
        this.log('checking apps');
        this.checking = true;
        this.checkedUrls = {};
        try
        {
            let users = this.getUsers();
            for(let user of users)
            {
                this.log('checking user', user);
                await this.checkUser(user);
            }
        }
        catch(err)
        {
            this.log('Check error: ' + err);
        }
        finally
        {
            this.checkedUrls = {};
            this.checking = false;
        }
    }

    getUsers(){
        return UserManager.getInstance().listUsers();
    }

    async checkUser(user){
        let config = await Settings.getForUser(user.Uid);
        if(!config?.Groups?.length)
            return;

        let toCheck = [];
        for(let grp of config.Groups)
        {
            for(let item of grp.Items){
                if(item._Type !== 'DashboardApp')
                    continue;
                console.log('will check: ' +  item.Url);
                toCheck.push({
                    Url: item.Url,
                    Uid: item.Uid
                });
            }
        }
        console.log('Items to check', toCheck);
        let tasks = [];
        for(let item of toCheck){
            this.log('about to check: ' + item.Url); 
            tasks.push(new Promise(async (resolve, reject) =>
            {
                this.log('Checking: ' + item.Url);
                item.Reachable = await isReachable(item.Url, {
                    timeout: 5000
                });
                this.log(item.Url + ': ' + item.Reachable);
                resolve();
            }));
        }
        await Promise.all(tasks);
        this.log('Items to check222', toCheck);
    }
}


module.exports = UpTimeService;
