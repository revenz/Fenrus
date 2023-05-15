class FenrusDriveApps
{
    initDone = false;
    constructor(){
        this.container = document.getElementById('apps-actual');
        this.eleIframeContainer = document.createElement('div');
        this.eleIframeContainer.innerHTML = '<div class="browser-container">' +
            '  <div class="browser-header">' +
            '    <div class="browser-address-bar">' +
            '      <img />' +
            '      <input type="text" readonly>' +  
            '    </div>' +
            '    <div class="browser-controls">' +
            '      <button class="open-new-tab"><i class="fa-solid fa-arrow-up-right-from-square"></i></button>' +
            '      <button class="close-button"><i class="fa-solid fa-xmark"></i></button>' +
            '    </div>' +
            '  </div>' +
            '  <div class="browser-iframe-container">' +
            '    <iframe></iframe>' +
            '  </div>' +
            '</div>';
        this.eleIframeContainer.setAttribute('id', 'fdrive-apps-iframe');
        this.eleIframe = this.eleIframeContainer.querySelector('iframe');
        this.eleIframeAddress = this.eleIframeContainer.querySelector('.browser-address-bar input[type=text]');
        this.eleIframeFavicon = this.eleIframeContainer.querySelector('.browser-address-bar img');
        this.eleIframeContainer.querySelector('.close-button').addEventListener('click', () => this.closeIframe());
        this.eleIframeContainer.querySelector('.open-new-tab').addEventListener('click', () => {
            let url = this.eleIframeAddress.value;
            if(!url)
                return;
            window.open(url, "_blank", "noopener,noreferrer");
        });
        // this.eleIframe.setAttribute('sandbox', 'allow-scripts');
        
        document.querySelector('.dashboard-main').appendChild(this.eleIframeContainer);
        
        this.initApps();
    }
    
    initApps(){
        for(let app of this.container.querySelectorAll('.drive-app'))
        {
            app.addEventListener('click', () => {
               this.openApp(app) 
            });
        }
    }

    hide(){
        this.closeIframe();
    }

    show(){
        if(this.initDone)
            return;
        this.initDone = true;
    }

    clear(){
        this.container.innerHTML = '';
    }
    
    async openApp(app)
    {
        for(let ele of this.container.querySelectorAll('.drive-app.selected'))
            ele.classList.remove('selected');

        app.classList.add('selected');
        
        let url = app.getAttribute('data-src');
        this.eleIframe.src = url;
        this.eleIframeAddress.value = url;
        
        this.eleIframeFavicon.src = app.querySelector('img').src;

        this.eleIframeContainer.className = 'visible';
        document.body.classList.add('drawer-item-opened');
    }


    closeIframe(){
        this.eleIframeContainer.className = '';
        for(let ele of this.container.querySelectorAll('.email.selected'))
            ele.classList.remove('selected');
        document.body.classList.remove('drawer-item-opened');
    }
}