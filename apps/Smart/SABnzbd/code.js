module.exports = { 

    status: async (args) => {
        let data = await args.fetch(`api?output=json&apikey=${args.properties['apikey']}&mode=queue`);
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

        return args.liveStats([
            ['Queue', mbleft],
            ['Speed', kbpersec ]
        ]);
    },

    test: async (args) => {
        let data = await args.fetch(`api?output=json&apikey=${args.properties['apikey']}&mode=queue`);
        return isNaN(data?.queue?.mbleft) === false;
    }
}