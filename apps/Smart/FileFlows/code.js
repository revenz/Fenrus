class FileFlows
{
    async status(args)
    {
        const [ data, updateAvailable ] = await Promise.all([
            await args.fetch('api/status'),
            await this.updateAvailable(args)
        ]);

        args.setStatusIndicator(updateAvailable ? 'update' : '');

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
    }

    async updateAvailable(args){
        let data = await args.fetch('api/status/update-available');
        return data?.UpdateAvailable === true;
    }

    async getStatusIndicator(args){
        if(await this.updateAvailable(args))
            return 'update';
        return 'recording';
    }

    async test(args){
        let data = await args.fetch(args.url + '/api/status');
        return (data.processed === 0 || data.processed);          
    }
}

module.exports = FileFlows;