class adguardhome
{
	doFetch (args) {
        return args.fetch({
            url: `control/stats`, 
			timeout: 10, 
			headers: {
                'Authorization': 'Basic ' + args.Utils.btoa(args.properties['username'] + ':' + args.properties['password'])
            }
        });
    }  
	
    async status(args) {
        let data = await this.doFetch(args);
		
        var dns_queries_total=0;
	var blocked_filtering_total=0;	

	if(Array.isArray(data?.dns_queries)) {
		let dns_queries = data?.dns_queries;
		for (var i = 0; i < dns_queries.length; i++) { 
			dns_queries_total += dns_queries[i];
		}

		let blocked_filtering = data?.blocked_filtering;
		for (var i = 0; i < blocked_filtering.length; i++) { 
			blocked_filtering_total += blocked_filtering[i];
		}		
	} else {
		dns_queries_total = data?.dns_queries ?? 0;
		blocked_filtering_total = data?.blocked_filtering ?? 0;
	}

        return args.liveStats([
            ['Queries', dns_queries_total],
            ['Blocked', blocked_filtering_total]
        ]);
    }
    
    async test(args) {
        let data = await this.doFetch(args);
        console.log('data', data);
        return isNaN(data?.num_dns_queries) === false;
    }
}

module.exports = adguardhome;
