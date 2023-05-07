class FenrusDrive {
    eleAddMenu;
    currentPath; // always ends with a / or empty for root
    lastReload;
    container;
    lblFiles;
    folderInfo = {};
    viewListHeight = 32;
    viewGridHeight = 240;
    
    addressBar;

    extensions = {
        plain: "clike",
        plaintext: "clike",
        text: "clike",
        txt: "clike",
        extend: "markup",
        insertBefore: "markup",
        dfs: "markup",
        markup: "markup",
        html: "html",
        mathml: "markup",
        svg: "markup",
        xml: "markup",
        ssml: "markup",
        atom: "markup",
        rss: "markup",
        css: "css",
        clike: "clike",
        javascript: "javascript",
        js: "javascript",
        bash: "bash",
        sh: "bash",
        shell: "bash",
        batch: "batch",
        c: "c",
        csharp: "csharp",
        cs: "csharp",
        csproj: "xml",
        sln: 'text',
        user: 'xml',
        dotnet: "markup",
        cpp: "cpp",
        csv: "csv",
        docker: "docker",
        dockerfile: "docker",
        dockerignore: 'docker',
        json: "json",
        webmanifest: "json",
        sql: "sql",
        typescript: "typescript",
        ts: "typescript",
        yaml: "yaml",
        yml: "yaml",
        gitignore: 'txt'
    };
    constructor() {
        this.currentFolderInfo = { 
            view: 'list',
            sort: 0
        };
        this.container =  document.getElementById('fdrive-list');
        this.currentPath = localStorage.getItem('DRIVE_FOLDER') || '';
        this.addressBar = new AddressBar();
        this.addressBar.onSearch((path, searchPattern) => {
            this.reload('/files/search?path=' + encodeURIComponent(path) + '&searchPattern=' + encodeURIComponent(searchPattern));
        });
        this.addressBar.onClick((path) => {
            this.changeFolder(path);
        })
        this.uploader = new FileUploader();
        this.uploader.onUploaded((event) => this.onUploaded(event));
        this.container.addEventListener('drop', (event) => this.uploader.handleFileDrop(event, this.currentPath));
        this.container.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
        }, false);
        this.fileList = new FileList(this.container);
        this.fileList.onFolderDblClick((folder) => {
            this.changeFolder(folder.fullPath);
        })
        this.fileList.onNewFolder(() => this.add('folder'));
        this.fileList.onUpload(() => this.add('upload'));
        this.fileList.onFileDblClick((file) =>
        {   
            if(file.mimeType.startsWith('image')) {
                this.slideshow.open(this.fileList.items, file);
                return;
            }
            let extension = file.fullPath.substring(file.fullPath.lastIndexOf('.') + 1).toLowerCase();
            if(this.extensions[extension] || extension === 'txt')
                this.openTextPreview('/files/download?path=' + encodeURIComponent(file.fullPath));
            else
                this.download(file.fullPath);
        });
        this.fileList.onFileDownload((file) => {
            this.download(file.fullPath);
        });
        this.fileList.onDelete((files) => {
            this.deleteFiles(files);
        });
        this.fileList.onRename(async (file) => {
            let newName = await modalPrompt('Rename', '', file.name, (input) => {
                if (/[<>:"\/\\|?*\x00-\x1F]/.test(input) === false)
                    return true;
                Toast.error('Error', 'Invalid characters.');
                return false;
            });
            if(!newName)
                return;
            let isFolder = file.mimeType === 'folder;'
            showBlocker();
            try{
                let result = await fetch('/files/rename', {
                    method: 'PUT', 
                    body: JSON.stringify({ path: file.fullPath, newName: newName }),
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });
                if(result.ok)
                {
                    let updated  = await result.json();
                    this.fileList.update(file, updated);
                    Toast.success(isFolder ? 'Folder Renamed' : 'File Renamed');
                }else {
                    let error = await result.text();
                    Toast.error(error || 'Failed to rename');
                }
            }catch(err){
                Toast.error(err || 'Failed to rename');                
            }
            hideBlocker();
        });
        this.fileList.onMove(async (destination, items) => {
            try {
                let result = await fetch('/files/move', {
                    method:'PUT',
                    body: JSON.stringify({
                        destination: destination,
                        items: items
                    }),
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });
                if(result.ok) {
                    await this.reload();
                    return;
                }
                Toast.error('Error', (await result.text()) || 'Failed to move');                
            }
            catch(err)
            {
                Toast.error('Error', err);
            } 
        });

        this.createToolbar();
        this.container.addEventListener('paste', (event) => this.pasteEventListener(event));
        this.lblFiles = document.querySelector('.fdrive-pane-title .title').innerText;
        this.eleAddMenu = document.getElementById('fdrive-add-menu');
        this.folderInfo = localStorage.getItem('DRIVE_FOLDER_INFO') || {};
        if (typeof (this.folderInfo) === 'string')
            this.folderInfo = JSON.parse(this.folderInfo);
        if (typeof (this.folderInfo) !== 'object')
            this.folderInfo = {};
        // if (this.folderInfo[''])
        //     this.changeView(this.folderInfo['']);

        this.slideshow = new SlideShow();
    }

    show() {
        this.loadFolder(this.currentPath);
    }

    changeFolder(path) {
        localStorage.setItem('DRIVE_FOLDER', path);
        this.loadFolder(path);
    }
    
    getFolderInfo(){
        let info = this.folderInfo[this.currentPath || ''];
        if(info)
            return info;
        
        info = {};
        this.folderInfo[this.currentPath || ''] = info;
        return info;
    }
    
    saveFolderInfo()
    {
        this.currentFolderInfo = this.getFolderInfo();
        localStorage.setItem('DRIVE_FOLDER_INFO', JSON.stringify(this.folderInfo));        
    }
    
    changeView(view) {
        let info = this.getFolderInfo();
        info.view = view;
        this.saveFolderInfo();
        let viewInfo = this.fileList.changeView(view);
        if(viewInfo)
            this.viewIcon.className = 'view-icon fa-solid ' + viewInfo.icon; 
    }
    
    nextView(){
        let view = this.fileList.nextView();
        this.viewIcon.className = 'view-icon fa-solid ' + view.icon;
        let info = this.getFolderInfo();
        info.view = view.name;
        this.saveFolderInfo();
    }


    download(path) {
        var link = document.createElement("a");
        let url = '/files/download?path=' + encodeURIComponent(path);
        link.href = url;
        link.download = url.split("/").pop();
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
    loadFolder(path) {
        let currentFolderInfo = this.currentFolderInfo;
        this.fileList.clear();
        path = path || '';
        this.addressBar.show(path);
        if (path.endsWith('/.'))
            path = path.substring(0, path.length - 1);
        this.currentPath = path;
        
        let info = this.getFolderInfo();
        info.view = info.view || currentFolderInfo.view;
        info.sort = info.sort || currentFolderInfo.sort || this.fileList.sorts[0].name;

        this.changeView(this.folderInfo[path].view);
        let radio = document.querySelector('.file-sort-' + info.sort.toLowerCase().replace(/\s/, '-'));
        if(radio)
            radio.click();
        this.reload();
    }

    async deleteFiles(files) {
        if(!files?.length)
            files = this.getSelectedUids();

        let confirmed = await modalConfirm('Delete', 'Are you sure you want to delete the selected file' + (files.length === 1 ? '' : 's') +'?');
        if (!confirmed)
            return;
        showBlocker('Deleting file(s)');
        try {
            let result = await fetch('/files', {
                method: 'delete',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({files: files})
            });
            hideBlocker();
            if(result.ok) {
                Toast.success('Deleted');
                await this.reload();
            }
            else
            {
                let msg = await result.text();
                Toast.error(msg || 'Failed to delete file(s)');
            }
        } catch (err) {
            hideBlocker();
            Toast.error(err);
        }
    }

    upload() {
        var input = document.createElement("input");
        input.type = "file";
        input.style.display = "none";
        input.name = "file";
        input.multiple = true;
        document.body.appendChild(input);

        input.addEventListener("change", () => {
            if (!input.files.length)
                return;
            var formData = new FormData();
            let count = 0;
            let size = 0;
            for (let file of input.files) {
                formData.append('file', file);
                count++;
                size += file.size;
            }
            if (count > 0)
                this.uploadForm(formData, count, size);
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
        let count = 0;
        let size = 0;
        for (let i = 0; i < cbPayload.length; i++) {
            let file = cbPayload[i].getAsFile();
            if (!file)
                continue;
            formData.append('file', file);
            size += file.size;
            count++;
        }
        this.uploadForm(formData, count, size);
    }


    onUploaded(event) {
        console.log('onUploaded', event, this.currentPath);
        if(event?.path === this.currentPath) {
            if(!document.querySelector(`.file[x-uid='${event.file.fullPath}']`))
                this.fileList.addItems([event.file]);
            return;
        }
        if(event?.path?.startsWith(this.currentPath) === false)
            return;
        // its a subfolder.  see if this subfolder exists
        let folderName = event.path.substring(this.currentPath.length);
        if(folderName.indexOf('/') > 0)
            folderName = folderName.substring(0, folderName.indexOf('/'));
        let fullPath = this.currentPath + folderName + '/';
        
        if(document.querySelector(`.file[x-uid='${fullPath}']`))
            return; // it exists

        this.fileList.addItems([{
            created: new Date(),
            extension: '',
            fullPath: fullPath,
            icon: "fa-solid fa-folder",
            mimeType: "folder",
            name: folderName,
            size: 0
        }]);
    }

    addClicked() {
        this.eleAddMenu.className = /visible/.test(this.eleAddMenu.className) ? '' : 'visible';
    }

    async add(mode) {
        this.eleAddMenu.className = '';
        if (mode === 'upload')
            this.upload();
        else if (mode === 'folder') {
            var name = await modalPrompt('New Folder', 'Enter a name of the the folder to create');
            if (/[~"#%&*:<>?/\\{|}]+/.test(name))
                return; // invalid
            if (!name)
                return;
            if (!this.currentPath) this.currentPath = '';
            let fullPath = name;
            if (this.currentPath) {
                if (this.currentPath.endsWith('/'))
                    fullPath = this.currentPath + name;
                else
                    fullPath = this.currentPath + '/' + name;
            }
            try {
                let result = await fetch('/files/create-folder?path=' + encodeURIComponent(fullPath), {method: 'POST'});
                if(result.ok) {
                    Toast.success('Folder created');
                    this.reload();
                }
                else
                {
                    let msg = await result.text(); 
                    Toast.error(msg || 'Failed to create folder');
                }
            }catch(err){
                Toast.error(err);
            }
        }
    }

    async uploadForm(formData, fileCount, size) {
        if (size > 10_737_418_240) {
            await modalMessage('Exceeds Limit', 'The file(s) you are attempting to upload exceed to 10GB limit.');
            return;
        }
        showBlocker('Uploading');
        try {
            let eleProgress = document.querySelector('.blocker-message');
            await this.uploadFileWithProgress(formData, eleProgress);
            await this.reload();
        } catch (error) {
            await modalMessage('Error', error || 'Unexpected upload error');
            console.error(error);
        }
        hideBlocker();
    }


    async reload(url) {
        try {
            url = url || '/files?path=' + encodeURI(this.currentPath || '');
            const response = await fetch(url);
            const data = await response.json();
            for(let d of data)
            {
                if(typeof(d.created) === 'string')
                    d.created = new Date(d.created);
                if(typeof(d.modified) === 'string')
                    d.modified = new Date(d.modified);
            }
            this.fileList.setItems(data);
            this.lastReload = new Date();
        } catch (error) {
            console.log('error', error);
        }
    }

    

    async openTextPreview(url) {
        try {
            const response = await fetch(url);
            if (!response.ok) {
                throw new Error("Network response was not ok");
            }

            let filename = this.getFileNameFromContentDisposition(response.headers.get("content-disposition"));
            const fileSize = response.headers.get("content-length");

            const previewContainer = document.createElement("div");
            previewContainer.className = 'fdrive-text-preview-container fdrive-preview';

            const previewBox = document.createElement("div");
            previewBox.classList.add("fdrive-text-preview-box");

            const header = document.createElement("div");
            header.classList.add("fdrive-text-preview-header");

            const text = await response.text();
            
            const filenameLink = document.createElement("a");
            filenameLink.classList.add("fdrive-text-preview-filename");
            filenameLink.innerText = filename;
            filenameLink.innerHTML = '<i class="fas fa-download"></i>' + filenameLink.innerHTML;
            filenameLink.addEventListener('click', () => {
                this.download(url);
            })
            header.appendChild(filenameLink);

            const close = () => {
                document.body.removeChild(previewContainer);
                document.removeEventListener("keydown",keyDownListener);
            }
            const keyDownListener = (event) => {
                if (event.key === "Escape") {
                    close();
                }
            };


            const closeSpan = document.createElement("span");
            closeSpan.classList.add("fdrive-text-preview-close");
            closeSpan.innerHTML = '<i class="fas fa-times"></i>';
            closeSpan.addEventListener("click", () => {
                close();
            });
            header.appendChild(closeSpan);

            const footer = document.createElement("div");
            footer.classList.add("fdrive-text-preview-footer");

            if (location.protocol === "https:") {
                const copySpan = document.createElement("span");
                copySpan.classList.add("fdrive-text-preview-copy");
                copySpan.innerHTML = '<i class="fa-solid fa-copy"></i> Copy';
                copySpan.addEventListener('click', async() => {
                    try {
                        await navigator.clipboard.writeText(text);
                        Toast.info('Copied to clipboard');
                    } catch (err) {
                        console.error('Failed to copy text: ', err);
                    }
                });
                footer.appendChild(copySpan);                
            }

            const fileSizeSpan = document.createElement("span");
            fileSizeSpan.classList.add("fdrive-text-preview-filesize");
            fileSizeSpan.innerText = humanizeFileSize(fileSize);
            footer.appendChild(fileSizeSpan);

            const textContainer = document.createElement("div");
            textContainer.classList.add("fdrive-text-preview-text-container");
            
            this.highlightCode(filename, text, textContainer);

            previewBox.appendChild(header);
            previewBox.appendChild(textContainer);
            previewBox.appendChild(footer);
            previewContainer.appendChild(previewBox);

            document.body.appendChild(previewContainer);
            document.addEventListener("keydown", keyDownListener);
            
        } catch (error) {
            console.error("There was a problem with the text preview", error);
        }
    }


    highlightCode(filename, text, textContainer) {
        const extension = filename.substring(filename.lastIndexOf('.') + 1).toLowerCase();

        if (this.extensions[extension]) {
            textContainer.innerHTML = `<pre><code class="language-${this.extensions[extension]}">${Prism.highlight(text, Prism.languages[this.extensions[extension]], this.extensions[extension])}</code></pre>`;
        } else {
            textContainer.textContent = text;
        }
    }


    getFileNameFromContentDisposition(contentDisposition) {
        if (!contentDisposition) {
            return null;
        }

        const filenameRegex = /filename[^;=\n]*=((['"]).*?\2|[^;\n]*)/;
        const matches = contentDisposition.match(filenameRegex);

        if (!matches) {
            return null;
        }

        let filename = matches[1];

        if (filename.startsWith('"') && filename.endsWith('"')) {
            filename = filename.slice(1, -1);
        }

        filename = decodeURIComponent(filename);

        return filename;
    }
    
    createToolbar()
    {
        let toolbar = document.querySelector('#fdrive-files .toolbar');
        if(!toolbar)
            return;
        toolbar.appendChild(this.createSorter());
    }
    
    createSorter()
    {
        let eleSorter = document.createElement('div');
        let sorts = '';
        for(let sort of this.fileList.sorts){
            sorts +='<label><input type="radio" name="sort-by" ' +
                ' class="file-sort-' +htmlEncode(sort.name.toLowerCase().replace(/\s/, '-')) + '" ' +
                ' value="' +htmlEncode(sort.name.toLowerCase()) + '" ' +
                '>' + htmlEncode(sort.name) + '</label>';
        }
        eleSorter.innerHTML = '\n' +
            '  <div class="fdrive-sorter">\n' +
            '    <div class="dropdown">\n' +
            '      <button class="dropdown-toggle">\n' +
            '        <i class="view-icon fa-solid fa-list"></i>\n' +
            '        <i class="caret fas fa-caret-down"></i>\n' +
            '      </button>\n' +
            '      <div class="dropdown-menu">\n' +
            '        <span class="dropdown-label">Sort</span>\n' +            
            '        <div class="radio-group">' + sorts + '</div>' +
            '        <div class="advanced">' +
            '           <label><input type="checkbox" class="show-hidden" />Show Hidden</label>'      
            '        </div>' + 
            '      </div>\n' +
            '    </div>\n' +
            '  </div>';
        let eleMenu = eleSorter.querySelector('.dropdown-menu');
        eleSorter.querySelector('.dropdown-toggle .caret').addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation();
            
            if(eleMenu.classList.contains('visible'))
                eleMenu.classList.remove('visible');
            else
                eleMenu.classList.add('visible');
        });
        document.addEventListener('click', () => {
            eleMenu.classList.remove('visible');
        });
        
        let eleShowHidden = eleSorter.querySelector('.show-hidden');
        let showHidden = localStorage.getItem('DRIVE_SHOW_HIDDEN') === '1';
        this.fileList.setShowHidden(showHidden);
        eleShowHidden.checked = showHidden;
        eleSorter.querySelector('.advanced').addEventListener('click', (e) => {
            e.stopPropagation();
        });
        eleShowHidden.addEventListener('change', (e) => {
           showHidden = e.target.checked;
           localStorage.setItem('DRIVE_SHOW_HIDDEN', showHidden ? '1' : '0');
           this.fileList.setShowHidden(showHidden);
        });
        
        
        let count = 0;
        for(let radio of eleSorter.querySelectorAll('input[type=radio]')) {
            let index = count++;
            radio.addEventListener('change', (event) => {
                let info = this.getFolderInfo();
                info.sort = this.fileList.sort(index);
                this.saveFolderInfo();
            });
        }
        
        this.viewIcon = eleSorter.querySelector('.view-icon');
        this.viewIcon.addEventListener('click', () =>{
            this.nextView();            
        });
        return eleSorter;
    }
}