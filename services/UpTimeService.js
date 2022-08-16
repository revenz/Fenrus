const UserManager = require('../helpers/UserManager');
const Settings = require('../models/Settings');
const isReachable = require('is-reachable');
const fsExists = require('fs.promises.exists');
const fsPromises = require('fs.promises');
const dns = require('dns');
const net = require('net');
const fetch = require("node-fetch");
const {promisify} = require('util');

const dnsLookupP = promisify(dns.lookup);

class UpTimeService 
{
    checking = false;
    checkedUrls = {};

    static UserApps = {};

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

    async getAppsForUser(userUid) {        
        let config = await Settings.getForUser(userUid);
        if(!config?.Groups?.length)
            return [];

        let apps = [];
        for(let grp of config.Groups)
        {
            for(let item of grp.Items){
                if(item._Type !== 'DashboardApp' && item._Type !== 'DashboardLink')
                    continue;
                apps.push({
                    Name: item.Name,
                    Url: item.ApiUrl || item.Url,
                    Uid: item.Uid
                });
            }
        }
        return apps;
    }

    getLastIsUp(userUid, url) {
        let apps = UpTimeService.UserApps[userUid];
        if(!apps)
            return 2; // unknown
        let result = apps[url];
        if(result === true || result === false)
            return result;
        return 2; // unknown
    }

    async checkUser(user){
        let config = await Settings.getForUser(user.Uid);
        if(!config?.Groups?.length)
            return;

        let toCheck = await this.getAppsForUser(user.Uid);
        let tasks = [];
        let date = Date.now();
        if(!UpTimeService.UserApps[user.Uid])
            UpTimeService.UserApps[user.Uid] = {};
        for(let item of toCheck){
            tasks.push(new Promise(async (resolve, reject) =>
            {
                try
                {
                    let isUp = this.isReachable(item.Url);
                    
                    UpTimeService.UserApps[user.Uid][item.Url] = isUp;
                    await this.recordUpTime(user.Uid, item.Uid, date, isUp)
                }catch(err) {
                    this.log('error in checking uptime: ' + err);
                }
                resolve();
            }));
        }
        await Promise.all(tasks);
    }

    async recordUpTime(userUid, appUid, date, isUp){
        let dir = `./data/uptime/${userUid}`;
        if(await fsExists(dir) == false)
            await fsPromises.mkdir(dir, {recursive: true});
            
        let file = `${dir}/${appUid}.json`;
        let uptimes = [];
        if(await fsExists(file))
        {
            try
            {
                uptimes = JSON.parse(await fsPromises.readFile(file));
                if(Array.isArray(uptimes) !== true)
                    uptimes = [];
            }
            catch(err) {
                uptimes = [];
            }
        }
        uptimes.unshift({
            date: date,
            up: isUp
        });
        const maxItems = (60 / 5) * 24 * 7; // 60 / 5 == number of 5mins in an hour, * 24 == in a day, * 7 in a week
        if(uptimes.length > maxItems)
            uptimes.length = maxItems;

        let json = JSON.stringify(uptimes, null, '\t');
        await fsPromises.writeFile(file, json);
    }

    async isReachable(url)
    {
        try{
            const req = await fetch(url);
            let status = req.status; // 200
            return true;
        }
        catch(err) {
            return false;
        }
    }

    async isReachableOld(url)
    {
        let hostname = url.replace(/http(s)?:\/\//i, '');
        let slashIndex = hostname.indexOf('/');
        if(hostname.indexOf('/') > 0)
            hostname = hostname.substring(0, slashIndex);
        let port = url.toLowerCase().startsWith('https') ? 443 : 80;
        let portIndex = hostname.indexOf(':');
        if(portIndex > 0)
        {
            let nPort = parseInt(hostname.substring(portIndex + 1), 10);
            if(isNaN(nPort) === false)
                port = nPort;
            hostname = hostname.substring(0, portIndex);
        }

        let address = await this.getAddress(hostname);
        console.log('######## address: ', address);
        let reachable = await this.isPortReachable(address, port);
        console.log('######## reachable: ', address, port, reachable);
        return reachable;
    }

    async getAddress(hostname)
    {
        return net.isIP(hostname) ? hostname : (await dnsLookupP(hostname)).address;
    } 

    async isPortReachable(host, port, timeout){
        const socket = new net.Socket();


        let result = true;
		const onError = (err) => {
            console.log('### onError', err);
			socket.destroy();
			result = false;
		};

		socket.setTimeout(timeout || 5000);
		socket.once('error', onError);
		socket.once('timeout', onError);
        try{
            await socket.connect(port, host);
            socket.end();
        }catch(err){
            console.log('### errrrr', err);
            result = false;
        }
        return result;
    }
}


module.exports = UpTimeService;
