class SABnzbd
{ 
    async status(args) {
        let data = await args.fetch(`api?output=json&apikey=${args.properties['apikey']}&mode=queue`);        
        if (isNaN(data?.queue?.mbleft) || isNaN(data?.queue?.kbpersec)){
            return '';
        }
        //console.log('data.queue', data.queue[0]);
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
            ['Speed', kbpersec ],
            //['File', data.filename],
            //['Time Left', data.timeleft]
        ]);
    }

    async test(args) {
        let data = await args.fetch(`api?output=json&apikey=${args.properties['apikey']}&mode=queue`);
        return isNaN(data?.queue?.mbleft) === false;
    }
}

module.exports = SABnzbd;