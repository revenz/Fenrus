/**
 * An abstract class that is the base class for all the fenrus cloud app panes
 */
class FenrusPreview {

    /**
     * Constructs a new instance of a Fenrus preview
     */
    constructor() {
        if (new.target === FenrusPreview) {
            throw new Error('Cannot instantiate abstract class');
        }
    }
    
    /**
     * If this pane instance is open
     * @returns {boolean} true if open, otherwise false
     */
    isOpened() {
        return !!this.visible;
    }

    /**
     * Opens the pane
     */
    open(){
        this.visible = true;
        if(this.container && this.container.className.indexOf('visible') < 0)
            this.container.classList.add('visible');
        if(document.body.className.indexOf('drawer-item-opened') < 0)
            document.body.classList.add('drawer-item-opened');
        document.getElementById('search-text')?.setAttribute('disabled', '');
    }

    /**
     * Closes the pane
     */
    close(){
        document.getElementById('search-text')?.removeAttribute('disabled');
        
        if(this.container)
            this.container.classList.remove('visible');
        
        if(document.body.className.indexOf('drawer-item-opened') >= 0)
            document.body.classList.remove('drawer-item-opened');
        this.visible = false;
        if(FenrusPreview.openedInstance === this)
            FenrusPreview.openedInstance = null;
    }

    /**
     * Shows a blocker in this pane, used for loading
     */
    showBlocker(){

        //' <div class="blocker visible"><div class="blocker-indicator"><div class="blocker-spinner"></div></div>';
    }

    /**
     * Closes the blocker
     */
    hideBlocker(){
        
    }

    /**
     * Fetches data from a URL
     * @param url the URL to fetch the data from
     * @returns {Promise<any>} the response from the fetch
     */
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

    /**
     * Opens a pane
     * @param type the type of pane to open
     * @param args any additional arguments to pass to the opening pane
     */
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
                instance = new SshTerminalPane();
            else if(type === 'docker')
                instance = new DockerTerminalPane();
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

    /**
     * Closes the active pane
     * @param type an optional type, the pane will only be closed if the currently open pane is of this type
     */
    static closeActive(type){
        if(!FenrusPreview.openedInstance)
            return;
        if(type && FenrusPreview.openedInstance._type !== type)
            return; // no longer opened
        FenrusPreview.openedInstance.close();        
    }
}