class Readarr {
    getUrl(args, endpoint, containsParams) {
		if(containsParams)
			return `api/v1/${endpoint}&apikey=${args.properties['apikey']}`;
		return `api/v1/${endpoint}?apikey=${args.properties['apikey']}`;
    }

	
    status(args) {
		let filter = args.properties['filters'];
		let now = new Date();
		let tomorrowText = args.Utils.formatCalanderDate(new Date(now.getTime() + 86400*1000), '-');
		let nextMonthText = args.Utils.formatCalanderDate(new Date(now.getTime() + (86400*28*1000)), '-');

        let missing = args.fetch(this.getUrl(args, 'wanted/missing',false));
		let queue = args.fetch(this.getUrl(args, 'calendar?start=' +tomorrowText+'&end=' + nextMonthText ,true));
		
		//files that are released in next 4 weeks and we dont have files of
		let upcoming = queue?.filter((x, index, arr) => {
			let validEntry = x.grabbed == false;
            if (validEntry && filter == 'monitored') {
                validEntry = x.monitored == true;
            }
			 return validEntry;
        });
		

        return args.liveStats([
            ['Missing', missing.totalRecords ?? 0],
            ['Upcoming', upcoming.length ?? 0]
        ]);
    }

    test(args) {
        let data = args.fetch(this.getUrl(args, 'system/status',false));
		args.log("data",data);
        return data.appName != null;
    }
}
