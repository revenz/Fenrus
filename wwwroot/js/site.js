const abortController = new AbortController();
const signal = abortController.signal;
Scripts = [];

function abortRequests() {
    abortController.abort();
}

function LiveApp(name, instanceUid, interval) {
    console.log('live app: ' + name);
    if (!interval) interval = 3000;

    fetch(`/apps/${encodeURIComponent(name)}/${encodeURIComponent(instanceUid)}/status?name=` + encodeURIComponent(name), {        
        signal: signal
    })
        .then(res => res.text())
        .then(html => {
        console.log('result', html);
        let ele = document.getElementById(instanceUid).querySelector('.status');
        if (ele && html) {
            ele.innerHTML = html;
        }
        setTimeout(() => LiveApp(name, instanceUid, interval), interval);
    }).catch(error => {
        console.log('error: ' + error);
        setTimeout(() => LiveApp(name, instanceUid, interval), interval);
    });

}
function htmlEncode(text) {
    var node = document.createTextNode(text);
    var p = document.createElement('p');
    p.appendChild(node);
    return p.innerHTML;
}

function liveStats(items) {

    let html = '<ul class="livestats">';
    for (let item of items) {

        html += `<li><span class="title">${htmlEncode(item[0])}</span><span class="value">${htmlEncode(item[1])}</span></li>`;
    }
    html += '</ul>';
    return html;
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