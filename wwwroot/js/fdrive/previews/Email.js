class Email extends FenrusPreview
{
    async open(message, messageActionCallback)
    {
        if(this.message === message)
            return; // already opened
        
        this.createDomElements();
        this.messageActionCallback = messageActionCallback;
        this.message = message;
        this.setInitials(message.from);
        this.eleFrom.innerText = message.from;
        this.eleTo.innerText = message.to.join('; ');
        this.eleDate.innerText = this.formatDate(message.dateUtc);
        this.eleSubject.innerText = message.subject || 'No Subject';
        let url = '/email/' + message.uid;
        super.showBlocker();
        const json = await this.fetchData(url);        

        this.eleFrom.innerText = json.from;
        this.eleTo.innerText = json.to.join('; ');
        this.eleDate.innerText = this.formatDate(json.dateUtc);
        this.eleSubject.innerText = json.subject || 'No Subject';
        this.setInitials(json.from);
        this.setSafeHtml(json.body);

        super.hideBlocker();

        super.open();
    }
    
    close(){
        this.message = null;
        super.close();
    }
    
    createDomElements(){
        if(this.container)
            return;

        this.container = document.createElement('div');
        this.container.setAttribute('id', 'fdrive-email-message');
        this.container.className = 'fdrive-item-content';
        this.container.innerHTML = '' +
            '  <div class="email-header header">' +
            '    <span class="email-header-subject"></span>' +
            '    <div class="email-header-actions">' +
            '       <button class="btn-reply" title="Reply"><i class="fa-solid fa-reply"></i></button>' +
            '       <button class="btn-forward" title="Forward"><i class="fa-solid fa-share"></i></button>' +
            '       <button class="btn-archive" title="Archive"><i class="fa-solid fa-boxes-packing"></i></button>' +
            '       <button class="btn-delete" title="Delete"><i class="fa-solid fa-trash"></i></button>' +
            '       <button class="btn-close" title="Close"><i class="fa-solid fa-times"></i></button>' +
            '    </div>' +
            '    <div class="email-header-info">' +
            '      <span class="email-header-icon"></span>' +
            '      <span class="email-header-from"></span>' +
            '      <span class="email-header-to"></span>' +
            '      <span class="email-header-date"></span>' +
            '    </div>' +
            '  </div>' +
            '  <div class="email-body body">' +
            '    <span class="email-body-content"></span>' +
            '  </div>';
        this.eleFrom = this.container.querySelector('.email-header-from');
        this.eleTo = this.container.querySelector('.email-header-to');
        this.eleDate = this.container.querySelector('.email-header-date');
        this.eleSubject = this.container.querySelector('.email-header-subject');
        this.eleIcon = this.container.querySelector('.email-header-icon');
        let target = this.container.querySelector('.email-body-content');
        let targetChild = document.createElement('div');
        let targetChild2 = document.createElement('div');
        targetChild.appendChild(targetChild2)
        target.appendChild(targetChild)
        this.eleEmailBody = targetChild2;

        this.container.querySelector('.btn-close').addEventListener('click', () => {
            this.close();
        });
        this.container.querySelector('.btn-delete').addEventListener('click', () => {
            this.deleteMessage();
        });
        this.container.querySelector('.btn-archive').addEventListener('click', () => {
            this.archiveMessage();
        });
        document.querySelector('.dashboard-main').appendChild(this.container);
    }
    
    deleteMessage() {
        if(this.messageActionCallback)
            this.messageActionCallback('delete', this.message);   
    }
    archiveMessage(){
        if(this.messageActionCallback)
            this.messageActionCallback('archive', this.message);        
    }

    setInitials(sender) {

        let senderMatch = /["][^"]+["]/.exec(sender);
        let senderName = senderMatch?.length ? senderMatch[0].slice(1, -1) : sender;
        let initials = senderName.split(" ").map(word => word.substring(0, 1).toUpperCase()).join("").slice(0, 2);

        this.eleIcon.innerText = initials;
        if(/^[a-zA-Z]/.test(initials))
            this.eleIcon.classList.add('initials-' + initials.substring(0,1).toLowerCase());
        else if(/^[\d]/.test(initials))
            this.eleIcon.classList.add('initials-digit');
        else
            this.eleIcon.classList.add('initials-other');
        return senderName;
    }

    setSafeHtml(html) {
        // Create a new div element
        const div = document.createElement('div');

        // Set the innerHTML of the div element to the email's HTML
        div.innerHTML = html;

        // Remove all external resources (e.g. images, stylesheets)
        Array.from(div.getElementsByTagName('*')).forEach((element) => {
            const tag = element.tagName.toLowerCase();
            if (tag === 'script' || tag === 'link' || tag === 'iframe') {
                element.remove();
            } else if (tag === 'img' && element.src.startsWith('http')) {
                element.src = '/proxy/safe-image/' + btoa(element.src).replace(/\//g, '-');
            } else if (tag === 'a') {
                // element.removeAttribute('href');
                element.setAttribute('rel', 'noopener noreferrer');
            }
        });

        // Sanitize the HTML to remove any potentially dangerous tags and attributes
        const sanitizedHtml = DOMPurify.sanitize(div.innerHTML);

        // Set the innerHTML of the sanitized div element to the sanitized HTML
        this.eleEmailBody.innerHTML = sanitizedHtml;
        this.eleEmailBody.style.backgroundColor = '#fff';

        let child = this.eleEmailBody.firstElementChild || this.eleEmailBody;

        // Set the styles to be scoped to the sanitized div element
        this.eleEmailBody.style.all = 'initial';
        this.eleEmailBody.style.fontFamily = 'inherit';
        this.eleEmailBody.style.fontSize = 'inherit';
        this.eleEmailBody.style.color = 'inherit';

        return this.eleEmailBody.innerHTML;
    }
    formatDate(date){
        if(!date)
            return '';

        if(typeof(date) === 'string')
            date = new Date(date);
        let today = new Date();
        let yesterday = new Date();
        yesterday.setDate(yesterday.getDate() - 1);

        if (date.toDateString() === today.toDateString())
            return date.toLocaleString('default', { hour: 'numeric', minute: 'numeric', hour12: true });
        else if (date.toDateString() === yesterday.toDateString())
            return 'Yesterday';
        else if (date.getFullYear() === today.getFullYear())
            return date.toLocaleString('default', { day: 'numeric', month: 'short' }); // Shows "d MMM"

        return date.toLocaleString('default', { day: 'numeric', month: 'short', year: 'numeric' });
    }
}