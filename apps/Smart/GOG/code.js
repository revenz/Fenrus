
class EpicGames {
    dataAge;
    data;
    dataIndex = 0;

    async getData(args)
    {        
        if(this.data?.length && this.dataAge && this.dataAge >= new Date().getTime() - (10 * 60 * 1000)){
            ++this.dataIndex;

            if(this.dataIndex >= this.data.length - 1)
                this.dataIndex = 0;
            return this.data[this.dataIndex];
        }

        let results = await this.getOnSale(args) || [];
        this.data = results;
        this.dataIndex = 0;
        this.dataAge = new Date().getTime();
        return this.data[this.dataIndex];
    }


    async getOnSale(args){
        let currency = args.properties['currency'] || 'AUD';
        let country = args.properties['country'] || 'NZ';
        let data = await args.fetch(`https://menu.gog.com/v1/store/configuration?locale=en-US&currency=${currency}&country=${country}`);
        if(!data || !data['on-sale-now'] || isNaN(data['on-sale-now'].products?.length))
            return;

        let onSale = data['on-sale-now'].products;
        let results = [];
        for(let product of onSale)
        {
            let item = this.getItem(product, args);
            if(item)
                results.push(item);
        }
        return results;
    }

    getItem(data, args) {
        if(!data)
            return;
        let currencySymobol = args.properties['currencySymobol'] || '$';
        let item = {};
        item.title = data.title;
        item.image = 'https:' + data.image + '_product_tile_304.webp';
        item.id = data.id;        
        item.link = 'https://www.gog.com' + data.url;
        item.price = data.price.isFree ? 'FREE' : currencySymobol + data.price.amount;
        item.discount = data.price.isFree ? '100%' : data.price.discountPercentage + '%';
        return item;
    }

    async status(args) {
        let item = await this.getData(args);
        if(!item)
            return;

        return args.carousel(this.data.map(x => {
            return this.getItemHtml(args, x);
        }));
    }

    getItemHtml(args, item) {
        return `
        <div class="gog fill" style="background-image:url('${args.Utils.htmlEncode(item.image)}');">
            
            <div class="name">${args.Utils.htmlEncode(item.title)}</div>
            <div class="price">
                ${item.price}
            </div>
            <div class="discount">
                <div class="down-icon"><span class="icon-arrow-left"></span></div>
                ${item.discount}
            </div>
            <a class="cover-link" target="${args.linkTarget}" href="${item.link}" />
            <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="/apps/GOG/icon.png" /></a>
        </div>
        `;
    }
}

module.exports = EpicGames;