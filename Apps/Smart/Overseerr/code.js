class Overseerr
{
    fetch(args) {
        let result = args.fetch({ url: 'api/v1/request/count',
            headers: { 'X-Api-Key': args.properties['apikey']} 
        });
        return result.data;
    }
    
    status(args) {
        if (!args.properties || !args.properties['apikey'])
            throw 'No API Key';

        let data = this.fetch(args);
        if(!data)
            return;

        return args.liveStats([
            ['Pending', data.pending],
            ['Processing', data.processing ]
        ]);
    }

    test(args) {
        let data = this.fetch(args)        
        return isNaN(data.pending) === false;
    }
}
