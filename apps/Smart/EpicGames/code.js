const e = require("express");

class EpicGames {
    dataAge;
    data;
    dataIndex = 0;

    getCountryCode(args){        
        let country = args.properties['country'] || 'en-NZ';
        country = country.substring(country.indexOf('-') + 1).toUpperCase();
        return country;
    }

    async getData(args)
    {        
        if(this.data?.length && this.dataAge && this.dataAge >= new Date().getTime() - (10 * 60 * 1000)){
            ++this.dataIndex;

            if(this.dataIndex >= this.data.length - 1)
                this.dataIndex = 0;
            return this.data[this.dataIndex];
        }

        let freeGame = await this.getFreeGame(args);
        let results = await this.getOnSale(args) || [];
        
        if(freeGame)
            results.splice(0, 0, freeGame);

        this.data = results;
        this.dataIndex = 0;
        this.dataAge = new Date().getTime();
        return this.data[this.dataIndex];
    }

    async getFreeGame(args)
    {
        let data = await args.fetch('https://store-site-backend-static.ak.epicgames.com/freeGamesPromotions?country=' + this.getCountryCode(args));
        if (!data?.data?.Catalog?.searchStore?.elements?.length)
            return null;            
        let now = new Date().getTime();
        let sorted = data.data.Catalog.searchStore.elements.filter(x =>{
            let lineOffers = x.price?.lineOffers[0]?.appliedRules;
            return lineOffers?.length > 0;
        });
        return this.getItem(sorted[0], args);
    }

    async getOnSale(args){
        let data = await args.fetch('https://store-site-backend-static-ipv4.ak.epicgames.com/storefrontLayout?country=' + this.getCountryCode(args));
        
        let onSale = data?.data?.Storefront?.storefrontModules?.filter(x => x.id === 'games-on-sale');
        if(!onSale)
            return;
        onSale = onSale[0];
        let results = [];
        for(let offer of onSale.offers)
        {
            let item = this.getItem(offer.offer, args);
            if(item)
                results.push(item);
        }
        return results;
    }

    getItem(data, args) {
        if(!data)
            return;
        let item = {};
        let country = args.properties['country'] || 'en-NZ';
        item.title = data.title;
        item.image = data.keyImages.filter(x => x.type === 'Thumbnail')[0].url;
        let slug = data.productSlug;
        if(!slug){
            if(data.offerMappings?.length && data.offerMappings[0].pageSlug)
                slug = data.offerMappings[0].pageSlug;
        }else if(slug.indexOf('/') > 0)
            slug = slug.substring(0, slug.indexOf('/')); // sometimes has a /home on the end which breaks the url
        if(!slug)
            return;
        item.link = `https://www.epicgames.com/store/${country}/p/${slug}`;
        item.price = data.price.totalPrice.discountPrice === 0 ? 'FREE' : data.price.totalPrice.fmtPrice.discountPrice;
        if(item.price.indexOf('$') > 0)
            item.price = item.price.substring(item.price.indexOf('$'));
        item.discount = data.price.totalPrice.discountPrice === 0 ? '100%' : 
                       (100 - (data.price.totalPrice.discountPrice / data.price.totalPrice.originalPrice * 100).toFixed(0)) + '%';
        return item;
    }

    async status(args) {
        
        if(args.size === 'small' || args.size === 'medium')
            return;

        let item = await this.getData(args);
        if(!item)
            return;

        if(this.data.length > 10)
            this.data.splice(10);

        return args.carousel(this.data.map(x => {
            return this.getItemHtml(args, x);
        }));
    }
    
    getItemHtml(args, item) {
        return `
<div class="epicgames fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');">
    
    <div class="name tr">${args.Utils.htmlEncode(item.title)}</div>
    <div class="price br">
        ${item.price}
    </div>
    <div class="discount bl">
        <div class="down-icon"><span class="icon-arrow-left"></span></div>
        ${item.discount}
    </div>
    <a class="cover-link" target="${args.linkTarget}" href="${item.link}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="${args.appIcon || '/apps/Epic%20Games/icon.png'}?version=${args.version}" /></a>
</div>
`;
    }
}

module.exports = EpicGames;