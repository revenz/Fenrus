const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');
const Globals = require('./Globals');

// middleware
const morgan = require('morgan');
const adminMiddleware = require('./middleware/AdminMiddleware');
const themeMiddleware = require('./middleware/ThemeMiddleware');
const fileBlockerMiddleware = require('./middleware/FileBlockerMiddleware');

// routers
const routerHome = require('./routes/HomeRouter');
const routerApp = require('./routes/AppRouter');
const routerSettings = require('./routes/SettingsRouter');
const GroupsRouter = require('./routes/GroupsRouter');
const GroupRouter = require('./routes/GroupRouter');
const routerUsers = require('./routes/UsersRouter');
const routerSystem = require('./routes/SystemRouter');
const routerTheme = require('./routes/ThemeRouter');
const SearchEngineRouter = require('./routes/SearchEngineRouter');
const Four01Router = require('./routes/Four01Router');

const AppHelper = require('./helpers/appHelper');
const UserManager = require('./helpers/UserManager');
const System = require('./models/System');
const InitialConfigRouter = require('./routes/InitialConfigRouter');

// load static configs
AppHelper.getInstance().load();
let system = System.getInstance();
system.load();
UserManager.getInstance().load();

// set the version number
if(fs.existsSync('./buildnum.txt')){            
    let build = fs.readFileSync('./buildnum.txt', { encoding: 'utf-8'});
    if(build){
        build = build.trim();
        Globals.Version = `${Globals.MajorVersion}.${Globals.MinorVersion}.${Globals.Revision}.${build}`;
        console.log('Version: ', Globals.Version);
    }
}


// create default directories
if(fs.existsSync('./wwwroot/images/icons') == false){
    fs.mkdirSync('./wwwroot/images/icons', {recursive: true});
}
if(fs.existsSync('./wwwroot/images/backgrounds') == false){
    fs.mkdirSync('./wwwroot/images/backgrounds', {recursive: true});
}
if(fs.existsSync('./data/configs') == false){
    fs.mkdirSync('./data/configs', {recursive: true});
}
// express app
const app = express();



// register view engine
app.set('view engine', 'ejs');

// listen on port 
app.listen(3000);

// Calling the express.json() method for parsing
app.use(bodyParser.json({ limit: '50mb' }));
app.use(bodyParser.urlencoded({ extended: true, limit: '50mb' }));


// set cache control for files
app.use(function (req, res, next) {
    if (req.url.match(/(css|js|img|images|background|font)/)) {
        res.setHeader('Cache-Control', 'public, max-age=3600'); // cache header
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

app.use('/', ((req, res, next) => {
    console.log('checking is configured');
    var system = System.getInstance();
    if(system.getIsConfigured() === false)
    {
        res.redirect('/initial-config');
        return;
    }
    next();
}));

app.use('/401', new Four01Router().get());


function configureRoutes(app, authStrategy)
{
    app.system = system;
    authStrategy.init(app);

    // anything past this point will now need to be authenticated against the JWT middleware
    //app.use(jwtAuthMiddleware);
    if(authStrategy.authMiddleware)
        app.use(authStrategy.authMiddleware);

    app.use(themeMiddleware);

    app.use('/', routerHome);

    app.use('/apps', routerApp);

    app.use('/settings', routerSettings);
    app.use('/groups', new GroupsRouter(false).get());
    app.use('/group', new GroupRouter(false).get());
    app.use('/theme-settings', routerTheme);
    app.use('/search-engines', new SearchEngineRouter(false).get());

    // below are admin only routes, so use the Admin middlweare
    app.use(adminMiddleware);


    app.use('/users', routerUsers);
    app.use('/system/guest', new GroupsRouter(true).get());
    app.use('/system/guest/group', new GroupRouter(true).get());
    app.use('/system', routerSystem);
    app.use('/system/search-engines', new SearchEngineRouter(true).get());

    if(authStrategy.errorHandler)
        app.use(authStrategy.errorHandler);    
}

