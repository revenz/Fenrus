class FenrusDriveEmail
{
    initDone = false;
    constructor(){
        this.container = document.getElementById('email-actual');
        this.eleMessage = document.createElement('div');
        this.eleMessage.setAttribute('id', 'fdrive-email-message');
        document.querySelector('.dashboard-main').appendChild(this.eleMessage);

        document.addEventListener('fenrusEmail', (event) => {
            console.log('received event: ', event);
            if(event.detail.event === 'refresh') {
                console.log('refresh emails');
                this.refresh();
            }
        });
    }

    hide(){
        this.closeMessage();
    }

    show(){
        if(this.initDone)
            return;
        this.initDone = true;
        this.refresh();
    }
    
    clear(){
        this.container.innerHTML = '';
    }

    async refresh(){
        try {
            let url = '/email';
            const response = await fetch(url);
            const data = await response.json();
            this.clear();
            this.addToList(data);
        } catch (error) {
            console.log('error', error);
        }
    }
    
    addToList(items){
        if(!items || Array.isArray(items) === false)
            return;
        
        for(let item of items)
        {
            let ele = this.createElement();
            
            if(typeof(item.dateUtc) === 'string')
                item.dateUtc = new Date(item.dateUtc);

            let icon = ele.querySelector('.icon');
            let senderName = this.setInitials(item.from, icon);
            
            ele.querySelector('.sender').innerText = senderName;
            ele.querySelector('.date').innerText = this.formatDate(item.dateUtc);
            ele.querySelector('.subject').innerText = item.subject || '(No Subject)';
            ele.addEventListener('click', async () => {
                await this.openMessage(item, ele);
            });
            this.container.appendChild(ele);
        }
    }
    
    setInitials(sender, ele) {

        let senderMatch = /["][^"]+["]/.exec(sender);
        let senderName = senderMatch?.length ? senderMatch[0].slice(1, -1) : sender;
        let initials = senderName.split(" ").map(word => word.substring(0, 1).toUpperCase()).join("").slice(0, 2);
        
        ele.innerText = initials;
        if(/^[a-zA-Z]/.test(initials))
            ele.classList.add('initials-' + initials.substring(0,1).toLowerCase());
        else if(/^[\d]/.test(initials))
            ele.classList.add('initials-digit');
        else
            ele.classList.add('initials-other');
        return senderName;
    }
    
    createElement(){
        let ele = document.createElement('div')
        ele.className = 'email';
        ele.innerHTML = '<span class="icon"></span>' +
            '<span class="sender"></span>' +
            '<span class="date"></span>' +
            '<span class="subject"></span>';
        return ele;            
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
    
    async openMessage(message, ele) {
        for(let ele of this.container.querySelectorAll('.email.selected'))
            ele.classList.remove('selected');
        
        ele.classList.add('selected');
        
        this.eleMessage.className = 'visible';
        this.eleMessage.innerHTML = '' +
            '  <div class="email-header">' +  
            '    <span class="email-header-subject"></span>' +
            '    <div class="email-header-actions">' +
            '       <button class="btn-reply" title="Reply"><i class="fa-solid fa-reply"></i></button>' +
            '       <button class="btn-forward" title="Forward"><i class="fa-solid fa-share"></i></button>' +
            '       <button class="btn-delete" title="Delete"><i class="fa-solid fa-trash"></i></button>' +
            '       <button class="btn-archive" title="Archive"><i class="fa-solid fa-archive"></i></button>' +
            '       <button class="btn-close" title="Close"><i class="fa-solid fa-times"></i></button>' +
            '    </div>' +
            '    <div class="email-header-info">' +
            '      <span class="email-header-icon"></span>' +
            '      <span class="email-header-from"></span>' +
            '      <span class="email-header-to"></span>' +
            '      <span class="email-header-date"></span>' +
            '    </div>' +
            '  </div>' +
            '  <div class="email-body">' +
            '    <span class="email-body-content"></span>' +
            '  </div>';        
        document.body.classList.add('email-opened');
        let url = '/email/' + message.uid;
        const json = await this.fetchData(url);        
        console.log('json', json);
        
        this.eleMessage.querySelector('.btn-close').addEventListener('click', () => {
            this.closeMessage();
        });
        
        this.eleMessage.querySelector('.email-header-from').innerText = json.from;
        this.eleMessage.querySelector('.email-header-to').innerText = json.to.join('; ');
        this.eleMessage.querySelector('.email-header-date').innerText = this.formatDate(json.dateUtc);
        this.eleMessage.querySelector('.email-header-subject').innerText = json.subject || 'No Subject';
        this.setInitials(json.from, this.eleMessage.querySelector('.email-header-icon'));

        let target = this.eleMessage.querySelector('.email-body-content');
        let targetChild = document.createElement('div');
        let targetChild2 = document.createElement('div');
        targetChild.appendChild(targetChild2)
        target.appendChild(targetChild)
        this.getSafeHtml(json.body, targetChild2);
    }

    async fetchData(url) {
        const cache = await caches.open('fenrus-cache');
        const cachedResponse = await cache.match(url);
    
        if (cachedResponse) {
            console.log('Data found in cache');
            return cachedResponse.json();
        } else {
            console.log('Data not found in cache. Fetching from network...');
            const response = await fetch(url);
            const data = await response.json();
            await cache.put(url, new Response(JSON.stringify(data)));
            return data;
        }
    }


    closeMessage(){
        this.eleMessage.className = '';
        for(let ele of this.container.querySelectorAll('.email.selected'))
            ele.classList.remove('selected');
        document.body.classList.remove('email-opened');
    }
    getGmailMessageUrl(message) {
        let messageId = message.messageId;        
        var url = `https://mail.google.com/mail/u/0/#search/rfc822msgid:${encodeURIComponent(messageId)}`;
        console.log('url', url);
        return url;
    }

    getSafeHtml(html, sanitizedDiv) {
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
                element.removeAttribute('href');
            }
        });

        // Sanitize the HTML to remove any potentially dangerous tags and attributes
        const sanitizedHtml = DOMPurify.sanitize(div.innerHTML);
        
        // Set the innerHTML of the sanitized div element to the sanitized HTML
        sanitizedDiv.innerHTML = sanitizedHtml;
        sanitizedDiv.style.backgroundColor = '#fff';
        
        let child = sanitizedDiv.firstElementChild || sanitizedDiv;

        // Set the styles to be scoped to the sanitized div element
        sanitizedDiv.style.all = 'initial';
        sanitizedDiv.style.fontFamily = 'inherit';
        sanitizedDiv.style.fontSize = 'inherit';
        sanitizedDiv.style.color = 'inherit';

        return sanitizedDiv.innerHTML;
    }
}