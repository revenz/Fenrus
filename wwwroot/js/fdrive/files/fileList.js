class FileList {
    constructor(container) {
        this.container = container;
        this.items = [];

        this.showHidden = true;
        this.virtualizeList = new VirtualizeList(container);
        this.virtualizeList.setItemHeight(this.viewListHeight);
        this.virtualizeList.setNumColumns(1);
        document.body.addEventListener('driveResizeEvent', (event) => {
            let view = this.views[this.currentView];
            if (view.columns === 0)
                this.virtualizeList.changeLayout(view.columns, view.height);
        });

        this.container.addEventListener('contextmenu', (event) => this.onContextMenu(event));
        this.container.addEventListener('mousedown', (e) => {
            //if (!fDriveDrawer.isFiles) return;
            if (e.button === 0) // only want left click button
                this.containerOnMouseDown(e);
        });
        this.container.tabIndex = 0;
        this.container.style.outline = 'none';
        this.container.addEventListener('keydown', (e) => {
            if (e.key === 'F2') {
                e.preventDefault();
                e.stopPropagation();
                let selected = this.getSelectedUids();
                if (selected?.length === 1) {
                    let index = this.items.findIndex(x => x.fullPath === selected[0]);
                    if (index < 0)
                        return;
                    let item = this.items[index];
                    this.onRenameAction(item);
                }
            } else if (e.key === 'Delete' || e.key === 'Backspace') {
                e.preventDefault();
                e.stopPropagation();
                let selected = this.getSelectedUids();
                if (selected?.length)
                    this.onDeleteAction(selected);
            }
        });
        this.views = [
            {name: 'List', icon: 'fa-list', columns: 1, height: 32},
            {name: 'Grid', icon: 'fa-grip', columns: 0, height: 96},
            {name: 'Thumbnail', icon: 'fa-image', columns: 0, height: 240}
        ];
        this.currentView = 0;
        this.sorts = [
            {name: 'A-Z', sorter: (a, b) => a.name.toLowerCase().localeCompare(b.name.toLowerCase())},
            {name: 'Z-A', sorter: (a, b) => b.name.toLowerCase().localeCompare(a.name.toLowerCase())},
            {name: 'Oldest', sorter: (a, b) => a.created.getTime() - b.created.getTime()},
            {name: 'Newest', sorter: (a, b) => b.created.getTime() - a.created.getTime()},
            {name: 'Size', sorter: (a, b) => a.size - b.size},
            {name: 'Type', sorter: (a, b) => a.mimeType.toLowerCase().localeCompare(b.mimeType.toLowerCase())},
        ];
        this.currentSort = 0;
    }

    setItems(items) {
        if (!items) items = [];
        else if (Array.isArray(items) === false)
            items = [items];
        let sort = this.sorts[this.currentSort];
        this.items = items.sort((a, b) => {
            let aFolder = a.mimeType === 'folder';
            let bFolder = b.mimeType === 'folder';
            if (aFolder && bFolder)
                return a.name.toLowerCase().localeCompare(b.name.toLowerCase());
            if (aFolder)
                return -1;
            if (bFolder)
                return 1;
            return sort.sorter(a, b);
        });
        this.virtualizeList.clear();
        this.eleItems = [];
        this.addToList(items);
    }

    addItems(items) {
        if (Array.isArray(items) === false)
            items = [items];
        this.items = [...this.items, ...items];
        this.addToList(items);
    }

    setShowHidden(show) {
        this.showHidden = show;
        this.eleItems = [];
        this.virtualizeList.clear();
        this.addToList(this.items);
    }

    clear() {
        this.items = [];
        this.eleItems = [];
        this.virtualizeList.clear();
    }

    update(file, updated) {
        if (!file?.fullPath || !updated?.fullPath)
            return;
        let ele = this.eleItems[file.fullPath];
        if (!ele)
            return;
        if (file.fullPath !== updated.fullPath) {
            delete this.eleItems[file.fullPath];
            this.eleItems[updated.fullPath] = ele;
        }
        Object.assign(file, updated);

        ele.setAttribute('x-uid', file.fullPath);
        ele.querySelector('.name').innerText = file.name;
    }


    nextView() {
        if (++this.currentView >= this.views.length)
            this.currentView = 0;
        let view = this.views[this.currentView];
        this.changeView(view);
        return view;
    }

    sort(name) {
        let index = typeof (name) === 'number' ? name : this.sorts.findIndex(x => x.name.toLowerCase() === name.toLowerCase());
        if (index < 0)
            return false;
        this.currentSort = index;
        this.setItems(this.items); // this resorts the data
        return this.sorts[index].name;
    }

    changeView(view) {
        if (typeof (view) === 'string')
            view = this.views.filter(x => x.name.toLowerCase() === view.toLowerCase())[0];
        if (!view)
            return;

        this.container.className = 'content ' + view.name.toLowerCase();
        this.currentView = this.views.indexOf(view);
        if (this.virtualizeList) {
            this.virtualizeList.changeLayout(view.columns, view.height);
        }
        return view;
    }


    getSelectedUids() {
        return this.virtualizeList.items.filter(x => x.classList.contains('selected'))
            .map((x) => {
                return x.getAttribute('x-uid');
            });
    }

    onFileDblClick(action) { this.onFileDblClickAction = action; }
    onFolderDblClick(action) { this.onFolderDblClickAction = action; }
    onFileDownload(action) { this.onFileDownloadAction = action; }
    onUpload(action) { this.onUploadAction = action; }
    onNewFolder(action) { this.onNewFolderAction = action; }
    onRename(action) { this.onRenameAction = action; }
    onMove(action) { this.onMoveAction = action; }
    onDelete(action) { this.onDeleteAction = action; }


    addToList(items) {
        if (!items)
            return;
        let count = 0;
        this.virtualizeList.startBulkUpdates();
        const accentColor = getComputedStyle(document.body).getPropertyValue('--accent');
        for (let item of items) {
            let ele = this.eleItems[item.fullPath];
            if (!ele) {
                ele = this.createElement();
                this.eleItems[item.fullPath] = ele;
                ele.className += ' ' + item.mimeType;
                ele.setAttribute('x-uid', item.fullPath);
                ele.querySelector('.name').innerText = item.name;
                ele.querySelector('.modified').innerText = this.formatDate(item.modified)
                if (item.mimeType.startsWith('image')) {
                    let img = ele.querySelector('img');
                    img.className = 'media';
                    img.addEventListener('error', () => {
                        this.replaceImage(img);
                    })
                    img.draggable = false;
                    img.setAttribute('data-src', 'files/media?path=' + encodeURIComponent(item.fullPath));
                    img.src = 'images/placeholder.svg';
                    img.classList.add('lazy');
                    img.setAttribute('x-filename', item.name);
                    ele.classList.add('image');
                    ele.addEventListener('dblclick', () => {
                        this.onFileDblClickAction(item);
                    })
                } else {
                    ele.className += ' no-img';
                    let extension = item.fullPath.substring(item.fullPath.lastIndexOf('.') + 1).toLowerCase();
                    if (this.fileTypeIcons.indexOf(extension) >= 0)
                        ele.querySelector('.icon').innerHTML = `<img draggable="false" src="images/filetypes/${extension}.svg" />`;
                    else if (item.mimeType === 'folder')
                        ele.querySelector('.icon').innerHTML = `<img draggable="false" src="images/folder-accent.svg?color=${encodeURIComponent(accentColor)}" />`; // `<img src="images/filetypes/folder.svg" />`;
                    else
                        ele.querySelector('.icon').innerHTML = `<img draggable="false" src="images/filetypes/blank.svg" />`;

                    ele.addEventListener('dblclick', () => {

                        if (item.mimeType === 'folder')
                            this.onFolderDblClickAction(item);
                        else
                            this.onFileDblClickAction(item);
                    });
                }
                ele.querySelector('.size').innerText = humanizeFileSize(item.size);
            }

            if (item.name.startsWith('.') && this.showHidden === false)
                continue; // hidden item

            this.virtualizeList.addItem(ele, true);
            ++count;
        }
        this.virtualizeList.completeUpdates();
    }

    formatDate(date) {
        if (!date?.getTime)
            return ''; // not a date object

        const now = new Date();
        const isThisYear = date.getFullYear() === now.getFullYear();
        const isToday = date.toDateString() === now.toDateString();
        const isYesterday = date.toDateString() === new Date(now - 24 * 60 * 60 * 1000).toDateString();

        let formattedDate = '';
        if (isToday) {
            formattedDate = date.toLocaleTimeString([], {hour: 'numeric', minute: '2-digit'});
        } else if (isYesterday) {
            formattedDate = 'Yesterday';
        } else {
            formattedDate = `${date.getDate()} ${date.toLocaleString('default', {month: 'short'})}`;
            if (!isThisYear) {
                formattedDate += ` ${date.getFullYear()}`;
            }
        }

        return formattedDate;
    }

    createElement() {
        let ele = document.createElement('div');
        ele.className = 'file';
        ele.innerHTML = '<div class="file-inner">' +
            '<span class="icon"><img draggable="false" /></span>' +
            '<span class="name"></span>' +
            '<span class="size"></span>' +
            '<span class="modified"></span>' +
            '</div>';
        return ele;
    }

    getFileElementFromEvent(event) {
        const path = event.composedPath();
        return path.find(element => element.classList?.contains('file'));
    }
    
    getFileItemFromEvent(event)
    {
        let ele = this.getFileElementFromEvent(event);
        if(!ele)return null;

        let uid = ele.getAttribute('x-uid');
        let index = this.items.findIndex(x => x.fullPath === uid);
        if(index < 0)
            return null;
        return this.items[index];
    }
    
    getFileFromCoords(x, y)
    {
        const element = document.elementFromPoint(x, y);
        let file = element?.closest('.file');
        if(!file)
            return null;
        let uid = file.getAttribute('x-uid');
        let index = this.items.findIndex(x => x.fullPath === uid);
        if(index < 0)
            return null;
        return this.items[index];
    }
    

    async containerOnMouseDown(event) {

        const fileElement = this.getFileElementFromEvent(event);
        if(fileElement) {
            // NEED TO HANDLE SHIFT HERE            
            this.onFileDrag(event, fileElement)
            return;
        }
        this.lastSelected = null;
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

    onFileDrag(event, fileElement)
    {
        let fileUid = fileElement.getAttribute('x-uid');
        let previouslySelected = this.lastSelected;
        let selected = this.getSelectedUids();
        let singleClick = () => {
            let selected = this.getSelectedUids();
            let done = false;
            if (event.shiftKey && previouslySelected) {
                let index = this.items.findIndex(x => x.fullPath === fileUid);
                let otherIndex = this.items.findIndex(x => x.fullPath === previouslySelected.uid);
                let start = Math.min(index, otherIndex);
                let end = Math.max(index, otherIndex);
                if (start > 0 && start < end) {
                    let newItems = this.items.slice(start, end + 1).map(x => this.eleItems[x.fullPath]);
                    this.setSelected(newItems, true);
                    done = true;
                }
            }
            if (!done) {
                if (event.ctrlKey) {
                    let isSelected = fileElement.classList.contains('selected');
                    if (isSelected) {
                        fileElement.classList.remove('selected');
                        selected = selected.filter(x => x != fileUid);
                    } else {
                        this.setSelected([fileElement], true);
                        selected.push(fileUid);
                    }

                } else {
                    this.setSelected([fileElement], false);
                }
            }
        }
        if(!event.ctrlKey)
            this.lastSelected = { uid: fileUid, ele: fileElement };
        else
            this.lastSelected = null;
        
        let eleDrag = null;
        let heldAction = () => {
            if(!selected?.length)
                return;
            eleDrag = document.createElement('div');
            let html = '';
            for(let i=0;i<Math.min(selected.length, 3);i++)
            {
                let src = document.querySelector(`.file[x-uid='${selected[i]}'] img`).getAttribute('src');
                html += `<img draggable="false" src="${htmlEncode(src)}"  />`;
            }
            eleDrag.className = 'file-drag-handle';
            eleDrag.innerHTML = html;
            for(let img of eleDrag.querySelectorAll('img'))
            {
                img.addEventListener('error', () => {
                    this.replaceImage(img);
                })
            }
            eleDrag.style.left = event.clientX + 'px';
            eleDrag.style.top = event.clientY + 'px';
            document.body.appendChild(eleDrag);
        }
        let down = new Date().getTime();

        let onMouseMove = (event) => {
            if(eleDrag != null) {
                eleDrag.style.left = event.clientX + 'px';
                eleDrag.style.top = event.clientY + 'px';
            }
        };
        let heldTimer = setTimeout(() => {
            heldAction();   
        }, 300);
        let onMouseUp = async (event) => {
            clearTimeout(heldTimer);          
            document.removeEventListener('mouseup', onMouseUp);
            document.removeEventListener('mousemove', onMouseMove);
            if(eleDrag)
                eleDrag.remove();
            let up = new Date().getTime();
            if(Math.abs(down - up) < 300) {
                singleClick();
                return;
            }
            const target = this.getFileFromCoords(event.clientX, event.clientY);
            
            if(target?.mimeType !== 'folder') // we only want to allow drop on folders
                return;
            this.onMoveAction(target.fullPath, selected);
        };
        document.addEventListener('mouseup', onMouseUp);
        document.addEventListener('mousemove', onMouseMove);
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



    onContextMenu(event) {
        event.preventDefault();
        event.stopPropagation();

        const fileElement = this.getFileElementFromEvent(event);
        let selected = this.getSelectedUids();
        if (fileElement) {
            let uid = fileElement.getAttribute('x-uid');
            if(selected.indexOf(uid) < 0) {
                this.setSelected([fileElement], false);
                selected = [uid];
            }
        }
        let menuItems = [];
        let item = null;
        if(selected.length === 1) {
            let index = this.items.findIndex(x => x.fullPath === selected[0]);
            item = index < 0 ? null : this.items[index];
        }           

        menuItems.push(
        {
            divider: 'bottom',
            content: `<i class="fa-solid fa-folder-plus"></i>&nbsp;&nbsp;New Folder`,
            events: {
                click: (e) => this.onNewFolderAction()
            }
        });
        if(item)
        {
            menuItems.push(
            {
                content: `<i class="fa-solid fa-pencil-alt"></i>&nbsp;&nbsp;Rename`,
                events: {
                    click: (e) => this.onRenameAction(item)
                }
            });
            
        }
        
        if(selected.length > 0) 
        {
            menuItems.push(
            {
                divider: 'bottom',
                content: `<i class="fa-solid fa-trash"></i>&nbsp;&nbsp;Delete`,
                events: {
                    click: (e) => this.onDeleteAction(selected)
                }
            });
        }
        
        if(item && item.mimeType !== 'folder')
        {
            menuItems.push(
            {
                content: `<i class="fa-solid fa-download"></i>&nbsp;&nbsp;Download`,
                events: {
                    click: (e) => this.onFileDownloadAction(item)
                }
            });
        }
        menuItems.push(
        {
            content: `<i class="fa-solid fa-upload"></i>&nbsp;&nbsp;Upload`,
            events: {
                click: (e) => this.onUploadAction()
            }
        });        

        let menu = new ContextMenu({ menuItems });
        menu.init();
        menu.open(event);
    }
    
    
    replaceImage(item)
    {
        item.src = '/images/filetypes/image.svg';
    }


    fileTypeIcons =['3g2','3ga','3gp','7z','aa','aac','ac','accdb','accdt','ace','adn','ai','aif',
        'aifc','aiff','ait','amr','ani','apk','app','applescript','asax','asc','ascx','asf','ash','ashx','asm',
        'asmx','asp','aspx','asx','au','aup','avi','axd','aze','bak','bash','bat','bin','blank','bmp','bowerrc',
        'bpg','browser','bz2','bzempty','c','cab','cad','caf','cal','cd','cdda','cer','cfg','cfm','cfml','cgi',
        'chm','class','cmd','code','coffee','coffeelintignore','com','compile','conf','config','cpp','cptx','cr2',
        'crdownload','crt','crypt','cs','csh','cson','csproj','css','csv','cue','cur','dart','dat','data','db',
        'dbf','deb','default','dgn','dist','diz','dll','dmg','dng','doc','docb','docm','docx','dot','dotm','dotx','download','dpj','ds_store','dsn','dtd','dwg','dxf','editorconfig','el','elf','eml','enc','eot','eps','epub','eslintignore','exe','f4v','fax','fb2','fla','flac','flv','fnt','folder','fon','gadget','gdp','gem','gif','gitattributes','gitignore','go','gpg','gpl','gradle','gz','h','handlebars','hbs','heic','hlp','hs','hsl','htm','html','ibooks','icns','ico','ics','idx','iff','ifo','image','img','iml','in','inc','indd','inf','info','ini','inv','iso','j2','jar','java','jpe','jpeg','jpg','js','json','jsp','jsx','key','kf8','kmk','ksh','kt','kts','kup','less','lex','licx','lisp','lit','lnk','lock','log','lua','m','m2v','m3u','m3u8','m4','m4a','m4r','m4v','map','master','mc','md','mdb','mdf','me','mi','mid','midi','mk','mkv','mm','mng','mo','mobi','mod','mov','mp2','mp3','mp4','mpa','mpd','mpe','mpeg','mpg','mpga','mpp','mpt','msg','msi','msu','nef','nes','nfo','nix','npmignore','ocx','odb','ods','odt','ogg','ogv','ost','otf','ott','ova','ovf','p12','p7b','pages','part','pcd','pdb','pdf','pem','pfx','pgp','ph','phar','php','pid','pkg','pl','plist','pm','png','po','pom','pot','potx','pps','ppsx','ppt','pptm','pptx','prop','ps','ps1','psd','psp','pst','pub','py','pyc','qt','ra','ram','rar','raw','rb','rdf','rdl','reg','resx','retry','rm','rom','rpm','rpt','rsa','rss','rst','rtf','ru','rub','sass','scss','sdf','sed','sh','sit','sitemap','skin','sldm','sldx','sln','sol','sphinx','sql','sqlite','step','stl','svg','swd','swf','swift','swp','sys','tar','tax','tcsh','tex','tfignore','tga','tgz','tif','tiff','tmp','tmx','torrent','tpl','ts','tsv','ttf','twig','txt','udf','vb','vbproj','vbs','vcd','vcf','vcs','vdi','vdx','vmdk','vob','vox','vscodeignore','vsd','vss','vst','vsx','vtx','war','wav','wbk','webinfo','webm','webp','wma','wmf','wmv','woff','woff2','wps','wsf','xaml','xcf','xfl','xlm','xls','xlsm','xlsx','xlt','xltm','xltx','xml','xpi','xps','xrb','xsd','xsl','xspf','xz','yaml','yml','z','zip','zsh'];
}