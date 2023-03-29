class HDHomeRun
{       
    getChannelCount(args){
        let data = args.fetch('lineup.json?show=found');
        if(!data)
            return 0;
        return data.length || 0;
    } 
    getTuners(args){
        let html = args.fetch({
            url: 'tuners.html',
            headers: {
                'Accept': 'text/html'
            }
        });
        if(!html)
            return;
            
        let inUse = 0;
        let total = 0;
        for(let i=0;i<10;i++){
            let rgx = new RegExp('Tuner ' + i + '(.*?)<\/tr>', 'g');
            let match = rgx.exec(html);
            if(!match)
                break;
            match = match[0];
            ++total;
            if(match.indexOf('<td>none</td>') < 0 && match.indexOf('not in use') < 0)
                ++inUse;
        }
        return { total, inUse };
    }

    status(args){
        let channels =this.getChannelCount(args);
        let tuners = this.getTuners(args);

        if(!channels && !tuners?.total)
            return;
        
        return args.liveStats([
            ['Channels', channels],
            ['Tuners In Use', `${tuners.inUse} of ${tuners.total}`]
        ]);
    }
    test(args) {
        let data = this.getChannelCount(args);
        return data > 0;
    }
}
