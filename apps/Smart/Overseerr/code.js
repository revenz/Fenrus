module.exports = 
{ 

    status: async (args) => {
        if (!args.properties || !args.properties['apikey'])
            throw 'No API Key';

        let data = await args.fetch({ url: 'api/v1/request/count', headers: { 'X-Api-Key': args.properties['apikey']} });
        return args.liveStats([
            ['Pending', data.pending],
            ['Processing', data.processing ]
        ]);
    },

    test: async (args) => {
        let data = await args.fetch({ url: 'api/v1/request/count', headers: { 'X-Api-Key': args.properties['apikey'] } })        
        return isNaN(data.pending) === false;
    }
}