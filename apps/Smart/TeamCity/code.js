class TeamCity
{
    doFetch (args, endpoint) {
        return args.fetch({ url: 'app/rest/' + endpoint, headers: { 'Authorization': 'Bearer ' + args.properties['token'] } })
    }
    
    async status(args) {
        if(!args.properties['token'])
            return;
        let data = await this.doFetch(args, 'buildQueue');
        let queue = data?.count ?? 0;

        data = await this.doFetch(args, 'builds?locator=running:true');
        if (data?.count > 1) {
            return args.liveStats([
                ['Queue', queue],
                ['Building', data.count]
            ]);
        }
        else if (data?.count == 1) {
            return args.liveStats([
                ['Queue', queue],//['Building', data.build[0].buildTypeId],
                ['Percent', data.build[0].percentageComplete + '%']
            ]);
        }
        else {
            data = await this.doFetch(args, 'builds');
            return args.liveStats([
                ['Queue', queue],
                ['Total', data?.build[0]?.id ?? '0']
            ]);
        }
    }
    
    async test(args) {
        let data = await this.doFetch(args, 'buildQueue');
        return isNaN(data?.count) === false;
    }
}

module.exports = TeamCity;