window.onpopstate = function(event) {
    let path = document.location.pathname;
    path = path.substring(path.lastIndexOf('/') + 1);
    fetchDashboard(path, true);
}

var eleTranslations = document.getElementById("translations");
if(eleTranslations){
    Translations = JSON.parse(eleTranslations.value); 
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
    
    console.log(`Live App ${name}: ${interval}`);

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
            let title = eleApp.querySelector('.content .title');
            if(title)
                title = title.textContent;
            else
                title = eleApp.getAttribute('title');
            divLaunchingApp.querySelector('.title').textContent = 'Launching ' + title;
            
            let img = eleApp.querySelector('.icon img');
            if(img)
                divLaunchingApp.querySelector('img').src = img.src;
            else{
                img = eleApp.querySelector('img');
                if(img)
                    divLaunchingApp.querySelector('img').src = img.src;
            }
            divLaunchingApp.style.display = 'unset';
        }
    }
}

window.onpageshow = function(event) {
    if (event.persisted) 
        hideLaunchingSplash();
};

document.addEventListener("DOMContentLoaded", function(event) {
    hideLaunchingSplash();
});

function hideLaunchingSplash()
{
    let divLaunchingApp = document.getElementById('launching-app');
    if(divLaunchingApp)
        divLaunchingApp.style.display = 'none';
}

function changeDashboard(uid){     
    let expires = new Date();
    expires.setFullYear(expires.getFullYear() + 1);
    document.cookie = 'dashboard=' + uid + ';expires=' + expires.toUTCString() + ';';
    window.location.reload();
}

function editGroup(system, groupUid){
    window.location.href = system ? '/settings/system/groups/' + groupUid : '/settings/groups/' + groupUid;
}

