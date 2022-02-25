class DefaultTheme 
{
    settings;
    constructor()
    {
        if(typeof window !== "undefined")
        {
            this.settings = JSON.parse(document.getElementById('theme-settings').value);
            window.addEventListener('load', (event) => {
                this.shrinkGroups();
            });
            window.addEventListener('resize', (event) => {
                this.shrinkGroups();
            });         

            if(!this.settings.Automatic && this.settings.Horizontal ){                
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

    getVariables(args){
        let classes = [];
        let bodyClasses = [];
        classes.push(args?.Placement || 'bottom-left');
        if(args?.Automatic)
            classes.push('automatic');
        else if(args?.Horizontal){
            classes.push('horizontal');
            bodyClasses.push('horizontal');
            classes.push('width-' + args.GroupSize);
        }
        else{
            classes.push('vertical');
            bodyClasses.push('vertical');
            classes.push('height-' + args.GroupSize);
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
            if(this.settings.Automatic && false){
                this.shrinkGroup(groups[i], i == 0 ? null : groups[i-1], i == groups.length - 1 ? null : groups[i+1]);
                continue;
            }
            if(this.settings.Automatic)
                forcedHeight = this.shrinkGroup(groups[i], i == 0 ? null : groups[i-1], i == groups.length - 1 ? null : groups[i+1]);
            

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
                    w = horizontal ? 6 : 2;
                    h = horizontal ? 2 : 6;    
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


            const {w, h, fill} = potpack(items, groupSize);
            let eleItems = group.querySelector('.items');
            eleItems.style.width = ((horizontal ? w : h) * 3.75) + 'rem';
            eleItems.style.height = ((horizontal ? h : w) * 3.75) + 'rem';
            eleItems.style.position = 'relative';
            for(let item of items){
                item.ele.style.position = 'absolute';
                item.ele.style.left = ((horizontal ? item.x : item.y) * 3.75) + 'rem';
                item.ele.style.top = ((horizontal ? item.y : item.x) * 3.75) + 'rem';
            }
            group.style.width = 'unset';
            group.style.height = 'unset';
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
        let oneUnit = oneRem * 3.5;
        let finalBounds = grp.getBoundingClientRect();
        return finalBounds.height / oneUnit;
    }
}

var defaultTheme = new DefaultTheme();

if(typeof(module) !== 'undefined')
    module.exports = defaultTheme;