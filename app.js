const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');
const Globals = require('./Globals');
const http = require('http');
const https = require("https");
const cron = require('node-cron');
const UpTimeService = require('./services/UpTimeService');
const JwtVerifier = require('./helpers/JwtVerifier');
const Settings= require('./models/Settings');
const Minifier = require('./helpers/Minifier');

// middleware
const morgan = require('morgan');
const themeMiddleware = require('./middleware/ThemeMiddleware');
const fileBlockerMiddleware = require('./middleware/FileBlockerMiddleware');
const cookieParser = require('cookie-parser');
const cookie = require('cookie');

// routers
const HomeRouter = require('./routes/HomeRouter');
const AppRouter = require('./routes/AppRouter');
const SettingsRouter = require('./routes/SettingsRouter');
const ThemeRouter = require('./routes/ThemeRouter');
const Four01Router = require('./routes/Four01Router');

const AppHelper = require('./helpers/appHelper');
const UserManager = require('./helpers/UserManager');
const System = require('./models/System');
const InitialConfigRouter = require('./routes/InitialConfigRouter');
const ProxyRouter = require('./routes/ProxyRouter');
const req = require('express/lib/request');
const SshService = require('./services/SshService');
const DockerService = require('./services/DockerService');

const consoleLogger = console.log;

let httpPort = process.env.HTTP_PORT || 3000;
let httpsPort = process.env.HTTPS_PORT || 4000;

