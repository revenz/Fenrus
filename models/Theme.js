const fs = require('fs');
const FileHelper = require('../helpers/FileHelper');

class Theme {
    Name = '';
    Css = [];
    Scripts = [];
    Templates = {};
    Directory = '';
    DirectoryName= '';
    Settings = [];

    constructor(name){
        this.Name = name;
        this.Css = ['theme.min.css'];
        this.Templates = {
            Group: '',
            App: '',
            Link: ''
        }
    }

    static getTheme(name)
    {
        if(!name || typeof(name) !== 'string')
            return new Theme(name);
        name = name.replace(/[^a-z0-9_]/gi, '').trim(); //  make the name safe
        if(!name)
            return new Theme(name);
        
        let theme = new Theme(name);
        theme.Directory = '../wwwroot/themes/' + name;
        theme.DirectoryName = name;
        let file = theme.Directory + '/theme.json';
        if(fs.existsSync(file.substring(1)) === false){
            console.log('NO THEME FILE FOUND IN: ', file);
            return theme;  // basic theme nothing more to load
        }
        
        // more complex theme, with theme file        
        let json = fs.readFileSync(file.substring(1), { encoding: 'utf-8'});
        if(json.charCodeAt(0) === 65279)
            json = json.substring(1);

        let obj = JSON.parse(json);
    
        Object.keys(obj).forEach(k => {
            theme[k] = obj[k];
        }); 

        console.log('# THEME', theme);
        
        if(!theme.Templates)
            theme.Templates = {
                Group: '',
                App: '',
                Link: ''
            };
        
        return theme;
    }

    static async getThemes(){
        let themeDirs = await FileHelper.getDirectories('./wwwroot/themes');
        let themes = [];
        for(let dir of themeDirs){
            let theme = Theme.getTheme(dir);
            console.log('got theme', theme);
            themes.push(theme);
        }
        return themes;
    }
}

module.exports = Theme;