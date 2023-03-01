class VantaCellsBackground
{
    type = 'cells';
    customSettings = {
        colorMode: 'lerp',
        backgroundColor: getComputedStyle(document.body).getPropertyValue('--background'),
        color1: this.getAccentColor(),
        color2: this.getAccentColorDarken()
    };
    effect;

    changeBackgroundColor(color){
        this.backgroundColor = color;
        if(this.effect) {
            this.effect.setOptions({
                backgroundColor: color
            });
        }
    }
    changeAccentColor(color){
        this.effect?.destroy();
        this.customSettings = {
            backgroundColor: getComputedStyle(document.body).getPropertyValue('--background'),
            color1: this.getAccentColor(),
            color2: this.getAccentColorDarken()
        };
        this.initEffect();
    }

    getAccentColor() {
        return getComputedStyle(document.body).getPropertyValue('--accent');
    }
    getAccentColorDarken() {
        return shadeColor(this.getAccentColor(), -70);
    }

    dispose(){
        this.effect?.destroy();
        this.removeById('vanta_' + this.type);
        this.removeById('vanta-bkg');
    }

    async init()
    {
        await this.addScript('vanta', '/backgrounds/vanta/_vanta.min.js');
        await this.addScript('vanta_' + this.type, `/backgrounds/vanta/vanta.${this.type}.min.js`);

        let bkg = document.getElementById('vanta-bkg');
        if(!bkg) {
            bkg = document.createElement("div");
            bkg.style.position = 'fixed';
            bkg.style.top = '0';
            bkg.style.bottom = '0';
            bkg.style.left = '0';
            bkg.style.right = '0';
            bkg.setAttribute('id', 'vanta-bkg');
            document.body.insertBefore(bkg, document.body.firstChild);
        }
        this.initEffect();
    }

    initEffect(){

        let settings = {
            el: '#vanta-bkg',
            mouseControls: false,
            touchControls: false,
            gyroControls: false,
            minHeight: 200.00,
            minWidth: 200.00,
            scale: 1.00,
            scaleMobile: 1.00
        };
        this.effect = VANTA[this.type.toUpperCase()]({ ...settings, ...this.customSettings });
    }

    removeById(id){
        let ele = document.getElementById(id);
        if(ele)
            ele.remove();
    }

    addScript(id, src){
        return new Promise((success, reject) => {
            var script = document.getElementById(id);
            if(script) {
                success();
                return;
            }
            script = document.createElement('script');
            script.setAttribute('src', src);
            script.setAttribute('id', id);
            script.onload = () => {
                success();
            }
            document.head.append(script);
        });
    }
}
window.BackgroundType = VantaCellsBackground;