class Glances
{
	doFetch (args, endpoint) {
        return args.fetch({
            url: `api/3/` + endpoint, 
			timeout: 10
        });
    }  
	
    async status(args) {
	 let firstQuery = args.properties['firstStatQuery'];
     let secondQuery = args.properties['secStatQuery'];
		
	let data = await this.doFetch(args, firstQuery);
		   
    let firstQueryValue = firstQuery.split(/[/]+/).pop();
	let firstQueryResult = data[firstQueryValue];
	
	console.log("firstQueryValue" , firstQueryValue);
	console.log("firstQueryResult" , firstQueryResult);
	console.log("firstQueryResult Data" , data);
	
	data = await this.doFetch(args, secondQuery);
		   
    let secondQueryValue = secondQuery.split(/[/]+/).pop(); 
	let secondQueryResult = data[secondQueryValue]

console.log("secondQueryValue" , secondQueryValue);
	console.log("secondQueryResult" , secondQueryResult);
	console.log("secondQueryResult Data" , data);
	
	let firstTitle = args.properties['firstStatTitle'];
    let secondTitle = args.properties['secStatTitle'];
		
        return args.liveStats([
            [firstTitle, firstQueryResult],
            [secondTitle, secondQueryResult]
        ]);
    }
    
    async test(args) {
        let data = await this.doFetch(args, 'processcount/total');
        console.log('data', data);
        return isNaN(data?.total) === false;
    }
}

module.exports = Glances;
