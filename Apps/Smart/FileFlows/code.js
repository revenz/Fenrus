class FileFlows
{
    pfImages = {};
    
    async fetch(args, url) {
        let result = await args.fetch(url);
        return result?.Result || result;
    }
    async status(args)
    {
        const [ data, shrinkage, updateAvailable ] = await Promise.all([
            await this.fetch(args, 'api/status'),
            await this.fetch(args, 'api/library-file/shrinkage-groups'),
            await this.updateAvailable(args)
        ]);

        args.setStatusIndicator(updateAvailable ? 'update' : '');

        if (!data || isNaN(data.queue)) {
            throw 'no data';
        }

        if(args.size.indexOf('large') >= 0) {
            return await this.statusXLarge(args, data, shrinkage);
        }
        else
            return this.statusMedium(args, data);
    }

    async updateAvailable(args){
        let data = await this.fetch(args, 'api/status/update-available');
        if(data.exception)
        {
            args.log('Exception fetching update-available: ' + (data.message || 'Unknown reason'));
            return false;
        }
        let result = data?.UpdateAvailable === true;
        return result;
    }

    async getStatusIndicator(args){
        if(await this.updateAvailable(args))
            return 'update';
        return 'recording';
    }
    

    async statusXLarge(args, data, shrinkage){
        if(!data.processingFiles?.length){
            if(shrinkage && Object.keys(shrinkage).length)
                return this.statusShrinkage(args, shrinkage)
            return this.statusMedium(args, data);
        }

        if(!this.pfImages)
            this.pfImages = {};
        
        let items =  [];
        for(let item of data?.processingFiles || [])
        {
            
            if(this.pfImages[item.name] === undefined)
            {
                let searchTerm = item.relativePath.replace(/\\/g, '/');
                if(/([^\/]+)s[\d]+e[\d]+/i.test(searchTerm)){
                    searchTerm = /([^\/]+)s[\d]+e[\d]+/i.exec(searchTerm)[1];
                    args.log('searchTerm', searchTerm);
                    searchTerm = searchTerm.replace(/\./g, ' ');
                }
                else if(/([^\/]+)(720p|1080p|4k|3840|BluRay)/i.test(searchTerm)){
                    searchTerm = /([^\/]+)(720p|1080p|4k|3840|480|576|BluRay)/i.exec(searchTerm)[1];
                    searchTerm = searchTerm.replace(/(720|1080|3840|480|576)[ip]/gi, '');
                    searchTerm = searchTerm.replace(/\./g, ' ');
                }
                else {
                    searchTerm = searchTerm.substring(searchTerm.lastIndexOf('/') + 1);
                }
                args.log('FileFlows search term: ' + searchTerm);
                let images = await args.imageSearch(searchTerm);
                this.pfImages[item.name] = images?.length ? images[0] : '';
            }
            let image = this.pfImages[item.name];
            items.push({
                file: item.relativePath,
                library: item.library,
                step: item.step,
                stepPercent: item.stepPercent,
                image: image
            });
        }
        let max = args.size === 'x-large' ? 7 : 10;
        if(items.length > max)
            items.splice(max);
        
        return args.carousel(items.map(x => {
            return this.getItemHtml(args, x);
        }));
    }

    statusMedium(args, data){
        let secondlbl = 'Time';
        let secondValue = data.time;

        if (!data.time) {
            if (!data.processing) {
                secondlbl = 'Processed';
                secondValue = data.processed;
            }
            else {
                secondlbl = 'Processing';
                secondValue = data.processing;
            }
        } 

        return args.liveStats([
            ['Queue', data.queue],
            [secondlbl, secondValue]
        ]);    
    }
    async statusShrinkage(args, shrinkage){
        let items = [];
            
        Object.keys(shrinkage).forEach(group => 
        {            
            let item = shrinkage[group];
            let increase = item.FinalSize > item.OriginalSize;
            let percent;
            let tooltip;
            if(increase){
                percent = 100 + ((item.FinalSize - item.OriginalSize) / item.OriginalSize * 100);
                tooltip = args.Utils.formatBytes(item.FinalSize - item.OriginalSize) + ' Increase';
            }else{
                percent = (item.FinalSize / item.OriginalSize) * 100;
                tooltip = args.Utils.formatBytes(item.OriginalSize - item.FinalSize) + ' Saved';
            }
            items.push({
                label: group === '###TOTAL###' ? 'Total' : group,
                percent: percent,
                tooltip: tooltip,
                icon: '/common/hdd.svg'
            });
        });
        
        return args.barInfo(items);
    }
    
    getItemHtml(args, item) {
        let title = args.Utils.htmlEncode(item.title);
        if(item.grandParentTitle){
            title = args.Utils.htmlEncode(item.grandParentTitle) + '<br/>' + title;
        }
        else if(item.parentTitle){
            title = args.Utils.htmlEncode(item.parentTitle) + '<br/>' + title;
        }
        return `
<div class="fileflows fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');">    
    <div class="name tr wrap">${item.file}</div>
    <div class="br">${item.library}</div>
    ${item.stepPercent ? `<div class="bl">${item.stepPercent.toFixed(1)}%</div>` : ''}
    <a class="cover-link" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="${args.appIcon || '/apps/FileFlows/icon.png'}?version=${args.version}" /></a>
</div>
`;
    }

    async test(args){
        let data = await args.fetch(args.url + '/api/status');
        return (data.processed === 0 || data.processed);          
    }
}
