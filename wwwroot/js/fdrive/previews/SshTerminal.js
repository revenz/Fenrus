class SshTerminal extends FenrusPreview
{
    open(url, icon){        
        if(url === this.url)
            return; // already opened
        if(this.url && this.url !== url)
            this.close();
        else
            this.createDomElements();
        
        this.url = url;
        this.eleTerminalFavicon.src = icon;
        this.container.className = 'visible';
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
            this.close();
        });
    }
    
    close(){
        if(this.fenrusTerminal)
        {
            this.fenrusTerminal.onDisconnected(null);
            this.fenrusTerminal.close();
        }
        this.fenrusTerminal = null;
        this.url = null;
        super.close();
    }

    createDomElements()
    {
        if(this.container)
            return;
        this.container = document.createElement('div');
        this.container.innerHTML = '<div class="terminal-container app-target-container">' +
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
        this.container.setAttribute('id', 'fdrive-apps-terminal');
        this.eleTerminal = this.container.querySelector('.inner-container div');
        this.eleTerminalAddress = this.container.querySelector('.address-bar input[type=text]');
        this.eleTerminalFavicon = this.container.querySelector('.address-bar img');
        this.container.querySelector('.close-button').addEventListener('click', () => this.close());
        document.querySelector('.dashboard-main').appendChild(this.container);
    }
}