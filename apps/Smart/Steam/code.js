
class Steam {

    dataAge;
    data;
    dataIndex = 0;

    async getData(args){
        if(this.data && this.dataAge && this.dataAge >= new Date().getTime() - (10 * 60 * 1000)){
            ++this.dataIndex;
            if(this.dataIndex >= this.data.length - 1)
                this.dataIndex = 0;
            return this.data[this.dataIndex];
        }
        let data = await args.fetch('https://store.steampowered.com/api/featuredcategories/');
        if (!data?.specials)
            return null;

        this.data = data.specials.items;
        this.dataIndex = 0;
        this.dataAge = new Date().getTime();
        return this.data[this.dataIndex];
    }

    async status(args) {
        let item = await this.getData(args);
        if(!item)
            return;
        
        return `<img class="background" src=\"${args.Utils.htmlEncode(item.small_capsule_image)}" />` + args.liveStats([
            ['Game', `:html:<a target="_blank" href="https://store.steampowered.com/app/${item.id}">${args.Utils.htmlEncode(item.name)}</a>`],
            ['Price', '$' + (item.final_price / 100).toFixed(2)],
            ['Discount', item.discount_percent +'%'],
        ]); 
    }
}

module.exports = Steam;