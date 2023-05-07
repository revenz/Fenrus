class FileUploader {
    constructor() {
        this.queue = [];
        this.activeUploads = 0;
        this.maxUploads = 2;
        this.uploadedCallback = () => {};
        this.failureCallback = () => {};
        this.createUploadContainer();

        this.successIcon = '<i class="fas fa-check-circle" style="color: var(--success)"></i>';
        this.failureIcon = '<i class="fas fa-times-circle" style="color: var(--error)"></i>';
    }

    setMaxUploads(maxUploads) {
        this.maxUploads = maxUploads;
        this.checkQueue();
    }

    queueFile(file, path) {
        this.queue.push({file: file, path: path});
        this.checkQueue();
    }

    createUploadContainer() {
        const container = document.createElement("div");
        container.classList.add("file-uploader-container");
        container.classList.add("hidden");
        document.body.appendChild(container);
        this.uploadContainer = container;
    }

    checkQueue() {
        while (this.activeUploads < this.maxUploads && this.queue.length > 0) {
            const file = this.queue.shift();
            this.uploadFile(file);
        }
    }

    async uploadFile(file) {
        this.uploadContainer.classList.remove("hidden");
        const uploadBar = this.createUploadBar(file.file);
        const cancelButton = uploadBar.querySelector(".file-uploader-cancel");
        const progressBarFill = uploadBar.querySelector('.file-uploader-progress-fill');
        let remove = () => {
            uploadBar.remove();
            if(this.uploadContainer.childNodes.length < 1)
                this.uploadContainer.classList.add("hidden");
        }
        this.activeUploads++;
        this.updateUploadCounts();
        try {
            const formData = new FormData();
            formData.append("file", file.file);

            const config = {
                method: "POST",
                body: formData,
                onUploadProgress: (progressEvent) => {
                    const percentCompleted = Math.round(
                        (progressEvent.loaded * 100) / progressEvent.total
                    );
                    progressBarFill.style.width = `${percentCompleted}%`;
                },
            };

            let response = await this.uploadFileWithProgress(formData, file.path, progressBarFill, cancelButton);
            if(typeof(response) === 'string')
                response = JSON.parse(response);            
            this.uploadedCallback({path: file.path, file: response});
            progressBarFill.classList.add("success");
            setTimeout(() => {
                uploadBar.classList.add('complete');
            }, 1000)
            setTimeout(()=> remove(), 1210);
        } catch (error) {
            if(error === 'aborted'){
                remove();         
            }else {
                Toast.error(error);
                this.failureCallback(file);
                progressBarFill.classList.add("failure");
                cancelButton.addEventListener("click", () => remove());
            }
        }
        this.activeUploads--;
        this.updateUploadCounts();
        this.checkQueue();
    }


    uploadFileWithProgress(file, path, progressDiv, cancelBtn) {
        return new Promise(async (resolve, reject) => {
            let aborted = false;
            try {
                const url = '/files/upload?path=' + encodeURIComponent(path);

                const xhr = new XMLHttpRequest();
                let done = false;
                cancelBtn.addEventListener('click', () => {
                    if(!done) {
                        done = true;
                        aborted = true;
                        xhr.abort();
                    }
                })

                xhr.upload.addEventListener('progress', (event) => {
                    if (event.lengthComputable) {
                        const percentComplete = (event.loaded / event.total) * 100;
                        progressDiv.style.width = `${percentComplete}%`;
                    }
                });

                xhr.onreadystatechange = () => {
                    if (xhr.readyState === 4) {
                        done = true;
                        if (xhr.status === 200) {
                            progressDiv.style.width = '100%';
                            resolve(xhr.responseText);
                        } else {
                            if(aborted)
                                reject('aborted');
                            else
                                reject(new Error('Network response was not ok'));
                        }
                    }
                };

                xhr.open('POST', url, true);
                xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
                xhr.send(file);
            }catch(err) {
                reject(err);
            }
        });
    }

    createUploadBar(file) {
        const uploadBar = document.createElement("div");
        uploadBar.classList.add("file-uploader-item");
        uploadBar.innerHTML = `
      <div class="file-uploader-name">${file.name}</div>
      <div class="file-uploader-cancel">&times;</div>
      <div class="file-uploader-progress"><div class="file-uploader-progress-fill"></div></div>
    `;
        this.uploadContainer.appendChild(uploadBar);
        return uploadBar;
    }

    removeUploadBar(uploadBar) {
        uploadBar.parentNode.removeChild(uploadBar);
        this.updateUploadCounts();
    }

    updateUploadCounts() {
        const successfulCount = document.querySelectorAll(
            ".file-uploader-bar.success"
        ).length;
        const failedCount = document.querySelectorAll(
            ".file-uploader-bar.failure"
        ).length;
        const remainingCount = this.queue.length;
        const totalCount =
            successfulCount + failedCount + remainingCount + this.activeUploads;
        console.log(`${successfulCount} / ${totalCount}`);
    }

    onUploaded(callback) {
        this.uploadedCallback = callback;
    }

    onFailure(callback) {
        this.failureCallback = callback;
    }


    handleFileDrop(e, currentPath) {
        e.preventDefault();
        const items = Array.from(e.dataTransfer.items);
        for (let i = 0; i < items.length; i++) {
            let item = items[i].webkitGetAsEntry ? items[i].webkitGetAsEntry() : items[i].getAsEntry();
            if (item) {
                this.traverseFileTree(item, '', currentPath);
            }
        }
    }

    openDialog(path) {
        let input = document.createElement("input");
        input.type = "file";
        input.style.display = "none";
        input.name = "file";
        input.multiple = true;
        document.body.appendChild(input);

        input.addEventListener("change", () => {
            if (!input.files.length)
                return;
            for (let file of input.files) {
                this.queueFileForUpload(file, '', path);
            }
        });

        input.addEventListener("cancel", () => {
            input.remove();
        });

        input.click();
    }

    traverseFileTree(item, path, currentPath) {
        path = path || "";
        if (item.isFile) {
            item.file((file) => {
                this.queueFileForUpload(file, path, currentPath);
            });
        } else if (item.isDirectory) {
            let dirReader = item.createReader();
            dirReader.readEntries((entries) => {
                for (let i = 0; i < entries.length; i++) {
                    let entry = entries[i];
                    if(entry.isFile)
                        entry.file((file) => {
                            this.queueFileForUpload(file, path + item.name, currentPath);
                        });
                    else if(entry.isDirectory)
                        this.traverseFileTree(entries[i], path + item.name + "/");
                }
            });
        }
    }

    queueFileForUpload(file, path, currentPath) {
        path = !path ? currentPath :
            currentPath.endsWith('/') ? currentPath + path :
                currentPath + '/' + path;
        this.queueFile(file, path);
    }
}
