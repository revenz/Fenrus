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
    }
    
    toggle(){
        this.visible = !this.visible;
        this.eleWrapper.className =  this.visible ? 'expanded' : 'collapsed';
        if(!this.visible)
            return;
        this.setMode(this.mode);
    }
    
    setMode(mode){
        if(this.mode !== mode) {
            this.mode = mode;
            localStorage.setItem('DRIVE_MODE', mode);
        }
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
        if(this.eleWrapper) {
            let wrapper = event.target.closest('#fdrive-wrapper, .fenrus-modal, .blocker, .fdrive-preview, .fc-highlight, .fenrus-modal-background-overlay');
            if (!wrapper)
                this.eleWrapper.className = 'collapsed';
        }

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
        fDriveDrawer = new FenrusDriveDrawer();
        fDrive = new FenrusDrive();
        fDriveNotes = new FenrusDriveNotes();
        fDriveCalendar = new FenrusDriveCalendar();
    }
});
