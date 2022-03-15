class octoprint {

    doFetch(args, endpoint) {
        var data = args.fetch({
            url: `api/` + endpoint,
            timeout: 5,
            headers: {
                "X-Api-Key": args.properties['apikey']
            }
        })
        return data;
    }

/* job: {
    estimatedPrintTime: null,
    filament: { length: null, volume: null },
    file: { date: null, name: null, origin: null, path: null, size: null },
    lastPrintTime: null,
    user: null
  },
  progress: {
    completion: null,
    filepos: null,
    printTime: null,
    printTimeLeft: null,
    printTimeOrigin: null
  },
  state: 'Offline'
  */
  
    async status(args) {
        let data = await this.doFetch(args, "job");

		let percentProgress = data?.progress?.completion;
		let printTimeLeftSecs =  data?.progress?.printTimeLeft;
		let state = data?.state ?? 'Unknown';
		if(percentProgress == null || state == 'Unknown' || state == "Offline") {
			//assume no print running so just show state
			return args.liveStats([['State' , state]]);
		}
		if(percentProgress > 100) {
			percentProgress = 100;
		} else {
			percentProgress = Math.floor(percentProgress * 100) /100;
		}
		
		if(printTimeLeftSecs == null) {
			printTimeLeftSecs = "Unknown";
		} else {
			printTimeLeftSecs = args.Utils.formatMilliTimeToWords(printTimeLeftSecs*1000, printTimeLeftSecs < 60);
		}	
        return args.liveStats([
		
            ['Progress: ',  percentProgress +'%'],
            ['Time Left: ', printTimeLeftSecs],
			['State' , state]
			//['Job' , data?.job?.file?.name]
        ]);

    }

    async test(args) {
        let data = await this.doFetch(args, "version");
        console.log('data', data);
        return isNaN(data?.api) === false;
    }
}

module.exports = octoprint;
