class SmartApp
{
    uid;
    name;
    interval;
    dashboardIntanceUid;
    renderCount = 0;
    controller;
    timerCarousel;

    constructor(args)
    {
        this.uid = args.uid || args.instanceUid;
        this.name = args.name;
        this.interval = args.interval;
        if(this.interval === 0)
            this.interval = 3000;
        this.dashboardIntanceUid = this.getDashboardInstanceUid();
        document.addEventListener('disposeDashboard', (e) => this.dispose(), false);
        this.trigger();
    }
    
    async trigger(){
        if(this.stillActive() === false)
            return;
        
        let result = await this.doWork();
        
        if(this.stillActive() === false)
            return;

        if(!result)
            return; // nothing more to do
        if(this.interval === 0)
            return; // nothing more to do
        
        setTimeout(()=> this.trigger(), this.interval);
    }
    
    getDashboardInstanceUid()
    {
        return document.getElementById('dashboard-instance')?.value;
    }

    dispose()
    {
        console.log('disposing of smart app!', this.name);
        if(this.timerCarousel)
            clearInterval(this.timerCarousel);
        this.controller?.abort();
    }

    stillActive(){
        let dashboard = this.getDashboardInstanceUid();
        if(dashboard != this.dashboardIntanceUid){
            console.log('No longer active: ' + this.name);
            return false;
        }
        return true;
    }

    async doWork() 
    {
        if(!this.stillActive())
            return false;        
        if(++this.renderCount < 2)
        {
            let saved = this.getFromLocalStorage();
            if(saved) 
            {            
                this.appSetStatus(saved.html);
                if(saved.old === false)
                {
                    // local storage is fine, dont need to fetch new data right now
                    return this.interval > 0; 
                }
            }
        }

        if(document.hasFocus() === false){
            return true; // prevent request if the page doesnt have focus
        }    

        return await this.refresh();
    }

    refresh()
    {        
        return new Promise((resolve, reject) =>
        {
            this.controller?.abort();

            this.controller = new AbortController();
            let timeoutId = setTimeout(() => this.controller.abort(), Math.min(this.interval, 5000));

            let success = true;
            let url = `/apps/${encodeURIComponent(this.name)}/${encodeURIComponent(this.uid)}/status?name=` + encodeURIComponent(this.name) + '&t=' + new Date().getTime();
            fetch(url, {
                signal: this.controller.signal
            })
            .then(res => {
                clearTimeout(timeoutId);
                timeoutId = null;
                this.controller = null;                

                let currentDashboard = this.getDashboardInstanceUid();
                if(currentDashboard != this.dashboardIntanceUid){
                    success = false;
                    return; // if they changed dashboards
                }

                if(!res.ok)
                    throw res;
                if(res.status === 302){
                    // redirect, to login            
                    success = false;
                    window.location.href = '/login';
                }
                return res.text();
            })
            .then(html => {
                if(html != undefined)
                    this.appSetStatus(html);  
            }).catch(error => {
                success = false;
                if(timeoutId)
                    clearTimeout(timeoutId);
                this.controller = null;

                let currentDashboard = this.getDashboardInstanceUid();
                if(currentDashboard != this.dashboardInstanceUid)
                    return; // if they changed dashboards
                console.log(name + ' error: ', error);    
            }).finally(() => { 
                resolve(success);
            });
        });
    }

    getItemSize()
    {
        let ele = document.getElementById(this.uid);
        if(!ele)
            return '';
        if(ele.classList.contains('x-large'))
            return 'x-large'
        if(ele.classList.contains('large'))
            return 'large'
        if(ele.classList.contains('medium'))
            return 'medium'
        if(ele.classList.contains('small'))
            return 'small';
        return '';
    }

    getFromLocalStorage()
    {
        try{
            let size = this.getItemSize();
            let item = JSON.parse(localStorage.getItem(this.uid + '-' + size));
            if(!item?.date)
                return;
            if(item.date < (new Date().getTime() - 60000))
                return { html: item.html, old: true}; // older than a minute reject it
            return { html: item.html, old: false};
        }
        catch(err) { return; }
    }

    setInLocalStorage(html)
    {
        let size = this.getItemSize(this.uid);
        localStorage.setItem(this.uid + '-' + size, JSON.stringify({
            date: new Date().getTime(),
            html: html
        }));
    }

    appSetStatus(content)
    {
        let eleItem = document.getElementById(this.uid);
        if(!eleItem)
            return;

        let ele = eleItem.querySelector('.status');
        if (ele && content) {
            this.setInLocalStorage(content);        
            if(/^:carousel:/.test(content)){
                content = content.substring(10);
                let index = content.indexOf(':');
                let carouselId = content.substring(0, index);
                content = content.substring(index + 1);
                if(!this.timerCarousel)
                    this.carouselTimer(carouselId);
                this.setItemClass(eleItem, 'carousel');
                ele.innerHTML = content;
                let indexes = ele.querySelectorAll('.controls a');
                console.log(indexes);
                for(let i=0;i<indexes.length;i++)
                {
                    console.log(indexes[i]);
                    indexes[i].addEventListener('click', (event) => {
                        event.stopImmediatePropagation();
                        event.preventDefault();
                        this.carouselItem(event, carouselId, i);
                    }, false);
                }
                return;
            }    
            else if(/^:bar-info:/.test(content)){
                content = content.substring(10);
                this.setItemClass(eleItem, 'bar-info');
            }
            else if(/^data:/.test(content)){
                content = `<img class="app-chart" src="${content}" />`;
                this.setItemClass(eleItem, 'chart');
            }
            else {
                this.setItemClass(eleItem, 'db-basic');
            }
            ele.innerHTML = content;


        }
    }

    setItemClass(item, className) {
        item.className = item.className.replace(/(carousel|chart|db-basic)/g, '') + ' ' + className;
    }

    carouselItem(e, id, itemIndex){
        if(e)
            e.preventDefault();
        let carousel = document.getElementById(id);    
        for(let item of carousel.querySelectorAll('.item')){
            let visible = item.classList.contains('visible');
            if(item.id === id + '-' + itemIndex) {
                item.classList.remove('hidden');
                if(visible === false)
                    item.classList.add('visible');
            }
            else if(visible)
            {
                item.classList.remove('visible');
                item.classList.add('hidden');
            }
            else
            {
                item.classList.remove('hidden');
            }
        }

        let controls = carousel.querySelectorAll('.controls a');
        for(let i=0;i<controls.length;i++){
            controls[i].classList.remove('selected');
            if(i === itemIndex)
                controls[i].classList.add('selected');
        }

        this.carouselTimer(id);
    }

    carouselTimer(id) {
        if(this.timerCarousel)
            clearInterval(this.timerCarousel);
        this.timerCarousel = setInterval(() => {
            let carousel = document.getElementById(id);
            if(!carousel)
                return; // happens if the carousel was replaced with newer html
            
            let visible = carousel.querySelector('.item.visible');
            let index = parseInt(visible.id.substring(visible.id.indexOf('-') + 1), 10);
            ++index;
            let hasNext = document.getElementById(id + '-' + index);
            this.carouselItem(null, id, hasNext ? index : 0);
        }, 5000);
    }
}