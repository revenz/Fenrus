class Radarr
{ 
    getUrl(args, endpoint){
        return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
    }
    
    async status(args) {
        let data = await args.fetch(this.getUrl(args, 'movie'));
        let missing = data?.filter(x => x.hasFile == false)?.length ?? 0;
        data = await args.fetch(this.getUrl(args, 'queue'));
        let queue = data?.records?.length ?? 0;
        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    }
    
    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'queue'));
        return isNaN(data?.records?.length) === false;
    }
}

module.exports = Radarr;