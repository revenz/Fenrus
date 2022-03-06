const abortController = new AbortController();
const signal = abortController.signal;
Scripts = [];

function abortRequests() {
    abortController.abort();
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

function LiveApp(name, instanceUid, interval, subsequent) {
    if (!interval) interval = 3000;

    if(!subsequent){
        let saved = getFromLocalStorage(instanceUid);
        if(saved) {            
            setStatus(name, instanceUid, interval, saved.html, !!subsequent);
            if(saved.old === false)
                return; // dont need to refresh
        }
    }

    var newTimeout = () => {
        setTimeout(() => LiveApp(name, instanceUid, interval, true), interval + Math.random() + 5);
    }

    if(subsequent && document.hasFocus() === false){
        newTimeout();
        return; // prevent request if the page doesnt have focus
    }

    fetch(`/apps/${encodeURIComponent(name)}/${encodeURIComponent(instanceUid)}/status?name=` + encodeURIComponent(name), {        
        signal: signal
    })
    .then(res => {
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
        setStatus(name, instanceUid, interval, html, !!subsequent);
    }).catch(error => {
        console.log(name + ' error: ' + error);        
        newTimeout();
    });
}

function setStatus(name, instanceUid, interval, content, subsequent){
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
        setTimeout(() => LiveApp(name, instanceUid, interval, true), interval + Math.random() + 5);
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