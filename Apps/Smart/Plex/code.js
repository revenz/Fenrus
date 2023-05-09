class Plex
{ 
    getUrl(endpoint, args) {
        return `library/${endpoint}?X-Plex-Token=${args.properties['token']}`;
    }

    fetch(args, url) {
        return args.fetch(url).data;
    }
    
    status(args) {
        if(!args.properties){
            args.log('Error in plex app, no properties set');
            return;
        }
        let data = this.fetch(args, this.getUrl('recentlyAdded', args));

        if(!data.MediaContainer)
            return;
        if(args.size.indexOf('large') >= 0)
            return this.statusXLarge(args, data);
        else
            return this.statusMedium(args, data);
    }

    statusXLarge(args, data){
        let url = args.url;
        if(url.endsWith('/'))
            url = url.substring(0, url.length - 1);
        let items =  [];
        for(let md of data.MediaContainer.Metadata || [])
        {
            if(items.length > 9)
                break;
            items.push({
                title: md.title,
                parentTitle: md.parentTitle,
                year: md.parentYear || md.year,
                image: md.art?.length > 2 ? args.proxy(url + md.art + '?X-Plex-Token='  + args.properties['token']) : null
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
        let recent = data?.MediaContainer?.size ?? 0;
        data = this.fetch(args, this.getUrl('onDeck', args));
        let ondeck = data?.MediaContainer?.size ?? 0;
        return args.liveStats([
            ['Recent', recent],
            ['On Deck', ondeck]
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
    <div class="br">${item.year}</div>
    <a class="cover-link" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="${args.appIcon || '/apps/Plex/icon.png'}?version=${args.version}" /></a>
</div>
`;
    }

    test(args) {
        if(!args.properties)
            return false;
        let url = this.getUrl('recentlyAdded', args);
        let data = this.fetch(args, url);
        return isNaN(data.MediaContainer?.size) === false;
    }
}
