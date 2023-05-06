class AddressBar {
    constructor() {
        this.container = document.getElementById('fdrive-files-address');
        this.container.className = "breadcrumb";
    }
    
    onClick(action) {
        this.onClickAction = action;
    }

    show(path) {
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

        // Position container in center of parent element
        const parent = this.container.parentNode;
        const parentRect = parent.getBoundingClientRect();
        const containerRect = this.container.getBoundingClientRect();
        const x = parentRect.left + (parentRect.width - containerRect.width) / 2;
        this.container.style.left = `${x}px`;

        // Check if container overflows to the left
        const containerLeft = containerRect.left - parentRect.left;
        if (containerLeft < 0) {
            const buttons = this.container.querySelectorAll("button");
            for (let i = buttons.length - 1; i >= 0; i--) {
                const buttonRect = buttons[i].getBoundingClientRect();
                if (buttonRect.left < -containerLeft) {
                    buttons[i].style.overflow = "hidden";
                    buttons[i].style.textOverflow = "ellipsis";
                    buttons[i].style.whiteSpace = "nowrap";
                    break;
                }
            }
        }
    }

    handleFolderClick(path) {
        if (this.onClick) {
            this.onClickAction(path);
        }
    }
}
