class IFrame extends FenrusPreview
{
    
    constructor() {
        super();
        this.instances = [];
    }
    
    
    open(url, icon){
        let index = this.instances.findIndex(x => x.url === url);
        this.createDomElements();
        let instance;
        if(index >= 0)
        {
            // already opened
            instance = this.instances[index];
            this.instances.splice(index, 1);
            this.instances.push(instance);            
        }
        else {
            // not opened, so open it
            while(this.instances.length > 5)
                this.disposeInstance(0);
            
            let iframe = this.createIframe();
            iframe.src = url;
            instance  = {
                url: url,
                icon: icon,
                iframe: iframe
            };
            this.instances.push(instance);
            this.eleIframeContainer.appendChild(instance.iframe);
        }
        
        
        if(this.current !== instance && this.current){
            // remove the current
            this.current.iframe.style.display = 'none';
        } 
        this.current = instance;
        instance.iframe.style.display = '';
        this.eleIframeAddress.value = url;
        this.eleIframeFavicon.src = icon;

        this.container.className = 'visible';
        document.body.classList.add('drawer-item-opened');
        
        super.open();
    }
    close(){
        
        super.close();
    }
    
    disposeInstance(index){
        let instance = this.instances[index];
        this.instances.splice(index, 1);
        instance.iframe.remove();
    }
    
    createDomElements(){
        if(this.container)
            return;
        this.container = document.createElement('div');
        
        this.container.innerHTML = '<div class="browser-container app-target-container">' +
            '  <div class="header">' +
            '    <div class="address-bar">' +
            '      <img />' +
            '      <input type="text" readonly>' +
            '    </div>' +
            '    <div class="controls">' +
            '      <button class="btn-refresh"><i class="fa-solid fa-arrow-rotate-right"></i></button>' +
            '      <button class="btn-newtab"><i class="fa-solid fa-arrow-up-right-from-square"></i></button>' +
            '      <button class="btn-close"><i class="fa-solid fa-xmark"></i></button>' +
            '    </div>' +
            '  </div>' +
            '  <div class="inner-container">' +
            '  </div>' +
            '</div>';
        this.container.setAttribute('id', 'fdrive-apps-iframe');
        this.eleIframeAddress = this.container.querySelector('.address-bar input[type=text]');
        this.eleIframeFavicon = this.container.querySelector('.address-bar img');
        this.eleIframeContainer = this.container.querySelector('.inner-container');
        this.container.querySelector('.btn-close').addEventListener('click', () => this.close());
        this.container.querySelector('.btn-refresh').addEventListener('click', () =>
        {
            this.current.iframe.src = this.current.url;
        });
        this.container.querySelector('.btn-newtab').addEventListener('click', () => {
            let url = this.eleIframeAddress.value;
            if(!url)
                return;
            window.open(url, "_blank", "noopener,noreferrer");
        });

        document.querySelector('.dashboard-main').appendChild(this.container);
        
    }
    
    createIframe(){
        let eleIframe = document.createElement('iframe');
        eleIframe.setAttribute('sandbox', 'allow-scripts allow-same-origin');
        return eleIframe;
    }
}