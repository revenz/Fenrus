const fs = require('fs');
const Utils = require('../helpers/utils');
const fetch = require('node-fetch');
const crypto = require('crypto');
const User = require('../models/User');
const bcrypt = require('bcrypt');

class UserManagerInstance {

    _File = './data/users.json';
    Users = [];
    saltRounds = 10;

    load() 
    {
        if(fs.existsSync(this._File) == false){
            this.Users = [];
            return;
        }
        
        let json = fs.readFileSync(this._File, {encoding: 'utf8'});
        if(json.charCodeAt(0) === 65279)
            json = json.substring(1);

        this.Users = typeof(json) === 'object' ? json : JSON.parse(json);
    }

    getNumberOfUsers() {
        if(!this.Users)
            return 0;
        return this.Users.length;
    }

    async hashString(input){
        return await bcrypt.hash(input, this.saltRounds);
    }

    hashStringOld(input)
    {
        if(!input || typeof(input) !== 'string')
            return '';
        let hash = crypto.createHash('sha256');
        let data = hash.update(input, 'utf-8');
        let gen_hash = data.digest('hex');
        return gen_hash.toLowerCase();
    }

    getUser(username)
    {        
        let user = this.Users.filter(x => x.Username === username);
        if(!user || user.length === 0)
            return null;

        return user[0];
    }
    
    getUserByUid(uid)
    {        
        let user = this.Users.filter(x => x.Uid === uid);
        if(!user || user.length === 0)
            return null;

        return user[0];
    }

    async validate(username, password)
    {            
        let user = this.getUser(username);
        if(!user)
            return false;
            
        let matches = await bcrypt.compare(password, user.Password);
        if(matches)
            return user;

        // compare SHA256, this code will eventually be removed
        let hash = this.hashStringOld(password);
        let expected = user.Password.toLowerCase();
        if(expected !== hash)
        {
            return false;
        }

        // password saved is a SHA256, update it to bcrypt
        user.Password = await this.hashString(password);
        await this.save();

        return user;
    }

    async register(username, password, isAdmin){
        if(!username)
            return;

        let existing = this.getUser(username)
        if(isAdmin == false && existing)
            return `Existing user with username '${username}'`;
        
        let user = existing || new User();
        user.Username = username;
        user.Password = await this.hashString(password);

        if(!existing) // otherwise we re-registering the user their config will go
            user.Uid = new Utils().newGuid();

        user.IsAdmin = isAdmin === true || this.Users.length === 0;        
        if(!existing)
            this.Users.push(user);

        await this.save();
        return user;
    }

    listUsers(){
        return this.Users.map(x => {
            return {
                Uid: x.Uid,
                Username: x.Username,
                IsAdmin: x.IsAdmin
            }
        });
    }

    toJson() { 
        return JSON.stringify(this.Users, null, 2);
    }

    async save() {

        this.Users.sort((a,b) =>{
            return a.Username.localeCompare(b.Username);
        });

        let json = this.toJson();
        let self = this;

        return new Promise(function (resolve, reject) {
            fs.writeFile(self._File, json, (err, data) => {
                if(err)
                    reject();
                else
                    resolve();
            });
        });
    }

    async deleteUser(uid)
    {
        this.Users = this.Users.filter(x => x.Uid !== uid);
        await this.save();        
        let configFile = `./data/configs/${uid}.json`;
        if(fs.existsSync(configFile)) {
            fs.unlink(configFile, (err => {
                console.warn('Failed to delete user config file: ' + configFile + ' => ' + err);
            }));
        }
    }
}

class UserManager {
    constructor() {
        throw new Error('Use UserManager.getInstance()');
    }

    static getInstance(){
        if(!UserManagerInstance.instance){
            UserManagerInstance.instance = new UserManagerInstance();
        }
        return UserManagerInstance.instance;
    }
}
module.exports = UserManager;