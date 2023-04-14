class AdGuardHome
{
	doFetch (args) {
        args.log('doing fetch');
        let uandp = (args.properties['username'] || '') + ':' + (args.properties['password'] || '');
        args.log('uandp: ' + uandp);
        let auth = 'Basic ' + args.Utils.btoa(uandp);
        args.log('auth: ' + auth);
        return args.fetch({
            url: `control/stats`, 
			timeout: 10, 
			headers: {
                'Authorization': auth
            }
        });
    }  
	
    status(args) {
        args.log('fetching data');
        args.log('test 2: ' + args.Utils.newGuid());
        let test = args.Utils.btoa('testing');
        args.log('btoa test: ' + test);
        let data = this.doFetch(args);

        args.log('got data');
        var dns_queries_total=0;
        var blocked_filtering_total=0;	
        args.log('check data is array');
    
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
    
    test(args) {
        let data = this.doFetch(args);
        args.log('data', data);
        return isNaN(data?.num_dns_queries) === false;
    }
}

