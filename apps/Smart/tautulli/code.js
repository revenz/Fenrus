const humanizeDuration = require("humanize-duration");

class Tautulli
{
    getUrl(args, endpoint) {
        return `api/v2?apikey=${args.properties['apikey']}&cmd=${endpoint}`;
    }
    async status(args) {
        const [ dr, topUsers ] = await Promise.all([
            await args.fetch(this.getUrl(args, 'get_activity')),
            await args.fetch(this.getUrl(args, 'get_home_stats&stat_id=top_users'))
        ]);

        let data = dr?.response?.data;
                
        if(args.size.indexOf('large') >= 0)
            return this.statusXLarge(args, data, topUsers?.response?.data?.rows);
        else
            return this.statusMedium(args, data);

    }

    statusXLarge(args, data, topUsers){
        if(!data.sessions?.length && !topUsers?.length)
            return this.statusMedium(args, data);
        let url = args.url;
        if(url.endsWith('/'))
            url = url.substring(0, url.length - 1);
        let items =  [];
        let usingUsers = !data.length;
        if(usingUsers)
        { 
            for(let item of topUsers)
            {
                items.push({
                    title: item.friendly_name || item.user,
                    duration: humanizeDuration(item.total_duration * 1000, { largest: 2 }),
                    image: item.user_thumb?.length > 2 ? args.proxy(url + '/pms_image_proxy?img=' + item.user_thumb) : null
                });
            }

        }
        else
        {        
            for(let item of data.sessions)
            {
                items.push({
                    title: item.title,
                    parentTitle: item.parentTitle,
                    grandParentTitle: item.grandparent_title,
                    user: item.user,
                    year: item.parentYear || item.year,
                    image: item.art?.length > 2 ? args.proxy(url + '/pms_image_proxy?img=' + item.art) : null
                })
            }
        }
        let max = args.size === 'x-large' ? 7 : 10;
        if(items.length > max)
            items.splice(max);
        
        return args.carousel(items.map(x => {
            return this.getItemHtml(args, x, usingUsers);
        }));
    }

    statusMedium(args, data){
        
        let streamCount = data?.stream_count ?? 0;
        
        return args.liveStats([
            ['Stream Count', streamCount]
        ]);
    }
    
    getItemHtml(args, item, usingUsers) {
        let title = args.Utils.htmlEncode(item.title);
        if(item.grandParentTitle){
            title = args.Utils.htmlEncode(item.grandParentTitle) + '<br/>' + title;
        }
        else if(item.parentTitle){
            title = args.Utils.htmlEncode(item.parentTitle) + '<br/>' + title;
        }
        if(usingUsers){
            return `
    <div class="tautulli fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');background-size:contain">    
        <div class="name tr">${title}</div>
        <div class="br">${item.duration}</div>
        <a class="cover-link" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}" />
        <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="${args.appIcon || '/apps/Plex/icon.png'}?version=${args.version}" /></a>
    </div>
    `;

        }
        return `
<div class="tautulli fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');">    
    <div class="name tr">${title}</div>
    <div class="br">${item.user}</div>
    <a class="cover-link" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="${args.appIcon || '/apps/Plex/icon.png'}?version=${args.version}" /></a>
</div>
`;
    }

    async test(args) {
        let data = await args.fetch(this.getUrl(args, 'arnold'));
        console.log('data', data);
        return data?.response?.result == 'success';
    }
}

module.exports = Tautulli;
