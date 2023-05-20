class TextPreview extends FenrusPreview
{
    static extensions = {
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
        
    async open(file){
        if(this.visible)
            this.close();
        else
            this.createDomElements();
        
        this.url = '/files/download?path=' + encodeURIComponent(file.fullPath);
        this.file = file;
        let result = await this.openTextPreview();
        if(!result)
            return;

        this.text = result.text;
        this.filename = result.name;
        this.title.innerText = result.name;
        this.highlightCode();        

        super.open();
    }


    download() {
        let link = document.createElement("a");
        let url = this.url;
        link.href = url;
        link.download = url.split("/").pop();
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
    }
    
    async copy() {

        try {
            await navigator.clipboard.writeText(this.text);
            Toast.info('Copied to clipboard');
        } catch (err) {
            console.error('Failed to copy text: ', err);
        }
    }

    async openTextPreview() {
        try {
            const response = await fetch(this.url);
            if (!response.ok) {
                throw new Error("Network response was not ok");
            }

            let filename = this.getFileNameFromContentDisposition(response.headers.get("content-disposition"));
            const fileSize = response.headers.get("content-length");
            const text = await response.text();
            return {
                name: filename,
                size: fileSize,
                text: text
            };

        } catch (error) {
            console.error("There was a problem with the text preview", error);
            return null;
        }
    }

    highlightCode() {
        const extension = this.filename.substring(this.filename.lastIndexOf('.') + 1).toLowerCase();

        if (TextPreview.extensions[extension]) {
            this.textPreview.innerHTML = `<pre><code class="language-${TextPreview.extensions[extension]}">${Prism.highlight(this.text, Prism.languages[TextPreview.extensions[extension]], TextPreview.extensions[extension])}</code></pre>`;
        } else {
            this.textPreview.textContent = this.text;
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


    createDomElements(){
        // Create slideshow elements
        if(this.container)
            return;
        this.container = document.createElement("div");
        this.container.setAttribute('id', 'fdrive-files-text-preview');
        this.container.className = "fdrive-preview fdrive-item-content";
        this.container.innerHTML = `
<div class="header">
    <span class="title"></span>
    <div class="actions">
        <button class="btn-copy" title="Copy"><i class="fa-solid fa-copy"></i></button>
        <button class="btn-download" title="Download"><i class="fa-solid fa-download"></i></button>
        <button class="btn-close" title="Close"><i class="fa-solid fa-times"></i></button>
    </div>
</div>
<div class="body">
    <div class="text-preview"></div>
</div>
`;
        const btnClose = this.container.querySelector('.btn-close');
        btnClose.addEventListener("click", () => this.close());

        const btnDownload = this.container.querySelector('.btn-download');
        btnDownload.addEventListener("click", () => this.download());
        
        const btnCopy = this.container.querySelector('.btn-copy');
        if (location.protocol === "https:")
            btnCopy.addEventListener("click", () => this.copy());
        else
            btnCopy.remove();

        this.title =  this.container.querySelector('.title');
        this.textPreview = this.container.querySelector('.text-preview');
        
        document.querySelector('.dashboard-main').appendChild(this.container);
    }
}