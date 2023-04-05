var notesLastLoaded = {};
var noteTab = false;
var notesDashboardUid;

function notesToggle(){
    let ele = document.getElementById('notes-wrapper');
    let expanding = /expanded/.test(ele.className) === false;
    ele.className =  expanding ? 'expanded' : 'collapsed';
    
    if(expanding && !noteTab){
        noteTab = localStorage.getItem('NOTES_TAB');
        if(!noteTab)
            noteTab = 'nt-personal';
        notesSetActiveTabClass();
    }
    
    if(expanding && notesNeedReload()) {
        notesReload();
    }
}

function notesSetActiveTabClass(){
    for(let tab of document.querySelectorAll('.notes-tabs .note-tab')){
        tab.className = 'note-tab' + (tab.getAttribute('id') === noteTab ? ' active' : '');
    }
    
}

function notesNeedReload() {
    if(noteTab === 'dashboard')
    {
        let dbUid = getDashboardUid()
        if(notesDashboardUid != dbUid)
            return true; // always refresh if theyve changed dashboards
    }
    if(!notesLastLoaded || !notesLastLoaded[noteTab])
        return true;
    return new Date().getTime() - notesLastLoaded[noteTab].getTime() > 5 * 60 * 1000;
}

function notesQueryParameters()
{
    var dbUid = getDashboardUid();
    return `?type=${noteTab.substring(3)}&db=${encodeURIComponent(dbUid)}`;
}

function notesSelectTab(tab){
    noteTab = tab;
    localStorage.setItem('NOTES_TAB', tab);
    notesSetActiveTabClass();
    if(notesNeedReload)
        notesReload();
}

async function notesReload(){
    try 
    {
        const response = await fetch('/notes' + notesQueryParameters());
        const data = await response.json();
        let list = document.getElementById('notes-list');
        list.innerHTML = '';
        for(let note of data)
        {
            let ele = notesCreateElement();
            ele.setAttribute('x-uid', note.uid);
            ele.querySelector('input').value = note.name;
            ele.querySelector('.content-editor').innerHTML = note.content;
            list.appendChild(ele);
        }
        if(!notesLastLoaded)
            notesLastLoaded = {};
        notesLastLoaded[noteTab] = new Date();
        if(noteTab === 'dashboard')
            notesDashboardUid = getDashboardUid();
    } catch (error) {
        console.log('error', error);
    }
}

function notesAdd(){
    document.getElementById('notes-list').appendChild(notesCreateElement());
}

function notesCreateElement(){
    let ele = document.createElement('div');
    ele.className = 'note';
    ele.innerHTML = '<div class="controls">' +
        '<i onclick="notesMove(event.target, true)" class="up fa-solid fa-caret-up"></i>' +
        '<i onclick="notesDelete(event.target)"class="delete fa-sharp fa-solid fa-trash"></i>' +
        '<i onclick="notesMove(event.target, false)"class="down fa-solid fa-caret-down"></i>' +
        '</div>' +
        '<input onchange="notesChanged(event)" type="text" placeholder="Note Title" />' +
        '<div class="content-editor" onfocusout="notesChanged(event)" contenteditable="true" spellcheck="false" onpaste="notesOnPaste(event)"></div>' +
        '';
    return ele;
    
}

function notesOnPaste(e){
    let cbPayload = [...(e.clipboardData || e.originalEvent.clipboardData).items];     // Capture the ClipboardEvent's eventData payload as an array
    cbPayload = cbPayload.filter(i => /image/.test(i.type));                       // Strip out the non-image bits

    if(!cbPayload.length || cbPayload.length === 0) return false;                      // If no image was present in the collection, bail.

    let reader = new FileReader();                                                     // Instantiate a FileReader...
    reader.onload = (e) => contentTarget.innerHTML = `<img src="${e.target.result}">`; // ... set its onLoad to render the event target's payload
    reader.readAsDataURL(cbPayload[0].getAsFile());                                    // ... then read in the pasteboard image data as Base64
}

async function notesChanged(event){
    if(!event)
        return;
    let note = event.target.parentNode;
    let uid = note.getAttribute('x-uid');
    if(!uid)
        uid = '00000000-0000-0000-0000-000000000000';
    let name = note.querySelector('input').value;
    let content = note.querySelector('.content-editor').innerHTML;
    let result = JSON.parse(await (await fetch('notes' + notesQueryParameters(), {
        method: 'post',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            Uid: uid,
            Name: name,
            Content: content
        })
    })).text());
    note.setAttribute('x-uid', result.uid);
}

function notesTextAreaInput(event){
    let target = event.target;
    target.style.height = ''; 
    target.style.height = target.scrollHeight + 'px';
}

async function notesMove(target, up){
    let note = target.parentNode.parentNode;
    let uid = note.getAttribute('x-uid');
    let response = await fetch( '/notes/' + uid + '/move/' + up + notesQueryParameters(), { method: 'post'} );
    let success = await response.text();
    if(success === 'true')
    {
        if(up)
            note.parentNode.insertBefore(note, note.previousSibling); // up
        else
            note.parentNode.insertBefore(note.nextSibling, note); // down
    }    
}

async function notesDelete(target){
    let note = target.parentNode.parentNode;
    let result = await modalConfirm('Delete Note', 'Are you sure you want to delete this note?');
    if(!result)
        return;
    let uid = note.getAttribute('x-uid');
    if(!uid) {
        note.remove();
        return;
    }
    let success =  await fetch( '/notes/' + uid, { method: 'delete'} );
    if(success)
        note.remove();
}