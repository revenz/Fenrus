Saved: false,
Saving: false,
isDirty: false,
cleanModel: {},
canceling: false,
init() {
    this.markClean();
    window.onbeforeunload = () => {
        if(this.canceling !== true && this.checkDirty()){
            return 'If you leave this page you will lose your unsaved changes';
        }
    }
    if(typeof(this.customInit) != 'undefined')
        this.customInit();
},  
checkDirty(){
    let strModel = JSON.stringify(this.model);
    return strModel != this.cleanModel;
},
cancelGoto(url) {
    this.canceling = true;
    window.location.href = url;
},
markClean(){
    this.cleanModel = JSON.stringify(this.model);
},
blur(){
    this.checkDirty();
    if(this.Saved === false)
        return;
    this.validate();
},
input(){
    if(this.Saved === false)
        return;
    this.validate();
},
isDisabled() {
    if(this.Saving)
        return true;
    return false;
},
validate() {
    let inputs = [...document.querySelectorAll(`.editor [data-rules]`)];
    let valid = true;
    inputs.map((input) => {
        if (Iodine.is(input.value, JSON.parse(input.dataset.rules)) !== true) {
            valid = false;
            input.classList.add("invalid");
        }else{
            input.classList.remove("invalid");
        }
    });
                    
    if(typeof(this.customValidation) !== 'undefined')
        valid = valid & this.customValidation();

    return valid;
},