class Tdarr {
    async status(args) {
		
		
      let url = args.url;
      if(url.endsWith('/'))
        url = url.substring(0, url.length - 1);
	
      const data = await args.fetch(
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
      );
  
      if (!data) {
        throw 'no data';
      }
  
      const queue = parseInt(data.table1Count) + parseInt(data.table4Count);
      const processed = parseInt(data.table2Count) + parseInt(data.table5Count);
      const errored = parseInt(data.table3Count) + parseInt(data.table6Count);

	  let results =	[['Queue', queue],
        ['Proc / Err', `${processed} / ${errored}`]] 
		
	  if(args.properties['showSpaceSaved'] == 'true') {	
	    const sizeSaved = args.Utils.formatBytesAdvanced(parseFloat(data.sizeDiff), 3)
		results.push(['Space saved', sizeSaved])
	  }
	  
      return args.liveStats(results);
    }
  
    async test(args) {
      let url = args.url;
      if(url.endsWith('/'))
        url = url.substring(0, url.length - 1);
      const data = await args.fetch(`${url}/api/v2/status`);
      return (data.status === 'good');
    }
  }
  
  module.exports = Tdarr;
  
