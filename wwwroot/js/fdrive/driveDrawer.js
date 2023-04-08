class FenrusDriveDrawer {
    
    eleAddMenu;eleList;
    eleInner;eleWrapper;
    visible;
    mode;
    eleFiles; eleNotes;
    eleTabFiles; eleTabNotes;

    constructor() {
        document.addEventListener('mousedown', (event) => this.mouseDownEventListener(event));
        this.eleList = document.getElementById('fdrive-list');
        this.eleInner = document.querySelector('#fdrive-wrapper .fdrive-inner');
        this.eleWrapper = document.getElementById('fdrive-wrapper');
        this.eleAddMenu = document.getElementById('fdrive-add-menu');
        this.eleFiles = document.getElementById('fdrive-files');
        this.eleNotes = document.getElementById('fdrive-notes');
        this.eleTabFiles = document.getElementById('fdrive-mode-files');
        this.eleTabNotes = document.getElementById('fdrive-mode-notes');
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
            this.eleTabFiles.className = 'fdrive-mode active';
            this.eleTabNotes.className = 'fdrive-mode';
            fDrive.show();
        }else{
            this.eleFiles.className = '';
            this.eleNotes.className = 'visible';
            this.eleTabFiles.className = 'fdrive-mode';
            this.eleTabNotes.className = 'fdrive-mode active';
            fDriveNotes.show();
        }
        
    }
    
    mouseDownEventListener(event) {
        let wrapper = event.target.closest('#fdrive-wrapper, .fenrus-modal, .blocker');
        if (!wrapper)
            this.eleWrapper.className = 'collapsed';

        let addMenu = event.target.closest('.fdrive-add-button');
        if (!addMenu)
            this.eleAddMenu.className = '';
    }
}


var fDrive;
var fDriveDrawer;
var fDriveNotes;
document.addEventListener("DOMContentLoaded", () => {    
    fDriveDrawer = new FenrusDriveDrawer();
    fDrive = new FenrusDrive();
    fDriveNotes = new FenrusDriveNotes();
});
