// https://glances.readthedocs.io/en/latest/api.html

class Glances {
    doFetch(args, endpoint) {
        return args.fetch({
            url: `api/3/` + endpoint,
            timeout: 10
        });
    }

    async status(args) {
        if(args.properties['displayChart'] === true){
            return await this.chart(args);
        }
        return await this.liveStats(args);
    }

    async chart(args) 
    {
        let chartType = (args.properties['chart'] || 'cpu').toLowerCase();
        switch(chartType){
            case 'cpu': return await this.chartCpu(args);
            default: return await this.chartGeneric(args, chartType);
        }
    }

    async chartCpu(args)
    {        
        let cpuStats = await this.doFetch(args, 'cpu/history/50');
        if(!cpuStats?.system || !cpuStats?.user)
            return;
        
        let labels = cpuStats.system.map(x => new Date(x[0]));
        let data = [
            cpuStats.system.map(x => x[1]),
            cpuStats.user.map(x => x[1])
        ];
        let title = (cpuStats.system.at(-1)[1] + cpuStats.user.at(-1)[1]).toFixed(0) + ' %';

          return await args.chart.line({
            title,
            labels,
            data
          });
    }

    
    async chartGeneric(args, endpoint)
    {        
        let stats = await this.doFetch(args, endpoint + '/history/50');
        if(!stats)
            return;
        let key = Object.keys(stats)[0];
        if(!key || !stats[key])
            return;

        
        let labels = stats[key].map(x => new Date(x[0]));
        let data = [
            stats[key].map(x => x[1]),
        ];
        let title = stats[key].at(-1)[1];

        return await args.chart.line({
            title,
            labels,
            data
        });
    }

    async liveStats(args) 
    {        

        let firstQuery = args.properties['firstStatQuery'];
        let secondQuery = args.properties['secStatQuery'];

        let data = await this.doFetch(args, firstQuery);

        let firstQueryValue = firstQuery.split(/[/]+/).pop();
        let firstQueryResult = data[firstQueryValue];

        data = await this.doFetch(args, secondQuery);

        let secondQueryValue = secondQuery.split(/[/]+/).pop();
        let secondQueryResult = data[secondQueryValue]

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
