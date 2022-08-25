var fs = require('fs');
var uglify = require("uglify-js");
var sass = require('sass');

class Minifier 
{
    minifyJavascript()
    {
        this.minifyFiles('js', /\.js$/, 'js', true);
    }

    minifyCss() {
        this.minifyFiles('css', /\.(scss)$/, 'css', false, (file, content) => {
            if(!file.endsWith('.scss'))
                return content;
            try
            {
                let result = sass.compile(file, {style: "compressed"});
                return result.css;
            }
            catch(err)
            {
                console.log('Error processing file: ' + file);
                throw err;
            }
        });
    }

    minifyFiles(folder, pattern, extension, addFileNames, processor){
        
        let jsFiles = [];
        let combined = '';
        fs.readdirSync(__dirname + '/../wwwroot/' + folder).forEach(file =>{
            if(pattern.test(file) === false)
                return;
            if(file.indexOf('_fenrus') >= 0)
                return;
            let path = __dirname + '/../wwwroot/' + folder + '/' + file;
            let content = fs.readFileSync(path, 'utf8');
            if(processor)
                content = processor(path, content);
            if(addFileNames)
                combined += '/**** ' + file + '****/\n' + content + '\n';
            else
                combined += content + '\n';
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

        fs.writeFileSync(__dirname + '/../wwwroot/' + folder +'/_fenrus.' + extension, combined, 'utf-8');
    }
}
module.exports = Minifier;