window.CreateFenrusColorPicker = (csharp, uid, color) => {
    return new FenrusColor(csharp, uid, color);
} 

class FenrusColor
{
    visible = false;
    input;
    baseR = 0; baseG = 0; baseB = 0;
    gradients = [
        [204, 0, 0, 0], [153, 153, 0, 17],
        [51, 204, 0, 33], [0, 204, 204, 50],
        [0, 0, 204, 66], [204, 0, 204, 83],
        [204, 0, 0, 100]
    ];
    
    constructor(csharp, uid, color) {
        this.csharp = csharp;
        let picker = document.getElementById(uid + '-picker');
        this.picker = picker;
        this.input = document.getElementById(uid + '-input');
        this.input.addEventListener('keyup', (e) => {
            this.manualEntry(); 
        });
        this.colorPickerRgb = picker.querySelector('.color-picker-rgb');
        this.preview = document.getElementById(uid +'-preview');
        this.preview.addEventListener('click', (e) => {
            this.visible = !this.visible;
            picker.className = 'color-picker ' + (this.visible ? 'visible' : 'hidden');
            e.stopPropagation();
        });
        
        for(let psw of picker.querySelectorAll('.color-palette-sw div')){
            psw.addEventListener('click', (e) => {
                let hex = psw.getAttribute('data-color');
                if(!/#[a-fA-F0-9]{6}/.test(hex)) {
                    let pswColor = hex.split(' ');
                    let pswR = parseInt(pswColor[0]);
                    let pswG = parseInt(pswColor[1]);
                    let pswB = parseInt(pswColor[2]);
                    hex = this.rgbToString(pswR, pswG, pswB);
                }
                this.updateValue(hex.toUpperCase());
                this.close();
            });
        }
        this.initSlider(picker);
        this.initArea(picker);
    }
    
    initArea(picker) {
        let crosshair = picker.querySelector('.crosshair');

        document.addEventListener('click', (event) => {
            if(!this.visible)return;
            const withinBoundaries = event.composedPath().includes(picker);
            if(!withinBoundaries) {
                this.close();
            }
        });
        let area = picker.querySelector('.color-picker-rgb');
        area.addEventListener('click', (event) => {
            crosshair.style.left = (event.offsetX + 5.5) + 'px';
            crosshair.style.top = (event.offsetY + 6) + 'px';
            this.mainPickerClicked(event);
        });

        let mouseMoveEvent = (event) => {
            crosshair.style.left = (event.offsetX + 5.5) + 'px';
            crosshair.style.top = (event.offsetY + 6) + 'px';
            this.mainPickerClicked(event);
        }
        area.addEventListener('mousedown', () => {
            area.addEventListener('mousemove', mouseMoveEvent);
        });
        document.addEventListener('mouseup', (event) => {
            area.removeEventListener('mousemove', mouseMoveEvent)
        });
    }
    
    initSlider(picker) {
        let sliderIndicator = picker.querySelector('.color-slider-bar-indicator');
        let colorSlider = picker.querySelector('.color-slider');
        colorSlider.addEventListener('click', (event) => {
            sliderIndicator.style.top = (event.offsetY - 3) + 'px';
            this.moveSlider(event);
        });
        let mouseMoveEvent = (event) => {
            sliderIndicator.style.top = (event.offsetY - 3) + 'px';
            this.moveSlider(event);
        }
        colorSlider.addEventListener('mousedown', () => {
            colorSlider.addEventListener('mousemove', mouseMoveEvent);
        });
        document.addEventListener('mouseup', (event) => {
            colorSlider.removeEventListener('mousemove', mouseMoveEvent)
        });
    }
    
    close() {
        this.visible = false;
        this.picker.className = 'color-picker ' + (this.visible ? 'visible' : 'hidden');
    }

