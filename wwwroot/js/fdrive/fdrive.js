class FenrusDrive
{
    eleAddMenu;
    filePaths = [];
    filesPath;
    lastReload;
    container;
    lblFiles;
    folderViews = {};
    
    constructor(){
        let container =  document.getElementById('fdrive-list');
        this.container = container;
        this.container.addEventListener('mousedown', (e) => {
            if(!fDriveDrawer.isFiles || e.button !== 2) return;
            container.contentEditable = true;
            setTimeout(() => container.contentEditable = false, 20);
        });
        this.container.addEventListener('drop', (event) => this.fileDropEventListener(event));
        this.container.addEventListener('dragstart', (e) => {
            e.dataTransfer.effectAllowed = 'all';
            e.dataTransfer.dropEffect = 'move';
        });
        this.container.addEventListener('dragover', (e) => {
            e.preventDefault();
        }, false);
        this.container.addEventListener('paste', (event) => this.pasteEventListener(event));
        this.lblFiles = document.querySelector('.fdrive-pane-title .title').innerText;
        this.eleAddMenu = document.getElementById('fdrive-add-menu');
        this.folderViews = localStorage.getItem('DRIVE_FOLDER_VIEWS') || {};
        if(typeof(this.folderViews) === 'string')
            this.folderViews = JSON.parse(this.folderViews);
        if(typeof(this.folderViews) !== 'object')
            this.folderViews = {};
        if(this.folderViews[''])
            this.changeView(this.folderViews[''], true);    
    }
    
    show(){
        this.reload();
    }
    
    changeFolder(path) 
    {
        this.filePaths.push(path);
        this.updateFilesBackVisiblity();
        this.loadFolder(path);
    }
    
    createElement()
    {
        let ele = document.createElement('div');
        ele.className ='file';
        ele.innerHTML = '<input class="check" type="checkbox" />' +
            '<span class="icon"><img /></span>' +
            '<span class="name"></span>' +
            '<span class="size"></span>' +
            '<span class="enter"><i class="fa-solid fa-chevron-right"></i></span>' +
            '<span class="download"><i class="fa-solid fa-download"></i></span>';
        return ele;
    }

    updateFilesBackVisiblity(){
        document.getElementById('files-back').className = this.filePaths.length === 0 ? '' : 'visible';
    }
    changeView(view, noSave){
        if(!noSave)
            this.folderViews[this.filePaths || ''] = view;
        localStorage.setItem('DRIVE_FOLDER_VIEWS', JSON.stringify(this.folderViews));
        document.getElementById('fdrive-list').className = 'content ' + view;
    }


    download(path){
        var link = document.createElement("a");
        let url = '/files/download?path=' + encodeURIComponent(path);
        link.href = url;
        link.download = url.split("/").pop();
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }

    changeFolder(path) {
        path = path || '';
        if(path.endsWith('/.'))
            path = path.substring(0, path.length - 1);
        if(path)
            this.filePaths.push(path);
        this.updateFilesBackVisiblity();
        this.loadFolder(path);
    }
    
    loadFolder(path)
    {
        path = path || '';
        if(path.endsWith('/.'))
            path = path.substring(0, path.length - 1);
        console.log('loading path: ' + path);
        let lbl = path;
        if(lbl.endsWith('/'))
            lbl = lbl.substring(0, lbl.length - 1);
        
        lbl = !lbl ? this.lblFiles : '/' + lbl;
        document.querySelector('.fdrive-pane-title .title').innerText = lbl;
        this.filesPath = path;
        
        if(this.folderViews[path])
            this.changeView(this.folderViews[path], true);
        this.reload();
    }


    parentPath(){
        console.log('going to parent', JSON.parse(JSON.stringify(this.filePaths)));
        let path = this.filePaths.pop(); // gets rid of current, we want the next
        if(!path)
            return;
    
        path = this.filePaths[this.filePaths.length - 1];
        console.log('path', path || '');
        this.loadFolder(path);
        this.updateFilesBackVisiblity();
    }
    async deleteFiles()
    {
        let checked = document.querySelectorAll('#fdrive-list .file .check:checked');
        if(!checked.length)
            return;
        let files = [];
        for(let chk of checked)
            files.push(chk.closest('.file').getAttribute('x-uid'));

        let confirmed = await modalConfirm('Delete', 'Are you sure you want to delete the selected file(s) ?');
        if(!confirmed)
            return;
        await fetch('/files', {
            method: 'delete',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({files: files})
        });
        await this.reload();
    }

    upload() {
        var input = document.createElement("input");
        input.type = "file";
        input.accept = "image/*";
        input.style.display = "none";
        input.name = "file";
        input.multiple = true;
        document.body.appendChild(input);
    
        input.addEventListener("change", () => {
            if(!input.files.length)
                return;
            var formData = new FormData();
            for(let file of input.files)
                formData.append('file', file);
            this.uploadForm(formData);
        });
    
        input.addEventListener("cancel", () => {
            input.remove();
        });
    
        input.click();
    }
    pasteEventListener(e) {
        let cbPayload = [...(e.clipboardData || e.originalEvent.clipboardData).items];
        cbPayload = cbPayload.filter(i => /image/.test(i.type));
        if (!cbPayload.length || cbPayload.length === 0)
            return;
    
        const formData = new FormData();
        let file = cbPayload[0].getAsFile();
        formData.append('file', file);
        this.uploadForm(formData);
    }

    fileDropEventListener(event){
        event.preventDefault();
        const files = event.dataTransfer.files;
        console.log('event', event);
        console.log('files length', files.length);
        console.log('files', files);
        const formData = new FormData();
        const allowedTypes = ['image/jpeg', 'image/png', 'image/gif'];
    
        let filesAdded = 0;
        for (let i = 0; i < files.length; i++) {
            console.log('file', files[i]);
            console.log('[type', files[i].type);
            if (allowedTypes.indexOf(files[i].type) !== -1) {
                console.log('adding file', files[i]);
                formData.append('file', files[i]);
                ++filesAdded;
            }
        }
        if(filesAdded === 0)
            return;
        this.uploadForm(formData);
    }

    addClicked(){
        this.eleAddMenu.className = /visible/.test(this.eleAddMenu.className) ? '' : 'visible';
    }   

    async add(mode)
    {
        this.eleAddMenu.className = '';
        if(mode === 'upload')
            this.upload();
        else if(mode === 'folder')
        {
            var name = await modalPrompt('New Folder', 'Enter a name of the the folder to create');
            if(/[~"#%&*:<>?/\\{|}]+/.test(name))
                return; // invalid
            if(!name)
                return;
            if(!this.filePaths) this.filePaths = '';
            let result = await fetch('/files/create-folder?path=' + encodeURIComponent(this.filePaths + name), { method: 'POST'});
            this.reload();
        }
    }

    async uploadForm(formData) {
        try 
        {
            let response = await fetch('/files/path?path=' + encodeURIComponent(this.filesPath || ''), {
                method: 'POST',
                body: formData
            })
            let text = await response.text();
            if (!text)
                return;
            let files = JSON.parse(text);
            this.addToList(files);
        } catch (error) {
            console.error(error);
        }
    }


    async reload(){
        try
        {
            let url = '/files/path?path=' + encodeURI(this.filePaths || '');
            const response = await fetch(url);
            const data = await response.json();
            let list = document.getElementById('fdrive-list');
            list.innerHTML = '';
            this.addToList(data);
            this.lastReload = new Date();
        } catch (error) {
            console.log('error', error);
        }
    }
    addToList(items){
        if(!items)
            return;
        if(Array.isArray(items) === false)
            items = [items];
        let list = document.getElementById('fdrive-list');
        console.log('notes add to list', items);
        for(let note of items)
        {
            let ele = this.createElement();
            ele.className += ' ' + note.mimeType;
            ele.setAttribute('x-uid', note.fullPath);
            ele.querySelector('.name').innerText = note.name;
            ele.querySelector('.enter').addEventListener('click', (e) => {
                if(note.mimeType === 'folder'){
                    this.changeFolder(note.fullPath);
                }
            })
            ele.querySelector('.download').addEventListener('click', (e) => {
                this.download(note.fullPath);
            })
            if(note.mimeType.startsWith('image'))
                ele.querySelector('img').src = 'files/media?path=' + encodeURIComponent(note.fullPath);
            else {
                ele.className += ' no-img';
                ele.querySelector('.icon').innerHTML = `<i class="${note.icon}" />`;
            }
            ele.querySelector('.size').innerText = humanizeFileSize(note.size);
            list.appendChild(ele);
        }
    }
}