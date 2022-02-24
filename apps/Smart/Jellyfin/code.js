class Jellyfin
{   
    async getToken(args){
        let username = args.properties['username'];
        let password = args.properties['password'];
        
        let data = await args.fetch({
            url: 'Users/AuthenticateByName',
            method: 'POST',
            headers: { 
                'Content-Type': 'application/json',
                'x-emby-authorization': 'MediaBrowser Client="Custom Client", Device="Custom Device", DeviceId="1", Version="0.0.1"'
            },
            body: JSON.stringify({
                Username: username,
                Pw: password
            })
        });
        return {token: data.AccessToken, userId: data?.User?.Id };
    }
    
    async getLibraries(args, token){
        if(!token)
            token = await this.getToken(args);        
        let data = await args.fetch({
            url: `Library/MediaFolders`,
            headers: {
                'X-MediaBrowser-Token': token.token
            }
        });
        return data?.Items?.map(x => x.Id);
    }

    async getData(args){
        let token = await this.getToken(args);     
        let libraries = await this.getLibraries(args, token);
        let results = [];
        for(let lib of libraries)
        {   
            let data = await args.fetch({
                url: `Users/${token.userId}/Items/Latest?Limit=7&ParentId=${lib}` ,
                headers: {
                    'X-MediaBrowser-Token': token.token
                }
            });

            let url = args.url;
            if(url.endsWith('/'))
                url = url.substring(0, url.length - 1);

            for(let d of data){
                
                results.push({
                    title: d.Name,
                    date: d?.PremiereDate ? new Date(d.PremiereDate) : null,
                    image: `${url}/Items/${d.Id}/Images/Primary`
                });
            }
        }
        return results;
    }

    async getCounts(args){
        let token = await this.getToken(args);        
        let data = await args.fetch({
            url: `Items/Counts`,
            headers: {
                'X-MediaBrowser-Token': token.token
            }
        });
        return data;
    }


    async status(args) {
        if(args.size.indexOf('large') >= 0)
             return await this.statusXLarge(args);
         else
            return await this.statusMedium(args);
    }

    async statusXLarge(args){
        
        let data = await this.getData(args);
        if(isNaN(data?.length) || data.length === 0)
            return;

        let url = args.url;
        if(url.endsWith('/'))
            url = url.substring(0, url.length - 1);
        let max = args.size === 'x-large' ? 7 : 10;
        if(data.length > max)
            data.splice(max);
        
        return args.carousel(data.map(x => {
            return this.getItemHtml(args, x);
        }));
    }

    async statusMedium(args){
        let data = await this.getCounts(args);
        if(!data || (!data.MovieCount && !SeriesCount))
            return;
        let movies = data.MovieCount || 0;
        let series  = data.SeriesCount || 0;

        return args.liveStats([
            ['Movies', movies],
            ['Series', series]
        ]);
    }
    
    getItemHtml(args, item) {
        let title = args.Utils.htmlEncode(item.title);
        if(item.parentTitle){
            title = args.Utils.htmlEncode(item.parentTitle) + '<br/>' + title;
        }
        return `
<div class="plex fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');">
    
    <div class="name tr">${title}</div>
    ${item.date ? `<div class="br">${item.date.getFullYear()}</div>` : ''}
    <a class="cover-link" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="/apps/Jellyfin/icon.png" /></a>
</div>
`;
    }

    async test(args) {
        let data = await this.getToken(args);
        return !!data?.token;
    }
}

module.exports = Jellyfin;