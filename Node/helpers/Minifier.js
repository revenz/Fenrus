const fs = require('fs');
const path = require('path');
const FileHelper = require('../helpers/FileHelper');
var uglify = require("uglify-js");
var sass = require('sass');

class Minifier 
{
    minifyJavascript()
    {
        this.minifyFiles('js', /\.js$/, 'js', true);
    }

    async minifyCss() {
        const minifyContent = function(file, content) {
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
        }
        this.minifyFiles('css', /\.(scss)$/, 'css', false, minifyContent);
        
        console.log('minifying theme css');
        let themeDirs = await FileHelper.getDirectories('./wwwroot/themes');
        for(let dir of themeDirs){
            console.log('minifying theme ' + dir);
            this.minifyFiles(path.join(__dirname, '..', 'wwwroot', 'themes', dir), /\.(scss)$/, 'css', false, minifyContent, '_theme');
        }
    }

    minifyFiles(folder, pattern, extension, addFileNames, processor, outputFile){
        
        if(!outputFile)
        outputFile = '_fenrus';
        console.log('minifying path: ' + folder);
        let jsFiles = [];
        let combined = '';
        if(folder.indexOf('themes') < 0)
            folder = __dirname + '/../wwwroot/' + folder;
        fs.readdirSync(folder).forEach(file =>{
            if(pattern.test(file) === false)
                return;
            if(file.indexOf(outputFile) >= 0)
                return;
            let path = folder + '/' + file;
            let content = fs.readFileSync(path, 'utf8');
            if(processor)
                content = processor(path, content);
            if(addFileNames)
                combined += '/**** ' + file + '****/\n' + content + '\n';
            else
                combined += content + '/* */\n'; // add /* */ to break any comments left open by previous file
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

        fs.writeFileSync(folder +'/' + outputFile + '.' + extension, combined, 'utf-8');
    }
}
module.exports = Minifier;