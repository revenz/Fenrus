class ContextMenu {
    contextMenu;
    static otherMenus = [];

    constructor(args) { //{ target = null, menuItems = [] }) {
      this.target = args.target;
      this.menuItems = args.menuItems;
      console.log('menuItems:', JSON.parse(JSON.stringify(args.menuItems)));
      this.menuItemsNode = this.getMenuItemsNode();
      this.isOpened = false;
      ContextMenu.otherMenus.push(this);
    }
  
    getMenuItemsNode() {
      const nodes = [];
  
      if (!this.menuItems) {
        return [];
      }
  
      this.menuItems.forEach((data, index) => {
        const item = this.createItemMarkup(data);
        item.firstChild.setAttribute(
          "style",
          `animation-delay: ${index * 0.08}s`
        );
        nodes.push(item);
      });
  
      return nodes;
    }
  
    createItemMarkup(data) {
      const button = document.createElement("BUTTON");
      const item = document.createElement("LI");
  
      button.innerHTML = data.content;
      button.classList.add("contextMenu-button");
      item.classList.add("contextMenu-item");

      if(data.submenu?.length){
        const caret = document.createElement('i');
        caret.className = 'caret fa-solid fa-angle-right';
        button.appendChild(caret);
        item.classList.add("has-submenu");
        const sub = document.createElement('ul');
        sub.className = 'submenu contextMenuCommon';
        for(let si of data.submenu)
          sub.appendChild(this.createItemMarkup(si));
        item.appendChild(sub);
      }
  
      if (data.divider) item.setAttribute("data-divider", data.divider);
      item.appendChild(button);
  
      if (data.events && data.events.length !== 0) {
        Object.entries(data.events).forEach((event) => {
          const [key, value] = event;
          button.addEventListener(key, value);
        });
      }
  
      return item;
    }
  
    renderMenu() {
      const menuContainer = document.createElement("UL");
  
      menuContainer.classList.add("contextMenu");
      menuContainer.classList.add("contextMenuCommon");
  
      this.menuItemsNode.forEach((item) => menuContainer.appendChild(item));
  
      return menuContainer;
    }
  
    closeMenu() {
      if (this.isOpened) {
        this.isOpened = false;
        this.contextMenu.remove();
      }
    }

    open(event){        
        for(let other of ContextMenu.otherMenus)
            other.closeMenu();
        event.preventDefault();
        this.isOpened = true;

        const { clientX, clientY } = event;
        document.body.appendChild(this.contextMenu);

        const positionY =
          clientY + this.contextMenu.scrollHeight >= window.innerHeight
            ? window.innerHeight - this.contextMenu.scrollHeight - 20
            : clientY;
        const positionX =
          clientX + this.contextMenu.scrollWidth >= window.innerWidth
            ? window.innerWidth - this.contextMenu.scrollWidth - 20
            : clientX;

        this.contextMenu.setAttribute(
          "style",
          `--width: ${this.contextMenu.scrollWidth}px;
          --height: ${this.contextMenu.scrollHeight}px;
          --top: ${positionY}px;
          --left: ${positionX}px;`
        );
    }
  
    init() {
      this.contextMenu = this.renderMenu();

      document.addEventListener("click", () => this.closeMenu());
      window.addEventListener("blur", () => this.closeMenu());
    }
  }