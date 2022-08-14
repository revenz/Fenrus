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

var contextMenus = {};
const copyIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path></svg>`;
const cutIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><circle cx="6" cy="6" r="3"></circle><circle cx="6" cy="18" r="3"></circle><line x1="20" y1="4" x2="8.12" y2="15.88"></line><line x1="14.47" y1="14.48" x2="20" y2="20"></line><line x1="8.12" y1="8.12" x2="12" y2="12"></line></svg>`;
const pasteIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px; position: relative; top: -1px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"></path><rect x="8" y="2" width="8" height="4" rx="1" ry="1"></rect></svg>`;
const downloadIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px; position: relative; top: -1px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>`;
const deleteIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none" style="margin-right: 7px" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>`;
const infoIcon = `<span class="icon icon-info-circle" style="padding-right:0.5rem"></span>`;
const addIcon = `<span class="icon icon-plus" style="padding-right:0.5rem"></span>`;
const editIcon = `<span class="icon icon-edit" style="padding-right:0.5rem"></span>`;
const dashboardIcon = `<span class="icon icon-home" style="padding-right:0.5rem"></span>`;
const terminalIcon = `<span class="icon icon-globe" style="padding-right:0.5rem"></span>`;

function openDefaultContextMenu(event) {
    event?.preventDefault();
    event?.stopPropagation();
    if(!contextMenus['DEFAULT'])
    {
        let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');

        const menuItems = [
            {
                content: `${dashboardIcon}Edit Dashboard`,
                events: {
                    click: (e) => {
                        document.location = '/settings/dashboards/' + dashboardUid                
                    }
                }
            },
            {
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal()
                }
            }

            ];
            
            let menu = new ContextMenu({
                menuItems
            });
              
            menu.init();
            contextMenus['DEFAULT'] = menu;
        }
    
        contextMenus['DEFAULT'].open(event);

}

function openContextMenu(event, app){
    event?.preventDefault();
    event?.stopPropagation();
    if(typeof(app) === 'string')
        app = JSON.parse(app);

        
    let uid = app.Uid;
    let ele = document.getElementById(uid);
    let groupUid = ele.closest('.db-group').getAttribute('id');
    let dashboardUid = ele.closest('.dashboard').getAttribute('x-uid');
    if(!contextMenus[uid])
    {
        const menuItems = [
        {
            content: `${infoIcon}Up-Time`,
            events: {
                click: (e) => openUpTime(app)                
            }
        },
        {
          content: `${deleteIcon}Delete`
        },
        {
            content: `${editIcon}Edit Group`,
            divider: "top", // top, bottom, top-bottom
            events: {
                click: (e) => {
                    document.location = '/settings/groups/' + groupUid                
                }
            }
        },
        {
            content: `${dashboardIcon}Edit Dashboard`,
            events: {
                click: (e) => {
                    document.location = '/settings/dashboards/' + dashboardUid                
                }
            }
        },
        {
            divider: "top",
            content: `${terminalIcon}Terminal`,
            events: {
                click: (e) => openTerminal()
            }
        }
        ];
        
        let menu = new ContextMenu({
            menuItems
        });
          
        menu.init();
        contextMenus[uid] = menu;
    }

    contextMenus[uid].open(event);
}

function closeUpTime(){
    document.getElementById('up-time-wrapper').style.display = '';
}

function openUpTime(app) {
    let upTime = document.getElementById('up-time-wrapper');
    document.getElementById('up-time-title').innerText = 'Up-Time For ' + app.Name;
    let utcContainer = document.getElementById('up-time-chart-container');
    utcContainer.className = '';
    utcContainer.innerHTML = '<div id="up-time-chart"></div>';
    let ctx = document.getElementById('up-time-chart');
    upTime.style.display = 'unset';
    
    fetch('/settings/up-time/' + app.Uid)
    .then((response) => response.json())
    .then((data) => {
        if(!data?.length)
        {
            utcContainer.className = 'no-data';
            utcContainer.innerText = 'No up-time data available';
            return;
        }
        data = data.map(x => ({
                x: x.date, //new Date(x.date),
                y: x.up === true ? 1 : 0
        }));
        var options = {
            chart: {
                height: 400,
                width:'100%',
                type: "line",
                stacked: false,
                toolbar: {
                    show: false
                },
                zoom: {
                    enabled: false
                },
                animations: {
                    enabled: false
                }
            },
            series: [
            {
                data: data
            }],
            stroke: {
                curve: 'stepline',
            },
            tooltip: {
                theme: 'dark',
                x: {
                    show: false
                },
                y: {
                    title: {
                        formatter: function formatter(val, o) {
                            let item = data[o.dataPointIndex];
                            let dt = new Date(item.x);
                            return dt.toLocaleTimeString();
                        }
                    },
                    formatter: function(){
                        return '';
                    }
                }
            },
            xaxis: {
                type: 'datetime',
                labels: {
                    show: true,
                    style: {
                        colors: 'var(--color)'
                    },
                }
            },
            yaxis: {
                show: false
            }  
        };

        console.log('options', options);
        var chart = new ApexCharts(ctx, options);
        chart.render();
    });

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


function openTerminal(){
    let div = document.createElement('div');
    div.setAttribute('id', 'terminal');
    document.body.appendChild(div);
    setTimeout(() => {

        const fitAddon = new FitAddon.FitAddon();
        console.log('fitAddon', fitAddon);
        var term = new Terminal({ cursorBlink: true, fontFamily: 'Courier New', fontSize: 16 });
        term.loadAddon(fitAddon);
        term.open(div); 
        fitAddon.fit();
        
        term.write('Welcome to the Fenrus Terminal\r\n');   
        term.write('Server: ');
        term.focus();
        let mode = 0; // 0 = server, 1 = username, 2 = password, 3 = ssh
        var socket;
        
        let line = '';
        let server, username, password;
        term.onKey(function (ev) {
            let key = ev.key;
            if(mode < 3){
                if (key.charCodeAt(0) === 13)
                {
                    // enter
                    if(mode === 0 || mode === 1){
                        if(mode === 0)
                            server = line;
                        else
                            username = line;
                        line = '';
                        term.write(`\r\n${mode === 0 ? 'Username' : 'Password'}: `);
                        ++mode;
                        return;
                    }
                    password = line;
                    line = '';
                    ++mode;
                    connect([server, username, password]);
                    return;
                }
                else if(key.charCodeAt(0) === 127){
                    if(line.length > 0){
                        line = line.substring(0, line.length - 1);
                        term.write("\b \b");
                    }
                }
                else 
                {
                    line += key;
                    if(mode !== 2)
                        term.write(key);
                }
            }
            else
            {
                socket.emit('data', key);
            }
        });

        const connect = function(args){
            socket = io(null,  {
                transports:['websocket']
            });
            socket.emit('ssh', args);
            socket.on('connect', function() {
                term.write('\r\n*** Connected to backend ***\r\n');
            });

            // Backend -> Browser
            socket.on('data', function(data) {
                term.write(data);
            });
            socket.on('ssh-closed', () => {
                term.write('\r\nssh-close\r\n');
                socket.close();
                closeTerminal();
            });

            socket.on('disconnect', function() {
                term.write('\r\n*** Disconnected from backend ***\r\n');
            });
        }

        const closeTerminal = function() {
            div.className = 'closing';
            setTimeout(() => {
                div.remove();
            }, 500);
        }

    }, 750);
}