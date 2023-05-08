class AddressBar {
    constructor() {
        this.container = document.getElementById('fdrive-files-address');
        let toolbar = document.querySelector('#fdrive-files .toolbar');
        let btnSearch = document.createElement('button');
        btnSearch.className = 'fdt-btn';
        btnSearch.innerHTML = '<i class="fa-solid fa-magnifying-glass"></i>';
        toolbar.appendChild(btnSearch);
        this.searching = false;
        btnSearch.addEventListener('click', () => {            
            if(this.searching){
                this.doSearch();                                
            }else {
                this.searched = false;
                this.searching = true;
                this.showSearch();
            }
        });        
        this.container.className = "address-bar";
    }
    
    onClick(action) {
        this.onClickAction = action;
    }
    onSearch(action) {
        this.onSearchAction = action;
    }
    
    doSearch(){
        let searchText = (this.container.querySelector('input[type=text]')?.value || '').trim();
        if(!searchText)
            return;
        this.searched = true;
        this.onSearchAction(this.currentPath, searchText);        
    }
    
    showSearch(){
        this.container.classList.add('searching');
        this.container.innerHTML = '<i class="fa-solid fa-xmark"></i>' +
            '<input type="text" class="file-search" placeholder="Search files and folders" />' +
            '<i class="fa-solid fa-magnifying-glass"></i>';
        this.container.querySelector('.fa-xmark').addEventListener('click', () => {
            this.searching = false;
            this.onClickAction(this.currentPath);
            this.show(this.currentPath);
        })
        this.container.querySelector('input').addEventListener('keydown', (event) => {
            if(event.key === 'Enter') {
                event.preventDefault();
                event.stopPropagation();
                this.doSearch();
            }
        })
        this.container.querySelector('input').focus();
    }

    show(path) {
        this.container.classList.remove('searching');
        this.currentPath = path;
        // Trim trailing slash if present
        if (path.endsWith("/")) {
            path = path.slice(0, -1);
        }
        if(path && !path.startsWith("/"))
            path = '/' + path;
        path = "Home" + path;

        // Clear container
        this.container.innerHTML = "";

        // Split path into individual folder names
        const folders = path.split("/");

        let fullPath = '';
        let items = [];
        for (let i = 0; i < folders.length; i++) {
            const folder = folders[i];
            if(i > 0)
                fullPath += folder + "/";

            const button = document.createElement("button");
            if(i === 0)
                button.innerHTML = 'Home <i class="fa-solid fa-house"></i>';   
            else
                button.textContent = folder.startsWith('.') ? folder.substring(1) + '.' : folder;
            let itemPath = fullPath;
            button.onclick = () => this.handleFolderClick(itemPath);
            items.push(button);
        }
        // we use right to left to ensure the current folder is always seen
        for(let i=items.length-1;i>=0;i--){
            let button = items[i];
            if (i < folders.length - 1) {
                const separator = document.createElement("span");
                separator.textContent = "/";
                this.container.appendChild(separator);
            }
            this.container.appendChild(button);
        }

        // Add container to DOM if it doesn't exist yet
        if (!this.container.parentNode) {
            document.body.appendChild(this.container);
        }
    }

    handleFolderClick(path) {
        if (this.onClick) {
            this.onClickAction(path);
        }
    }
}