    moveSlider(e)
    {        
        let percent = e.offsetY / 150.3;
        percent = Math.max(0, Math.min(1, percent));

        let c1r = 0, c1g = 0, c1b = 0, c2r = 0, c2g = 0, c2b = 0, start = 0, end = 100;
        for (let i = 0; i < this.gradients.length - 1; i++)
        {
            let g1 = this.gradients[i];
            let g2 = this.gradients[i + 1];
            if (percent * 100 > g2[3])
                continue;

            start = g1[3];
            end = g2[3];
            c1r = g1[0]; c1g = g1[1]; c1b = g1[2];
            c2r = g2[0]; c2g = g2[1]; c2b = g2[2];
            break;
        }

        // say percent is .42
        // start = 33, end is 50
        // percent 0 == 33, 100 = 50, 50 = 41.5
        let shifted = (percent * 100) - start; // 42 - 33 = 9
        let newP = shifted / (end - start); // 9 / (50 - 33) == 0.529

        this.baseR = this.adjustPercent(c1r, c2r, newP);
        this.baseG = this.adjustPercent(c1g, c2g, newP);
        this.baseB = this.adjustPercent(c1b, c2b, newP);
        this.BaseColor = this.rgbToString(this.baseR, this.baseG, this.baseB);
        this.colorPickerRgb.style.backgroundColor = this.BaseColor;

        this.calculateColor();
    }

    calculateColor()
    {
        let wPercent = this.PointerX / 211.2;
        let r = this.adjustPercent(255, this.baseR, wPercent);
        let g = this.adjustPercent(255, this.baseG, wPercent);
        let b = this.adjustPercent(255, this.baseB, wPercent);
        if(isNaN(r) || isNaN(g) || isNaN(b))
            return;
        //console.log('rgb: ',r,g,b);

        let bPercent = this.PointerY / 147.1;
        r = this.adjustPercent(r,0, bPercent);
        g = this.adjustPercent(g,0, bPercent);
        b = this.adjustPercent(b,0, bPercent);
        //console.log('rgb2: ',r,g,b);

        this.updateValue(this.rgbToString(r, g, b))
    };
    
    rgbToString(r, g, b) {
        return ("#" + this.decimalToHex(r, 2) + this.decimalToHex(g, 2) + this.decimalToHex(b, 2)).toUpperCase();
    }
    
    updateValue(value){
        this.preview.style.backgroundColor = value;
        this.csharp.invokeMethodAsync("updateValue", value);
    }

    adjustPercent(zeroC, hundredC, percent)
    {
        return Math.min(255, Math.max(0,Math.round(zeroC + percent * (hundredC - zeroC))));
    }
    
    mainPickerClicked(e)
    {
        this.PointerX = e.offsetX;
        this.PointerY = e.offsetY;
        this.calculateColor();
    }

    decimalToHex(d, padding)
    {
        let hex = Number(d).toString(16);
        padding = typeof (padding) === "undefined" || padding === null ? padding = 2 : padding;
    
        while (hex.length < padding) {
            hex = "0" + hex;
        }
    
        return hex;
    }

    manualEntry() {
        let v = this.input.value;
        if(!v) return;
        if(!/^#[a-fA-F0-9]{6}$/.test(v))
            return; // not a valid color
        this.updateValue(v);
        // make this color pure bright, to find it in the wheel
        let r = parseInt(v.substring(1, 2), 16);
        let g = parseInt(v.substring(3, 2), 16);
        let b = parseInt(v.substring(5, 2), 16);
        r = this.adjustPercent(255, r, 1);
        g = this.adjustPercent(255, g, 1);
        b = this.adjustPercent(255, b, 1);
        //console.log('white color: ' + this.rgbToString(r, g, b));
        r = this.adjustPercent(0, r, 1);
        g = this.adjustPercent(0, g, 1);
        b = this.adjustPercent(0, b, 1);
        let adjusted = this.rgbToString(r, g, b);
        //console.log('adjusted color: ' + adjusted);
        for(let i=0;i<this.gradients.length - 1;i++){
            let g1 = this.gradients[i];
            let g2 = this.gradients[i + 1];
            let diffRed   = Math.abs(g1[0] - g2[0]);
            let diffGreen = Math.abs(g1[1] - g2[1]);
            let diffBlue  = Math.abs(g1[2] - g2[2]);
            console.log('diff', diffRed, diffGreen, diffBlue);
        }
    }
}