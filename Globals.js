class Globals
{
    MajorVersion = 0;
    MinorVersion = 4;
    Revision = 0;
    Build = 0;

    Version;

    constructor(){
        console.log('new globals instance!');
    }
}


module.exports = new Globals();