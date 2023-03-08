function changePassword(event) {
    event.preventDefault();
    event.stopPropagation()

    let modal = document.createElement('div');
    modal.innerHTML = '<div class="background-overlay fenrus-modal-background-overlay" />' +
                      '<form class="fenrus-modal change-password" method="post">' +
                        '<div class="fenrus-modal-title"></div>' +
                        '<div class="fenrus-modal-body">' +
                            '<div class="field">' +
                                '<span class="label password-current"></span>' +
                                '<span><input class="change-password-current" type="password" required /></span>' +
                            '</div>' +
                            '<div class="field">' +
                                '<span class="label password-new"></span>' +
                                '<span><input class="change-password-new" type="password" required /></span>' +
                            '</div>' +
                            '<div class="field">' +
                                '<span class="label password-confirm"></span>' +
                                '<span><input class="change-password-confirm" type="password" required /></span>' +
                            '</div>' +
                        '</div>' +
                        '<div class="fenrus-modal-footer">' +
                            '<button type="submit" class="btn confirm-ok"></button>' +
                            '<button class="btn confirm-cancel"></button>' +
                        '</div>' +
                      '</form>';
    document.body.append(modal);
    modal.className = 'modal';
    let txtCurrent = modal.querySelector('.change-password-current');
    let txtNew = modal.querySelector('.change-password-new');
    let txtConfirm = modal.querySelector('.change-password-confirm');
    txtCurrent.setAttribute('placeholder', Translations.ChangePasswordCurrent);
    txtNew.setAttribute('placeholder', Translations.ChangePasswordNew);
    txtConfirm.setAttribute('placeholder', Translations.ChangePasswordConfirm);
    modal.querySelector('.password-current').innerText = Translations.ChangePasswordCurrent;
    modal.querySelector('.password-new').innerText = Translations.ChangePasswordNew;
    modal.querySelector('.password-confirm').innerText = Translations.ChangePasswordConfirm;
    modal.querySelector('.confirm-cancel').innerText = Translations.Cancel;
    modal.querySelector('.confirm-ok').innerText = Translations.Ok;
    modal.querySelector('.fenrus-modal-title').innerText = Translations.ChangePassword;
    
    modal.querySelector('form').addEventListener('submit', async(event) =>{
        event.preventDefault();
        let current = txtCurrent.value;
        let passNew = txtNew.value;
        let passConfirm = txtConfirm.value;
        if(passNew !== passConfirm){
            toast(Translations.ChangePasswordMismatch);
            return;
        }
        fetch('/change-password', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                current: current,
                password: passNew
            })
        })
        .then(response => response.json())
        .then(result => {
            console.log('result', result);
            toast(result.message, result.success);
        })
    });
    modal.querySelector('.confirm-cancel').addEventListener('click', () => {
        modal.remove();
    });
    txtCurrent.focus();
}