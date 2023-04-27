class FenrusDriveDrawer {
    
    eleAddMenu;eleList;
    eleInner;eleWrapper;
    visible;
    mode;
    eleFiles; eleNotes; eleCalendar;
    eleTabFiles; eleTabNotes; eleTabCalendar;

    constructor() {
        document.addEventListener('mousedown', (event) => this.mouseDownEventListener(event));
        this.eleList = document.getElementById('fdrive-list');
        this.eleInner = document.querySelector('#fdrive-wrapper .fdrive-inner');
        this.eleWrapper = document.getElementById('fdrive-wrapper');
        this.eleAddMenu = document.getElementById('fdrive-add-menu');
        this.eleFiles = document.getElementById('fdrive-files');
        this.eleNotes = document.getElementById('fdrive-notes');
        this.eleCalendar = document.getElementById('fdrive-calendar');
        this.eleTabFiles = document.getElementById('fdrive-mode-files');
        this.eleTabNotes = document.getElementById('fdrive-mode-notes');
        this.eleTabCalendar = document.getElementById('fdrive-mode-calendar');
        this.mode = localStorage.getItem('DRIVE_MODE') || 'files';
        this.width = parseInt(localStorage.getItem('DRIVE_WIDTH') || '');
        let minWidth = 100;
        if(isNaN(this.width) || this.width < 50)
        {
            let max = window.innerWidth;
            if(max < 720)
                this.width = 720;
            else
                this.width = Math.min(max * 0.4);
        }
        this.eleWrapper.style.width = this.width + 'px';
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
                this.setWidthClass(this.width);
            }
        }); 
        document.body.addEventListener('mouseup', (event) => {
            if(isResizing) {
                this.eleWrapper.classList.remove('is-resizing');
                isResizing = false;
                let width = parseInt(this.eleWrapper.style.width);
                localStorage.setItem('DRIVE_WIDTH', '' + width);
                document.body.dispatchEvent(new CustomEvent('driveResizeEvent', { width: width } ));
            }
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
            fDriveCalendar.hide();
            return;
        }        
        this.setMode(this.mode);
    }
    
    setMode(mode){
        let current = this.mode;
        if(this.mode !== mode) {
            this.mode = mode;
            localStorage.setItem('DRIVE_MODE', mode);
        }
        if(current === 'calendar')
            fDriveCalendar.hide();
        
        if(mode === 'files') {
            this.eleFiles.className = 'visible';
            this.eleNotes.className = '';
            this.eleCalendar.className = '';
            this.eleTabFiles.className = 'fdrive-mode active';
            this.eleTabNotes.className = 'fdrive-mode';
            this.eleTabCalendar.className = 'fdrive-mode';
            fDrive.show();
        }else if(mode === 'calendar'){
            this.eleFiles.className = '';
            this.eleNotes.className = '';
            this.eleCalendar.className = 'visible';
            this.eleTabFiles.className = 'fdrive-mode';
            this.eleTabNotes.className = 'fdrive-mode';
            this.eleTabCalendar.className = 'fdrive-mode active';
            fDriveCalendar.show();
        }        
        else
        {
            this.eleFiles.className = '';
            this.eleCalendar.className = '';
            this.eleNotes.className = 'visible';
            this.eleTabFiles.className = 'fdrive-mode';
            this.eleTabCalendar.className = 'fdrive-mode';
            this.eleTabNotes.className = 'fdrive-mode active';
            fDriveNotes.show();
        }
        
    }
    
    mouseDownEventListener(event) {
        // if(this.eleWrapper) {
        //     let wrapper = event.target.closest('#fdrive-wrapper, .fenrus-modal, .blocker, .fdrive-preview, .fc-highlight, .fenrus-modal-background-overlay, #fdrive-calendar-popover');
        //     if (!wrapper) {
        //         this.eleWrapper.className = 'dashboard-drive collapsed';
        //         fDriveCalendar.hide();
        //     }
        // }

        if(this.eleAddMenu) {
            let addMenu = event.target.closest('.fdrive-add-button');
            if (!addMenu)
                this.eleAddMenu.className = '';
        }
    }
}


var fDrive;
var fDriveDrawer;
var fDriveNotes;
var fDriveCalendar;
document.addEventListener("DOMContentLoaded", () => {    
    if(document.querySelector('.dashboard')) {
        fDrive = new FenrusDrive();
        fDriveNotes = new FenrusDriveNotes();
        fDriveCalendar = new FenrusDriveCalendar();
        fDriveDrawer = new FenrusDriveDrawer();
    }
});
