const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');
const path = require('path');
const Globals = require('./Globals');
const http = require('http');
const https = require("https");

// middleware
const morgan = require('morgan');
const themeMiddleware = require('./middleware/ThemeMiddleware');
const fileBlockerMiddleware = require('./middleware/FileBlockerMiddleware');
const cookieParser = require('cookie-parser');

// routers
const HomeRouter = require('./routes/HomeRouter');
const routerApp = require('./routes/AppRouter');
const routerSettings = require('./routes/SettingsRouter');
const routerTheme = require('./routes/ThemeRouter');
const Four01Router = require('./routes/Four01Router');

const AppHelper = require('./helpers/appHelper');
const UserManager = require('./helpers/UserManager');
const System = require('./models/System');
const InitialConfigRouter = require('./routes/InitialConfigRouter');
const ProxyRouter = require('./routes/ProxyRouter');
const fsExists = require('fs.promises.exists');

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
for(let dir of ['./wwwroot/images/icons', './wwwroot/images/backgrounds', './data/configs', './data/temp'])
{
    if(fs.existsSync(dir) == false)
        fs.mkdirSync(dir, {recursive: true});
}

// express app
const app = express();

// register view engine
app.set('view engine', 'ejs');

if(fs.existsSync('./data/certificate.crt') && fs.existsSync('./data/privatekey.key'))
{
    // setup https
    console.log('#### SETTING UP HTTPS');
    var privateKey  = fs.readFileSync('./data/privatekey.key', 'utf8');
    var certificate = fs.readFileSync('./data/certificate.crt', 'utf8');
    var credentials = {key: privateKey, cert: certificate};
    https.createServer(app).listen(3001);

    var httpServer = http.createServer(app);
    var httpsServer = https.createServer(credentials, app);

    httpServer.listen(3000);
    httpsServer.listen(4000);
}
else 
{
    console.log('#### SETTING UP HTTP');
    app.listen(3000);
}

// Calling the express.json() method for parsing
app.use(bodyParser.json({ limit: '50mb' }));
app.use(bodyParser.urlencoded({ extended: true, limit: '50mb' }));
app.use(cookieParser());


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
app.use(morgan('dev'));

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

app.use('/', (async (req, res, next) => {
    console.log('checking is configured');
    var system = System.getInstance();
    if(system.getIsConfigured() === false)
    {
        res.redirect('/initial-config');
        return;
    }
            
    // look for cached dashboard here
    if(!req.isGuest || system.AllowGuest)
    {
        let filename = req.isGuest ? 'guest-' + system.Revision : req.settings.uid + '-' + req.settings.Revision;
        filename = path.join(__dirname, 'data/temp/' + filename);
        console.log('cached file: ' + filename);
        if(await fsExists(filename)){
            console.log('cached file exists: ' + filename);
            res.setHeader('ETag', req.isGuest ? 'guest-' + system.Revision : req.settings.uid + '-' + req.settings.Revision);
            return res.sendFile(filename);
        }
    }
    

    next();
}));

app.use('/401', new Four01Router().get());

app.use('/proxy', new ProxyRouter().get());

function configureRoutes(app, authStrategy)
{
    app.system = system;
    authStrategy.init(app);

    // anything past this point will now need to be authenticated against the JWT middleware
    //app.use(jwtAuthMiddleware);
    if(authStrategy.authMiddleware)
        app.use(authStrategy.authMiddleware);

    app.use(themeMiddleware);

    app.use('/', new HomeRouter(app).get());

    app.use('/apps', routerApp);

    app.use('/settings', routerSettings);
    app.use('/theme-settings', routerTheme);

    if(authStrategy.errorHandler)
        app.use(authStrategy.errorHandler);    
}

