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
        this.createDomElements();
    }
    
    close() {
        this.slideshowDiv.remove();
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
        let maxWidth = document.body.clientWidth * 0.8;
        this.infoContainer.style.width =  Math.max(100, (((this.image.naturalWidth > maxWidth) ? maxWidth : this.image.naturalWidth) - 30) )+ 'px';
    }
    
    createDomElements(){

        // Create slideshow elements
        this.slideshowDiv = document.createElement("div");
        this.slideshowDiv.className = "fdrive-slideshow fdrive-preview";

        const backgroundDiv = document.createElement("div");
        backgroundDiv.className = "slideshow-background";
        backgroundDiv.addEventListener("click", () => this.close());
        this.slideshowDiv.appendChild(backgroundDiv);

        const imageContainer = document.createElement("div");
        imageContainer.className = "slideshow-image-container";
        this.slideshowDiv.appendChild(imageContainer);

        const closeButton = document.createElement("div");
        closeButton.className = "slideshow-close";
        closeButton.innerHTML = "<i class=\"fa-solid fa-xmark\"></i>";
        closeButton.addEventListener("click", () => this.close());
        this.slideshowDiv.appendChild(closeButton);

        const prevButton = document.createElement("div");
        prevButton.className = "slideshow-prev";
        prevButton.innerHTML = "&#8249;";
        prevButton.addEventListener("click", () => this.prevImage());
        this.slideshowDiv.appendChild(prevButton);

        const nextButton = document.createElement("div");
        nextButton.className = "slideshow-next";
        nextButton.innerHTML = "&#8250;";
        nextButton.addEventListener("click", () => this.nextImage());
        this.slideshowDiv.appendChild(nextButton);

        this.image = document.createElement("img");
        this.image.onload = () => {
            this.updateDimensions();
        };
        imageContainer.appendChild(this.image);

        // Display image dimensions below the image
        this.infoContainer = document.createElement('div');
        this.infoContainer.className = 'slideshow-info';
        imageContainer.appendChild(this.infoContainer);

        this.filename = document.createElement("div");
        this.filename.className = "slideshow-filename";
        this.infoContainer.appendChild(this.filename);

        this.dimensions = document.createElement("div");
        this.dimensions.className = "slideshow-dimensions";
        this.infoContainer.appendChild(this.dimensions);

        //this.updateDimensions();
        // Add slideshow elements to the page
        document.body.appendChild(this.slideshowDiv);
        this.initImage();
    }

}