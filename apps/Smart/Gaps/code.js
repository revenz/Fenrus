class Gaps {
    getUrl(args, endpoint) {
        return `${endpoint}`;
    } 

    async status(args) {
		const defaultLib1Name = 'Library 1';
		const defaultLib2Name = 'Library 2';
		const defaultLib3Name = 'Library 3';
		
        let id1 = args.properties['rssID'] ?? '';
		let id2 = args.properties['rssID2'] ?? '';
		let id3 = args.properties['rssID3'] ?? '';
		
		let rssData1 = [];
        let rssData2 = [];
		let rssData3 = [];
		
		[rssData1, rssData2, rssData3] = await Promise.all([
			id1.length > 1 ? args.fetch(this.getUrl(args, id1)) : Promise.resolve(),
			id2.length > 1 ? args.fetch(this.getUrl(args, id2)) : Promise.resolve(),
			id3.length > 1 ? args.fetch(this.getUrl(args, id3)) : Promise.resolve()
		]);
       
		let totalData = [];
		
		

		if(args.properties['group'] != "group") {
			let lib1Name = args.properties['lib1Name'] ?? defaultLib1Name;
			let lib2Name = args.properties['lib1Name'] ?? defaultLib2Name;
			let lib3Name = args.properties['lib1Name'] ?? defaultLib3Name;
			
			(rssData1 != null ) && totalData.push([lib1Name.length > 1 ? lib1Name : defaultLib1Name + ' Missing:', rssData1.length]); 
			(rssData2 != null ) && totalData.push([lib2Name.length > 1 ? lib2Name : defaultLib2Name + ' Missing:', rssData2.length]); 
			(rssData3 != null ) && totalData.push([lib3Name.length > 1 ? lib3Name : defaultLib3Name + ' Missing:', rssData3.length]); 
		} else {
			let count = 0;
			if(rssData1 != null) 
				count += rssData1.length; 
			if(rssData2 != null) 
				count += rssData2.length; 
			if(rssData3 != null)
				count += rssData3.length; 
			totalData.push(['Total Missing', count]); 
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