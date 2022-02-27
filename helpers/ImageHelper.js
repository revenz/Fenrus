const fsPromises = require("fs/promises");
const fs = require('fs');
const Utils = require('../helpers/utils');
const fetch = require('node-fetch');

class ImageHelper {

    async saveImageIfBase64(url, location, uid){
        if(!url || !url.startsWith)
            return url;

        if(url.startsWith('data:') === false)
            return url;

        uid = uid || new Utils().newGuid();
            
        // base64 image, we want to save this to disk and reference it, so we keep the data in the json small
        let extension = url.substring(url.indexOf('/') + 1).match(/^[a-zA-Z0-9]+/)[0];
        let data = url.substring(url.indexOf('base64,') + 7);
        
        console.log('Converting base64 to image: ' + location);
        let result = '/images/' + location + '/' + uid + '.' + extension;
        await fsPromises.writeFile('./wwwroot' + result, data, { encoding: 'base64'});
        return result;
    }

    async downloadFavIcon(url, uid){
        if(typeof(url) !== 'string')
            return;            
        let match = url.match(/^http(s)?:\/\/[^/]+/i);
        if(!match)
            return;
        let html = await fetch(url);
        html = await html.text();
        if(html){
            let rgxLinks = /<link[^>]+>/;
            let link;
            let favIconUrl;
            while(link = rgxLinks.exec(html)){
                console.log(link);
                if(link[0].toLowerCase().indexOf('shortcut') < 0 || link[0].toLowerCase().indexOf('icon') < 0)
                    continue;
                let matchIconUrl = link[0].match(/href="([^"]+)"/i, link[0]);
                if(matchIconUrl?.length){
                    favIconUrl = this.htmlDecode(matchIconUrl[1]);
                    break;
                }
                matchIconUrl = link[0].match(/href='([^']+)'/i, link[0]);
                if(matchIconUrl?.length){
                    favIconUrl = this.htmlDecode(matchIconUrl[1]);
                    break;
                }
            }
            if(favIconUrl){
                console.log('trying favicon: ' + favIconUrl);
                let extension = favIconUrl;
                if(extension.indexOf('?') > 0)
                    extension = extension.substring(0, extension.indexOf('?'));
                if(extension.indexOf('&') > 0)
                    extension = extension.substring(0, extension.indexOf('&'));
                extension = extension.substring(extension.lastIndexOf('.')+ 1);
                if(!extension || /^[a-z]{1,6}$/.test(extension)) // safety check
                    extension = 'ico';

                let iconUrl = `/images/icons/${uid}.${extension}`;
                if(await this.downloadImage(favIconUrl, iconUrl))
                    return iconUrl; 
            }
        }
        url = match[0] + '/';
        for(let file of ['favicon', 'icon']){
            for(let extension of ['.ico', '.png', '.gif', '.svg']){
                let attempt = url + file + extension;
                console.log('trying favicon: ' + attempt);
                let iconUrl = `/images/icons/${uid}${extension}`;
                if(await this.downloadImage(attempt, iconUrl))
                    return iconUrl;        
            }        
        }
    }

    async downloadImage(url, dest)
    {
        try
        {
            const res = await fetch(url);
            if(res.ok === false)
                return false;
            console.log('Got favicon: ' + url);
            console.log('Saving favicon to: ' + dest);
            res.body.pipe(fs.createWriteStream('./wwwroot' + dest));
            return true;            
        }
        catch(err)
        {
            // fails if cannot reach host
            console.warn('Failed to download favicon', err);
            return false;
        }
    }

    htmlDecode(input)
    {
        if(!input)
            return input;

        return input.replace(/&#([0-9]{1,3});/gi, function(match, numStr) {
            var num = parseInt(numStr, 10); // read num as normal number
            return String.fromCharCode(num);
        });
    }
}
module.exports = ImageHelper;