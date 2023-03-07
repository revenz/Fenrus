class DefaultBackground
{    
    dispose(){
        let removeIfExists = function(id)
        {
            let ele = document.getElementById(id);
            if(ele)
                ele.remove();
        }
        removeIfExists('bg1');
        removeIfExists('bg2');
        removeIfExists('bg3');
        removeIfExists('bkg-default-style');
    }
    
    init()
    {
        let createBackground = function(name) 
        {
            if(document.getElementById(name))
                return; // already exists
            let div = document.createElement("div");
            div.className = 'bg ' + name;
            div.setAttribute('id', name);
            document.body.insertBefore(div, document.body.firstChild);
            
        }
        createBackground('bg3');
        createBackground('bg2');
        createBackground('bg1');
            
        let css = `        
body
{
    background-image: none;
    --bg-base: rgb(10,10,10);
    background:var(--bg-base, rgb(10,10,10));
}

.bg 
{
    background-image: linear-gradient(-60deg, var(--accent, #ff0090) 50%, var(--bg-base, rgb(10,10,10)) 50%);
    bottom:0;
    left:-50%;
    opacity:.5;
    position:fixed;
    right:-50%;
    top:0;
    z-index:-1;
    animation:slide 6s ease-in-out infinite alternate;
}

.bg1 {
    opacity:0.1 !important;
}
.bg2 {
    opacity:0.05 !important;
    animation-direction:alternate-reverse;
    animation-duration:8s;
}
.bg3 {
    opacity:0.03 !important;
    animation-duration:10s;
}

@keyframes slide 
{
    0% {
        transform:translateX(-25%);
    }
    100% {
        transform:translateX(25%);
    }
}
`
        let styleEle = document.getElementById('bkg-default-style');
        if(!styleEle){
            styleEle = document.createElement('style');
            styleEle.setAttribute('id', 'bkg-default-style');
            document.body.insertBefore(styleEle, document.body.firstChild);            
        }
        styleEle.innerHTML = css;
    }
}
window.BackgroundType = DefaultBackground;