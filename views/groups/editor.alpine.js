let json = document.getElementById('GroupData').value;
let data = JSON.parse(json);
console.log(data);
data.ShowGroupTitle = !data.HideGroupTitle;
let groupEditor;
Alpine.data('Settings', () => ({
    init(){ groupEditor = this; },
    model: data, 
    Saved: false,
    isItemSaved: false,
    NewEdit:false,
    blur(){
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
        let inputs = [...document.querySelectorAll(`.settings-box [data-rules]`)];
        let valid = true;
        inputs.map((input) => {
            if (Iodine.is(input.value, JSON.parse(input.dataset.rules)) !== true) {
                valid = false;
                input.classList.add("invalid");
            }else{
                input.classList.remove("invalid");
            }
        });
        return valid;
    },
    cancel(){
        if(this.isDisabled()) return;

        window.location = '/settings/groups';
    },
    save() {
        if(this.isDisabled()) return;
        
        this.Saved = true;

        if(!this.validate())
            return false;

        this.model.HideGroupTitle = !this.model.ShowGroupTitle;
            
        const options = {
            method: 'POST',
            body: JSON.stringify(this.model),
            headers: {
                'Content-Type': 'application/json'
            }
        }
        fetch(`/settings/groups/${this.model.Uid}`, options).then(async (res)=>{
            if(!res.ok)
                throw await res.text();
                
            window.location = '/settings/groups';
        }).catch(err => {
            toast(err || 'Failed to save', false);
        });
        return true;
    }, 

    remove(item) {
        confirmPrompt(`Are you sure you want to delete the item "${item.Name}"?`).then(() => 
        {
            this.model.Items = this.model.Items.filter(x => x.Uid !== item.Uid);
            updatePreview();
        }).catch(err => {});
    },

    updatePreview() {
        if(themeInstance?.initPreview){
            themeInstance.initPreview();
            setTimeout(() => themeInstance.initPreview(), 1);
        }
    },

    // item editor stuff
    focusItem(){
        setTimeout(()=> { document.querySelector('.group-item-editor .content').querySelector('input, select').focus();}, 250);
    },
    addItem() {
        itemEditor.addItem();
    },
    editItem(item) {
        if(this.isDisabled()) return;
        
        itemEditor.editItem(item.Uid);
    }
}))