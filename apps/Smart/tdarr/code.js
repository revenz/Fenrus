class FileFlows {
    async status(args) {
      const data = await args.fetch(
        {
          url: `${args.url}/api/v2/cruddb/`,
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
  
      return args.liveStats([
        ['Queue', queue],
        ['Proc/Err', `${processed}/${errored}`],
      ]);
    }
  
    async test(args) {
      const data = await args.fetch(`${args.url}/api/v2/status`);
      return (data.status === 'good');
    }
  }
  
  module.exports = FileFlows;
  