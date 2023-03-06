class SmartApp
{
    uid;
    name;
    interval;
    dashboardIntanceUid;
    renderCount = 0;
    controller;
    timerCarousel;
    icon;
    ele;
    carouselCanUpdate = true;
    carouselWaitingUpdate = null;

    constructor(args)
    {
        this.uid = args.uid || args.instanceUid;
        this.name = args.name;
        this.interval = args.interval;
        this.ele = document.getElementById(this.uid)
        this.icon = document.getElementById(this.uid).querySelector('.icon img')?.getAttribute('src');
        console.log(`app ${args.name} interval: ${this.interval}`);
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

        if(this.interval <= 0)
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
    
    getTimeString(){
        let dt = new Date();
        return String(dt.getHours()).padStart(2, '0') + ':' +
               String(dt.getMinutes()).padStart(2, '0') + ':' +
               String(dt.getSeconds()).padStart(2, '0') + '.' +
               String(dt.getMilliseconds()).padStart(4, '0');
    }

    async doWork() 
    {
        if(!this.stillActive())
            return false;
        let dt = new Date();
        console.log(`${this.getTimeString()} - SmartApp doing work: ${this.name}`);
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

        if(this.hasFocus())
            return true; // prevent a refresh if cursor is over the dashboard item        

        return await this.refresh();
    }

    refresh()
    {        
        return new Promise((resolve, reject) =>
        {
            this.controller?.abort();

            this.controller = new AbortController();
            let timeoutId = setTimeout(() => this.controller.abort(), Math.min(Math.max(this.interval, 3000), 5000));

            let success = true;
            let url = `/apps/${encodeURIComponent(this.name)}/${encodeURIComponent(this.uid)}/status?name=` + encodeURIComponent(this.name) + '&size=' + encodeURIComponent(this.getItemSize()) + '&t=' + new Date().getTime();
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
                let xIcon = res.headers.get('x-icon');
                if(xIcon){
                    this.changeIcon(atob(xIcon));
                }
                let xStatusIndicator = res.headers.get('x-status-indicator');
                if(xStatusIndicator !== undefined && xStatusIndicator !== null){
                    this.setStatusIndicator(atob(xStatusIndicator));
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
                // console.log(name + ' error: ', error);    
            }).finally(() => { 
                resolve(success);
            });
        });
    }

    changeIcon(icon) {
        icon = icon || this.icon;        
        let imgIcon = document.getElementById(this.uid).querySelector('.icon img');
        if(imgIcon)
            imgIcon.setAttribute('src', icon);
    }

    setStatusIndicator(icon) 
    {        
        let imgIcon = document.getElementById(this.uid).querySelector('.status-indicator');
        if(imgIcon)
        {
            if(icon){
                imgIcon.style.display = 'block';
                imgIcon.style.backgroundImage = `url('${icon}')`;
            }
            else{
                imgIcon.style.display = 'none';
            }
        }
    }

    hasFocus() {
        if(!this.ele)
            return false;
        if(this.ele.matches(':hover'))
            return true;
        return false;
    }

    getItemSize()
    {
        let ele = this.ele;
        if(!ele)
            return '';
        if(ele.classList.contains('xx-large'))
            return 'xx-large'
        if(ele.classList.contains('x-large'))
            return 'x-large'
        if(ele.classList.contains('larger'))
            return 'larger'
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
        if(!content)
            return;
        let eleItem = document.getElementById(this.uid);
        if(!eleItem)
            return;
        this.setInLocalStorage(content);        
        if(/^:carousel:/.test(content)){
            if(this.carouselCanUpdate)
                this.initialCarouselContent(content);
            else
                this.carouselWaitingUpdate = content;
            return;
        }

        let ele = eleItem.querySelector('.status');
        if(!ele)
            return;
        if(/^:bar-info:/.test(content)){
            content = content.substring(10);
            this.setItemClass(eleItem, 'bar-info');
        }
        else if(/^data:/.test(content)){
            content = `<img class="app-chart" src="${content}" />`;
            this.setItemClass(eleItem, 'chart');
        }
        else if(/^chart:/.test(content))
        {
            content = content.substring(7);
            let colonIndex = content.indexOf(':')
            let chart = content.substring(0, colonIndex);
            content = content.substring(colonIndex + 1)
            this.renderChart(chart, JSON.parse(content), ele);
            return;
        }
        else if(content.indexOf('livestats') > 0){
            this.setItemClass(eleItem, 'db-basic live-stats');
        }
        else {
            this.setItemClass(eleItem, 'db-basic');
        }
        ele.innerHTML = content;
    }

    initialCarouselContent(content){
        content = content.substring(10);
        let index = content.indexOf(':');
        let carouselId = content.substring(0, index);
        content = content.substring(index + 1);
        if(this.timerCarousel){
            clearInterval(this.timerCarousel);
            this.timerCarousel = null;
        }
        this.carouselTimer(carouselId);
        this.setItemClass(this.ele, 'carousel');
        this.ele.innerHTML = content;
        let eleControls = this.ele.querySelector('.controls');
        let indexes = this.ele.querySelectorAll('.controls a');
        eleControls.className = 'controls items-' + indexes.length;
        for(let i=0;i<indexes.length;i++)
        {
            indexes[i].addEventListener('click', (event) => {
                event.stopImmediatePropagation();
                event.preventDefault();
                this.carouselItem(event, carouselId, i);
            }, false);
        }

    }

    setItemClass(item, className) {
        item.className = (item.className.replace(/(carousel|chart|db-basic|bar-info|live-stats)/g, '') + ' ' + className).replace(/\s\s+/g, ' ');
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
        let self = this;
        this.timerCarousel = setInterval(() => {
            let carousel = document.getElementById(id);
            if(!carousel)
                return; // happens if the carousel was replaced with newer html

            let hover = !!carousel.querySelector('.cover-link:hover');
            if(hover)
                return; // if hovering dont move the carousel
            
            let visible = carousel.querySelector('.item.visible');
            let index = parseInt(visible.id.substring(visible.id.indexOf('-') + 1), 10);
            ++index;            
            let hasNext = document.getElementById(id + '-' + index);
            index = hasNext ? index : 0;
            this.carouselCanUpdate = index === 0;
            if(index === 0 && this.carouselWaitingUpdate)
            {
                this.initialCarouselContent(this.carouselWaitingUpdate);
                this.carouselWaitingUpdate = null;
            }
            else
            {
                this.carouselItem(null, id, index);
            }
        }, 5000);
    }
    
    renderChart(type, args, ele){
        let title = args.title;
        let labels = args.labels;
        let datasets = args.data;
        let min = args.min;
        let max = args.max;
        var data = {
            labels: labels,
            datasets: datasets.map((x, index) => {
                return {
                    data: x,
                    fill: false,
                    borderColor: ['green', 'blue', 'yellow', 'red', 'purple', 'orange', 'cornflowerblue'][index],
                    lineTension: 0.1
                };
            })
        };
        //options
        var options = {
            responsive: true,
            maintainAspectRatio: false,
            animation: false,
            plugins: {
                title: {
                    display:true,
                    color:'white',
                    align:'end',
                    position:'top',
                    text: title.indexOf('\n') > 0 ? title : ('   ' + title + '   '),
                },
                legend: {
                    display:false
                }
            },
            elements: {
                point: {
                    radius:0
                }
            },
            scales: {
                yAxes: {
                    grid: {
                        display: false,
                        drawBorder: false
                    },
                    min: min == -1 ? null : min || 0,
                    max: max == -1 ? null : max || 100,
                    display: false,
                    ticks: {
                        display:false
                    }
                },
                xAxes: {
                    grid: {
                        display: true
                    },
                    ticks: {
                        display:false
                    }
                }
            }
        };
        ele.innerHTML = '<canvas></canvas>';
        let canvas = ele.querySelector('canvas');
        let eleDbItem = ele.parentNode.parentNode.parentNode;
        if(eleDbItem.classList.contains('chart') === false)
            eleDbItem.classList.add('chart');               
        new Chart(canvas, {
            type: 'line',
            data: data,
            options: options
        });
    }
}