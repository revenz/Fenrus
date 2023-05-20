class SlideShow
{
    constructor() {
        this.onKeyDown = (event) => {
            if (event.key === 'ArrowLeft')
                this.prevImage();
            else if (event.key === 'ArrowRight')
                this.nextImage();
            else if (event.key === 'Escape' || event.key === 'Backspace')
                this.close();
        }
    }
    open(files, startElement) 
    {
        this.images = files.filter(x => x.mimeType.startsWith('image'));
        if(!this.images || this.images.length < 1)
            return;
        
        this.index = this.images.indexOf(startElement);
        if(this.index < 0)
            return;
        document.addEventListener('keydown', this.onKeyDown);
        if(!this.slideshowDiv)
            this.createDomElements();
        if(this.slideshowDiv.className.indexOf('visible') < 0)
            this.slideshowDiv.classList.add('visible');
        if(document.body.className.indexOf('drawer-item-opened') < 0)
            document.body.classList.add('drawer-item-opened');
        if(this.visible)
            this.initImage();
        this.visible = true;
    }
    
    close() {
        if(this.slideshowDiv)
            this.slideshowDiv.classList.remove('visible');
        document.body.classList.remove('drawer-item-opened');
        document.removeEventListener('keydown', this.onKeyDown);
        this.visible = false;
    }
    
    prevImage()
    {
        this.index--;
        if (this.index < 0) {
            this.index = this.images.length - 1;
        }
        this.initImage();
    }
    nextImage() {
        this.index++;
        if (this.index >= this.images.length) {
            this.index = 0;
        }
        this.initImage();
    }
    
    initImage(){
        let img = this.images[this.index];
        this.image.src = 'files/media?path=' + encodeURIComponent(img.fullPath);
        this.filename.textContent = img.name;        
    }

    updateDimensions()
    {
        this.dimensions.textContent = `${this.image.naturalWidth} x ${this.image.naturalHeight}`;
    }
    
    createDomElements(){       
        // Create slideshow elements
        this.slideshowDiv = document.createElement("div");
        this.slideshowDiv.setAttribute('id', 'fdrive-files-item');
        this.slideshowDiv.className = "fdrive-slideshow fdrive-preview fdrive-item-content";
        this.slideshowDiv.innerHTML = `
<div class="header">
    <span class="title"></span>
    <div class="actions">
        <button class="btn-previous" title="Previous"><i class="fa-solid fa-caret-left"></i></button>
        <button class="btn-next" title="Next"><i class="fa-solid fa-caret-right"></i></button>
        <button class="btn-close" title="Close"><i class="fa-solid fa-times"></i></button>
    </div>
</div>
<div class="body">
    <img />
</div>
<div class="footer">
    <span class="slideshow-dimensions"></span>
</div>`;

        const backgroundDiv = document.createElement("div");
        backgroundDiv.className = "slideshow-background";
        backgroundDiv.addEventListener("click", () => this.close());
        this.slideshowDiv.appendChild(backgroundDiv);

        const closeButton = this.slideshowDiv.querySelector('.btn-close');
        closeButton.addEventListener("click", () => this.close());

        const prevButton = this.slideshowDiv.querySelector('.btn-previous');
        prevButton.addEventListener("click", () => this.prevImage());

        const nextButton = this.slideshowDiv.querySelector('.btn-next');
        nextButton.addEventListener("click", () => this.nextImage());
        
        this.image = this.slideshowDiv.querySelector('img');
        this.image.onload = () => {
            this.updateDimensions();
        };

        this.filename =  this.slideshowDiv.querySelector('.title');
        this.dimensions = this.slideshowDiv.querySelector('.slideshow-dimensions');

        //this.updateDimensions();
        // Add slideshow elements to the page
        document.querySelector('.dashboard-main').appendChild(this.slideshowDiv);
        this.initImage();
    }

}