function timeString() {
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

console.log = (...args) => {
    consoleLogger(timeString(), ...args);
}

// load static configs
AppHelper.getInstance().load();
let system = System.getInstance();
system.load();
UserManager.getInstance().load();

// set the version number
if(fs.existsSync('./buildnum.txt')){            
    let build = fs.readFileSync('./buildnum.txt', { encoding: 'utf-8'});
    if(build){
        Globals.Build = build.trim();        
        console.log('Version: ', Globals.getVersion());
    }
}


// create default directories
for(let dir of ['./wwwroot/images/icons', './wwwroot/images/backgrounds', './data/configs'])
{
    if(fs.existsSync(dir) == false)
        fs.mkdirSync(dir, {recursive: true});
}

new Minifier().minifyJavascript();

// express app
const app = express();

// register view engine
app.set('view engine', 'ejs');

let server;
if(fs.existsSync('./data/certificate.crt') && fs.existsSync('./data/privatekey.key'))
{
    // setup https
    console.log('#### SETTING UP HTTPS');
    var privateKey  = fs.readFileSync('./data/privatekey.key', 'utf8');
    var certificate = fs.readFileSync('./data/certificate.crt', 'utf8');
    var credentials = {key: privateKey, cert: certificate};

    var httpServer = http.createServer(app);
    
    var httpsServer = https.createServer(credentials, app);
    server = httpsServer;

    httpServer.listen(httpPort);
    httpsServer.listen(httpsPort);
    console.log(`listening on HTTP port: ${httpPort}`);
    console.log(`listening on HTTPS port: ${httpsPort}`);

    setInterval(() => {
        httpsServer.getConnections((error, count) => {
            console.log('### Number of HTTPS connections: ' + count);
        });
        httpServer.getConnections((error, count) => {
            console.log('### Number of HTTP connections: ' + count);
        });
    }, 2000);
}
else 
{
    console.log('#### SETTING UP HTTP');
    var httpServer = http.createServer(app);
    server = httpServer;
    httpServer.listen(httpPort);
    console.log(`listening on HTTP port: ${httpPort}`);
    setInterval(() => {
        httpServer.getConnections((error, count) => {
            console.log('### Number of connections: ' + count);
        });
    }, 2000);
}

var io = require('socket.io')(server);

// set cache control for files
app.use(function (req, res, next) {
    if (/version=([\d]+\.){3}[\d]+/.test(req.url) || /\/fonts\//.test(req.url)) {
        res.setHeader('Cache-Control', 'public, max-age=31536000, immutable'); // forever if have version on it
    }
    else if (/favicon\.svg/.test(req.url)) {
        res.setHeader('Cache-Control', 'public, max-age=31536000, immutable'); // unlikely to change, if i do change it, have to change this
    }
    else if (/(\.(woff|eot|ttf|css|js|svg|ico|jp(e)?(g)?|gif|png)$)|img|images|background|font/.test(req.url)) {
        res.setHeader('Cache-Control', 'public, max-age=' + (24 * 60 * 60)); // one day
    }
    next();
});

// this prevents any files from themes files being accessed that shouldn't be accessed
app.use(fileBlockerMiddleware);
// expose wwwroot files as public
app.use(express.static(__dirname + '/wwwroot'));


// morgan logs every request coming into the system 
morgan.token('date', (req, res, tz) => { return timeString(); })
morgan.format('myformat', ':date [:method] [:response-time ms] => :url');
//app.use(morgan('myformat'));

// Calling the express.json() method for parsing
app.use(bodyParser.json({ limit: '50mb' }));
app.use(bodyParser.urlencoded({ extended: true, limit: '50mb' }));
app.use(cookieParser());


if(system.getIsConfigured() === false){
    // not yet configured, we have to add a special /initial-config route
    let callback = (authStrategy) => {
        configureRoutes(app, authStrategy);
    }
    app.use('/initial-config', new InitialConfigRouter(callback).get());
}
else
{    
    let authStrategy =  system.getAuthStrategy();    
    configureRoutes(app, authStrategy);
}

app.use('/', ((req, res, next) => {
    var system = System.getInstance();
    if(system.getIsConfigured() === false)
    {
        res.redirect('/initial-config');
        return;
    }
    next();
}));

app.use('/401', new Four01Router().get());

app.use('/proxy', new ProxyRouter().get());

function configureRoutes(app, authStrategy)
{
    app.system = system;
    authStrategy.init(app);

    if(authStrategy.authMiddleware)
        app.use(authStrategy.authMiddleware);

    app.use(themeMiddleware);

    app.use('/', new HomeRouter(app).get());

    app.use('/apps', new AppRouter().get());

    app.use('/settings', new SettingsRouter().get());
    app.use('/theme-settings', new ThemeRouter().get());


    if(authStrategy.errorHandler)
        app.use(authStrategy.errorHandler);  

    errorHandler();
}

function errorHandler(){
    // Handle errors
    app.use((err, req, res, next) => {
        if (! err) {
            return next();
        }
        console.error(timeString() + ' Error:', err.stack);

        res.status(500).send('500: Internal server error');
    });
    process.on('uncaughtException', function (err) {
        console.error(timeString() + ' Uncaught Exception:', err.message)
        console.error(err.stack)
        process.exit(1);
    });
}

const upTimeService = new UpTimeService();
upTimeService.check();
cron.schedule("*/5 * * * *", () => {
    upTimeService.check();
});


io.on('connection', function(socket) {
    var cookies = cookie.parse(socket.handshake.headers.cookie);  
    let user = new JwtVerifier().verify(cookies.jwt_auth);
    if(!user)
        return;

    const cleanArgs = (args) => {
        if(!args?.length)
            return args;
        // remove quotes surrounding an argument
        for(let i=0;i<args.length;i++)
        {
            if(/^"(.*?)"$/.test(args[i]))
                args[i] = /^"(.*?)"$/.exec(args[i])[1];
        }
        return args;
    }
    let settings = Settings.getForUserSync(user.Uid);

    socket.on('ssh', (args) => {
        args = cleanArgs(args);
        let rows = args[0];
        let cols = args[1];
        args.splice(0, 2);
        if(args.length === 3)
        {
            new SshService(socket).init(args, rows, cols);    
        }
        else
        {
            let app = settings.findAppInstance(args[0]);
            if(!app.SshPassword)
            {
                socket.emit('request-pwd', [app.SshServer, app.SshUsername]);
            }
            else{
                new SshService(socket).init(app, rows, cols);
            }
        }
    });
    
    socket.on('docker', (args) => {
        args = cleanArgs(args);
        let rows = args[0];
        let cols = args[1];
        args.splice(0, 2);
        let app = settings.findAppInstance(args[0]);
        new DockerService(socket, app, system).init(rows, cols);
    });

  });