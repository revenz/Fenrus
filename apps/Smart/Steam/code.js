
class Steam {

    async getData(args){
        let data = await args.fetch('https://store.steampowered.com/api/featuredcategories/');
        if (!data?.specials)
            return null;

        let results = [];        
        for(let d of data.specials.items){
            let item = this.getItem(d, args);
            if(results.findIndex(x => x.title === item.title) === -1)
                results.push(item);
        }
        return results;
    }

    getItem(data, args) {
        if(!data)
            return;
        let currency = args.properties['currency'] || '$';
        let item = {};
        item.id = data.id;
        item.title = data.name;
        item.image = data.large_capsule_image;
        item.link = `https://store.steampowered.com/app/${data.id}`;
        item.price = currency + (data.final_price / 100).toFixed(2);
        item.discount = data.discount_percent + '%';
        return item;
    }

    async status(args) {
        let data = await this.getData(args);
        if(!data)
            return;
       
        if(data.length > 10)
            data.splice(10);
        
        return args.carousel(data.map(x => {
            return this.getItemHtml(args, x);
        }));
    }
    
    getItemHtml(args, item) {
        return `
<div class="steam fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');">
    
    <div class="name">${args.Utils.htmlEncode(item.title)}</div>
    <div class="price">${item.price}</div>
    <div class="discount">
        <div class="down-icon"><span class="icon-arrow-left"></span></div>
        ${item.discount}
    </div>
    <a class="cover-link" target="${args.linkTarget}" href="${item.link}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="/apps/Steam/icon.png" /></a>
</div>
`;
    }
}

module.exports = Steam;