class DefaultTheme 
{
    unit = 3.75;
    settings;
    constructor()
    {
        if(typeof window !== "undefined")
        {
            let json = document.getElementById('theme-settings').value;            
            this.settings = json ? JSON.parse(json) : {};
            window.addEventListener('load', (event) => {
                this.shrinkGroups();
            });
            window.addEventListener('resize', (event) => {
                this.shrinkGroups();
            });         

            if(this.settings.Horizontal){                
                let dashboard = document.querySelector('.dashboard');
                dashboard.addEventListener('wheel', (evt) => {
                    evt.preventDefault();
                    let scroll = dashboard.scrollLeft + (evt.deltaY * 3);
                    dashboard.scrollTo({
                        left: scroll
                    });
                });
            }
        }
    }

    init(){
        let eleDashboard = document.querySelector('.dashboard');
        let className = 'dashboard ' + this.settings.Placement;
        className += this.settings.Horizontal ? ' horizontal' : ' vertical';
        eleDashboard.className = className;

        this.shrinkGroups();

        document.body.classList.remove('horizontal');
        document.body.classList.remove('vertical');
        document.body.classList.add(this.settings.Horizontal ? 'horizontal' : 'vertical');
        eleDashboard.style.visibility = 'unset';
    }

    getVariables(args){
        let classes = [];
        let bodyClasses = [];
        classes.push(args?.Placement || 'bottom-left');
        
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
            let forcedHeight = 0;

            let group = groups[i];
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
                else if(item.classList.contains("x-large"))
                {
                    w = 4;
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
                maxHeight: this.settings.MaxHeight || 8, 
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
    shrinkGroup(grp, previous, next){
        let maxWidth = 0;

        let grpBounds = grp.getBoundingClientRect();
        let height = 0;
        // see if there is a previous element, if so match that height, if not a next element, if so match that height
        if(previous){
            let prevBounds = previous.getBoundingClientRect();
            if(prevBounds.y === grpBounds.y){
                // make sure same row
                height = prevBounds.height;
            }
        }
        if(next){
            let nextBounds = next.getBoundingClientRect();
            if(nextBounds.y === grpBounds.y){
                // make sure same row
                height = Math.max(nextBounds.height, height);
            }
        }
        if(height === 0){
            height = grpBounds.height;
        }
        grp.style.height = height +'px';

        let offset = grp.getBoundingClientRect().x;
        
        for(let item of grp.querySelectorAll('.items .db-item')){
            let bounds = item.getBoundingClientRect();
            let width = bounds.x + bounds.width - offset;
            if(maxWidth < width)
                maxWidth = width;
        }
        if(maxWidth > 0){
            grp.style.width = maxWidth + 'px';
        }

        // get new bounds, to make sure it didnt shift
        let newBounds = grp.getBoundingClientRect();
        if(newBounds.y != grpBounds.y && next){
            // it shifted, try do this again but pass null for next
            this.shrinkGroup(grp, previous, null);
        }
        let oneRem = parseFloat(getComputedStyle(document.documentElement).fontSize);
        let oneUnit = oneRem * this.unit;
        let finalBounds = grp.getBoundingClientRect();
        return finalBounds.height / oneUnit;
    }
}

var themeInstance = new DefaultTheme();

if(typeof(module) !== 'undefined')
    module.exports = themeInstance;