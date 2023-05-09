class Tdarr {
    status(args) {
		
		
      let url = args.url;
      if(url.endsWith('/'))
        url = url.substring(0, url.length - 1);
	
      const data = args.fetch(
        {
          url: `${url}/api/v2/cruddb/`,
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Accept: 'application/json',
          },
          body: JSON.stringify({
            data: {
              collection: 'StatisticsJSONDB',
              mode: 'getById',
              docID: 'statistics',
            },
          }),
        },
      ).data;
  
      if (!data) {
        throw 'no data';
      }
  
      const queue = parseInt(data.table1Count) + parseInt(data.table4Count);
      const processed = parseInt(data.table2Count) + parseInt(data.table5Count);
      const errored = parseInt(data.table3Count) + parseInt(data.table6Count);

	  let results =	[['Queue', queue],
        ['Proc / Err', `${processed} / ${errored}`]] 
		
	  if(args.properties['showSpaceSaved'] == 'true') {	
	    const sizeSaved = args.Utils.formatBytes(parseFloat(data.sizeDiff) * 1000000000)
		results.push(['Space saved', sizeSaved])
	  }
	  
      return args.liveStats(results);
    }
  
    test(args) {
      let url = args.url;
      if(url.endsWith('/'))
        url = url.substring(0, url.length - 1);
      const data = args.fetch(`${url}/api/v2/status`).data;
      return (data.status === 'good');
    }
  }
  
  module.exports = Tdarr;
  
