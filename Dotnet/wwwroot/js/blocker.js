
var blockerInstance;

function showBlocker(message){
    blockerInstance ??= new Blocker();
    blockerInstance.show(message);
}
function hideBlocker(){
    if(!blockerInstance)
        return;
    blockerInstance.hide();
}

class Blocker {

    messages = [];
    eleContainer;
    eleMessage;

    show(message) {
        this.messages.push(message || '');
        this.update();
    }

    hide(){
        if(this.messages.length < 0)
            return;
        this.messages.pop();
        this.update();
    }

    update(){
        if(!this.eleContainer){
            if(!this.messages.length)
                return;
            this.eleContainer = document.createElement('div');
            this.eleContainer.className = 'blocker';
            this.eleContainer.innerHTML = `
            <div class="blocker-indicator">
                <div class="blocker-spinner"></div>
                <div class="blocker-message"></div>
            </div>`;
            document.body.append(this.eleContainer);
            this.eleMessage = this.eleContainer.querySelector('.blocker-message');
        }

        this.eleContainer.classList.remove('visible');
        if(!this.messages.length)
            return; 
        this.eleContainer.classList.add('visible');
        let msg = this.messages[this.messages.length - 1];
        this.eleMessage.className = 'blocker-message ' + (msg ? 'visible' : 'hidden');
        this.eleMessage.innerText = msg || '';
    }
}