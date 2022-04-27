class Radarr {
    getUrl(args, endpoint) {
        return `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
    }

    async status(args) {
	let updateAvailable = await this.getStatusIndicator(args);
	args.setStatusIndicator(updateAvailable ? 'Update' : '');
        let filter = args.properties['filters'];

        let data = await args.fetch({
            url: this.getUrl(args, 'movie'), 
			timeout: 10000
        });
        let filteredData = data?.filter((x, index, arr) => {
            let validEntry = x.hasFile == false;
            if (validEntry && (filter == 'both' || filter == 'available')) {
                validEntry = x.isAvailable == true;
            }
            if (validEntry && (filter == 'both' || filter == 'monitored')) {
                validEntry = x.monitored == true;
            }
            return validEntry;
        })
        let missingCount = filteredData?.length ?? 0;

        data = await args.fetch(this.getUrl(args, 'queue'));
        let queue = data?.records?.length ?? 0;
        return args.liveStats([
            ['Missing', missingCount],
            ['Queue', queue]
        ]);
    }
    async getStatusIndicator(args){
        let data = await args.fetch(this.getUrl(args, 'update'));
		
        if(data[0].installed ==false && data[0].latest == true){
			return 'update';
			} else {
			return '';
		}
	}

    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'queue'));
        return isNaN(data?.records?.length) === false;
    }
}

module.exports = Radarr;
