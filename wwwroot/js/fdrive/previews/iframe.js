class IFrame extends FenrusPreview
{
    open(url, icon){
        this.createDomElements();
        
        this.eleIframe.src = url;
        this.eleIframeAddress.value = url;
        this.eleIframeFavicon.src = icon;

        this.container.className = 'visible';
        document.body.classList.add('drawer-item-opened');
        
        super.open();
    }
    close(){
        
        super.close();
    }
    
    createDomElements(){
        if(this.container)
            return;
        this.container = document.createElement('div');
        this.container.innerHTML = '<div class="browser-container app-target-container">' +
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
        this.container.setAttribute('id', 'fdrive-apps-iframe');
        this.eleIframe = this.container.querySelector('iframe');
        this.eleIframeAddress = this.container.querySelector('.address-bar input[type=text]');
        this.eleIframeFavicon = this.container.querySelector('.address-bar img');
        this.container.querySelector('.close-button').addEventListener('click', () => this.close());
        this.container.querySelector('.open-new-tab').addEventListener('click', () => {
            let url = this.eleIframeAddress.value;
            if(!url)
                return;
            window.open(url, "_blank", "noopener,noreferrer");
        });
        this.eleIframe.setAttribute('sandbox', 'allow-scripts allow-same-origin');

        document.querySelector('.dashboard-main').appendChild(this.container);
    }
}