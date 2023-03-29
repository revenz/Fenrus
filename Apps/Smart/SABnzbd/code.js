class SABnzbd
{
    history;
    pfImages;
    constructor(){
        this.history = [];
        this.pfImages = {};
    }

    fetch(args, url) {
        let result = args.fetch(url);
        return result?.Result || result;
    }
    getUrl(args, mode) {
        return `api?output=json&apikey=${args.properties['apikey']}&mode=${mode}`;
    }

    getData(args, mode) {
        let url = this.getUrl(args, mode);
        return this.fetch(args, url); 
    }

    status(args) 
    {        
        if(args.size.indexOf('large') >= 0)
            return this.statusLarge(args);
        return this.statusMedium(args);
    }

    statusMedium(args)
    {        
        let data = this.getData(args, 'queue');
        if (isNaN(data?.queue?.mbleft) || isNaN(data?.queue?.kbpersec)){
            return '';
        }
        let mbleft = parseFloat(data.queue.mbleft, 10);
        if(isNaN(mbleft))
            mbleft = 0;                    
        mbleft = args.Utils.formatBytes(mbleft * 1000 * 1000);

        let kbpersec = parseFloat(data.queue.kbpersec, 10);
        if(isNaN(kbpersec))
            kbpersec = 0;                    
        kbpersec = args.Utils.formatBytes(kbpersec * 1000) + '/s';

        let item = data.queue?.slots?.find(x => x?.status === 'Downloading');

        if(item?.filename){
			let display = args.properties['displayType'];
			if(display && display == 'overview'){
				return args.liveStats([
					[item.filename],
					['Current DL Speed', kbpersec ],
					['Total Queue Size', data.queue.sizeleft ?? 0],
					['Total Time Left', data.queue.timeleft ?? 0]
					
				]);
			} else {
				let mb = parseFloat(item.mb, 10);
				let mbleft = parseFloat(item.mbleft, 10);
				let percent = (mb - mbleft) / mb * 100;
				return args.liveStats([
					[item.filename],
					['Time Left', item.timeleft],
					['Speed', kbpersec ],
					['Percent', percent.toFixed(2) + '%']
				]);
			}
            

        }

        return args.liveStats([
            ['Queue', mbleft],
            ['Speed', kbpersec ]
        ]);
    }

    statusLarge(args)
    {
        let data = this.getData(args, 'queue');
        let history = this.getData(args, 'history');
        
        let mbleft = parseFloat(data?.queue?.mbleft, 10);
        if(isNaN(mbleft))
            mbleft = 0;      
        mbleft = args.Utils.formatBytes(mbleft * 1000 * 1000);

        let kbpersec = parseFloat(data?.queue?.kbpersec, 10);
        if(isNaN(kbpersec))
            kbpersec = 0;

        this.history.push({
            date: new Date(),
            speed: kbpersec,
        });
        let paused = data.paused;
        let isDownloading = data.queue.status === 'Downloading';

        kbpersec = args.Utils.formatBytes(kbpersec * 1000) + '/s';

        let item = data.queue?.slots?.find(x => x?.status === 'Downloading');
        if(isDownloading)
        {
            let speeds = this.history.map(x => x.speed);
            let mb = parseFloat(item.mb, 10);
            let mbleft = parseFloat(item.mbleft, 10);
            let percent = (mb - mbleft) / mb * 100;
            percent.toFixed(2) + '%'
            let chartBase64 = args.chart.line({
                title: '',
                min: Math.min(...speeds),   
                max: Math.max(...speeds),
                labels: this.history.map(x => x.date),
                data: [speeds]
            });

            let image = this.searchForImage(args, item.filename);
            
            if(image)
            {
                return args.carousel([
                    this.getCarouselItemHtml(args, image, item.filename, kbpersec, percent.toFixed(1) + '%'),
                    this.getCarouselItemHtml(args, chartBase64, item.filename, kbpersec, percent.toFixed(1) + '%')
                ]);
            }
            return args.carousel([this.getCarouselItemHtml(args, chartBase64, item.filename, kbpersec, percent.toFixed(1) + '%')]);
        }
        else if(history?.history?.slots?.length)
        {
            // history
            let items = [];
            for(let item of history.history.slots){
                if(item.status !== 'Completed')
                    continue;
                let image = this.searchForImage(args, item.series || item.name);
                let millisecondsAgo = new Date().getTime() - (item.completed * 1000);
                args.log('about to push item');
                items.push(this.getCarouselItemHtml(args, image, item.name, item.size, 
                    '<i style="color:#6ebd6e;margin-right:0.5rem" class="fa-solid fa-download"></i>' + args.humanizer.Milliseconds(millisecondsAgo, { Precision: 1 }) + ' ago'));
                if(items.length > 4)
                    break;
            }
            if(items.length)
                return args.carousel(items);
        }

        if(item?.filename){
			let display = args.properties['displayType'];
			if(display && display == 'overview'){
				return args.liveStats([
					[item.filename],
					['Current DL Speed', kbpersec ],
					['Total Queue Size', data.queue.sizeleft ?? 0],
					['Total Time Left', data.queue.timeleft ?? 0]
					
				]);
			} else {
				let mb = parseFloat(item.mb, 10);
				let mbleft = parseFloat(item.mbleft, 10);
				let percent = (mb - mbleft) / mb * 100;
				return args.liveStats([
					[item.filename],
					['Time Left', item.timeleft],
					['Speed', kbpersec ],
					['Percent', percent.toFixed(2) + '%']
				]);
			}
        }

        return args.liveStats([
            ['Queue', mbleft],
            ['Speed', kbpersec ]
        ]);
    }

    searchForImage(args, filename) {
        if(this.pfImages[filename] === undefined)
        {
            let searchTerm = filename.replace(/\\/g, '/');
            searchTerm = searchTerm.replace(/\/[\d]+\/[\d]+$/, '');
            if(/([^\/]+)s[\d]+e[\d]+/i.test(searchTerm)){
                searchTerm = /([^\/]+)s[\d]+e[\d]+/i.exec(searchTerm)[1];
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

            args.log('SABnzbd search term: ' + searchTerm);

            let images = args.imageSearch(searchTerm);
            this.pfImages[filename] = images?.length ? images[0] : '';      
        }
        return this.pfImages[filename];
    }

    getCarouselItemHtml(args, imgSrc, name, br, bl){
        return `
        <div class="sabnzbd fill" style="background-image:url('${args.Utils.htmlEncode(imgSrc || '/apps/SABnzbd/icon.png')}');">    
            <div class="name tr wrap">${name}</div>
            ${br ? `<div class="br">${br}</div>` : ''}
            ${bl ? `<div class="bl">${bl}</div>` : ''}
            <a class="cover-link" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}" />
            <a class="app-icon" target="${args.linkTarget}" href="${args.Utils.htmlEncode(args.url)}"><img src="${args.appIcon || '/apps/SABnzbd/icon.png'}?version=${args.version}" /></a>
        </div>
        `
    }

    test(args) {
        let data = this.fetch(args, `api?output=json&apikey=${args.properties['apikey']}&mode=queue`);
        return isNaN(data?.queue?.mbleft) === false;
    }
}
