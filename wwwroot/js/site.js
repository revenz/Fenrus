var abortController = new AbortController();
var signal = abortController.signal;
Scripts = [];

LiveAppsTimers = [];

window.onpopstate = function(event) {
    console.log('document.location', document.location, event);
    let path = document.location.pathname;
    path = path.substring(path.lastIndexOf('/') + 1);
    fetchDashboard(path, true);
}

function abortRequests() {
    abortController.abort();
    for(let app of LiveAppsTimers){
        clearInterval(app);
    }
    LiveAppsTimers = [];
}

function getFromLocalStorage(instaceUid){
    try{
        let size = getItemSize(instanceUid);
        let item = JSON.parse(localStorage.getItem(instaceUid + '-' + size));
        if(!item?.date)
            return;
        if(item.date < (new Date().getTime() - 60000))
            return { html: item.html, old: true}; // older than a minute reject it
        return { html: item.html, old: false};
    }
    catch(err) { return; }
}

function getItemSize(instanceUid)
{
    let ele = document.getElementById(instanceUid);
    if(!ele)
        return '';
    if(ele.classList.contains('x-large'))
        return 'x-large'
    if(ele.classList.contains('large'))
        return 'large'
    if(ele.classList.contains('medium'))
        return 'medium'
    if(ele.classList.contains('small'))
        return 'small';
    return '';
}

function setInLocalStorage(instanceUid, html)
{
    let size = getItemSize(instanceUid);
    localStorage.setItem(instanceUid + '-' + size, JSON.stringify({
        date: new Date().getTime(),
        html: html
    }));
}

function getDashboardInstanceUid()
{
    return document.getElementById('dashboard-instance')?.value;
}

function LiveApp(name, instanceUid, interval) 
{    
    if(typeof(name) !== 'string')
        throw 'Name is not a string';

    let dashboardInstanceUid = getDashboardInstanceUid();
    let args =  {
        name: name,
        instanceUid: instanceUid,
        interval: interval,
        dashboardInstanceUid: dashboardInstanceUid
    };
    LiveAppActual(args);
}


function LiveAppActual(args, subsequent)
{
    if(typeof(args.name) !== 'string')
        throw 'name is not a string';
    let name = args.name;
    let instanceUid = args.instanceUid;
    let interval = args.interval;
    let dashboardInstanceUid = args.dashboardInstanceUid
    if (!interval) interval = 3000;


    if(!subsequent){
        let saved = getFromLocalStorage(instanceUid);
        if(saved) {            
            appSetStatus(args, saved.html, !!subsequent);
            if(saved.old === false)
                return; // dont need to refresh
        }
    }

    var newTimeout = () => {
        let timer = setTimeout(() => {
            let currentDashboard = getDashboardInstanceUid();
            if(currentDashboard != dashboardInstanceUid)
                return; // if they changed dashboards

            LiveAppActual(args, true);
            LiveAppsTimers = LiveAppsTimers.filter(x => x != timer);
        }, interval + Math.random() + 5);
        LiveAppsTimers.push(timer);
    }

    if(subsequent && document.hasFocus() === false){
        newTimeout();
        return; // prevent request if the page doesnt have focus
    }

    fetch(`/apps/${encodeURIComponent(name)}/${encodeURIComponent(instanceUid)}/status?name=` + encodeURIComponent(name), {        
        signal: signal
    })
    .then(res => {
        let currentDashboard = getDashboardInstanceUid();
        if(currentDashboard != dashboardInstanceUid)
            return; // if they changed dashboards

        if(!res.ok)
            throw res;
        if(res.status === 302){
            // redirect, to login            
            window.location.href = '/login';
        }
        return res.text();
    })
    .then(html => {
        if(html == undefined)
            return;
        appSetStatus(args, html, !!subsequent);
    }).catch(error => {
        let currentDashboard = getDashboardInstanceUid();
        if(currentDashboard != dashboardInstanceUid)
            return; // if they changed dashboards
        console.log(name + ' error: ', error);        
        newTimeout();
    });
}

