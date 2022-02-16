function getUrl(args, endpoint) {
    return `api/v1/${endpoint}?apikey=${args.properties['apikey']}`;
}

module.exports = { 

    status: async (args) => {
        let data = await args.fetch(getUrl(args, 'wanted/missing'));

        let missing = data?.totalRecords ?? 0;

        data = await args.fetch(getUrl(args, 'queue'));
        let queue = data?.totalRecords ?? 0;

        return args.liveStats([
            ['Missing', missing],
            ['Queue', queue]
        ]);
    },
    
    test: async (args) => {
        let data = await args.fetch(getUrl(args, 'wanted/missing'));
        console.log('data', data);
        return isNaN(data?.totalRecords) === false;
    }
}