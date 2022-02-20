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
        url = match[0] + '/';
        for(let file of ['favicon', 'icon']){
            for(let extension of ['.ico', '.png', '.gif', '.svg']){
                let attempt = url + file + extension;
                console.log('trying favicon: ' + attempt);
                try
                {
                    const res = await fetch(attempt);
                    if(res.ok === false)
                        continue;
                    console.log('Got favicon: ' + attempt);
                    let iconUrl = `/images/icons/${uid}${extension}`;
                    console.log('Saving favicon to: ' + iconUrl);
                    res.body.pipe(fs.createWriteStream('./wwwroot' + iconUrl));
                    return iconUrl;            
                }
                catch(err)
                {
                    // fails if cannot reach host
                    console.warn('Failed to download favicon', err);
                    return;
                }
            }        
        }
    }
}
module.exports = ImageHelper;