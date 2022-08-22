var fs = require('fs');
var uglify = require("uglify-js");

class Minifier 
{
    minifyJavascript()
    {
        let jsFiles = [];
        let combined = '';
        fs.readdirSync(__dirname + '/../wwwroot/js').forEach(file =>{
            if(/\.js$/i.test(file) === false)
                return;
            if(file.indexOf('fenrus') >= 0)
                return;
            let path = __dirname + '/../wwwroot/js/' + file;
            let content = fs.readFileSync(path, 'utf8');
            combined += '/**** ' + file + '****/\n' + content + '\n';
            return;
            let result = uglify.minify(content);
            if (result.error) {
                combined += content + '\n';      
                console.error("Error minifying: " + result.error);
                return;
            }
            combined += result.code + '\n';            
            //jsFiles.push(path);
        })

        //console.log('files', jsFiles);
        //var uglified = uglify.minify(jsFiles);
        //console.log('uglified', uglified);

        console.log('combined length: ', combined.length);

        fs.writeFileSync(__dirname + '/../wwwroot/js/fenrus.js', combined, 'utf-8');
    }
}
module.exports = Minifier;