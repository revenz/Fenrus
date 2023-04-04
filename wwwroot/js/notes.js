var notesInitDone = false;
function notesToggle(){
    let ele = document.getElementById('notes-wrapper');
    ele.className = /expanded/.test(ele.className) ? 'collapsed' : 'expanded';
    if(notesInitDone === false)
    {
        setTimeout(() => {
            let notesTextArea = document.querySelectorAll('.note textarea');
            for(let nta of notesTextArea)
                notesTextAreaInput({ target: nta});            
        }, 500);
        notesInitDone = true;
    }
}

function notesAdd(){
    let ele = document.createElement('div');
    ele.className = 'note';
    ele.innerHTML = '<div class="controls">' +
        '<i onclick="notesMove(event.target, true)" class="up fa-solid fa-caret-up"></i>' +
        '<i onclick="notesDelete(event.target)"class="delete fa-sharp fa-solid fa-trash"></i>' +
        '<i onclick="notesMove(event.target, false)"class="down fa-solid fa-caret-down"></i>' +
        '</div><input onchange="notesChanged(event)" type="text" placeholder="Note Title" /><textarea oninput="notesTextAreaInput(event)" onchange="notesChanged(event)" placeholder="Note Content"></textarea>';
    document.getElementById('notes-list').appendChild(ele);
    notesTextAreaInput({ target: ele.querySelector('textarea')});
}

async function notesChanged(event){
    if(!event)
        return;
    let note = event.target.parentNode;
    let uid = note.getAttribute('x-uid');
    if(!uid)
        uid = '00000000-0000-0000-0000-000000000000';
    let name = note.querySelector('input').value;
    let content = note.querySelector('textarea').value;
    let result = JSON.parse(await (await fetch('notes', {
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
    let response = await fetch( '/notes/' + uid + '/move/' + up, { method: 'post'} );
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