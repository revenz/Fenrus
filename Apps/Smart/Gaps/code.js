class Gaps {
    getUrl(args, endpoint) {
        return `${endpoint}`;
    } 

    status(args) {
		const defaultLibName = ['Library 1','Library 2','Library 3'];
		
        let id1 = args.properties['rssID'] ?? '';
		let id2 = args.properties['rssID2'] ?? '';
		let id3 = args.properties['rssID3'] ?? '';

		let rssData = [null, null, null];
		
		rssData[0] = id1.length > 1 ? args.fetch(this.getUrl(args, id1)).data : null;
		rssData[1] = id2.length > 1 ? args.fetch(this.getUrl(args, id2)).data : null;
		rssData[2] = id3.length > 1 ? args.fetch(this.getUrl(args, id3)).data : null;
       
		let totalData = [];
		let groupStats = args.properties['group'] == "group";
		let i = 0;
		let totalEntryCount = 0;
		let currentYear = new Date().getFullYear();
		rssData.forEach(rss => {
			if(rss == null){
				return;
			}
			rss = rss?.filter((x, index, arr) => {
				return x.release_date <= currentYear;
			})

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
	
    test(args) {
		let id1 = args.properties['rssID'];
		if(id1 == null || id1.length < 1) {
			args.log("Test failed for Gaps, no rssID provided");
			return false;
		}
		let data = args.fetch(this.getUrl(args, id1)).data;
		args.log(data)
        return isNaN(data.length) === false;
    }
}
