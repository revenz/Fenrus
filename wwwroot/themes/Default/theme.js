class DefaultTheme 
{
    constructor()
    {
        window.addEventListener('load', (event) => {
            this.init();
        });
        window.addEventListener('resize', (event) => {
            this.init();
        });
    }

    init(){
        console.log('init default theme!');
        this.shrinkGroups();
    }


    shrinkGroups(){
        let groups = document.querySelectorAll('.db-group');
        for(let i=0;i<groups.length;i++)
        {
            this.shrinkGroup(groups[i], i == 0 ? null : groups[i-1], i == groups.length - 1 ? null : groups[i+1]);
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
    }
}

var defaultTheme = new DefaultTheme();