// https://glances.readthedocs.io/en/latest/api.html

class Glances {
    doFetch(args, endpoint) {
        return args.fetch({
            url: `api/3/` + endpoint,
            timeout: 10
        });
    }

    async status(args) {
        let mode = args.properties['mode'] || (
                        args.size === 'large' ? 'cpu' : 
                        args.size === 'x-large' ? 'overview' : 
                        args.size === 'xx-large' ? 'overview' : 
                        'basic'
                   );

        if(mode === 'overview')                   
            return await this.systemInfo(args);
        if(mode === 'basic')
            return await this.liveStats(args);
            
        return await this.chart(args, mode);
    }

    async systemInfo(args){
        const [ cpu, memory, fileSystem ] = await Promise.all([
          await this.doFetch(args, 'cpu'),
          await this.doFetch(args, 'mem'),
          await this.doFetch(args, 'fs'),
        ]);

        let items = [];

      
        // cpu 
        // {"interrupts": 861015, "system": 1.6, "time_since_update": 33.861000061035156, "idle": 90.8, "dpc": 0.1, "user": 7.4, 
        // "syscalls": 1358165, "interrupt": 0.1, "cpucore": 16, "total": 9.2, "soft_interrupts": 0, "ctx_switches": 921648}        
        items.push({label:'CPU', percent: cpu?.total || 0, icon: '/apps/Glances/www/cpu.svg'});
        // ram
        // {"available": 31510020096, "total": 68659789824, "percent": 54.1, "free": 31510020096, "used": 37149769728}
        if(!memory?.total)
            return;
        items.push({label:'RAM', percent:memory.percent, icon: '/apps/Glances/www/ram.svg'});

        if(fileSystem?.length){
            for(let fs of fileSystem){
                items.push({
                    label: fs.mnt_point,
                    percent: fs.percent,
                    icon: '/apps/Glances/www/hdd.svg'
                });
            }
        }
        return args.barInfo(items);
    }

    async chart(args, mode) 
    {
        let chartType = mode;
        switch(chartType){
            case 'cpu': return await this.chartCpu(args);
            case 'ram': return await this.chartGeneric(args, 'mem', (value) => value + ' %');
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

    
    async chartGeneric(args, endpoint, titleFormatter)
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
        if(titleFormatter)
            title = titleFormatter(title);

        return await args.chart.line({
            title,
            labels,
            data
        });
    }

    async liveStats(args) 
    {        

        let firstQuery = args.properties['firstStatQuery'] || 'cpu/total';
        let secondQuery = args.properties['secStatQuery'] || 'mem/percent';
        let firstTitle = args.properties['firstStatTitle'] || 'CPU';
        let secondTitle = args.properties['secStatTitle'] || 'RAM';

        let firstQueryValue = firstQuery.split(/[/]+/).pop();
        let secondQueryValue = secondQuery.split(/[/]+/).pop();

        const [ first, second ] = await Promise.all([
            await this.doFetch(args, firstQuery),
            await this.doFetch(args, secondQuery)
        ]);

        if(!first || !second)
            return;
        
        let firstQueryResult = first[firstQueryValue];
        let secondQueryResult = second[secondQueryValue];

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
