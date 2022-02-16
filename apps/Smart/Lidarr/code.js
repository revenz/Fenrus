function getUrl(args, endpoint) {
    return `api/v1/${endpoint}?apikey=${args.properties['apikey']}`;
}

module.exports = { 

    status: async (args) => {
        let data = await args.fetch(url('wanted/missing'))

        let missing = data?.totalRecords ?? 0;

        data = await args.fetch(url('queue'))
        let queue = data?.totalRecords ?? 0;

        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    },
    
    test: async (args) => {
        let data = await args.fetch(url('wanted/missing'))
        return isNaN(data?.totalRecords) === false;
    }
}