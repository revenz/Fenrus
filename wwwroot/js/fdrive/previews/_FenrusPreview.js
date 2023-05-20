class FenrusPreview {
    
    isOpened() {
        return this.visible;
    }
    
    open(){
        this.visible = true;
        if(this.container && this.container.className.indexOf('visible') < 0)
            this.container.classList.add('visible');
        if(document.body.className.indexOf('drawer-item-opened') < 0)
            document.body.classList.add('drawer-item-opened');
    }
    close(){
        if(this.container)
            this.container.classList.remove('visible');
        
        if(document.body.className.indexOf('drawer-item-opened') < 0)
            document.body.classList.add('drawer-item-opened');
        this.visible = false;
        if(FenrusPreview.openedInstance === this)
            FenrusPreview.openedInstance = null;
    }
    
    showBlocker(){

        //' <div class="blocker visible"><div class="blocker-indicator"><div class="blocker-spinner"></div></div>';
    }
    hideBlocker(){
        
    }

    async fetchData(url) {
        const cache = await caches.open('fenrus-cache');
        const cachedResponse = await cache.match(url);

        if (cachedResponse) {
            console.log('Data found in cache');
            return cachedResponse.json();
        } else {
            console.log('Data not found in cache. Fetching from network...');
            const response = await fetch(url);
            const data = await response.json();
            await cache.put(url, new Response(JSON.stringify(data)));
            return data;
        }
    }
    
    static openedInstance;
    static constructedInstances = {};
    static open(type, ...args){
        if(!type)
            return;
        type = type.toLowerCase();
        let instance = FenrusPreview.constructedInstances[type];
        if(!instance) {
            if (type === 'slideshow') 
                instance = new SlideShow();
            else if(type === 'iframe')
                instance = new IFrame();
            else if(type === 'ssh')
                instance = new SshTerminal();
            else if(type === 'text')
                instance = new TextPreview();
            else if(type === 'email')
                instance = new Email();
            else
                return;
            instance._type = type;
            FenrusPreview.constructedInstances[type] = instance;
        }
        if(FenrusPreview.openedInstance && FenrusPreview.openedInstance !== instance)
            FenrusPreview.openedInstance.close();
        
        FenrusPreview.openedInstance = instance;
        instance.open(...args);            
    }
    
    static closeActive(type){
        if(!FenrusPreview.openedInstance)
            return;
        if(type && FenrusPreview._type !== type)
            return; // no longer opened
        FenrusPreview.openedInstance.close();        
    }
}