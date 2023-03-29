class DefaultTheme 
{
    unit = 3.75;
    settings;
    oneRem;
    oneUnit;
    constructor()
    {
        if(typeof window !== "undefined")
        {
            this.oneRem = parseFloat(getComputedStyle(document.documentElement).fontSize);
            this.oneUnit = this.oneRem * this.unit;

            for(let i=3;i>=1;i--){
                let div = document.createElement('div')
                div.classList.add('bg');
                div.classList.add('bg' + i);
                document.body.insertAdjacentElement("afterbegin", div);
            }

            document.addEventListener('fenrus-item-resized', (e) => this.onItemResized(e));
            document.addEventListener('fenrus-item-deleted', (e) => this.onItemDeleted(e));


            let json = document.getElementById('theme-settings').value;          
            this.settings = json ? JSON.parse(json) : {};

            window.addEventListener('load', (event) => { this.load()});
            setTimeout(() => { this.load() }, 250); // incase something is prevent the page from firing the load event, eg a script is hanging

            window.addEventListener('resize', (event) => {
                this.shrinkGroups();
            });         

            if(this.settings.Horizontal){                
                let dashboard = document.querySelector('.dashboard');
                dashboard?.addEventListener('wheel', (evt) => {
                    evt.preventDefault();
                    let scroll = dashboard.scrollLeft + (evt.deltaY * 3);
                    dashboard.scrollTo({
                        left: scroll
                    });
                });
            }
        }
    }

    onItemResized(event) {
        if(!event?.detail?.element)
            return;

        let group = event.detail.element.closest('.db-group');
        if(group)
            this.shrinkGroup(group);
    }
    
    onItemDeleted(event) {
        console.log('recieved item deleted custom event!');
        if(!event?.detail?.group)
            return;

        let group = event.detail.group;
        if(group)
            this.shrinkGroup(group);
    }

    load() 
    {        
        if(typeof(GrowingPacker) === 'undefined' || !this.settings)
            return setTimeout(()=> this.load(), 50);

        let eleDashboard = document.querySelector('.dashboard');
        if(!eleDashboard)
            return;            
        let className = eleDashboard.className || '';

        if(className.indexOf('dashboard') < 0)
            className += ' dashboard';
        className = className.replace(/(bottom|left|top|right|vertical|horizontal|center)/g, ' ');
        className += ' ' + (this.settings.Placement || 'center'); 
        className += this.settings.Horizontal ? ' horizontal' : ' vertical';
        className = className.replace(/  +/g, ' ');
        eleDashboard.className = className;
        
        this.shrinkGroups();            

        document.body.classList.remove('horizontal');
        document.body.classList.remove('vertical');
        document.body.classList.remove('show-background');        
        document.body.classList.add(this.settings.Horizontal ? 'horizontal' : 'vertical');
        if(this.settings.BackgroundImage && document.body.classList.contains('custom-background'))
            document.body.classList.add('show-background');        
        eleDashboard.style.visibility = 'unset';

    }

    init(){
        this.load();
    }

    initPreview() {
        this.shrinkGroup(document.querySelector('.preview-dashboard .db-group'));
    }

    getVariables(args){
        let classes = [];
        let bodyClasses = [];
        classes.push(args?.Placement || 'bottom-left');
        // let animateBackground = args?.AnimatedBackground !== false;
        // bodyClasses.push((animateBackground ? '' : 'no-') + 'animate-background');
        
        if(args?.Horizontal){
            classes.push('horizontal');
            bodyClasses.push('horizontal');
        }
        else{
            classes.push('vertical');
            bodyClasses.push('vertical');
        }   
        return {
            ClassName: classes.join(' '),
            BodyClassName: bodyClasses.join(' ')
        };
    }


    shrinkGroups(){
        let groups = document.querySelectorAll('.db-group');
        for(let i=0;i<groups.length;i++)
        {
            this.shrinkGroup(groups[i]);
        }
    }

    shrinkGroup(group) {
        
        let forcedHeight = 0;

        let groupClass = group.getAttribute("class");
        let width = 6;
        let height = 12;
        let matchWidth = groupClass.match(/width\-([\d]+)/);
        if(matchWidth)
            width = parseInt(matchWidth[1], 10);
        let matchHeight = groupClass.match(/height\-([\d]+)/);
        if(matchHeight)
            height = parseInt(matchHeight[1], 10);

        let items = [];
        let horizontal = this.settings.horizontal;
        let groupSize = this.settings.GroupSize;

        if(forcedHeight)
        {
            groupSize = forcedHeight;
            horizontal = false;
        }
        let maxWidth = 0;
        let maxHeight = this.settings.MaxHeight || 8;
        
        let largeWidth = 4;
        if(screen.width <= 600){
            // its getting small, we will limit it
            maxHeight = 0;
            maxWidth = 6;
            largeWidth= 6;
            console.log('maxWidth', maxWidth);
        }

        for(let item of group.querySelectorAll('.items .db-item')){
            let w = 1, h = 1;
            if(item.classList.contains("medium"))
            {
                w = 2;
                h = 2;    
            }
            else if(item.classList.contains("large"))
            {
                w = 6;
                h = 2;
            }
            else if(item.classList.contains("larger"))
            {
                w = 6;
                h = 4;
            }
            else if(item.classList.contains("x-large"))
            {
                w = largeWidth;
                h = 4;    
            }
            else if(item.classList.contains("xx-large"))
            {
                w = 6;
                h = 6;    
            }
            items.push({uid: item.id, ele: item, w:w, h:h});
        }
      
        var packer = new GrowingPacker();
        packer.fit({
            maxHeight: maxHeight, 
            maxWidth: maxWidth,
            blocks: items
        });
        let groupW = 0, groupH = 0;
        for(var n = 0 ; n < items.length ; n++) {
            var item = items[n];
            if (item.fit) {
                item.ele.style.position = 'absolute';
                item.ele.style.left = (item.fit.x * this.unit) + 'rem';
                item.ele.style.top = (item.fit.y * this.unit) + 'rem';
                groupW = Math.max(item.fit.x + item.fit.w, groupW);
                groupH = Math.max(item.fit.y + item.fit.h, groupH);
            }
        }
        let eleItems = group.querySelector('.items');
        eleItems.style.width = (groupW  * this.unit) + 'rem';
        eleItems.style.height = (groupH * this.unit) + 'rem';
        eleItems.style.position = 'relative';

        group.style.width = 'unset';
        group.style.minWidth = 'unset';
        group.style.height = 'unset';
        group.style.minHeight = 'unset';
    }
}

var themeInstance = new DefaultTheme();

if(typeof(module) !== 'undefined')
    module.exports = themeInstance;