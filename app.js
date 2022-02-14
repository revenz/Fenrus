const http = require('http');
const express = require('express');
const bodyParser = require('body-parser');
const fs = require('fs');

const morgan = require('morgan');
const routerHome = require('./routes/HomeRouter');
const routerApp = require('./routes/AppRouter');
const routerSettings = require('./routes/SettingsRouter');
const routerGroups = require('./routes/GroupsRouter');
const routerGroup = require('./routes/GroupRouter');

const AppHelper = require('./helpers/appHelper');
const Settings = require('./models/settings')

let appHelper = AppHelper.getInstance();
appHelper.load();

let settings = Settings.getInstance();
settings.load();

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

// listen on port 
app.listen(3000);

app.use(express.static(__dirname + '/wwwroot'));

// Calling the express.json() method for parsing
app.use(bodyParser.json({ limit: '50mb' }));
app.use(bodyParser.urlencoded({ extended: true, limit: '50mb' }));

app.use(morgan('dev'));

app.use('/', routerHome);
app.use('/apps', routerApp);
app.use('/settings', routerSettings);
app.use('/groups', routerGroups);
app.use('/group', routerGroup);