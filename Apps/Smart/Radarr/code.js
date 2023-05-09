class Radarr {
    getUrl(args, endpoint) {
        let url = `api/v3/${endpoint}?apikey=${args.properties['apikey']}`;
        args.log('radarr url: ' + url);
        return url;
    }
    
    fetch(args, url) {
        return args.fetch(url).data;
    }

    status(args) {
		let updateAvailable = this.getStatusIndicator(args);
		args.setStatusIndicator(updateAvailable ? 'Update' : '');
        let filter = args.properties['filters'];

        
        let data = this.fetch(args, {
            url: this.getUrl(args, 'movie'),
            timeout: 10000
        });
        let queueData = [];
        if (args.properties['fetchWarnings'] == 'true') {      
            queueData = args.fetch({
                url: this.getUrl(args, 'queue') + '&pageSize=10000',
                timeout: 5000
            }).data;
        } 

        args.log('about to filter data');
        let filteredData = !data || !data.filter ? [] : data.filter((x, index, arr) => {
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

        data = args.fetch(this.getUrl(args, 'queue')).data;
        let queue = data?.records?.length ?? 0;

        if (args.properties['fetchWarnings'] != 'true') {
            return args.liveStats([
                ['Missing', missingCount],
                ['Queue', queue]
            ]);
        } else {
            let queueDataRecords = queueData?.records;
            let filteredQueue = queueDataRecords?.filter((x, index, arr) => {
                return x.trackedDownloadStatus == 'warning'
            })
            let totalWarnings = filteredQueue?.length ?? 0;

            return args.liveStats([
                ['Missing', missingCount],
                ['Queue', queue],
                ['Warnings', totalWarnings]
            ]);
        }



    }

	getStatusIndicator(args){
        let data = this.fetch(args, this.getUrl(args, 'update'));
        if(data?.length > 0 && data[0].installed === false && data[0].latest === true)
		    return 'update';
    	return '';
	}
	
    test(args) {
        let data = this.fetch(args, this.getUrl(args, 'queue'));
        return isNaN(data?.records?.length) === false;
    }
}

