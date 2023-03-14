class Emby
{   
    getToken(args){
        let username = args.properties['username'];
        let password = args.properties['password'];
        
        let data = args.fetch({
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

    getLibraries(args, token){
        if(!token)
            token = this.getToken(args);        
        let data = args.fetch({
            url: `Library/MediaFolders`,
            headers: {
                'X-MediaBrowser-Token': token.token
            }
        });
        return data?.Items?.map(x => x.Id);
    }
    
    getData(args){
        let token = this.getToken(args);        
        let libraries = this.getLibraries(args, token);
        let results = [];
        for(let lib of libraries)
        {
            let data = args.fetch({
                url: `Users/${token.userId}/Items/Latest?Limit=7&ParentId=${lib}&Fields=ProductionYear` ,
                headers: {
                    'X-MediaBrowser-Token': token.token
                }
            });

            let url = args.url;
            if(url.endsWith('/'))
                url = url.substring(0, url.length - 1);

            for(let d of data){
                console.log(d);
                results.push({
                    title: d.Name,
                    date: d?.ProductionYear,
                    image: `${url}/Items/${d.Id}/Images/Primary`
                });
            }
        }
        return results;
    }

    getCounts(args){
        let token = this.getToken(args);        
        let data = args.fetch({
            url: `Items/Counts`,
            headers: {
                'X-MediaBrowser-Token': token.token
            }
        });
        return data;
    }


    status(args) {
        if(args.size.indexOf('large') >= 0)
             return this.statusXLarge(args);
         else
            return this.statusMedium(args);
    }

    statusXLarge(args){
        
        let data = this.getData(args);
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

    statusMedium(args){
        let data = this.getCounts(args);
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
<div class="emby fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');">
    
    <div class="name tr">${title}</div>
    ${item.date ? `<div class="br">${item.date}</div>` : ''}
    <a class="cover-link" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="${args.appIcon || '/apps/Emby/icon.png'}?version=${args.version}" /></a>
</div>
`;
    }

    test(args) {
        let data = this.getToken(args);
        return !!data?.token;
    }
}
