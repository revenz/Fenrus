class FileFlows
{
    fetch(args) {
        let url = args.url.replace(/\/+$/, '');
        
        if (!/\/webhook/i.test(url))
            url += '/webhook/fenrus';
        
        args.log('Fetching URL: ' + url);
        if(!args.properties["apiToken"])
            return args.fetch(url).data;

        return args.fetch({
            url: url,
            method: 'GET',
            headers: {
                'x-webhook-token': args.properties['apiToken']                
            }            
        }).data;
    }
    
    status(args)
    {
        let data = this.fetch(args);
        let updateAvailable = this.updateAvailable(args);

        args.setStatusIndicator(updateAvailable ? 'update' : '');

        if (!data || isNaN(data.Unprocessed)) {
            throw 'no data';
        }

        if(args.size.indexOf('large') >= 0) {
            return this.statusXLarge(args, data);
        }
        else
            return this.statusMedium(args, data);
    }

    updateAvailable(args){
        let data = this.fetch(args);
        if(data.exception)
        {
            args.log('Exception fetching update-available: ' + (data.message || 'Unknown reason'));
            return false;
        }
        let result = data?.UpdateAvailable === true;
        return result;
    }

    statusXLarge(args, data){
        if(data.Libraries?.length)
            return this.statusShrinkage(args, data.Libraries)
        return this.statusMedium(args, data);
    }

    statusMedium(args, data){
        let secondlbl = '';
        let secondValue = 0;

        if (!data.Processing) {
            secondlbl = 'Processed';
            secondValue = data.Processed;
        }
        else {
            secondlbl = 'Processing';
            secondValue = data.Processing;
        }

        return args.liveStats([
            ['Queue', data.Unprocessed],
            [secondlbl, secondValue]
        ]);    
    }
    statusShrinkage(args, shrinkage){
        let items = [];
            
        for(let item of shrinkage) 
        {            
            let increase = item.FinalSize > item.OriginalSize;
            let percent;
            let tooltip;
            if(item.FinalSize === 0)
            {
                percent = 100;
                tooltip = 'No Change';
            }
            else if(increase){
                percent = 100 + ((item.FinalSize - item.OriginalSize) / item.OriginalSize * 100);
                tooltip = args.Utils.formatBytes(item.FinalSize - item.OriginalSize) + ' Increase';
            }else{
                percent = (item.FinalSize / item.OriginalSize) * 100;
                tooltip = args.Utils.formatBytes(item.OriginalSize - item.FinalSize) + ' Saved';
            }
            items.push({
                label: item.Library === '###TOTAL###' ? 'Total' : item.Library,
                percent: percent,
                tooltip: tooltip,
                icon: '/common/hdd.svg'
            });
        }
        
        return args.barInfo(items);
    }

    test(args){        
        let data = this.fetch(args);
        args.log('data: ' + (data === null ? 'null' : JSON.stringify(data)));
        return isNaN(data.Processed) === false;          
    }
}
