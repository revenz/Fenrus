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