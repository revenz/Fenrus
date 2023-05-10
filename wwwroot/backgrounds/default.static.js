class DefaultStaticBackground
{    
    dispose(){
        let removeIfExists = function(id)
        {
            let ele = document.getElementById(id);
            if(ele)
                ele.remove();
        }
        removeIfExists('bg1-static');
        removeIfExists('bg2-static');
        removeIfExists('bg3-static');
        removeIfExists('bkg-default-static-style');
    }
    
    init()
    {
        let createBackground = function(name) 
        {
            if(document.getElementById(name))
                return; // already exists
            let div = document.createElement("div");
            div.className = 'bg-static ' + name;
            div.setAttribute('id', name);
            document.body.insertBefore(div, document.body.firstChild);
            
        }
        createBackground('bg3-static');
        createBackground('bg2-static');
        createBackground('bg1-static');
            
        let css = `        
body
{
    background-image: none;
    --bg-base: rgb(10,10,10);
    background:var(--bg-base, rgb(10,10,10));
}

.bg-static 
{
    background-image: linear-gradient(-60deg, var(--accent, #ff0090) 50%, var(--bg-base, rgb(10,10,10)) 50%);
    bottom:0;
    left:-50%;
    opacity:.5;
    position:fixed;
    right:-30%;
    top:0;
    z-index:-1;
}

.bg1-static {
    opacity:0.1 !important;
}
.bg2-static {
    opacity:0.05 !important;
    right:-80%;
}
.bg3-static {
    opacity:0.03 !important;
    right:-125%;
}
`
        let styleEle = document.getElementById('bkg-default-static-style');
        if(!styleEle){
            styleEle = document.createElement('style');
            styleEle.setAttribute('id', 'bkg-default-static-style');
            document.body.insertBefore(styleEle, document.body.firstChild);            
        }
        styleEle.innerHTML = css;
    }
}
window.BackgroundType = DefaultStaticBackground;

if(document.querySelector('body.login-page, body.initial-config-page'))
    new DefaultStaticBackground().init();