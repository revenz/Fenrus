class FileFlows
{
    async status(args)
    {
        const [ data, updateAvailable ] = await Promise.all([
            await args.fetch('api/status'),
            await this.updateAvailable(args)
        ]);

        args.setStatusIndicator(updateAvailable ? 'update' : '');

        if (!data || isNaN(data.queue))
            throw 'no data';        

        if(args.size.indexOf('large') >= 0)
            return await this.statusXLarge(args, data);
        else
            return this.statusMedium(args, data);
    }

    async updateAvailable(args){
        let data = await args.fetch('api/status/update-available');
        return data?.UpdateAvailable === true;
    }

    async getStatusIndicator(args){
        if(await this.updateAvailable(args))
            return 'update';
        return 'recording';
    }

    pfImages = {};
    

    async statusXLarge(args, data){
        if(!data.processingFiles?.length)
            return this.statusMedium(args, data);
            
        let items =  [];
        for(let item of data.processingFiles || [])
        {
            if(this.pfImages[item.name] === undefined)
            {
                let searchTerm = item.relativePath.replace(/\\/g, '/');
                if(/([^\/]+)s[\d]+e[\d]+/i.test(searchTerm)){
                    searchTerm = /([^\/]+)s[\d]+e[\d]+/i.exec(searchTerm)[1];
                    console.log('searchTerm', searchTerm);
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
                console.log('FileFlows search term: ' + searchTerm);
                let images = await args.imageSearch(searchTerm);
                this.pfImages[item.name] = images?.length ? images[0] : '';                
                console.log('image:', this.pfImages[item.name]);
            }
            let image = this.pfImages[item.name]
            items.push({
                file: item.relativePath,
                library: item.library,
                step: item.step,
                stepPercent: item.stepPercent,
                image: image
            })
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

module.exports = FileFlows;