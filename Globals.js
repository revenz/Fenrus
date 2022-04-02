class Globals
{
    MajorVersion = 0;
    MinorVersion = 4;
    Revision = 1;
    Build = 0;

    Version;

    constructor(){
        console.log('new globals instance!');
    }

    getVersion(){
        if(!this.Version)
            this.Version = `${this.MajorVersion}.${this.MinorVersion}.${this.Revision}.${this.Build}`;
        return this.Version;
    }
}


module.exports = new Globals();