
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
        
        return `
<div class="steam fill" style="background-image:url('${args.Utils.htmlEncode(item.large_capsule_image)}');">
    
    <div class="name">${args.Utils.htmlEncode(item.name)}</div>
    <div class="price">
        ${'$' + (item.final_price / 100).toFixed(2)}
    </div>
    <div class="discount">
        <div class="down-icon"><span class="icon-arrow-left"></span></div>
        ${item.discount_percent +'%'}
    </div>
    <a class="cover-link" target="${args.linkTarget}" href="https://store.steampowered.com/app/${item.id}" />
    <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="/apps/Steam/icon.png" /></a>
</div>
`;
        // return `<img class="background" src=\"${args.Utils.htmlEncode(item.small_capsule_image)}" />` + args.liveStats([
        //     ['Game', `:html:<a target="_blank" href="https://store.steampowered.com/app/${item.id}">${args.Utils.htmlEncode(item.name)}</a>`],
        //     ['Price', '$' + (item.final_price / 100).toFixed(2)],
        //     ['Discount', item.discount_percent +'%'],
        // ]); 
    }
}

module.exports = Steam;