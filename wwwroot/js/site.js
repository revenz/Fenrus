window.onpopstate = function(event) {
    console.log('document.location', document.location, event);
    let path = document.location.pathname;
    path = path.substring(path.lastIndexOf('/') + 1);
    fetchDashboard(path, true);
}

function abortRequests() {
    let uid = this.getDashboardInstanceUid();
    let event = new CustomEvent("disposeDashboard", 
        {
            detail: {
                uid: uid,
            },
            bubbles: true,
            cancelable: true
        }
    );
    document.body.dispatchEvent(event);
}


function getDashboardInstanceUid()
{
    return document.getElementById('dashboard-instance')?.value;
}


function LiveApp(name, instanceUid, interval) 
{    
    if(typeof(name) !== 'string')
        throw 'Name is not a string';

    new SmartApp({
        name: name,
        uid: instanceUid,
        interval: interval,
    });
}

function changeTheme(event) {
    let theme = event?.target?.value;
    if (!theme)
        return;
    document.getElementById('theme-style').setAttribute('href', `/themes/${theme}/theme.min.css`);
}

function htmlEncode(text) {
    var node = document.createTextNode(text);
    var p = document.createElement('p');
    p.appendChild(node);
    return p.innerHTML;
}

function newGuid() 
{
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
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

function changeTheme(name){
    window.location.reload();
}


function fetchDashboard(uid,  backwards) {
    let currentTheme = document.getElementById('hdn-dashboard-theme')?.value || 'Default';
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

        let eleDashboard = document.querySelector('.dashboard');
        eleDashboard.innerHTML = html;
        if(typeof(themeInstance) !== 'undefined')
            themeInstance.init();

        let dashboardBackground = document.getElementById('hdn-dashboard-background')?.value || null;
        document.body.style.backgroundImage = dashboardBackground ? `url('${dashboardBackground}')` : null;
        document.body.classList.remove('custom-background');
        document.body.classList.remove('no-custom-background');
        document.body.classList.add((!dashboardBackground ? 'no-' : '') + 'custom-background');

        let dashboardTheme = document.getElementById('hdn-dashboard-theme')?.value || 'Default';
        if(currentTheme != dashboardTheme)
            changeTheme(dashboardTheme);


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

function launch(event, uid) {
    abortRequests();
    if(event && event.ctrlKey)
        return;
    let divLaunchingApp = document.getElementById('launching-app');
    let eleApp = document.getElementById(uid);    
    if(eleApp && divLaunchingApp){
        let target = eleApp.getAttribute('target');
        if(!target || target === '_self'){
            divLaunchingApp.querySelector('.title').textContent = 'Launching ' + eleApp.querySelector('.content .title').textContent;
            divLaunchingApp.querySelector('img').src = eleApp.querySelector('.icon img').src;
            divLaunchingApp.style.display = 'unset';
        }
    }
}

document.addEventListener("DOMContentLoaded", function(event) {
    let divLaunchingApp = document.getElementById('launching-app');
    if(divLaunchingApp)
        divLaunchingApp.style.display = 'none';
});

function changeDashboard(uid){    
    document.cookie = 'dashboard=' + uid;
    window.location.reload(true);
}

function openIframe(event, app){
    event?.preventDefault();
    event?.stopPropagation();
    if(typeof(app) === 'string')
        app = JSON.parse(app);

    let appItem = document.getElementById(app.Uid);
    if(!appItem)
        return;
    let group = appItem.parentNode;
    console.log('group', group);

    console.log('open iframe', app);
    let div = document.createElement('div');
    div.className = 'iframe-content';

    let side = document.createElement('div');
    side.className = 'side';
    div.appendChild(side);

    let iframe = document.createElement('iframe');

    const addItem  = function(item)
    {        
        let divChild = document.createElement('a');        
        divChild.className = 'db-item db-basic db-link medium';
        let divInner = document.createElement('div');
        divInner.className = 'inner';
        divChild.appendChild(divInner);
        let appImg = item?.querySelector('img');
        if(appImg){
            let img = document.createElement('img');
            img.src = appImg.src;
            let imgWrapper = document.createElement('div');
            imgWrapper.className = 'icon';
            imgWrapper.appendChild(img);
            divInner.appendChild(imgWrapper);
        }
        else
        {
            let img = document.createElement('i');
            img.className = 'icon icon-close';
            let imgWrapper = document.createElement('div');
            imgWrapper.className = 'icon';
            imgWrapper.appendChild(img);
            divInner.appendChild(imgWrapper);
        }

        let divContent = document.createElement('div');
        divContent.className = 'content';

        let divTitle = document.createElement('div');
        divTitle.className = 'title';
        divTitle.innerText = item ? item.querySelector('.title').innerText : 'Close';
        divContent.appendChild(divTitle);
        divInner.appendChild(divContent);

        divChild.addEventListener('click', (event) => {
            event.preventDefault();
            if(item === null){
                div.classList.add('closing');
                setTimeout(()=> {
                    div.remove();
                }, 500);
                return false;
            }
            else if(item.className.indexOf('iframe') > 0) {
                iframe.setAttribute('src', item.getAttribute('href'));
            }
            else {
                item.click();
            }
            return false;
        });
        side.appendChild(divChild);
    }
    addItem(null);

    for(let item of group.querySelectorAll('.db-item'))
    {
        addItem(item);
    }

    iframe.setAttribute('seamless', true);
    iframe.setAttribute('src', app.Url);
    iframe.setAttribute('frameBorder', 0);
    div.appendChild(iframe);

    document.body.appendChild(div);


}