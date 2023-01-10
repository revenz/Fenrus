function modalConfirm(title, message) {
    return new Promise((resolve, reject) => {
        let modal = document.createElement('div');
        modal.innerHTML = '<div class="background-overlay fenrus-modal-background-overlay" />' +
                          '<div class="fenrus-modal confirm">' +
                            '<div class="fenrus-modal-title"></div>' +
                            '<div class="fenrus-modal-body"></div>' +
                            '<div class="fenrus-modal-footer">' +
                                '<button class="btn confirm-ok">OK</button>' +
                                '<button class="btn confirm-cancel">Cancel</button>' +
                            '</div>'
                          '</div>';
        document.body.append(modal);
        modal.className = 'modal';
        modal.querySelector('.fenrus-modal-title').innerText = title;
        modal.querySelector('.fenrus-modal-body').innerText = message;
        modal.querySelector('.confirm-ok').addEventListener('click', () => {
            modal.remove();
            resolve(true);
        });
        modal.querySelector('.confirm-cancel').addEventListener('click', () => {
            modal.remove();
            resolve(false);
        });
    });
}