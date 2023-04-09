class FenrusDrive {
    eleAddMenu;
    filePaths = [];
    currentPath;
    lastReload;
    container;
    lblFiles;
    folderViews = {};

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
    }

    show() {
        this.reload();
    }

    changeFolder(path) {
        this.filePaths.push(path);
        this.updateFilesBackVisiblity();
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

    updateFilesBackVisiblity() {
        document.getElementById('files-back').className = this.filePaths.length === 0 ? '' : 'visible';
    }

    changeView(view, noSave) {
        if (!noSave)
            this.folderViews[this.currentPath || ''] = view;
        localStorage.setItem('DRIVE_FOLDER_VIEWS', JSON.stringify(this.folderViews));
        document.getElementById('fdrive-list').className = 'content ' + view;
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
        this.updateFilesBackVisiblity();
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
        this.updateFilesBackVisiblity();
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
            await fetch('/files', {
                method: 'delete',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({files: files})
            });
        } catch (err) {

        }
        hideBlocker();
        await this.reload();
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
            let result = await fetch('/files/create-folder?path=' + encodeURIComponent(fullPath), {method: 'POST'});
            this.reload();
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
            let list = document.getElementById('fdrive-list');
            if (this.currentPath)
                data.unshift({
                    extension: '',
                    fullPath: '..',
                    icon: "fa-solid fa-arrow-turn-up",
                    mimeType: 'parent',
                    name: '..',
                    size: 0
                });
            list.innerHTML = '';
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
        let list = document.getElementById('fdrive-list');
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
                img.src = 'files/media?path=' + encodeURIComponent(item.fullPath);
                img.setAttribute('x-filename', item.name);
                ele.addEventListener('dblclick', () => {
                    this.openSlideshow(ele);
                })
            } else {
                ele.className += ' no-img';
                ele.querySelector('.icon').innerHTML = `<i class="${item.icon}" />`;
                ele.addEventListener('dblclick', () => {
                    if (item.mimeType === 'parent')
                        this.parentPath();
                    else if (item.mimeType === 'folder')
                        this.changeFolder(item.fullPath);
                    else
                        this.download(item.fullPath);
                });
            }
            ele.querySelector('.size').innerText = humanizeFileSize(item.size);
            list.appendChild(ele);
        }
    }

    openImagePreview(url) {
        // Create a div to hold the image and background
        const imageDiv = document.createElement("div");
        imageDiv.style.position = "fixed";
        imageDiv.style.top = "0";
        imageDiv.style.left = "0";
        imageDiv.style.width = "100%";
        imageDiv.style.height = "100%";
        imageDiv.style.backgroundColor = "rgba(0, 0, 0, 0.8)";
        imageDiv.style.zIndex = "9999";

        // Create the close button
        const closeButton = document.createElement("div");
        closeButton.innerHTML = "X";
        closeButton.style.position = "absolute";
        closeButton.style.top = "0";
        closeButton.style.left = "0";
        closeButton.style.padding = "10px";
        closeButton.style.color = "#fff";
        closeButton.style.cursor = "pointer";
        closeButton.style.zIndex = "10000";

        // Add event listeners to close the image when clicked or when escape is pressed
        closeButton.addEventListener("click", closeImage);
        document.addEventListener("keydown", (event) => {
            if (event.key === "Escape") {
                closeImage();
            }
        });

        // Create the image element
        const image = new Image();
        image.src = url;
        image.style.maxWidth = "80%";
        image.style.maxHeight = "80%";
        image.style.position = "absolute";
        image.style.top = "50%";
        image.style.left = "50%";
        image.style.transform = "translate(-50%, -50%)";

        // Add the elements to the div
        imageDiv.appendChild(closeButton);
        imageDiv.appendChild(image);

        // Add the div to the body
        document.body.appendChild(imageDiv);

        function closeImage() {
            // Remove the event listeners
            closeButton.removeEventListener("click", closeImage);
            document.removeEventListener("keydown", (event) => {
                if (event.key === "Escape") {
                    closeImage();
                }
            });

            // Remove the image div from the body
            document.body.removeChild(imageDiv);
        }
    }

    openSlideshow( startChild) {
        // Get all the child elements of the container
        let childElements = [];
        for(let ele of this.container.children){
            if(ele.querySelector('img'))
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
            image.src = prevImage.src;
            filename.textContent = prevImage.getAttribute('x-filename');
        }

        // Function to go to the next image
        let nextImage = () => {
            startIndex++;
            if (startIndex >= numChildren) {
                startIndex = 0;
            }
            const nextImage = childElements[startIndex].querySelector("img");
            image.src = nextImage.src;
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
        slideshowDiv.className = "fdrive-slideshow";

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
        image.src = startImg.src;
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