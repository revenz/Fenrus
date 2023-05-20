class FenrusDriveEmail
{
    initDone = false;
    constructor(){
        this.container = document.getElementById('email-actual');
        this.eleMessage = document.createElement('div');
        this.eleMessage.setAttribute('id', 'fdrive-email-message');
        this.eleMessage.classList.add('fdrive-item-content');
        document.querySelector('.dashboard-main').appendChild(this.eleMessage);

        document.addEventListener('fenrusEmail', (event) => {
            console.log('received event: ', event);
            if(event.detail.event === 'refresh') {
                console.log('refresh emails');
                this.refresh();
            }
        });
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
            if((item.flags & 1) == 1) 
                ele.classList.add('seen');
            
            if(typeof(item.dateUtc) === 'string')
                item.dateUtc = new Date(item.dateUtc);

            let icon = ele.querySelector('.icon');
            let senderName = this.setInitials(item.from, icon);
            
            if(this.openedMessageUid === item.uid)
                ele.classList.add('selected');
            
            ele.querySelector('.sender').innerText = senderName;
            ele.querySelector('.date').innerText = this.formatDate(item.dateUtc);
            ele.querySelector('.subject').innerText = item.subject || '(No Subject)';
            ele.addEventListener('click', async () => {
                if((item.flags & 1) != 1) 
                {
                    item.flags += 1;
                    if(ele.classList.contains('seen') === false)
                        ele.classList.add('seen');
                }
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
    
    async openMessage(message, ele) 
    {
        this.openedMessageUid = message.uid;
        for(let ele of this.container.querySelectorAll('.email.selected'))
            ele.classList.remove('selected');       
        
        ele.classList.add('selected');
        
        FenrusPreview.open('email', message, (action, msg) => {
            if(action === 'delete')
                this.deleteMessage(msg);
            else if(action === 'archive')
                this.archiveMessage(msg);
        });
    }
    
    async deleteMessage(message) {
        let confirmed = await modalConfirm('Delete Message', 'Are you sure you want to delete this message?');
        if(!confirmed)
            return;
        let result = await fetch('/email/' + message.uid, {
            method: 'DELETE'
        });
        if(result.ok)
        {
            if(this.openedMessageUid === message.uid)
                FenrusPreview.closeActive('email');
            return;            
        }
        let msg = await result.text();
        Toast.error('Failed to delete', msg);
    }

    async archiveMessage(message) {
        let confirmed = await modalConfirm('Archive Message', 'Are you sure you want to archive this message?');
        if(!confirmed)
            return;
        let result = await fetch(`/email/${message.uid}/archive`, {
            method: 'PUT'
        });
        if(result.ok)
        {
            if(this.openedMessageUid === message.uid)
                FenrusPreview.closeActive('email');
            return;
        }
        let msg = await result.text();
        Toast.error('Failed to archive', msg);
    }
}