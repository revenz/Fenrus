class VirtualizeList {
    constructor(container) {
        let wrapper = document.createElement('div');
        wrapper.className = 'vlist';
        wrapper.style.position = 'absolute';
        wrapper.style.top = 0;
        wrapper.style.bottom = 0;
        wrapper.style.left = 0;
        wrapper.style.right = 0;
        container.appendChild(wrapper);
        this.container = container;
        this.wrapper = wrapper;
        this.itemHeight = 240;
        this.numColumns = 1;
        this.visibleStartIndex = 0;
        this.visibleEndIndex = 0;
        this.items = [];
        this.visibleItems = [];
        this.bulkUpdating = false;

        this.container.addEventListener('scroll', () => this.updateVisibleItems());

        this.clear();
    }

    clear() {
        this.wrapper.scrollTop = 0;
        this.items = [];
        this.visibleItems = [];
        this.wrapper.style.height = '';
        this.wrapper.innerHTML = '';
        this.updateDOM();
    }

    addItem(item) {
        this.items.push(item);
        if (!this.bulkUpdating) {
            this.updateDOM();
        }
    }

    setItemHeight(height) {
        this.itemHeight = height;
        this.updateDOM();
    }

    setNumColumns(num) {
        this.numColumns = num;
        this.calculateColumns();
    }
    
    changeLayout(numColumns, itemHeight){
        this.itemHeight = itemHeight;
        if(numColumns === 1)
            this.numColumns = numColumns;
        else
            this.calculateColumns();
        if(this.updateDOM() === false)
            this.render();
    }

    startBulkUpdates() {
        this.bulkUpdating = true;
    }

    completeUpdates() {
        this.bulkUpdating = false;
        this.updateDOM();
    }

    scrollItemIntoView(index) {
        const item = this.items[index];
        if (!item) {
            return;
        }

        const itemTop = index * this.itemHeight;
        const itemBottom = itemTop + this.itemHeight;
        const containerTop = this.container.scrollTop;
        const containerBottom = containerTop + this.container.clientHeight;

        if (itemBottom > containerBottom) {
            this.container.scrollTop = itemBottom - this.container.clientHeight;
        } else if (itemTop < containerTop) {
            this.container.scrollTop = itemTop;
        }
    }

    updateDOM() {
        const totalRows = Math.ceil(this.items.length / this.numColumns);
        console.log('totalRows: ' + totalRows);
        const totalHeight = totalRows * this.itemHeight;
        console.log('totalHeight: ' + totalHeight);
        this.wrapper.style.height = `${totalHeight}px`;

        const visibleTop = this.container.scrollTop;
        const visibleBottom = visibleTop + this.container.clientHeight;

        const startIndex = Math.floor(visibleTop / this.itemHeight) * this.numColumns;
        const endIndex = Math.min(Math.ceil(visibleBottom /  this.itemHeight) * this.numColumns, this.items.length);
        console.log('startIndex:' + startIndex);
        console.log('endIndex:' + endIndex);

        if (this.visibleStartIndex !== startIndex || this.visibleEndIndex !== endIndex) {
            this.visibleStartIndex = startIndex;
            this.visibleEndIndex = endIndex;
            this.visibleItems = this.items.slice(startIndex, endIndex);
            this.render();
            return true;
        }
        return false;
    }

    render() {
        const columnWidth = `${100 / this.numColumns}%`;
        const rowHeight = `${this.itemHeight}px`;

        let currentColumn = 0;
        let currentRow = this.visibleStartIndex / this.numColumns;
        let currentTop = currentRow * rowHeight;
        console.log('visibleItems: ' + this.visibleItems.length);

        for (let i = 0; i < this.visibleItems.length; i++) {
            console.log('rednering: ' + i);
            const element = this.visibleItems[i];
            
            let itemIndex = this.visibleStartIndex + i;
            
            element.style.position = 'absolute';
            element.style.top = `${currentTop}px`;
            element.style.left = `${currentColumn * parseFloat(columnWidth)}%`;
            element.style.width = columnWidth;
            element.style.height = rowHeight;
            element.setAttribute('data-index', itemIndex);

            if (!element.parentElement) {
                this.wrapper.appendChild(element);
            }
            let img = element.querySelector('img.lazy:not(.loaded)');
            if(img){
                setTimeout(() => {
                    if(itemIndex < this.visibleStartIndex || i > this.visibleEndIndex)
                        return;
                    img.src = img.getAttribute('data-src') + '&thumbnail=true';
                    img.classList.add('loaded');
                }, 10);
            }

            currentColumn++;
            if (currentColumn >= this.numColumns) {
                currentColumn = 0;
                currentRow++;
            }

            currentTop = currentRow * this.itemHeight;
        }
    }

    updateVisibleItems() {
        this.updateDOM();
    }

    calculateColumns(){        
        this.numColumns = Math.round(this.container.clientWidth / 240);
        console.log('numColumns2: ' + this.numColumns);
        this.wrapper.innerHTML = '';
        this.updateDOM();
    }
}
