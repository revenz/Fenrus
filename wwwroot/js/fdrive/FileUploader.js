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
        const progressBarFill = uploadBar.querySelector('.file-uploader-progress-fill');
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

            let response = await this.uploadFileWithProgress(formData, file.path, progressBarFill);
            if(typeof(response) === 'string')
                response = JSON.parse(response);            
            this.uploadedCallback({path: file.path, file: response});
            progressBarFill.classList.add("success");
            setTimeout(() => {
                uploadBar.classList.add('complete');
            }, 1000)
            setTimeout(()=> {
                uploadBar.remove();
                if(this.uploadContainer.childNodes.length < 1)
                    this.uploadContainer.classList.add("hidden");
            }, 1210);
        } catch (error) {
            Toast.error(error);
            this.failureCallback(file);
            progressBarFill.classList.add("failure");
        }
        this.activeUploads--;
        this.updateUploadCounts();
        this.checkQueue();
    }


    uploadFileWithProgress(file, path, progressDiv) {
        return new Promise(async (resolve, reject) => {
            try {
                const url = '/files/upload?path=' + encodeURIComponent(path);

                const xhr = new XMLHttpRequest();

                xhr.upload.addEventListener('progress', (event) => {
                    if (event.lengthComputable) {
                        const percentComplete = (event.loaded / event.total) * 100;
                        console.log('percentComplete', percentComplete);
                        progressDiv.style.width = `${percentComplete}%`;
                    }
                });

                xhr.onreadystatechange = () => {
                    if (xhr.readyState === 4) {
                        if (xhr.status === 200) {
                            progressDiv.style.width = '100%';
                            resolve(xhr.responseText);
                        } else {
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
        const cancelButton = uploadBar.querySelector(".file-uploader-cancel");
        cancelButton.addEventListener("click", () => {
            this.removeUploadBar(uploadBar);
        });
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
}
