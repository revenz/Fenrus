class FenrusDrive {
    eleAddMenu;
    currentPath; // always ends with a / or empty for root
    lastReload;
    container;
    lblFiles;
    currentView;
    uploader;
    folderViews = {};
    virtualizeList;
    slideshow;
    viewListHeight = 32;
    viewGridHeight = 240;
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
        dotnet: "markup",
        cpp: "cpp",
        csv: "csv",
        docker: "docker",
        dockerfile: "docker",
        json: "json",
        webmanifest: "json",
        sql: "sql",
        typescript: "typescript",
        ts: "typescript",
        yaml: "yaml",
        yml: "yaml"
    };
   
    fileTypeIcons =['3g2','3ga','3gp','7z','aa','aac','ac','accdb','accdt','ace','adn','ai','aif',
        'aifc','aiff','ait','amr','ani','apk','app','applescript','asax','asc','ascx','asf','ash','ashx','asm',
        'asmx','asp','aspx','asx','au','aup','avi','axd','aze','bak','bash','bat','bin','blank','bmp','bowerrc',
        'bpg','browser','bz2','bzempty','c','cab','cad','caf','cal','cd','cdda','cer','cfg','cfm','cfml','cgi',
        'chm','class','cmd','code','coffee','coffeelintignore','com','compile','conf','config','cpp','cptx','cr2',
        'crdownload','crt','crypt','cs','csh','cson','csproj','css','csv','cue','cur','dart','dat','data','db',
        'dbf','deb','default','dgn','dist','diz','dll','dmg','dng','doc','docb','docm','docx','dot','dotm','dotx','download','dpj','ds_store','dsn','dtd','dwg','dxf','editorconfig','el','elf','eml','enc','eot','eps','epub','eslintignore','exe','f4v','fax','fb2','fla','flac','flv','fnt','folder','fon','gadget','gdp','gem','gif','gitattributes','gitignore','go','gpg','gpl','gradle','gz','h','handlebars','hbs','heic','hlp','hs','hsl','htm','html','ibooks','icns','ico','ics','idx','iff','ifo','image','img','iml','in','inc','indd','inf','info','ini','inv','iso','j2','jar','java','jpe','jpeg','jpg','js','json','jsp','jsx','key','kf8','kmk','ksh','kt','kts','kup','less','lex','licx','lisp','lit','lnk','lock','log','lua','m','m2v','m3u','m3u8','m4','m4a','m4r','m4v','map','master','mc','md','mdb','mdf','me','mi','mid','midi','mk','mkv','mm','mng','mo','mobi','mod','mov','mp2','mp3','mp4','mpa','mpd','mpe','mpeg','mpg','mpga','mpp','mpt','msg','msi','msu','nef','nes','nfo','nix','npmignore','ocx','odb','ods','odt','ogg','ogv','ost','otf','ott','ova','ovf','p12','p7b','pages','part','pcd','pdb','pdf','pem','pfx','pgp','ph','phar','php','pid','pkg','pl','plist','pm','png','po','pom','pot','potx','pps','ppsx','ppt','pptm','pptx','prop','ps','ps1','psd','psp','pst','pub','py','pyc','qt','ra','ram','rar','raw','rb','rdf','rdl','reg','resx','retry','rm','rom','rpm','rpt','rsa','rss','rst','rtf','ru','rub','sass','scss','sdf','sed','sh','sit','sitemap','skin','sldm','sldx','sln','sol','sphinx','sql','sqlite','step','stl','svg','swd','swf','swift','swp','sys','tar','tax','tcsh','tex','tfignore','tga','tgz','tif','tiff','tmp','tmx','torrent','tpl','ts','tsv','ttf','twig','txt','udf','vb','vbproj','vbs','vcd','vcf','vcs','vdi','vdx','vmdk','vob','vox','vscodeignore','vsd','vss','vst','vsx','vtx','war','wav','wbk','webinfo','webm','webp','wma','wmf','wmv','woff','woff2','wps','wsf','xaml','xcf','xfl','xlm','xls','xlsm','xlsx','xlt','xltm','xltx','xml','xpi','xps','xrb','xsd','xsl','xspf','xz','yaml','yml','z','zip','zsh'];
    addressBar;

    constructor() {
        this.container =  document.getElementById('fdrive-list');;
        this.addressBar = new AddressBar();
        this.addressBar.onClick((path) => {
            this.changeFolder(path);
        })
        
        this.container.addEventListener('mousedown', (e) => {            
            //if (!fDriveDrawer.isFiles) return;
            this.containerOnMouseDown(e);
        });
        this.uploader = new FileUploader();
        this.uploader.onUploaded((event) => this.onUploaded(event));
        this.container.addEventListener('drop', (event) => this.uploader.handleFileDrop(event, this.currentPath));
        this.container.addEventListener('dragover', (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = 'copy';
        }, false);
        this.createToolbar();
        this.container.addEventListener('paste', (event) => this.pasteEventListener(event));
        this.lblFiles = document.querySelector('.fdrive-pane-title .title').innerText;
        this.eleAddMenu = document.getElementById('fdrive-add-menu');
        this.folderViews = localStorage.getItem('DRIVE_FOLDER_VIEWS') || {};
        if (typeof (this.folderViews) === 'string')
            this.folderViews = JSON.parse(this.folderViews);
        if (typeof (this.folderViews) !== 'object')
            this.folderViews = {};
        if (this.folderViews[''])
            this.changeView(this.folderViews[''], true);
        
        this.virtualizeList = new VirtualizeList(document.getElementById('fdrive-list'));
        this.virtualizeList.setItemHeight(this.viewListHeigh);
        this.virtualizeList.setNumColumns(1);
        document.body.addEventListener('driveResizeEvent', (event) => {
            this.virtualizeList.calculateColumns();
        })

        this.slideshow = new SlideShow();
    }

    show() {
        this.loadFolder(this.currentPath);
    }

    changeFolder(path) {
        console.log('changing path: ' + path);
        this.loadFolder(path);
    }

    createElement() {
        let ele = document.createElement('div');
        ele.className = 'file';
        ele.innerHTML = '<div class="file-inner">' +
            '<span class="icon"><img /></span>' +
            '<span class="name"></span>' +
            '<span class="size"></span>' +
            '<span class="enter"><i class="fa-solid fa-chevron-right"></i></span>' +
            '<span class="download"><i class="fa-solid fa-download"></i></span>' +
            '</div>';
        return ele;
    }

    checkAll(checked){
        for(let ele of this.container.querySelectorAll('input[type=checkbox]')){
            ele.checked = checked;
        }
    }

    changeView(view, noSave) {
        console.log('changing views', view);
        if (!noSave)
            this.folderViews[this.currentPath || ''] = view;
        localStorage.setItem('DRIVE_FOLDER_VIEWS', JSON.stringify(this.folderViews));
        document.getElementById('fdrive-list').className = 'content ' + view;
        this.currentView = view;
        let isGrid = view === 'grid';
        if(this.viewIcon) {
            this.viewIcon.classList.remove('fa-grip');
            this.viewIcon.classList.remove('fa-list');
            this.viewIcon.classList.add(isGrid ? 'fa-grip' : 'fa-list');
        }
        if(this.virtualizeList) {
            this.virtualizeList.changeLayout(isGrid ? 0 : 1, isGrid ? this.viewGridHeight : this.viewListHeight);
        }
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
        this.virtualizeList.clear();
        path = path || '';
        this.addressBar.show(path);
        if (path.endsWith('/.'))
            path = path.substring(0, path.length - 1);
        let lbl = path;
        let hasParts = !!lbl;
        lbl = !lbl ? this.lblFiles : lbl;
        let eleTitle = document.querySelector('.fdrive-pane-title .title');
        eleTitle.innerHTML = '';
        let parts = lbl.split(/\//);
        for(let part of parts){
            if(hasParts) {
                let eleSeparator = document.createElement('span');
                eleSeparator.className = 'path-separator';
                eleSeparator.innerText = '/';
                eleTitle.appendChild(eleSeparator);
            }
            
            let elePart = document.createElement('span');
            elePart.innerText = part;
            eleTitle.appendChild(elePart);
        }
        this.currentPath = path;

        if (this.folderViews[path])
            this.changeView(this.folderViews[path], true);
        this.reload();
    }

    async deleteFiles() {
        let files = this.getSelectedUids();

        let confirmed = await modalConfirm('Delete', 'Are you sure you want to delete the selected file(s) ?');
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
                this.addToList([event.file]);
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
        
        this.addToList([{
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


    async reload() {
        try {
            let url = '/files?path=' + encodeURI(this.currentPath || '');
            const response = await fetch(url);
            const data = await response.json();
            this.virtualizeList.clear();
            
            this.addToList(data);
            this.lastReload = new Date();
        } catch (error) {
            console.log('error', error);
        }
    }

    addToList(items) {
        if (!items)
            return;
        if (Array.isArray(items) === false)
            items = [items];
        let count = 0;
        this.virtualizeList.startBulkUpdates();
        for (let item of items) {
            let ele = this.createElement();
            ele.className += ' ' + item.mimeType;
            ele.setAttribute('x-uid', item.fullPath);
            ele.querySelector('.name').innerText = item.name;
            ele.querySelector('.enter').addEventListener('click', (e) => {
                if (item.mimeType === 'folder') {
                    this.changeFolder(item.fullPath);
                }
            })
            ele.querySelector('.download').addEventListener('click', (e) => {
                this.download(item.fullPath);
            })
            if (item.mimeType.startsWith('image')) {
                let img = ele.querySelector('img');
                img.className = 'media';
                img.setAttribute('data-src', 'files/media?path=' + encodeURIComponent(item.fullPath));
                img.src = 'images/placeholder.svg';
                img.classList.add('lazy');
                img.setAttribute('x-filename', item.name);
                ele.classList.add('image');
                ele.addEventListener('dblclick', () => {
                    this.slideshow.open(this.virtualizeList.items, ele);
                })
            } else {
                ele.className += ' no-img';
                let extension = item.fullPath.substring(item.fullPath.lastIndexOf('.') + 1).toLowerCase();
                if(this.fileTypeIcons.indexOf(extension) >= 0)
                    ele.querySelector('.icon').innerHTML = `<img src="images/filetypes/${extension}.svg" />`;
                else if(item.mimeType === 'folder')
                    ele.querySelector('.icon').innerHTML = `<img src="images/filetypes/folder.svg" />`;
                else
                    ele.querySelector('.icon').innerHTML = `<img src="images/filetypes/blank.svg" />`;

                ele.addEventListener('dblclick', () => {
                    if (item.mimeType === 'folder')
                        this.changeFolder(item.fullPath);
                    else if(this.extensions[extension] || extension === 'txt')
                        this.openTextPreview('/files/download?path=' + encodeURIComponent(item.fullPath));
                    else
                        this.download(item.fullPath);
                });
            }
            ele.querySelector('.size').innerText = humanizeFileSize(item.size);
            this.virtualizeList.addItem(ele, true);
            ++count;
        }
        this.virtualizeList.completeUpdates();        
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

            const keyDownListener = (event) => {
                if (event.key === "Escape") {
                    document.body.removeChild(previewContainer);
                }
            };

            const close = () => {
                document.body.removeChild(previewContainer);
                document.removeEventListener("keydown",keyDownListener);
            }
            
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

    async containerOnMouseDown(event) {
        // Record the starting position of the selection
        const selectionStartX = event.clientX;
        const selectionStartY = event.clientY;
        

        let container = this.container;
        let overlay = document.createElement('div');
        this.overlay = overlay;
        
        let onMouseMove = (event) => {
            // Update the selection end position as the mouse is moved
            const selectionEndX = event.clientX;
            const selectionEndY = event.clientY;

            // Set the dimensions of the overlay to match the selection area
            overlay.style.left = Math.min(selectionStartX, selectionEndX) + 'px';
            overlay.style.top = Math.min(selectionStartY, selectionEndY) + 'px';
            overlay.style.width = Math.abs(selectionEndX - selectionStartX) + 'px';
            overlay.style.height = Math.abs(selectionEndY - selectionStartY) + 'px';
        };
        overlay.style.position = 'fixed';
        overlay.style.top = selectionStartX + 'px';
        overlay.style.left = selectionStartY + 'px';
        overlay.style.width = '1px';
        overlay.style.height = '1px';
        overlay.style.backgroundColor = 'rgba(255, 255, 255, 0.1)';
        container.appendChild(overlay);

        document.addEventListener('mousemove', onMouseMove);        
        let onMouseUp = (event) => {

            // Remove the mouse event listeners
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);

            // Remove the overlay element
            if(overlay)
                overlay.remove();
            overlay = null;
            let control = event.ctrlKey === true;
            let shift = event.shiftKey === true;

            let overlayLeft = Math.min(selectionStartX, event.clientX);
            let overlayRight = Math.max(selectionStartX, event.clientX);
            let overlayTop =  Math.min(selectionStartY, event.clientY);
            let overlayBottom = Math.max(selectionStartY, event.clientY);
            // Find all elements that intersect with the selection area
            const selectedElements = Array.from(container.querySelectorAll('.file')).filter((element) => {
                const rect = element.getBoundingClientRect();
                if(rect.bottom <= overlayTop)
                    return false;
                if(rect.top >= overlayBottom)
                    return false;
                if(rect.left >= overlayRight)
                    return false;
                if(rect.right <= overlayLeft)
                    return false;
                return true;
            });
            
            let single = selectedElements.length === 1;
            
            if(this.lastSelected && shift && single&& this.lastSelected !== selectedElements[0]){
                // select everything in between
                let lastIndex = this.virtualizeList.items.indexOf(this.lastSelected);
                let thisIndex = this.virtualizeList.items.indexOf(selectedElements[0]);
                let start = Math.min(lastIndex, thisIndex);
                let end = Math.max(lastIndex, thisIndex) + 1;
                this.lastSelected = null;
                this.setSelected(this.virtualizeList.items.slice(start, end), true);
                return;
            }
            if(single && control && selectedElements[0].classList.contains('selected'))
            {
                // toggle off
                selectedElements[0].classList.remove('selected')
                this.lastSelected = null;
                return;                
            }

            this.setSelected(selectedElements, control);
            this.lastSelected = single ? selectedElements[0] : null;
        };

        document.addEventListener('mouseup', onMouseUp);
    }
    
    setSelected(items, append)
    {
        if(!items)
            items = [];        
        else if(Array.isArray(items) === false)
            items = [items];
        
        let all = this.container.querySelectorAll('.file');
        for(let item of all)
        {
            let selected = item.classList.contains('selected');
            if(items.indexOf(item) >= 0)
            {
                if(selected)
                    continue;
                item.classList.add('selected');
            }else if(!append){
                if(!selected)
                    continue;
                item.classList.remove('selected');
            }
        }
    }

    getSelectedUids()
    {
        let selected = this.virtualizeList.items.filter(x => x.classList.contains('selected'))
            .map((x) => {
                return x.getAttribute('x-uid');
            });
                
        console.log('selected', selected);
        return selected;
    }
    
    createToolbar()
    {
        let toolbar = document.querySelector('#fdrive-files .view-mode');
        if(!toolbar)
            return;
        toolbar.innerHTML = '';
        toolbar.appendChild(this.createSorter());
    }
    
    createSorter()
    {
        let eleSorter = document.createElement('div');
        eleSorter.innerHTML = '\n' +
            '  <div class="fdrive-sorter">\n' +
            '    <div class="dropdown">\n' +
            '      <button class="dropdown-toggle">\n' +
            '        <i class="view-icon fa-solid fa-list"></i>\n' +
            '        <i class="caret fas fa-caret-down"></i>\n' +
            '      </button>\n' +
            '      <div class="dropdown-menu">\n' +
            '        <span class="dropdown-label">Sort</span>\n' +            
            '        <div class="radio-group">\n' +
            '          <label><input type="radio" name="sort-by" value="name" checked>A-Z</label>' +
            '          <label><input type="radio" name="sort-by" value="name" checked>Z-A</label>' +
            '          <label><input type="radio" name="sort-by" value="name" checked>Last Modified</label>' +
            '          <label><input type="radio" name="sort-by" value="name" checked>First Modified</label>' +
            '          <label><input type="radio" name="sort-by" value="name" checked>Size</label>' +
            '          <label><input type="radio" name="sort-by" value="name" checked>Type</label>' +
            '        </div>\n' +
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
        
        this.viewIcon = eleSorter.querySelector('.view-icon');
        this.viewIcon.addEventListener('click', () =>{
            let currentView = this.viewIcon.classList.contains('fa-list') ? 'list' : 'grid';
            this.changeView(currentView === 'list' ? 'grid' : 'list');            
        });
        return eleSorter;
    }
}