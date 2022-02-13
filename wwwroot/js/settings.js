function messageBox(message, title){
    return modal(message, title || 'Message', false);
}
function confirmPrompt(message, title){
    return modal(message, title || 'Confirm', true);
}

function modal(message, title, confirm){
    return new Promise(function (resolve, reject) {
        let divBackground = document.createElement('div');
        divBackground.classList.add('modal-background');
        document.body.appendChild(divBackground);

        let divModal = document.createElement('div');
        divBackground.appendChild(divModal);
        divModal.classList.add('modal');

        let divTitle = document.createElement('div');
        divTitle.classList.add('modal-title');
        divModal.appendChild(divTitle);
        
        let divTitleSpan = document.createElement('span');
        divTitleSpan.innerText = title;
        divTitle.appendChild(divTitleSpan);

        let divMessageBody = document.createElement('div');
        divMessageBody.classList.add('modal-message');
        divModal.appendChild(divMessageBody);
        
        let divMessageSpan = document.createElement('span');
        divMessageSpan.innerText = message;
        divMessageBody.appendChild(divMessageSpan);

        // buttons
        let divButtons = document.createElement('div');
        divButtons.classList.add('modal-buttons');
        divModal.appendChild(divButtons);
        for(let i=0;i<(confirm ? 2 : 1);i++){
            let btn = document.createElement('button');
            btn.innerText = confirm ? (i == 0 ? 'Yes' : 'No') : 'OK';
            btn.classList.add('btn');
            btn.addEventListener('click', () => {
                divBackground.remove();
                if(i == 0)
                    resolve();
                else
                    reject();
            });
            divButtons.appendChild(btn);            
        }
    });
}

function toast(message, success) {
    var toast = document.getElementById('toast');
    if (!toast) {
        toast = document.createElement('toast');
        toast.setAttribute('id', 'toast');
        document.body.appendChild(toast);
    }
    toast.innerText = message;
    toast.classList.add(success ? 'success' : 'failure');
    toast.classList.add('show');

    setTimeout(() => {
        toast.classList.remove('show');
        toast.classList.remove('success');
        toast.classList.remove('failure');
    }, 3000);
}


document.addEventListener('DOMContentLoaded', (event) => {

    var dragSrcEl = null;

    function handleDragStart(e) {
        this.style.opacity = '0.4';

        dragSrcEl = this;

        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('element', this.getAttribute('id'));
    }

    function handleDragOver(e) {
        if (e.preventDefault) {
            e.preventDefault();
        }

        e.dataTransfer.dropEffect = 'move';

        return false;
    }

    function handleDragEnter(e) {
        this.classList.add('over');
    }

    function handleDragLeave(e) {
        this.classList.remove('over');
    }

    function handleDrop(e) {
        if (e.stopPropagation) {
            e.stopPropagation(); // stops the browser from redirecting.
        }

        if (dragSrcEl != this) {
            // this is the target, where we are adding it
            // dragSrcEl is the item being dragged
            this.parentNode.insertBefore(dragSrcEl, this);

            saveLayout();
        }

        return false;
    }

    function saveLayout() {
        let uids = Array.from(document.querySelectorAll('.group-list li')).map(x => x.getAttribute('id'));

        const options = {
            method: 'POST',
            body: JSON.stringify({ uids: uids}),
            headers: {
                'Content-Type': 'application/json'
            }
        }

        return fetch('/groups/order', options);
    }

    function handleDragEnd(e) {
        this.style.opacity = '1';

        items.forEach(function (item) {
            item.classList.remove('over');
        });
    }
        

    let items = document.querySelectorAll('.group-list li');
    items.forEach(function (item) {
        item.addEventListener('dragstart', handleDragStart, false);
        item.addEventListener('dragenter', handleDragEnter, false);
        item.addEventListener('dragover', handleDragOver, false);
        item.addEventListener('dragleave', handleDragLeave, false);
        item.addEventListener('drop', handleDrop, false);
        item.addEventListener('dragend', handleDragEnd, false);
    });
});