let json = document.getElementById('GroupData').value;
let data = JSON.parse(json);
let isSystem = data.IsSystem;
data.ShowGroupTitle = !data.HideGroupTitle;
let groupEditor;
Alpine.data('Settings', () => ({
    customInit(){ 
        groupEditor = this; 
        setTimeout(() => {
            this.updatePreview();
        }, 0);
    },
    model: data, 
    isItemSaved: false,
    NewEdit:false,
    <%- include('../generic-alpine-editor.js') %>
    cancel(){
        if(this.isDisabled()) return;
        this.cancelGoto('/settings/groups');
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
        let url = (isSystem ? '/settings/system/groups/' : '/settings/groups/') + this.model.Uid;
        fetch(url, options).then(async (res)=>{
            if(!res.ok)
                throw await res.text();
            this.markClean();                
            window.location = isSystem ? '/settings/system/groups' : '/settings/groups';
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