function moveGroup(groupUid, up){
    let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');
    fetch(`/settings/dashboard/${dashboardUid}/move-group/${groupUid}/${up}`, { method: 'POST'}).then(res => {
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

async function removeGroup(groupUid, groupName) 
{
    let msg = Translations.RemoveGroupMessage.replace('#NAME#', groupName);
    try {
        if((await modalConfirm(Translations.RemoveGroupTitle, msg)) != true)
            return;
        console.log('test');
        let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');
        fetch(`/settings/dashboard/${dashboardUid}/remove-group/${groupUid}`, {method: 'POST'}).then(res => {
            let eleGroup = document.getElementById(groupUid);
            eleGroup?.remove();
        });
    }catch(err) {
        console.log('err', err);
    }
}

function UpdateSetting(setting, event)
{
    if(setting === 'Dashboard'){
        changeDashboard(event);
        return;
    }
    UpdateSettingValue(`/settings/update-setting/${setting}/#VALUE#`, event);
}

function UpdateDashboardSetting(setting, event)
{
    return new Promise(async (resolve, reject) =>
    {
        let uid = document.querySelector('div.dashboard[x-uid]').getAttribute('x-uid');
        if (!uid) {
            reject();
            return;
        }
        let result = await UpdateSettingValue(`/settings/dashboard/${uid}/update-setting/${setting}/${event.extension || '#VALUE#'}`, event);
        resolve(result);
    });
}

function ChangeBackgroundColor(event, color){
    if(window.BackgroundInstance?.changeBackgroundColor)
        window.BackgroundInstance?.changeBackgroundColor(color);
    UpdateDashboardSetting('BackgroundColor', color);
}
function ChangeAccentColor(event, color){
    document.body.style.setProperty('--accent', color);
    UpdateDashboardSetting('AccentColor', color);
    if(window.BackgroundInstance?.changeAccentColor)
        window.BackgroundInstance?.changeAccentColor(color);
}
function ChangeBackground(event){
    let background = event;
    if(background === 'image-picker'){
        // special case, open an image picker 
        BackgroundImagePicker();
        return;
    }
    if(typeof(background) !== 'string') {
        let index = background.target.selectedIndex;
        background = background.target.options[index].value;
    }
    
    let bkgSrc = '/backgrounds/' + background;

    let backgroundScript = document.getElementById('background-script');
    if(background !== 'image.js' && backgroundScript) {
        // check if its the same background
        if(backgroundScript.getAttribute('src') === bkgSrc)
            return; // same background, nothing to do 
        
        backgroundScript.remove();
    }
    
    if(window.BackgroundInstance?.dispose) {
        window.BackgroundInstance.dispose();
        delete Background;
    }
        
    InitBackground(background);
    UpdateDashboardSetting('Background', event);
}

var loadedBackgrounds = {};
function InitBackground(background){
    if(!background)
        return;    
    let bkgSrc = '/backgrounds/' + background;
    
    if(typeof(loadedBackgrounds[bkgSrc]) === 'function'){
        window.BackgroundInstance = new loadedBackgrounds[bkgSrc]();
        window.BackgroundInstance.init();
        return;
    }
        
    let backgroundScript = document.createElement('script');
    backgroundScript.setAttribute('id', 'background-script');
    backgroundScript.onload = () => {
        loadedBackgrounds[bkgSrc] = window.BackgroundType;
        window.BackgroundInstance = new window.BackgroundType();
        window.BackgroundInstance.init();
    };
    backgroundScript.setAttribute('src', bkgSrc);
    document.head.append(backgroundScript);
}

function BackgroundImagePicker(){
    let setValue = (value) =>{
        let target = document.querySelector('#DashboardBackgroundSelect ul');
        if(value === 'CurrentImage') {
            let opt = target.querySelector('.opt-CurrentImage');
            if (!opt) {
                // option doesn't exist, they didnt have a current image, we have to add it
                opt = document.createElement('li');
                opt.setAttribute('data-value', 'image');
                opt.setAttribute('class', 'opt-CurrentImage');
                opt.innerText = 'Current Image';
                opt.addEventListener('click', (event) => {
                    fenrusSelectOptionClick(event);
                    ChangeBackground('current');
                });
                target.insertBefore(opt, target.firstChild);
            }
            opt.click();
        }
        else {
            target.querySelector('.opt-' + value)?.click();
        }
    } 
    
    const id = 'background-image-picker';
    let ele = document.getElementById(id);
    if(ele)
        ele.remove();
    ele = document.createElement('input');
    ele.setAttribute('id', id);
    ele.setAttribute('type', 'file');
    ele.setAttribute('accept', 'image/*');
    ele.style.display = 'hidden';
    document.body.append(ele);
    ele.addEventListener('change', (event) => {
        let file = event.target.files[0];
        if (!file) {
            setValue('default');
            return;
        }
        let extension = file.name.substring(file.name.lastIndexOf('.') + 1);;
        let reader = new FileReader();
        reader.onload = async (e) => {
            ele.remove();            
            let contents = e.target.result;            
            let result = await UpdateDashboardSetting('BackgroundImage', { extension: extension, data: contents });
            setValue('CurrentImage');        
        };
        reader.readAsArrayBuffer(file);
    }, false);
    ele.click();
}

async function UpdateThemeSetting(theme, setting, event)
{
    let value = (await UpdateSettingValue(`/settings/theme/${theme}/update-setting/${setting}/#VALUE#`, event))?.value;
    if(value === undefined)
        return;

    themeInstance.settings[setting] = value;
    if(themeInstance.init)
        themeInstance.init();
}

function UpdateSettingValue(url, event)
{
    return new Promise((resolve, reject) => {
       
        if(event?.target?.className === 'slider round') {
            reject();
            return;
        }
        let value = event;
        if(event?.target?.tagName === 'SELECT')
        {
            let index = event.target.selectedIndex;
            value = event.target.options[index].value;
        }
        else if(event?.target?.tagName === 'INPUT' && event.target.type === 'checkbox')
            value = event.target.checked;
        else if(event?.target?.tagName === 'INPUT' && event.target.type === 'range') {
            value = event.target.value;
            let min = parseInt(event.target.getAttribute('min'), 10);
            let max = parseInt(event.target.getAttribute('max'), 10);
            let percent = (value - min) / (max - min) * 100;
            event.target.style = `background-size: ${percent}% 100%`;
            let rangeValue = event.target.parentNode.querySelector('.range-value');
            if(rangeValue)
                rangeValue.textContent = value;
        }
        url = url.replace('#VALUE#', encodeURIComponent(value));
        fetch(url, { method: 'POST', body: event?.data}).then(res => {
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

            let searchContainer = document.getElementById('search-container');
            if(searchContainer)
                searchContainer.style.display = json.showSearch ? '' : 'none';
            
            if(json.linkTarget){
                // need to update all the targets
                for(let a of eleDashboard.querySelectorAll('a'))
                {
                    if(a.getAttribute('href').length > 1)
                        a.setAttribute('target', json.linkTarget);
                }
            }
            resolve({ result: json, value: value});
        });
    });
}

function shadeColor(color, percent) {

    let R = parseInt(color.substring(1,3),16);
    let G = parseInt(color.substring(3,5),16);
    let B = parseInt(color.substring(5,7),16);

    R = parseInt(R * (100 + percent) / 100);
    G = parseInt(G * (100 + percent) / 100);
    B = parseInt(B * (100 + percent) / 100);

    R = (R<255)?R:255;
    G = (G<255)?G:255;
    B = (B<255)?B:255;

    R = Math.round(R)
    G = Math.round(G)
    B = Math.round(B)

    let RR = ((R.toString(16).length===1)?"0"+R.toString(16):R.toString(16));
    let GG = ((G.toString(16).length===1)?"0"+G.toString(16):G.toString(16));
    let BB = ((B.toString(16).length===1)?"0"+B.toString(16):B.toString(16));

    return "#"+RR+GG+BB;
}

function CreateColorPicker(element)
{
    if(typeof(element) === 'string') 
        element = document.getElementById(element);
    
    var value = element.value;
    new JSColor(element, 
        { 
            value: value, 
            mode: 'HVS', 
            shadow: false, 
            controlBorderColor: 'var(--input-border)',
            borderColor: 'var(--input-border)', 
            borderRadius: 0, 
            alphaChannel: false 
        });
}