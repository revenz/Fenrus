const http = require('http');
const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');

// middleware
const morgan = require('morgan');
const jwtAuthMiddleware = require('./middleware/JwtAuthMiddleware');
const cookieParser = require('cookie-parser');

// routers
const routerHome = require('./routes/HomeRouter');
const routerApp = require('./routes/AppRouter');
const routerSettings = require('./routes/SettingsRouter');
const routerGroups = require('./routes/GroupsRouter');
const routerGroup = require('./routes/GroupRouter');
const routerLogin = require('./routes/LoginRouter');

const AppHelper = require('./helpers/appHelper');
const UserManager = require('./helpers/UserManager');
const Settings = require('./models/settings')

let appHelper = AppHelper.getInstance();
appHelper.load();

let userManager = UserManager.getInstance();
userManager.load();

if(fs.existsSync('./wwwroot/images/icons') == false){
    fs.mkdirSync('./wwwroot/images/icons');
}
if(fs.existsSync('./wwwroot/images/backgrounds') == false){
    fs.mkdirSync('./wwwroot/images/backgrounds');
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


app.use(function (req, res, next) {
    if (req.url.match(/(css|js|img|images|background|font)/)) {
        res.setHeader('Cache-Control', 'public, max-age=3600'); // cache header
    }
    next();
});

app.use(express.static(__dirname + '/wwwroot'));

app.use(morgan('dev'));

app.use(cookieParser());

app.use('/login', routerLogin);

app.use(jwtAuthMiddleware);

app.use('/', routerHome);
app.use('/apps', routerApp);
app.use('/settings', routerSettings);
app.use('/groups', routerGroups);
app.use('/group', routerGroup);