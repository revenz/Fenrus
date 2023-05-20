class SlideShow extends FenrusPreview
{
    constructor() {
        super();
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
        if(!this.container)
            this.createDomElements();

        if(this.visible)
            this.initImage();
        super.open();
    }
    
    close() {
        super.close();
        document.removeEventListener('keydown', this.onKeyDown);
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
        this.container = document.createElement("div");
        this.container.setAttribute('id', 'fdrive-files-item');
        this.container.className = "fdrive-slideshow fdrive-preview fdrive-item-content";
        this.container.innerHTML = `
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
        this.container.appendChild(backgroundDiv);

        const closeButton = this.container.querySelector('.btn-close');
        closeButton.addEventListener("click", () => this.close());

        const prevButton = this.container.querySelector('.btn-previous');
        prevButton.addEventListener("click", () => this.prevImage());

        const nextButton = this.container.querySelector('.btn-next');
        nextButton.addEventListener("click", () => this.nextImage());
        
        this.image = this.container.querySelector('img');
        this.image.onload = () => {
            this.updateDimensions();
        };

        this.filename =  this.container.querySelector('.title');
        this.dimensions = this.container.querySelector('.slideshow-dimensions');

        //this.updateDimensions();
        // Add slideshow elements to the page
        document.querySelector('.dashboard-main').appendChild(this.container);
        this.initImage();
    }

}