const https = require('https');
const fs = require('fs');

class HttpHelper {

    async download(url, dest){
        return new Promise((resolve, reject) => {
            const request = https.get(url, response => {
                if (response.statusCode === 200) 
                {        
                    const file = fs.createWriteStream(dest, { flags: 'wx' });
                    file.on('finish', () => resolve());
                    file.on('error', err => {
                        file.close();
                        if (err.code === 'EEXIST') reject('File already exists');
                        else fs.unlink(dest, () => reject(err.message)); // Delete temp file
                    });
                    response.pipe(file);
                } else if (response.statusCode === 302 || response.statusCode === 301) {
                    //Recursively follow redirects, only a 200 will resolve.
                    this.download(response.headers.location, dest).then(() => resolve());
                } else {
                    reject(`Server responded with ${response.statusCode}: ${response.statusMessage}`);
                }
            });
        
            request.on('error', err => {
                reject(err.message);
            });
        });
    }
}
module.exports = new HttpHelper();