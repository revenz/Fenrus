class Gaps {
    getUrl(args, endpoint) {
        return `${endpoint}`;
    } 

    async status(args) {
		const defaultLibName = ['Library 1','Library 2','Library 3'];
		
        let id1 = args.properties['rssID'] ?? '';
		let id2 = args.properties['rssID2'] ?? '';
		let id3 = args.properties['rssID3'] ?? '';

		let rssData = [];
		
		rssData = await Promise.all([
			id1.length > 1 ? args.fetch(this.getUrl(args, id1)) : Promise.resolve(),
			id2.length > 1 ? args.fetch(this.getUrl(args, id2)) : Promise.resolve(),
			id3.length > 1 ? args.fetch(this.getUrl(args, id3)) : Promise.resolve()
		]);
       
		let totalData = [];
		let groupStats = args.properties['group'] == "group";
		let i = 0;
		let totalEntryCount = 0;
		let currentYear = new Date().getFullYear();
		rssData.forEach(rss => {
			if(rss == null){
				return;
			}
			if(args.properties['showReleasedFutureYears'] != 'yes'){
				rss = rss?.filter((x, index, arr) => {
					return x.release_date <= currentYear;
				})
			}

			if(!groupStats) {
				let libName = args.properties['lib' + (i+1) +'Name'] ?? defaultLibName[i];
				totalData.push([libName.length > 1 ? libName : defaultLibName[i] + ' Missing:', rss.length]); 

			} else {
				totalEntryCount += rss.length; 
			}
			++i;
		});
		
		if(groupStats){
			totalData.push(['Total Missing', totalEntryCount]); 
		}
		
		return args.liveStats(
			totalData
		);
    }
	
    async test(args) {
		let id1 = args.properties['rssID'];
		if(id1 == null || id1.length < 1) {
			console.error("Test failed for Gaps, no rssID provided");
			return false;
		}
		let data = await args.fetch(this.getUrl(args, id1));
		console.log(data)
        return isNaN(data.length) === false;
    }
}

module.exports = Gaps;