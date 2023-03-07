class ImageBackground
{    
    dispose()
    {
        delete document.body.style.backgroundImage;
    }
    
    init()
    {
        let uid = document.querySelector('div.dashboard[x-uid]').getAttribute('x-uid');
        let url = '/fimage/' + uid +'?ts=' + new Date().getTime();
        console.log('bkg url: ' + url);
        document.body.style.backgroundImage = `url('${url}')`;
    }
}
window.BackgroundType = ImageBackground;