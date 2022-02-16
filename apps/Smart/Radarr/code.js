function getUrl(args, endpoint){
    return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
}

module.exports = { 
    status: async (args) => {
        let data = await args.fetch(getUrl(args, 'movie'));
        let missing = data?.filter(x => x.hasFile == false)?.length ?? 0;
        data = await args.fetch(getUrl(args, 'queue'));
        let queue = data?.records?.length ?? 0;
        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    },
    
    test: async (args) => {
        let data = await args.fetch(getUrl(args, 'queue'));
        return isNaN(data?.records?.length) === false;
    }
}