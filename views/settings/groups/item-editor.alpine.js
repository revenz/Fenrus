let itemEditor;
let apps = JSON.parse(document.getElementById('Apps').value);

Alpine.data('ItemEditor', () => ({
    init(){ itemEditor = this; },
    model: {}, 
    Saved: false,
    Opened: false,
    EditorTitle: '',
    EditorApp:{},
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
        let inputs = [...document.querySelectorAll(`.side-editor [data-rules]`)];
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


    typeChanged(event){
        if(!event){
            this.EditorApp = {TestFunction: false};
            return;
        }
        let type = typeof(event) === 'string' ? event : event.target?.value;
        if(type !== 'DashboardApp')
            this.EditorApp = {};
        this.model.AppName = '';
    },
    appChanged(event) {
        if(!event){
            this.EditorApp = {};
            return;
        }

        let resetUrl = !this.model.Url || 
                       this.model.Url === 'http://' ||
                       this.model.Url == this.EditorApp?.DefaultUrl;

        if(this.model.Name === this.EditorApp?.Name)
            this.model.Name = '';
        let fromInit = typeof(event) === 'string';
        let newApp = typeof(event) === 'string' ? event : event.target?.value;
        if(!newApp){
            this.EditorApp = {};
            return;
        }
        if(fromInit === false || !this.model.Properties){
            this.model.Properties = {}; // clear these
        }
        let index = apps.findIndex(x => x.Name === newApp);
        if(index >= 0){
            if(apps[index].Icon.indexOf('/') < 0)
                apps[index].Icon = `/apps/${apps[index].Name}/${apps[index].Icon}`;

            this.EditorApp = JSON.parse(JSON.stringify(apps[index]));
            if((fromInit === false || !this.model.Icon) && this.EditorApp)
                this.model.Icon = this.EditorApp.Icon;


            if(this.model.Icon?.toLowerCase()?.startsWith('app'))
                this.model.Icon = '/' + this.model.Icon;
        }
        else
            this.EditorApp = {};
        if(this.EditorApp?.Properties?.length)
        {
            // set the default property values if this app defines them
            for(let prop of this.EditorApp.Properties)
            {
                if(prop.Default && this.model.Properties[prop.Id] === undefined)
                    this.model.Properties[prop.Id] = prop.Default;
            }
        }    

        if(!this.NewEdit)
            this.model.Size = this.EditorApp.DefaultSize || 'medium';
        else
            this.NewEdit = false; // clear it

        if(resetUrl)
        {
            this.model.Url = this.EditorApp.DefaultUrl || 'http://';
        }

        if(!this.model.Name)
            this.model.Name = this.EditorApp.Name;
    },


    // item editor stuff
    focusItem(){
        setTimeout(()=> { document.querySelector('.side-editor').querySelector('input, select').focus();}, 250);
    },
    addItem() {
        if(this.isDisabled()) return;

        this.isItemSaved = false;
        this.EditorApp.Properties = {};
        this.model = 
        {
            _Type: 'DashboardApp',
            Uid: newGuid(),
            Name: '',
            Url: 'http://',
            Icon: '',
            Size: 'medium',
            Properties:{}
        };
        this.EditorTitle = 'New Item';                    
        this.Opened = true;
        this.focusItem();
    },
    editItem(uid) {
        if(this.isDisabled()) return;

        this.isItemSaved = false;
        this.EditorApp.Properties = {};
        let item = groupEditor.model.Items.find(x => x.Uid === uid);
        if(!item)
            return;
        
        this.EditorTitle = 'Edit Item';
        this.model = JSON.parse(JSON.stringify(item)); // clone the object so any changes arent written directly to the model                                            
        if(!this.model.DisplayName)
            this.model.DisplayName = this.model.Name;
        if(!this.model.Properties)
            this.model.Properties = {};
        if(!this.model.Size)
            this.model.Size = 'medium';
        this.NewEdit = this.model._Type === 'DashboardApp';
        this.appChanged(this.model.AppName);
        this.Opened = true;
        this.focusItem();
    },
    close(){
        if(this.isDisabled()) return;

        this.Opened = false;
    },
    getmodel() {          
        // first validate it
        this.isItemSaved = true; // this tells the form its now able to be treated as dirty and errors shown
        
        if(this.validate() === false)
            return;
            
        return JSON.parse(JSON.stringify(this.model));
    },
    save() {                
        if(this.isDisabled()) return;

        let item = this.getmodel();
        if(!item)
            return; // must be invalid

        
        let group = groupEditor.model;
        let index = group.Items.findIndex(x => x.Uid === item.Uid);
        if(index >= 0){
            group.Items[index] = item;
        }
        else{
            group.Items.push(item);
        }
        groupEditor.updatePreview();
        this.Opened = false;
    },
    imageChosen(event){
        this.fileToDataUrl(event, src => 
        {
            this.model.Icon = src;
            this.validate();
        })
    },                
    fileToDataUrl(event, callback) {
        if (! event.target.files.length) return;

        let file = event.target.files[0];
        let reader = new FileReader();

        reader.readAsDataURL(file);
        reader.onload = e => callback(e.target.result);
    },
    testing: false,
    testApp() {
        if(this.isDisabled()) return;

        let model = this.getmodel();
        if(!model)
            return;
        const options = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({AppInstance: model})
        }
        testing = true;
        fetch(`/apps/${this.EditorApp.Name}/test`, options).then(result => {
            if(!result.ok)
                throw result;
            this.testing = false;
            toast('Success', true);
        }).catch(error => {
            this.testing = false;
            toast('Failure', false);
        });
    },

    itemChange() {
        // this makes the ifValue conditions re-run, messy but works
        if(this.EditorApp?.Properties?.map)
            this.EditorApp.Properties = this.EditorApp.Properties.map(x => JSON.parse(JSON.stringify(x)));
    },

    ifValue(ifValue) {
        if(!ifValue)
            return true;
        let show = true;
        Object.keys(ifValue).forEach(key => {
            let ok = this.model[key] == ifValue[key];
            if(ok)
                return;
            ok = this.model.Properties[key] == ifValue[key];
            if(ok)
                return;

            show = false;
        });
        return show;
    },

    selectFirstIfNull(prop) {
        if(!this.model || !prop?.Options?.length)
            return;
        if(!this.model.Properties)
            this.model.Properties = {};
        if(this.model.Properties[prop.Id])
            return;
        this.model.Properties[prop.Id] = prop.Options[0].Value;
    },
}))