function openIframe(event, app){
    event?.preventDefault();
    event?.stopPropagation();
    if(typeof(app) === 'string')
        app = JSON.parse(app);

    let appItem = document.getElementById(app.Uid);
    if(!appItem)
        return;
    let group = appItem.parentNode;

    let div = document.createElement('div');
    div.className = 'iframe-content';

    let side = document.createElement('div');
    side.className = 'side';
    div.appendChild(side);

    let iframe = document.createElement('iframe');

    const addItem  = function(item)
    {        
        let divChild = document.createElement('a');        
        divChild.className = 'db-item db-basic db-link medium';
        let divInner = document.createElement('div');
        divInner.className = 'inner';
        divChild.appendChild(divInner);
        let appImg = item?.querySelector('img');
        if(appImg){
            let img = document.createElement('img');
            img.src = appImg.src;
            let imgWrapper = document.createElement('div');
            imgWrapper.className = 'icon';
            imgWrapper.appendChild(img);
            divInner.appendChild(imgWrapper);
        }
        else
        {
            let img = document.createElement('i');
            img.className = 'fa-solid fa-times';
            let imgWrapper = document.createElement('div');
            imgWrapper.className = 'icon';
            imgWrapper.appendChild(img);
            divInner.appendChild(imgWrapper);
        }

        let divContent = document.createElement('div');
        divContent.className = 'content';

        let divTitle = document.createElement('div');
        divTitle.className = 'title';
        divTitle.innerText = item ? item.querySelector('.title').innerText : 'Close';
        divContent.appendChild(divTitle);
        divInner.appendChild(divContent);

        divChild.addEventListener('click', (event) => {
            event.preventDefault();
            if(item === null){
                div.classList.add('closing');
                setTimeout(()=> {
                    div.remove();
                }, 500);
                return false;
            }
            else if(item.className.indexOf('iframe') > 0) {
                iframe.setAttribute('src', item.getAttribute('href'));
            }
            else {
                item.click();
            }
            return false;
        });
        side.appendChild(divChild);
    }
    addItem(null);

    for(let item of group.querySelectorAll('.db-item'))
    {
        addItem(item);
    }

    iframe.setAttribute('seamless', true);
    iframe.setAttribute('src', app.Url);
    iframe.setAttribute('frameBorder', 0);
    div.appendChild(iframe);

    document.body.appendChild(div);
}