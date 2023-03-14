class Overseerr
{
    status(args) {
        if (!args.properties || !args.properties['apikey'])
            throw 'No API Key';

        let data = args.fetch({ url: 'api/v1/request/count', headers: { 'X-Api-Key': args.properties['apikey']} });
        if(!data)
            return;

        return args.liveStats([
            ['Pending', data.pending],
            ['Processing', data.processing ]
        ]);
    }

    test(args) {
        let data = args.fetch({ url: 'api/v1/request/count', headers: { 'X-Api-Key': args.properties['apikey'] } })        
        return isNaN(data.pending) === false;
    }
}
