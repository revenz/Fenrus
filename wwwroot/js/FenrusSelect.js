function fenrusSelectValueClick(event)
{
    let container = event.target.parentNode;
    if(container.classList.contains('opened'))
        container.classList.remove('opened');
    else
        container.classList.add('opened');
}

function fenrusSelectOptionClick(event)
{
    let container = event.target.parentNode.parentNode;
    container.classList.remove('opened');
    let value = container.querySelector('.fenrus-select-value');
    value.innerText = event.target.innerText;
}