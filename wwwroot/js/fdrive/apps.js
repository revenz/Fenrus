class FenrusDriveApps
{
    initDone = false;
    constructor(){
        this.divLaunchingApp = document.getElementById('launching-app');
        this.container = document.getElementById('apps-actual');
       
        this.initApps();
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

    show(){
        if(this.initDone)
            return;
        this.initDone = true;
    }
    
    hide(){
        this.clearSelected();        
    }

    clear(){
        this.container.innerHTML = '';
    }
    
    clearSelected(){
        for(let ele of this.container.querySelectorAll('.drive-app.selected'))
            ele.classList.remove('selected');
    }
    
    async openApp(app, newTab)
    {        
        let addSelectedClass = () =>
        {
            this.clearSelected();
            app.classList.add('selected');
        }
        
        let type = app.getAttribute('data-app-type').toLowerCase();
        
        let url = app.getAttribute('data-src');
        if(type === 'ssh')
        {
            if(newTab)
                return; // dont support this yet
            
            addSelectedClass();

            FenrusPreview.open('ssh', 
                url,
                app.querySelector('img').src
            );
            return;
        }
        if(type === 'docker')
        {
            if(newTab)
                return; // dont support this yet

            addSelectedClass();

            FenrusPreview.open('docker',
                app.querySelector('.name').textContent,
                url,
                app.querySelector('img').src
            );
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
        addSelectedClass();
        
        FenrusPreview.open('iframe', url, app.querySelector('img').src);
    }
}