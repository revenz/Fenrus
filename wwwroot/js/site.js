const abortController = new AbortController();
const signal = abortController.signal;
Scripts = [];

function abortRequests() {
    abortController.abort();
}

function getFromLocalStorage(instaceUid){
    try{
        let item = JSON.parse(localStorage.getItem(instaceUid));
        if(!item?.date)
            return;
        if(item.date < (new Date().getTime() - 60000))
            return { html: item.html, old: true}; // older than a minute reject it
        return { html: item.html, old: false};
    }
    catch(err) { return; }
}

function setInLocalStorage(instanceUid, html){
    localStorage.setItem(instanceUid, JSON.stringify({
        date: new Date().getTime(),
        html: html
    }));
}

function LiveApp(name, instanceUid, interval, subsequent) {
    if (!interval) interval = 3000;

    if(!subsequent){
        let saved = getFromLocalStorage(instanceUid);
        if(saved) {            
            setStatus(name, instanceUid, interval, saved.html);
            if(saved.old === false)
                return; // dont need to refresh
        }
    }

    fetch(`/apps/${encodeURIComponent(name)}/${encodeURIComponent(instanceUid)}/status?name=` + encodeURIComponent(name), {        
        signal: signal
    })
    .then(res => {
        if(!res.ok)
            throw res;
        return res.text();
    })
    .then(html => {
        if(html == undefined)
            return;
        setStatus(name, instanceUid, interval, html);
    }).catch(error => {
        console.log(name + ' error: ' + error);
        setTimeout(() => LiveApp(name, instanceUid, interval, true), interval + Math.random() + 5);
    });
}

function setStatus(name, instanceUid, interval, html){
    let ele = document.getElementById(instanceUid).querySelector('.status');
    if (ele && html) {
        ele.innerHTML = html;
        setInLocalStorage(instanceUid, html);
    }
    setTimeout(() => LiveApp(name, instanceUid, interval, true), interval + Math.random() + 5);
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