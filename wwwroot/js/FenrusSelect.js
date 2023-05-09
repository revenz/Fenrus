var fenrusSelectors =  {};

class FenrusSelect {
    constructor(container)
    {
        fenrusSelectors[container.getAttribute('id')] = this;
        this.container = container;
        this.eleValue = container.querySelector('.fenrus-select-value');;
        this.select = this.container.querySelector('.fenrus-select-value');
        this.opened = false;
        this.select.addEventListener('click', () => {
            this.opened ? this.close() : this.open();
        });
        
        document.addEventListener('click', (event) => {
            let clickedElement = event.target;
            if (clickedElement !== container && !container.contains(clickedElement)) {
                this.close();
            }
            
        });
    }

    open(){
        this.opened = true;
        this.container.className = 'fenrus-select opened';
    }
    close(){
        this.opened = false;
        this.container.className = 'fenrus-select';        
    }
    
    setValue(value)
    {
        this.eleValue.innerText = value;
    }
}
function fenrusSelectOptionClick(event)
{
    let container = event.target.parentNode.parentNode;
    let selector = fenrusSelectors[container.getAttribute('id')];
    selector.setValue(event.target.innerText);
    selector.close();
}


document.addEventListener('DOMContentLoaded', () => {
    for(let select of document.querySelectorAll('.fenrus-select'))
    {
        new FenrusSelect(select);
    }
});