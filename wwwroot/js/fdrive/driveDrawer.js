class FenrusDriveDrawer {
    
    eleAddMenu;
    eleWrapper;
    visible;
    mode;

    constructor() {
        document.addEventListener('mousedown', (event) => this.mouseDownEventListener(event));
        this.features = [];
        if(document.getElementById('fdrive-notes'))
            this.features.push({ name: 'Notes', instance: new FenrusDriveNotes(), icon: 'fa-solid fa-file-pen' });
        if(document.getElementById('fdrive-files'))
            this.features.push({ name: 'Files', instance: new FenrusDrive(), icon: 'fa-solid fa-hard-drive' });
        if(document.getElementById('fdrive-calendar'))
            this.features.push({ name: 'Calendar', instance: new FenrusDriveCalendar(), icon: 'fa-solid fa-calendar', displayName: 'Cal' });
        if(document.getElementById('fdrive-email'))
            this.features.push({ name: 'Email', instance: new FenrusDriveEmail(), icon: 'fa-solid fa-envelope' });
        if(document.getElementById('fdrive-apps'))
            this.features.push({ name: 'Apps', instance: new FenrusDriveApps(), icon: '/images/apps.svg'});
        
        let ftabs = document.getElementById('fdrive-tabs');
        for(let i=0;i<this.features.length;i++)
        {
            let f = this.features[i];
            f.tab = document.createElement('div');
            f.tab.className = 'fdrive-mode';
            f.tab.addEventListener('click', (e) => {
                this.setMode(i);
            });
            f.show = () => { if(f.instance.show) f.instance.show(); }
            f.hide = () => { if(f.instance.hide) f.instance.hide(); }
            let faIcon = f.icon.indexOf('fa-') >= 0;
            let svg = f.icon.startsWith('<svg');
            f.tab.innerHTML = ( svg ? f.icon : faIcon ? `<i class="${f.icon}"></i>` : `<img src="${f.icon}" />` ) +
                                `<span class="label">${f.displayName || f.name}</span>`;
            f.container = document.getElementById('fdrive-' + f.name.toLowerCase().replace(/\s/, ''));
            ftabs.appendChild(f.tab);
        }
        let mode = (localStorage.getItem('DRIVE_MODE') || 'files').toLocaleLowerCase();
        this.selectedFeature = this.features.findIndex(x => x.name.toLowerCase().localeCompare(mode) === 0);
        if(this.selectedFeature < 0)
            this.selectedFeature = 0;
                
        this.features[this.selectedFeature].show();
        
        this.eleWrapper = document.getElementById('fdrive-wrapper');
        this.width = parseInt(localStorage.getItem('DRIVE_WIDTH') || '');
        let minWidth = 400;
        if(isNaN(this.width) || this.width < 50)
        {
            let max = window.innerWidth;
            if(max < 720)
                this.width = 720;
            else
                this.width = Math.min(max * 0.4);
        }
        this.eleWrapper.style.width = this.width + 'px';
        this.eleWrapper.style.minWidth = this.width + 'px';
        this.setWidthClass(this.width);

        this.visible = localStorage.getItem('DRIVE_VISIBLE') === '1';
        if(this.visible){
            this.visible = false;
            this.toggle();
        }
        setTimeout(() => {
            this.eleWrapper.classList.add('init-done');
        }, 500);
        
        let isResizing = false;
        document.querySelector('#fdrive-wrapper .resizer').addEventListener('mousedown', (event) => {
            isResizing = true;            
            this.eleWrapper.classList.add('is-resizing');
        });
        document.body.addEventListener('mousemove', (event) => {
            if (isResizing) {
                this.width = Math.max(minWidth, event.pageX);
                this.eleWrapper.style.width = this.width + 'px';
                this.eleWrapper.style.minWidth = this.width + 'px';
                this.setWidthClass(this.width);
            }
        }); 
        let saveResize = () => {
            this.eleWrapper.classList.remove('is-resizing');
            isResizing = false;
            let width = parseInt(this.eleWrapper.style.width);
            localStorage.setItem('DRIVE_WIDTH', '' + width);
            document.body.dispatchEvent(new CustomEvent('driveResizeEvent', { width: width } ));            
        }
        document.body.addEventListener('mouseup', (event) => {
            if(isResizing)
                saveResize();
        });
        document.body.addEventListener('mouseleave', (event) => {
            if(isResizing)
                saveResize();
        });
    }
    
    setWidthClass(width){
        this.eleWrapper.className =this.eleWrapper.className.replace(/small|medium|large/, '');
        if(width > 900)
            this.eleWrapper.classList.add('large');
        else if(width > 600)
            this.eleWrapper.classList.add('medium');
        else
            this.eleWrapper.classList.add('small');
    }
    
    toggle(){
        this.visible = !this.visible;
        this.eleWrapper.classList.remove('expanded');
        this.eleWrapper.classList.remove('collapsed');
        this.eleWrapper.classList.add(this.visible ? 'expanded' : 'collapsed');
        localStorage.setItem('DRIVE_VISIBLE', this.visible ? '1' : '0');
        if(!this.visible){
            this.features[this.selectedFeature].hide();
            return;
        }        
        this.setMode(this.selectedFeature);
    }
    
    setMode(feature){
        let current = this.selectedFeature;
        if(this.selectedFeature !== feature) {
            this.selectedFeature = feature;
            localStorage.setItem('DRIVE_MODE', this.features[feature].name);

            this.features[current].hide();
        }
        for(let i=0;i<this.features.length;i++)
        {
            let f = this.features[i];
            f.tab.className = 'fdrive-mode' + (i === feature ? ' active' : '');
            f.container.className = i === feature ? 'visible' : '';
        }
        this.features[feature].show();        
    }
    
    mouseDownEventListener(event) {
        if(this.eleAddMenu) {
            let addMenu = event.target.closest('.fdrive-add-button');
            if (!addMenu)
                this.eleAddMenu.className = '';
        }
    }
}


var fDriveDrawer;
document.addEventListener("DOMContentLoaded", () => {    
    if(document.querySelector('.dashboard')) {
        fDriveDrawer = new FenrusDriveDrawer();
    }
});
