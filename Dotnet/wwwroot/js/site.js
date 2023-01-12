window.onpopstate = function(event) {
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

function changeTheme(theme) {
    if (!theme)
        return;
    document.getElementById('theme-style').setAttribute('href', `/themes/${theme}/theme.css`);
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
    document.cookie = 'dashboard=' + uid + '; expires=Tue, 19 Jan 2038 04:14:07 GMT';
    window.location.reload(true);
}

function moveGroup(groupUid, up){
    let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');
    fetch(`/settings/dashboards/${dashboardUid}/move-group/${groupUid}/${up}`, { method: 'POST'}).then(res => {
        let eleGroup = document.getElementById(groupUid);
        let dashboard = eleGroup.parentElement;
        let groups = dashboard.querySelectorAll('.db-group');
        let grpIndex = Array.prototype.indexOf.call(groups, eleGroup);
        let first, second;
        if(up){
            first = eleGroup;
            second = groups[grpIndex - 1];
        }else{
            first = groups[grpIndex + 1];
            second = eleGroup;
        }
        dashboard.insertBefore(first, second);
    });
}

function removeGroup(groupUid, groupName) 
{
    if(confirm(`Do you want to remove the group '${groupName}'?`) !== true)
        return;
    let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');        
    fetch(`/settings/dashboards/${dashboardUid}/remove-group/${groupUid}`, { method: 'POST'}).then(res => {
        let eleGroup = document.getElementById(groupUid);
        eleGroup?.remove();
    });
}

function UpdateSetting(setting, event)
{
    if(event.target?.className === 'slider round')
        return;
    let value = event;
    if(event.target?.tagName === 'SELECT')
    {
        let index = event.target.selectedIndex;
        value = event.target.options[index].value;
    }
    else if(event.target?.tagName === 'INPUT' && event.target.type === 'checkbox')
        value = event.target.checked;
    
    if(setting === 'Dashboard'){
        changeDashboard(value);
        return;
    }
    fetch(`/settings/update-setting/${setting}/${value}`, { method: 'POST'}).then(res => {
        return res.json();
    }).then(json => {
        if(json.reload) {
            window.location.reload();
            return;
        }

        let eleDashboard = document.querySelector('.dashboard');
        eleDashboard.classList.remove('hide-group-titles');
        if(json.showGroupTitles === false)
            eleDashboard.classList.add('hide-group-titles');

        eleDashboard.classList.remove('status-indicators');
        if(json.showStatusIndicators === true)
            eleDashboard.classList.add('status-indicators');

        if(json.linkTarget){
            // need to update all the targets
            for(let a of eleDashboard.querySelectorAll('a'))
            {
                if(a.getAttribute('href').length > 1)
                    a.setAttribute('target', json.linkTarget);
            }
        }
    });
}