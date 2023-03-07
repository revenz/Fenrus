class OctoPrint {

    doFetch(args, endpoint) {
        var data = args.fetch({
            url: `api/` + endpoint,
            timeout: 5,
            headers: {
                "X-Api-Key": args.properties['apikey']
            }
        })
        return data;
    }

    async status(args) {
        let data = await this.doFetch(args, "job");

        let percentProgress = data?.progress?.completion;
        let printTimeLeftSecs = data?.progress?.printTimeLeft;
        let state = data?.state ?? 'Unknown';

        if(args.properties['webcam'])
        {
            let url = args.url;
            if(url.endsWith('/') === false)
                url += '/';
            args.changeIcon(url + 'webcam/?action=snapshot&_ts=' + new Date().getTime());
        }

        if (percentProgress == null || state == 'Unknown' || state == "Offline" || state == "Operational") {
            //assume no print running so just show state
            return args.liveStats([
                ['State', state]
            ]);
        }
        if (percentProgress > 100) {
            percentProgress = 100;
        } else {
            percentProgress = Math.floor(percentProgress * 100) / 100;
        }

        if (printTimeLeftSecs == null) {
            printTimeLeftSecs = "Unknown";
        } else if(printTimeLeftSecs == 0) {
            printTimeLeftSecs = "Finished"
        } else {
            printTimeLeftSecs = args.Utils.formatMilliTimeToWords(printTimeLeftSecs * 1000, printTimeLeftSecs < 60);
        }

        return args.liveStats([

            ['Progress: ', percentProgress + '%'],
            ['Time Left: ', printTimeLeftSecs],
            ['State', state]
            //['Job' , data?.job?.file?.name]
        ]);

    }

    async test(args) {
        let data = await this.doFetch(args, "version");
        console.log('data', data);
        return isNaN(data?.api) === false;
    }
}

