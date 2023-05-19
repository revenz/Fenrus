class FenrusDriveApps
{
    initDone = false;
    constructor(){
        this.divLaunchingApp = document.getElementById('launching-app');
        this.container = document.getElementById('apps-actual');
        this.eleIframeContainer = document.createElement('div');
        this.eleIframeContainer.innerHTML = '<div class="browser-container app-target-container">' +
            '  <div class="header">' +
            '    <div class="address-bar">' +
            '      <img />' +
            '      <input type="text" readonly>' +  
            '    </div>' +
            '    <div class="controls">' +
            '      <button class="open-new-tab"><i class="fa-solid fa-arrow-up-right-from-square"></i></button>' +
            '      <button class="close-button"><i class="fa-solid fa-xmark"></i></button>' +
            '    </div>' +
            '  </div>' +
            '  <div class="inner-container">' +
            '    <iframe></iframe>' +
            '  </div>' +
            '</div>';
        this.eleIframeContainer.setAttribute('id', 'fdrive-apps-iframe');
        this.eleIframe = this.eleIframeContainer.querySelector('iframe');
        this.eleIframeAddress = this.eleIframeContainer.querySelector('.address-bar input[type=text]');
        this.eleIframeFavicon = this.eleIframeContainer.querySelector('.address-bar img');
        this.eleIframeContainer.querySelector('.close-button').addEventListener('click', () => this.closeIframe());
        this.eleIframeContainer.querySelector('.open-new-tab').addEventListener('click', () => {
            let url = this.eleIframeAddress.value;
            if(!url)
                return;
            window.open(url, "_blank", "noopener,noreferrer");
        });
        // this.eleIframe.setAttribute('sandbox', 'allow-scripts');
        
        document.querySelector('.dashboard-main').appendChild(this.eleIframeContainer);

        this.createTerminalContainer();
        
        this.initApps();
    }
    
    createTerminalContainer(){

        this.eleTerminalContainer = document.createElement('div');
        this.eleTerminalContainer.innerHTML = '<div class="terminal-container app-target-container">' +
            '  <div class="header">' +
            '    <div class="address-bar">' +
            '      <img />' +
            '      <input type="text" readonly>' +
            '    </div>' +
            '    <div class="controls">' +
            '      <button class="close-button"><i class="fa-solid fa-xmark"></i></button>' +
            '    </div>' +
            '  </div>' +
            '  <div class="inner-container">' +
            '    <div></div>' +
            '  </div>' +
            '</div>';
        this.eleTerminalContainer.setAttribute('id', 'fdrive-apps-terminal');
        this.eleTerminal = this.eleTerminalContainer.querySelector('.inner-container div');
        this.eleTerminalAddress = this.eleTerminalContainer.querySelector('.address-bar input[type=text]');
        this.eleTerminalFavicon = this.eleTerminalContainer.querySelector('.address-bar img');
        this.eleTerminalContainer.querySelector('.close-button').addEventListener('click', () => this.closeTerminal(this.openedTerminal));
        document.querySelector('.dashboard-main').appendChild(this.eleTerminalContainer);
    }
    
    initApps(){
        for(let app of this.container.querySelectorAll('.drive-app'))
        {
            app.addEventListener('click', (event) => {
               this.openApp(app, event.ctrlKey) 
            });
            app.addEventListener('mousedown', (e) => {
                if(e.which === 2) {
                    this.openApp(app, true)
                }
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
    
    async openApp(app, newTab)
    {
        for(let ele of this.container.querySelectorAll('.drive-app.selected'))
            ele.classList.remove('selected');

        app.classList.add('selected');
        
        let type = app.getAttribute('data-app-type').toLowerCase();
        
        let url = app.getAttribute('data-src');
        if(type === 'ssh')
        {
            if(newTab)
                return; // dont support this yet
            let uid = app.getAttribute('x-uid');
            if(this.openedTerminal === uid)
                return; // already opened

            this.closeIframe();
            this.closeTerminal(this.openedTerminal);
            this.openedTerminal = uid;
            this.eleTerminalFavicon.src = app.querySelector('img').src;
            this.eleTerminalContainer.className = 'visible';
            document.body.classList.add('drawer-item-opened');
            let user = '';
            let pwd = ''
            let server = url;
            let atIndex = url.indexOf('@');
            if(atIndex > 0) {
                server = url.substring(atIndex + 1);
                user = url.substring(0, atIndex);
                let colonIndex = user.indexOf(':');
                if (colonIndex > 0) {
                    pwd = user.substring(colonIndex + 1);
                    user = user.substring(0, colonIndex);
                }
            }
            this.eleTerminalAddress.value = server;
            this.fenrusTerminal = new FenrusTerminal({container: this.eleTerminal, server: server, user: user, password: pwd});
            this.fenrusTerminal.onDisconnected(() => {
                this.closeTerminal(uid);
            });
            return;
        }
        
        if(type === 'vnc'){
            const regex = /^((?:[a-fA-F0-9]{1,4}:){7}[a-fA-F0-9]{1,4}|(?:[a-fA-F0-9]{1,4}:)*:[a-fA-F0-9]{1,4}|(?:\d{1,3}\.){3}\d{1,3}|[a-zA-Z0-9-]+(?:\.[a-zA-Z0-9-]+)*)(?::(\d{1,5}))?$/;

            const match = regex.exec(url);
            const hostname = match[1];
            const port = match[2] || 5900;
            //url = `/NoVNC/vnc_lite.html?host=${encodeURIComponent(hostname)}&port=${encodeURIComponent(port)}&scale=true`;
            url = `/NoVNC/vnc_lite.html?scale=true&path=websockify/${encodeURIComponent(hostname)}/${encodeURIComponent(port)}`;
        }        
        
        if(type === 'external' || newTab)
        {
            window.open(url, "_blank", "noopener,noreferrer");
            return;
        }
        else if(type === 'externalsame')
        {
            let a = document.createElement('a');
            a.setAttribute('href', url);
            a.setAttribute('target', 'fenrus-popup');
            a.style.display ='none';
            document.body.appendChild(a);
            a.click();
            a.remove();
            return;
        }
        else if(type === 'internal' && !newTab){
            this.divLaunchingApp.querySelector('.title').textContent = 'Launching ' + app.querySelector('.name').textContent;
            this.divLaunchingApp.querySelector('img').src = app.querySelector('img').src;
            this.divLaunchingApp.style.display = 'unset';
            window.location.href = url;
            return;
        }
        this.closeTerminal(this.openedTerminal);
        this.eleIframe.src = url;
        this.eleIframeAddress.value = url;
        
        this.eleIframeFavicon.src = app.querySelector('img').src;

        this.eleIframeContainer.className = 'visible';
        document.body.classList.add('drawer-item-opened');
    }

    closeIframe(){
        this.eleIframeContainer.className = '';
        for(let ele of this.container.querySelectorAll('.selected'))
            ele.classList.remove('selected');
        document.body.classList.remove('drawer-item-opened');
    }

    closeTerminal(uid){
        if(uid && uid !== this.openedTerminal)
            return; // opening a different terminal, dont close this
        if(this.fenrusTerminal)
        {
            this.fenrusTerminal.onDisconnected(null);
            this.fenrusTerminal.close();
        }
        this.openedTerminal = null;
        this.fenrusTerminal = null;
        this.eleTerminalContainer.className = '';
        for(let ele of this.container.querySelectorAll('.selected'))
            ele.classList.remove('selected');
        document.body.classList.remove('drawer-item-opened');
    }
}