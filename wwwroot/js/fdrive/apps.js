class FenrusDriveApps
{
    initDone = false;
    constructor(){
        this.container = document.getElementById('apps-actual');
        this.eleIframeContainer = document.createElement('div');
        this.eleIframeContainer.setAttribute('id', 'fdrive-apps-iframe');
        this.eleIframe = document.createElement('iframe');
        // this.eleIframe.setAttribute('sandbox', 'allow-scripts');
        this.eleIframeContainer.appendChild(this.eleIframe);
        
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
        
        this.eleIframe.src = app.getAttribute('data-src');

        this.eleIframeContainer.className = 'visible';
        document.body.classList.add('drawer-item-opened');
    }


    closeIframe(){
        this.eleIframe.className = '';
        for(let ele of this.container.querySelectorAll('.email.selected'))
            ele.classList.remove('selected');
        document.body.classList.remove('drawer-item-opened');
    }
}