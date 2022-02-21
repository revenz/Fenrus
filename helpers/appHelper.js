const fs = require('fs');
const path = require('path');

class AppHelperInstance {

    apps = {};

    getList(){
        let list = [];
        Object.keys(this.apps).forEach(key => {
            list.push(this.apps[key]);
        })
        
        list.sort((a, b) =>{
            return a.Name.localeCompare(b.Name);
        });
        return list;
    }

    load() 
    {
        let getApps = (startPath) => {
          var files=fs.readdirSync(startPath);
            let results = {};
            for(var i=0;i<files.length;i++){
                var filename=path.join(startPath,files[i]);
                var stat = fs.lstatSync(filename);
                if (stat.isDirectory()){
                    let sub = getApps(filename);
                    Object.keys(sub).forEach(x => results[x] = sub[x]);
                }
                else if (/app\.json$/.test(filename)){
                    try{
                        let json = fs.readFileSync(filename, {encoding: 'utf8'});
                        if(json.charCodeAt(0) === 65279)
                            json = json.substring(1);
                        let obj = typeof(json) === 'object' ? json : JSON.parse(json);
                        if(obj?.Name){
                            obj.Directory = startPath.replace(/\\/g, '/');
                            obj.Directory = obj.Directory.substring(obj.Directory.indexOf('apps/') + 5);
                            obj.Icon = obj.Icon ?? 'icon.png';
                            
                            let css = path.join(startPath, 'app.css');
                            if(fs.existsSync(css))
                                obj.Css = `/apps/${obj.Name}/app.css`;

                            if(!obj.DefaultUrl){
                                obj.DefaultUrl = `http://${obj.Name.toLowerCase().replace(/[\s]/g, '-')}.lan/`;
                            }

                            results[obj.Name] = obj;
                        }else{
                            console.log('no name: ', obj);
                        }
                    }catch(err){
                        console.log('error with app: ' + filename + ' => ' + err);
                    }
                }
            };
            return results;
        };
        
        this.apps = getApps('./apps');        
    }

    get(name) {
        return this.apps[name];
    }
}

class AppHelper {
    constructor() {
        throw new Error('Use AppHelper.getInstance()');
    }

    static getInstance(){
        if(!AppHelperInstance.instance){
            AppHelperInstance.instance = new AppHelperInstance();
        }
        return AppHelperInstance.instance;
    }
}

module.exports = AppHelper;
