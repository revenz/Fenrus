const jwt = require('jsonwebtoken');
const Settings = require('../models/Settings');
const System = require('../models/System');
const crypto = require('crypto');
const routerLogin = require('../routes/LoginRouter');
const UserManager = require('../helpers/UserManager');
const cookieParser = require('cookie-parser');
const Globals = require('../Globals');

class LocalStrategy 
{
    JwtSecret;

    async saveInitialConfig(settings){
        let adminUser = settings.Username;
        let adminPassword = settings.Password;
        let userManager = UserManager.getInstance();
        await userManager.register(adminUser, adminPassword, true);
        return true;
    }

    init(app) 
    {
        this.JwtSecret = app.system.getProperty('JwtSecret');
        if(!this.JwtSecret)
        {
            this.JwtSecret = crypto.randomBytes(256).toString('base64');
            app.system.AllowRegister = true;
            app.system.setProperty('JwtSecret', this.JwtSecret);
        }

        // add cookieparser so the JWT cookie can be easily read
        app.use(cookieParser());
                
        app.get('/login', (req, res) => {
            
            var system = System.getInstance();
            res.render('login', 
            { 
                title: 'Login',
                version: Globals.Version,
                allowRegister: system.AllowRegister,
                allowGuest: system.AllowGuest,
            });
        });

        app.post('/login', async (req, res) => {
            const username = req.body.username;
            const password = req.body.password;
            const register = req.body.register;

            var system = System.getInstance();
            if(!system.AllowRegister && register)
                register = false; // dont allow registrations

            if(!username || !password)
            {        
                res.json({
                    success: false,
                    error: 'Invalid username or password'
                });
                return;
            }

            
            let manager = UserManager.getInstance();
            let user;
            if(register)
                user = await manager.register(username, password);
            else
                user = await manager.validate(username, password);
            
            console.log('user', user);
            if(!user || typeof(user) === 'string'){
                console.log('user', user, register);
                res.json({
                    success: false,
                    error: user || (register ? 'Failed to register' : 'Invalid username or password')
                });
                return;
            }

            if(register === false)
            {
                // clear their cached settings if loaded
                Settings.clearUser(user.Uid);
            }

            // The jwt.sign method are used
            // to create token
            const token = jwt.sign(
                JSON.stringify(user),
                this.JwtSecret
            );

            let maxAge = 31 * 24 * 60 * 60 * 1000 // milliseconds, 31 days
            
            res.cookie("jwt_auth", token, {
                secure: false,
                httpOnly: true,
                maxAge: maxAge 
            });

            // Pass the data or token in response
            res.json({
                success: true,
                token: token
            });
        });
        
    }

    authMiddleware = async (req, res, next) => {
        var token = req.cookies?.jwt_auth;
        let system = System.getInstance();
        if (!token) {
            if(!system.AllowGuest)
                res.status(401).redirect('/login?error=401');
            else
            {
                req.isGuest = true;
                req.settings = await Settings.getForGuest();
                next();
            }
            return;
        } 
       

        try
        {
            const decode = jwt.verify(token, this.JwtSecret);
            if(typeof(decode) === 'string')
                decode = JSON.parse(decode);

            if(/^[a-zA-Z0-9\-]+$/.test(decode.Uid) === false)
                throw 'Invalid UID';

            req.user = decode;
            req.settings = await Settings.getForUser(req.user.Uid);
        }
        catch(err)
        {
            if(!system.AllowGuest)
            {
                res.status(401).redirect('/login?error=401');
                return;
            }

            req.isGuest = true;
            req.settings = await Settings.getForGuest();
        }
        next();
    }
}

module.exports = LocalStrategy;