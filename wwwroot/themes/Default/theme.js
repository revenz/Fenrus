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
        for(let grp of document.querySelectorAll('.db-group')){
            this.shrinkGroup(grp);
        }
    }
    shrinkGroup(grp){
        let maxWidth = 0;
        let offset = grp.getBoundingClientRect().x;
        let height = grp.getBoundingClientRect().height;
        for(let item of grp.querySelectorAll('.items .db-item')){
            let bounds = item.getBoundingClientRect();
            let width = bounds.x + bounds.width - offset;
            if(maxWidth < width)
                maxWidth = width;
        }
        if(maxWidth > 0){
            grp.style.width = maxWidth + 'px';
            grp.style.height = height +'px';
        }
    }
}

var defaultTheme = new DefaultTheme();