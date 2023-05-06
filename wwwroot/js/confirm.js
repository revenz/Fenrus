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
function modalPrompt(title, message, value, validator) {
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
        if(validator && typeof(validator) === 'object'){
            validator = (_v) => {
                if(validator.test(_v))
                    return true;
                Toast.error('Error', 'Invalid input');
                return false;                
            } 
        }
        modal.querySelector('.confirm-ok').innerText = Translations.Ok;
        modal.querySelector('.confirm-cancel').innerText = Translations.Cancel;
        document.body.append(modal);
        modal.className = 'modal';
        modal.querySelector('.fenrus-modal-title').innerText = title;
        modal.querySelector('.fenrus-modal-body .message').innerText = message;
        let input = modal.querySelector('.fenrus-modal-body input');
        input.focus();
        if(value)
            input.value = value;
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
            if(validator && !validator(value))
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


function modalForm(title, domElement, buttons) {
    return new Promise((resolve, reject) => {
        let modal = document.createElement('div');
        modal.innerHTML = '<div class="background-overlay fenrus-modal-background-overlay" />' +
            '<div class="fenrus-modal form">' +
            (title ? '<div class="fenrus-modal-title"></div>' : '') +
            '<div class="fenrus-modal-body"><form></form></div>' +
            '<div class="fenrus-modal-footer">' +
            '</div>'
        '</div>';
        let form = modal.querySelector('form');
        let formValidate = () => {
            if (form.checkValidity() === false)
                return false;
            // Get all form inputs and construct an object from their values
            const formData = {};
            const inputs = form.querySelectorAll('input, select');
            for (let i = 0; i < inputs.length; i++) {
                const input = inputs[i];
                const name = input.getAttribute('name');
                const value = input.type === 'checkbox' ? input.checked : input.value;
                formData[name] = value;
            }
            modal.remove();
            resolve(formData);
        }
        form.addEventListener('submit', (event) => {
            console.log('form submit!');
            event.preventDefault();
            formValidate();
            return false;
        })
        if(typeof(domElement) === 'string')
            form.innerHTML = domElement;
        else
            form.appendChild(domElement);


        let footer = modal.querySelector('.fenrus-modal-footer');
        if(buttons?.length)
        {
            for(let btn of buttons){
                let eleBtn = document.createElement('button');
                eleBtn.className = 'btn';
                eleBtn.innerText = btn.label;
                eleBtn.addEventListener('click', (event) => {
                    event.stopPropagation();
                    event.preventDefault();
                    if(btn.action() === true)
                    {
                       formValidate();
                    }
                });
                footer.appendChild(eleBtn);
            }
        }
        else
        {
            footer.innerHTML = '<button class="btn confirm-ok"></button>' +
                '<button class="btn confirm-cancel"></button>';
            let btnOk = modal.querySelector('.confirm-ok');
            btnOk.innerText = Translations.Ok;
            let btnCancel = modal.querySelector('.confirm-cancel');
            btnCancel.innerText = Translations.Cancel;
            btnOk.addEventListener('click', (event) => {
                event.stopPropagation();
                event.preventDefault();
                formValidate();
            });
            btnCancel.addEventListener('click', (event) => {
                event.stopPropagation();
                event.preventDefault();
                modal.remove();
                resolve();
            });
        }
        document.body.append(modal);
        modal.className = 'modal';
        if(title)
            modal.querySelector('.fenrus-modal-title').innerText = title;
        let input = modal.querySelector('.fenrus-modal-body input, .fenrus-modal-body select');
        if(input) {
            input.focus();
            input.addEventListener('keydown', (event) => {
                if (event.key === 'Enter')
                    formValidate();
            });
        }
    });
}

function modalFormInput({label, name, type, value, required = true}){    
    let html = '<div class="modal-form-input">' +       
               `<div class="label">${htmlEncode(label)}</div>` +
                '<div class="value">';
    
    if(type === 'text')
        html += `<input type=text id="${name}" name="${name}" ${required ? 'required' : ''} ${value ? `value="${htmlEncode(value)}"` : ''} />`;
    else if(type === 'datetime')
        html += `<input type=datetime-local id="${name}" name="${name}" ${required ? 'required' : ''} ${value ? `value="${htmlEncode(value)}"` : ''} />`;
    
    html += '</div>';
    return html;
}

