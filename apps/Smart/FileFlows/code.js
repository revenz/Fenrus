module.exports = { 

    status: async (args) => {
        let data = await args.fetch('api/status');
        if (!data || isNaN(data.queue))
            throw 'no data';            
        let secondlbl = 'Time';
        let secondValue = data.time;

        if (!data.time) {
            if (!data.processing) {
                secondlbl = 'Processed';
                secondValue = data.processed;
            }
            else {
                secondlbl = 'Processing';
                secondValue = data.processing;
            }
        } 

        return args.liveStats([
            ['Queue', data.queue],
            [secondlbl, secondValue]
        ]);        
    },

    test: async (args) => {
        let data = await args.fetch(args.url + '/api/status');
        return (data.processed === 0 || data.processed);          
    }
}