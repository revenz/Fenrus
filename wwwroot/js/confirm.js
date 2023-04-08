function modalConfirm(title, message) {
    return new Promise((resolve, reject) => {
        let modal = document.createElement('div');
        modal.innerHTML = '<div class="background-overlay fenrus-modal-background-overlay" />' +
                          '<div class="fenrus-modal confirm">' +
                            '<div class="fenrus-modal-title"></div>' +
                            '<div class="fenrus-modal-body"></div>' +
                            '<div class="fenrus-modal-footer">' +
                                '<button class="btn confirm-ok"></button>' +
                                '<button class="btn confirm-cancel"></button>' +
                            '</div>'
                          '</div>';
        modal.querySelector('.confirm-ok').innerText = Translations.Ok;
        modal.querySelector('.confirm-cancel').innerText = Translations.Cancel;
        document.body.append(modal);
        modal.className = 'modal';
        modal.querySelector('.fenrus-modal-title').innerText = title;
        modal.querySelector('.fenrus-modal-body').innerText = message;
        modal.querySelector('.confirm-ok').addEventListener('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            modal.remove();
            resolve(true);
        });
        modal.querySelector('.confirm-cancel').addEventListener('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            modal.remove();
            resolve(false);
        });
    });
}
function modalMessage(title, message) {
    return new Promise((resolve, reject) => {
        let modal = document.createElement('div');
        modal.innerHTML = '<div class="background-overlay fenrus-modal-background-overlay" />' +
            '<div class="fenrus-modal confirm">' +
            '<div class="fenrus-modal-title"></div>' +
            '<div class="fenrus-modal-body"></div>' +
            '<div class="fenrus-modal-footer">' +
            '<button class="btn confirm-ok"></button>' +
            '</div>'
        '</div>';
        modal.querySelector('.confirm-ok').innerText = Translations.Ok;
        document.body.append(modal);
        modal.className = 'modal';
        modal.querySelector('.fenrus-modal-title').innerText = title;
        modal.querySelector('.fenrus-modal-body').innerText = message;
        modal.querySelector('.confirm-ok').addEventListener('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            modal.remove();
            resolve();
        });
    });
}
function modalPrompt(title, message) {
    return new Promise((resolve, reject) => {
        let modal = document.createElement('div');
        modal.innerHTML = '<div class="background-overlay fenrus-modal-background-overlay" />' +
            '<div class="fenrus-modal prompt">' +
            '<div class="fenrus-modal-title"></div>' +
            '<div class="fenrus-modal-body"><div class="message"></div><input type="text" required /></div>' +
            '<div class="fenrus-modal-footer">' +
            '<button class="btn confirm-ok"></button>' +
            '<button class="btn confirm-cancel"></button>' +
            '</div>'
        '</div>';
        modal.querySelector('.confirm-ok').innerText = Translations.Ok;
        modal.querySelector('.confirm-cancel').innerText = Translations.Cancel;
        document.body.append(modal);
        modal.className = 'modal';
        modal.querySelector('.fenrus-modal-title').innerText = title;
        modal.querySelector('.fenrus-modal-body .message').innerText = message;
        let input = modal.querySelector('.fenrus-modal-body input');
        input.focus();
        let btnOk = modal.querySelector('.confirm-ok');
        input.addEventListener('keydown', (event) => {
            if(event.key === 'Enter')
                btnOk.click();
        });
        btnOk.addEventListener('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            let value = input.value.trim();
            if(!value)
                return;
            modal.remove();
            resolve(value);
        });
        modal.querySelector('.confirm-cancel').addEventListener('click', (event) => {
            event.stopPropagation();
            event.preventDefault();
            modal.remove();
            resolve();
        });
    });
}