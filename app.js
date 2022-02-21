const http = require('http');
const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');

// middleware
const morgan = require('morgan');
const jwtAuthMiddleware = require('./middleware/JwtAuthMiddleware');
const adminMiddleware = require('./middleware/AdminMiddleware');
const cookieParser = require('cookie-parser');

// routers
const routerHome = require('./routes/HomeRouter');
const routerApp = require('./routes/AppRouter');
const routerSettings = require('./routes/SettingsRouter');
const routerGroups = require('./routes/GroupsRouter');
const routerGroup = require('./routes/GroupRouter');
const routerLogin = require('./routes/LoginRouter');
const routerUsers = require('./routes/UsersRouter');
const routerSystem = require('./routes/SystemRouter');

const AppHelper = require('./helpers/appHelper');
const UserManager = require('./helpers/UserManager');
const System = require('./models/System');

// load static configs
AppHelper.getInstance().load();
System.getInstance().load();
UserManager.getInstance().load();


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

app.use('/', routerHome);
app.use('/apps', routerApp);
app.use('/settings', routerSettings);
app.use('/groups', routerGroups);
app.use('/group', routerGroup);

// below are admin only routes, so use the Admin middlweare
app.use(adminMiddleware);
app.use('/users', routerUsers);
app.use('/system', routerSystem);