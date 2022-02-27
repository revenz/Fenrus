const http = require('http');
const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');
const Globals = require('./Globals');

// middleware
const morgan = require('morgan');
const jwtAuthMiddleware = require('./middleware/JwtAuthMiddleware');
const adminMiddleware = require('./middleware/AdminMiddleware');
const themeMiddleware = require('./middleware/ThemeMiddleware');
const fileBlockerMiddleware = require('./middleware/FileBlockerMiddleware');
const cookieParser = require('cookie-parser');

// routers
const routerHome = require('./routes/HomeRouter');
const routerApp = require('./routes/AppRouter');
const routerSettings = require('./routes/SettingsRouter');
const GroupsRouter = require('./routes/GroupsRouter');
const GroupRouter = require('./routes/GroupRouter');
const routerLogin = require('./routes/LoginRouter');
const routerUsers = require('./routes/UsersRouter');
const routerSystem = require('./routes/SystemRouter');
const routerTheme = require('./routes/ThemeRouter');
const SearchEngineRouter = require('./routes/SearchEngineRouter');

const AppHelper = require('./helpers/appHelper');
const UserManager = require('./helpers/UserManager');
const System = require('./models/System');

// load static configs
AppHelper.getInstance().load();
System.getInstance().load();
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

// Import jwt for API's endpoints authentication
const jwt = require('jsonwebtoken');

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

// add cookieparser so the JWT cookie can be easily read
app.use(cookieParser());

// login before the JWT middleware to expose it to the public
app.use('/login', routerLogin);

// anything past this point will now need to be authenticated against the JWT middleware
app.use(jwtAuthMiddleware);
app.use('/apps', routerApp);

app.use(themeMiddleware);
app.use('/', routerHome);
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