const fs = require('fs');
const fsPromises = require("fs/promises");

class FileHelper {
    async getDirectories(source) {
        if(fs.existsSync(source) === false)
            return [];
            
        let list = await fsPromises.readdir(source, { withFileTypes: true });
        return list.filter(dirent => dirent.isDirectory()).map(dirent => dirent.name);
    }
}

module.exports = new FileHelper();