function appSetStatus(args, content, subsequent){
    let instanceUid = args.instanceUid;
    let interval = args.interval;
    let eleItem = document.getElementById(instanceUid);
    let ele = eleItem.querySelector('.status');
    if (ele && content) {
        setInLocalStorage(instanceUid, content);        
        if(/^:carousel:/.test(content)){
            content = content.substring(10);
            let index = content.indexOf(':');
            let carouselId = content.substring(0, index);
            content = content.substring(index + 1);
            if(!carouselTimers[carouselId])
                carouselTimer(carouselId);
            setItemClass(eleItem, 'carousel');
        }    
        else if(/^:bar-info:/.test(content)){
            content = content.substring(10);
            setItemClass(eleItem, 'bar-info');
        }
        else if(/^data:/.test(content)){
            content = `<img class="app-chart" src="${content}" />`;
            setItemClass(eleItem, 'chart');
        }
        else {
            setItemClass(eleItem, 'db-basic');
        }
        ele.innerHTML = content;
    }
    if((subsequent && interval === -1) === false)
        setTimeout(() => LiveAppActual(args, true), interval + Math.random() + 5);
}

function setItemClass(item, className) {
    item.className = item.className.replace(/(carousel|chart|db-basic)/g, '') + ' ' + className;
}

function htmlEncode(text) {
    var node = document.createTextNode(text);
    var p = document.createElement('p');
    p.appendChild(node);
    return p.innerHTML;
}

function changeTheme(event) {
    let theme = event?.target?.value;
    if (!theme)
        return;
    document.getElementById('theme-style').setAttribute('href', `/themes/${theme}/theme.min.css`);
}
function newGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

var carouselTimers = {};


function carouselItem(e, id, itemIndex){
    if(e)
        e.preventDefault();
    let carousel = document.getElementById(id);    
    for(let item of carousel.querySelectorAll('.item')){
        let visible = item.classList.contains('visible');
        if(item.id === id + '-' + itemIndex) {
            item.classList.remove('hidden');
            if(visible === false)
                item.classList.add('visible');
        }
        else if(visible)
        {
            item.classList.remove('visible');
            item.classList.add('hidden');
        }
        else
        {
            item.classList.remove('hidden');
        }
    }

    let controls = carousel.querySelectorAll('.controls a');
    for(let i=0;i<controls.length;i++){
        controls[i].classList.remove('selected');
        if(i === itemIndex)
            controls[i].classList.add('selected');
    }

    carouselTimer(id);
}

function carouselTimer(id) {
    let existing = carouselTimers[id];
    if(existing)
        clearInterval(existing);
    let timerId = setInterval(() => {
        let carousel = document.getElementById(id);
        if(!carousel)
            return; // happens if the carousel was replaced with newer html
        
        let visible = carousel.querySelector('.item.visible');
        let index = parseInt(visible.id.substring(visible.id.indexOf('-') + 1), 10);
        ++index;
        let hasNext = document.getElementById(id + '-' + index);
        carouselItem(null, id, hasNext ? index : 0);
    }, 5000);
    carouselTimers[id] = timerId;
}



function getCookie(cname) {
    let name = cname + "=";
    let decodedCookie = decodeURIComponent(document.cookie);
    let ca = decodedCookie.split(';');
    for(let i = 0; i <ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
        c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
        return c.substring(name.length, c.length);
        }
    }
    return "";
}

function fetchDashboard(uid,  backwards) {
    let fetchUrl = '/dashboard/' + (uid || 'Default') + '?inline=true';
    fetch(fetchUrl).then(res => {
        if(!res.ok)
            return;
        this.abortRequests();
        return res.text();        
    }).then(html => {
        html = html.replace(/x-text=\"[^"]+\"/g, '');
        if(!backwards)
            history.pushState({uid:uid}, 'Fenrus', '/dashboard/' + uid);

        abortController = new AbortController();
        signal = abortController.signal;
        let eleDashboard = document.querySelector('.dashboard');
        eleDashboard.innerHTML = html;
        if(typeof(themeInstance) !== 'undefined')
            themeInstance.init();

        let dashboardBackground = document.getElementById('hdn-dashboard-background')?.value || null;
        document.body.style.backgroundImage = dashboardBackground ? `url('${dashboardBackground}')` : null;

        let name = document.getElementById('hdn-dashboard-name').value;
        document.getElementById('dashboard-name').innerText = name === 'Default' ? '' : name;

        let rgx = /LiveApp\('([^']+)', '([^']+)', ([\d]+)\);/g;
        let count = 0;
        while (match = rgx.exec(html)){
          let name = match[1];
          let uid = match[2];
          let interval = parseInt(match[3], 10);
          LiveApp(name, uid, interval);

          if(++count > 100) // avoid infinite while loop, shouldnt happen, but safety first
            break;
        }
    });
}