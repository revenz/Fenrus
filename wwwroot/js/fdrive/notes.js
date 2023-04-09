class FenrusDriveNotes 
{
    notesLastLoaded = {};
    selectedTab;
    dashboardUid;
    eleList;
    eleTabs = {};
    
    constructor(){
        this.eleList = document.getElementById('notes-list');
        this.selectedTab = localStorage.getItem('NOTES_TAB') || 'nt-personal';
        this.eleTabs['nt-personal'] = document.getElementById('nt-personal');
        this.eleTabs['nt-dashboard'] = document.getElementById('nt-dashboard');
        this.eleTabs['nt-shared'] = document.getElementById('nt-shared');
        this.setSelectedNoteClass();
    }
    
    show(){
        if(this.needReload())
            this.reload();
    }
    selectTab(tab){
        if(this.selectedTab === tab)
            return;

        localStorage.setItem('NOTES_TAB', tab);
        this.selectedTab = tab;
        this.notesLastLoaded = {};
        this.setSelectedNoteClass();
        this.reload();
    }
    
    setSelectedNoteClass(){
        Object.keys(this.eleTabs).forEach(x => {
            this.eleTabs[x].className = this.selectedTab === x ? 'active' : '';
        });
    }
    
    needReload() {
        if (this.selectedTab === 'dashboard') {
            let dbUid = getDashboardUid()
            if (this.dashboardUid !== dbUid)
                return true; // always refresh if theyve changed dashboards
        }
        if (!this.notesLastLoaded || !this.notesLastLoaded[this.selectedTab])
            return true;
        return new Date().getTime() - this.notesLastLoaded[this.selectedTab].getTime() > 5 * 60 * 1000;
    }


    queryParameters() {
        var dbUid = getDashboardUid();
        return `?type=${this.selectedTab.substring(3)}&db=${encodeURI(dbUid)}`;
    }

    async reload() 
    {        
        try {
            let url = '/notes' + this.queryParameters();
            const response = await fetch(url);
            const data = await response.json();
            this.eleList.innerHTML = '';
            this.addToList(data);
            this.notesLastLoaded[this.selectedTab] = new Date();
            if (this.selectedTab === 'dashboard')
                this.dashboardUid = getDashboardUid();
        } catch (error) {
            console.log('error', error);
        }
    }

    addToList(items) {
        if (!items)
            return;
        if (Array.isArray(items) === false)
            items = [items];
        for (let note of items) {
            let ele = this.createElement(note.readOnly);
            ele.setAttribute('x-uid', note.uid);
            ele.querySelector('input').value = note.name;
            ele.querySelector('.content-editor').innerHTML = note.content;
            this.eleList.appendChild(ele);
        }
    }

    add() {
        this.eleList.appendChild(this.createElement(false));
    }
    
    createElement(readOnly) {
        let ele = document.createElement('div');
        ele.className = 'note'  + (readOnly ? ' readonly' : '');
        ele.innerHTML = '<div class="controls">' +
            '<i class="up fa-solid fa-caret-up"></i>' +
            (readOnly ? '' : '<i class="delete fa-sharp fa-solid fa-trash"></i>') +
            '<i class="down fa-solid fa-caret-down"></i>' +
            '</div>' +
            `<input type="text" placeholder="Note Title" ${(readOnly ? 'disabled' : '')}  />` +
            `<div class="content-editor" ${(readOnly ? '' : 'contenteditable="true"')} spellcheck="false"></div>` +
            '';
        ele.querySelector('.up').addEventListener('click', (event) => this.move(event.target, true));
        ele.querySelector('.down').addEventListener('click', (event) => this.move(event.target, false));
        if(!readOnly) {
            ele.querySelector('.delete').addEventListener('click', (event) => this.deleteNote(event.target));
            ele.querySelector('input').addEventListener('change', (event) => this.onChange(event));
            let contentEditor = ele.querySelector('.content-editor');
            contentEditor.addEventListener('paste', (event) => this.onPaste(event));
            contentEditor.addEventListener('focusout', (event) => this.onChange(event));
        }
        return ele;

    }

    onPaste(e) {
        let cbPayload = [...(e.clipboardData || e.originalEvent.clipboardData).items];     // Capture the ClipboardEvent's eventData payload as an array
        cbPayload = cbPayload.filter(i => /image/.test(i.type));                       // Strip out the non-image bits

        if (!cbPayload.length || cbPayload.length === 0) return false;                      // If no image was present in the collection, bail.

        let reader = new FileReader();                                                     // Instantiate a FileReader...
        reader.onload = (e) => contentTarget.innerHTML = `<img src="${e.target.result}">`; // ... set its onLoad to render the event target's payload
        reader.readAsDataURL(cbPayload[0].getAsFile());                                    // ... then read in the pasteboard image data as Base64
    }

    async onChange(event) {
        if (!event)
            return;
        let note = event.target.parentNode;
        let uid = note.getAttribute('x-uid');
        if (!uid)
            uid = '00000000-0000-0000-0000-000000000000';
        let name = note.querySelector('input').value;
        let content = note.querySelector('.content-editor').innerHTML;
        let result = JSON.parse(await(await fetch('notes' + this.queryParameters(), {
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

    async move(target, up) {
        let note = target.parentNode.parentNode;
        let uid = note.getAttribute('x-uid');
        let response = await fetch('/notes/' + uid + '/move/' + up + this.queryParameters(), {method: 'post'});
        let success = await response.text();
        if (success === 'true') {
            if (up)
                note.parentNode.insertBefore(note, note.previousSibling); // up
            else
                note.parentNode.insertBefore(note.nextSibling, note); // down
        }
    }

    async deleteNote(target) {
        let note = target.parentNode.parentNode;
        let result = await modalConfirm('Delete', `Are you sure you want to delete this note?`);
        if (!result)
            return;
        let uid = note.getAttribute('x-uid');
        if (!uid) {
            note.remove();
            return;
        }
        let success = await fetch('/notes/' + uid + this.queryParameters(), {method: 'delete'});
        if (success)
            note.remove();
    }
}