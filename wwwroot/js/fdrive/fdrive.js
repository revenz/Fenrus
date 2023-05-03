class FenrusDrive {
    eleAddMenu;
    filePaths = [];
    currentPath;
    lastReload;
    container;
    lblFiles;
    currentView;
    folderViews = {};
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

    virtualizeList;
    
    constructor() {
        let container = document.getElementById('fdrive-list');
        this.container = container;
        this.container.addEventListener('mousedown', (e) => {
            if (!fDriveDrawer.isFiles || e.button !== 2) return;
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
        if (typeof (this.folderViews) === 'string')
            this.folderViews = JSON.parse(this.folderViews);
        if (typeof (this.folderViews) !== 'object')
            this.folderViews = {};
        if (this.folderViews[''])
            this.changeView(this.folderViews[''], true);
        
        this.virtualizeList = new VirtualizeList(document.getElementById('fdrive-list'));
        this.virtualizeList.setItemHeight(40);
        this.virtualizeList.setNumColumns(1);
        
        document.body.addEventListener('driveResizeEvent', (event) => {
            this.virtualizeList.calculateColumns();
        })
    }

    show() {
        this.reload();
    }

    changeFolder(path) {
        this.filePaths.push(path);
        this.loadFolder(path);
    }

    createElement() {
        let ele = document.createElement('div');
        ele.className = 'file';
        ele.innerHTML = '<input class="check" type="checkbox" />' +
            '<span class="icon"><img /></span>' +
            '<span class="name"></span>' +
            '<span class="size"></span>' +
            '<span class="enter"><i class="fa-solid fa-chevron-right"></i></span>' +
            '<span class="download"><i class="fa-solid fa-download"></i></span>';
        return ele;
    }

    checkAll(checked){
        for(let ele of this.container.querySelectorAll('input[type=checkbox]')){
            ele.checked = checked;
        }
    }

    changeView(view, noSave) {
        if (!noSave)
            this.folderViews[this.currentPath || ''] = view;
        localStorage.setItem('DRIVE_FOLDER_VIEWS', JSON.stringify(this.folderViews));
        document.getElementById('fdrive-list').className = 'content ' + view;
        this.currentView = view;
        if(this.virtualizeList) {
            this.virtualizeList.setItemHeight(view === 'thumbnail' ? 240 : 40);
            this.virtualizeList.setNumColumns(view === 'thumbnail' ? 3 : 1);
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

    changeFolder(path) {
        path = path || '';
        if (path.endsWith('/.'))
            path = path.substring(0, path.length - 1);
        if (path)
            this.filePaths.push(path);
        this.loadFolder(path);
    }

    loadFolder(path) {
        path = path || '';
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


    parentPath() {
        let path = this.filePaths.pop(); // gets rid of current, we want the next
        if (!path)
            return;

        path = this.filePaths[this.filePaths.length - 1];
        this.loadFolder(path);
    }

    async deleteFiles() {
        let checked = document.querySelectorAll('#fdrive-list .file .check:checked');
        if (!checked.length)
            return;
        let files = [];
        for (let chk of checked)
            files.push(chk.closest('.file').getAttribute('x-uid'));

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

    fileDropEventListener(event) {
        event.preventDefault();
        const files = event.dataTransfer.files;
        const formData = new FormData();

        let size = 0;
        let filesAdded = 0;
        for (let i = 0; i < files.length; i++) {
            formData.append('file', files[i]);
            size += files[i].size;
            ++filesAdded;
        }
        if (filesAdded === 0)
            return;
        this.uploadForm(formData, filesAdded, size);
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

    uploadFileWithProgress(formData, progressDiv) {
        return new Promise(async (resolve, reject) => {
            const url = '/files/path?path=' + encodeURIComponent(this.currentPath || '');

            const xhr = new XMLHttpRequest();

            xhr.upload.addEventListener('progress', (event) => {
                if (event.lengthComputable) {
                    const percentComplete = (event.loaded / event.total) * 100;
                    progressDiv.textContent = percentComplete.toFixed(0) + '%';
                }
            });

            xhr.onreadystatechange = () => {
                if (xhr.readyState === 4) {
                    if (xhr.status === 200) {
                        progressDiv.textContent = '100%';
                        resolve(xhr.responseText);
                    } else {
                        reject(new Error('Network response was not ok'));
                    }
                }
            };

            xhr.open('POST', url, true);
            xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
            xhr.send(formData);
        });
    }

    async reload() {
        try {
            let url = '/files/path?path=' + encodeURI(this.currentPath || '');
            const response = await fetch(url);
            const data = await response.json();
            if (this.currentPath)
                data.unshift({
                    extension: '',
                    fullPath: '..',
                    icon: "fa-solid fa-arrow-turn-up",
                    mimeType: 'parent',
                    name: '..',
                    size: 0
                });
            //let list = document.getElementById('fdrive-list');
            //list.innerHTML = '';
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
                ele.addEventListener('dblclick', () => {
                    this.openSlideshow(ele);
                })
            } else {
                ele.className += ' no-img';
                let extension = item.fullPath.substring(item.fullPath.lastIndexOf('.') + 1).toLowerCase();
                if(this.fileTypeIcons.indexOf(extension) >= 0)
                    ele.querySelector('.icon').innerHTML = `<img src="images/filetypes/${extension}.svg" />`;
                else if(item.mimeType === 'folder' || item.mimeType === 'parent')
                    ele.querySelector('.icon').innerHTML = `<img src="images/filetypes/folder.svg" />`;
                else
                    ele.querySelector('.icon').innerHTML = `<img src="images/filetypes/blank.svg" />`;

                ele.addEventListener('dblclick', () => {
                    if (item.mimeType === 'parent')
                        this.parentPath();
                    else if (item.mimeType === 'folder')
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


    openSlideshow( startChild) {
        // Get all the child elements of the container
        let childElements = [];
        for(let ele of this.container.children){
            if(ele.querySelector('img.media'))
                childElements.push(ele);
        }        
        
        const numChildren = childElements.length;

        // Find the index of the start child
        let startIndex = 0;
        for (let i = 0; i < numChildren; i++) {
            if (childElements[i] === startChild) {
                startIndex = i;
                break;
            }
        }

        let dimensions, filename, infoContainer;
        let updateDimensions = () => {
            dimensions.textContent = `${image.naturalWidth} x ${image.naturalHeight}`;
            let maxWidth = document.body.clientWidth * 0.8;                
            infoContainer.style.width =  Math.max(100, (((image.naturalWidth > maxWidth) ? maxWidth : image.naturalWidth) - 30) )+ 'px';
        };
        let closeSlideshow = () => {
            slideshowDiv.remove();
            document.removeEventListener('keydown', keyDownEvent);
        }
        // Function to go to the previous image        
        let prevImage = () => {
            startIndex--;
            if (startIndex < 0) {
                startIndex = numChildren - 1;
            }
            const prevImage = childElements[startIndex].querySelector("img");
            image.src = prevImage.getAttribute('data-src');
            filename.textContent = prevImage.getAttribute('x-filename');
        }

        // Function to go to the next image
        let nextImage = () => {
            startIndex++;
            if (startIndex >= numChildren) {
                startIndex = 0;
            }
            const nextImage = childElements[startIndex].querySelector("img");
            image.src = nextImage.getAttribute('data-src');
            filename.textContent = nextImage.getAttribute('x-filename');
        }

        let keyDownEvent = (event) => {
            if (event.key === 'ArrowLeft')
                prevImage();
            else if (event.key === 'ArrowRight')
                nextImage();
            else if (event.key === 'Escape' || event.key === 'Backspace')
                closeSlideshow();
        }

        document.addEventListener('keydown', keyDownEvent);

        // Create slideshow elements
        const slideshowDiv = document.createElement("div");
        slideshowDiv.className = "fdrive-slideshow fdrive-preview";

        const backgroundDiv = document.createElement("div");
        backgroundDiv.className = "slideshow-background";
        backgroundDiv.addEventListener("click", closeSlideshow);
        slideshowDiv.appendChild(backgroundDiv);

        const imageContainer = document.createElement("div");
        imageContainer.className = "slideshow-image-container";
        slideshowDiv.appendChild(imageContainer);

        const closeButton = document.createElement("div");
        closeButton.className = "slideshow-close";
        closeButton.innerHTML = "<i class=\"fa-solid fa-xmark\"></i>";
        closeButton.addEventListener("click", closeSlideshow);
        slideshowDiv.appendChild(closeButton);

        const prevButton = document.createElement("div");
        prevButton.className = "slideshow-prev";
        prevButton.innerHTML = "&#8249;";
        prevButton.addEventListener("click", prevImage);
        slideshowDiv.appendChild(prevButton);

        const nextButton = document.createElement("div");
        nextButton.className = "slideshow-next";
        nextButton.innerHTML = "&#8250;";
        nextButton.addEventListener("click", nextImage);
        slideshowDiv.appendChild(nextButton);

        const image = document.createElement("img");
        image.onload = function() {
            updateDimensions();
        };
        let startImg = startChild.querySelector("img");
        image.src = startImg.getAttribute('data-src')
        imageContainer.appendChild(image);

        // Display image dimensions below the image
        infoContainer = document.createElement('div');
        infoContainer.className = 'slideshow-info';
        imageContainer.appendChild(infoContainer);

        filename = document.createElement("div");
        filename.textContent = startImg.getAttribute('x-filename');
        filename.className = "slideshow-filename";
        infoContainer.appendChild(filename);
        
        dimensions = document.createElement("div");
        dimensions.className = "slideshow-dimensions";
        infoContainer.appendChild(dimensions);

        updateDimensions();
        // Add slideshow elements to the page
        document.body.appendChild(slideshowDiv);        
    }

}