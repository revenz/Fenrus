const fsPromises = require("fs/promises");
const Utils = require('../helpers/utils');

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
}
module.exports = ImageHelper;