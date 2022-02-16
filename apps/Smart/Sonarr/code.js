function getUrl(args, endpoint){
    return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
}

module.exports = { 
    status: async (args) => {
        let url = getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc';

        let data = await args.fetch(url);
        let missing = data?.totalRecords ?? 0;
        data = await args.fetch(getUrl(args, ('queue')))
        let queue = data?.totalRecords ?? 0;

        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    },
    
    test: async (args) => {
        let data = await args.fetch(getUrl(args, 'wanted/missing') + '&sortKey=airDateUtc&sortDir=desc');
        return isNaN(data?.totalRecords) === false;
    }
}