class ColorBackground
{
    dispose(){
    }

    init() {
        document.body.style.background = getComputedStyle(document.body).getPropertyValue('--background');
    }

    changeBackgroundColor(color){
        document.body.style.background = color;
    }
}
window.BackgroundType = ColorBackground;