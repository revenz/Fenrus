class Flaresolverr {

        async getLastRelease(args){
	        const data = await args.fetch({
		        url: 'https://api.github.com/repos/FlareSolverr/FlareSolverr/releases/latest',
		        timeout: 500
	        });
	        return data;
        }
        async getStatus(args){
		const data = await args.fetch(args.url);
		return data;
        }

async status(args) {
		const [ data, latest ] = await Promise.all([
			await this.getStatus(args),
			await this.getLastRelease(args)
		]);
		let indicator = '';
		if(latest.tag_name != data.version){
			indicator = 'update';
		}
		args.setStatusIndicator(indicator ? 'update' : '');
		if(!data?.version)
		return args.liveStats([
			['Up','false']
		]);
		
		
		return args.liveStats([
			['Up',data.msg == 'FlareSolverr is ready!'],
		]);
	}
        async test(args) {
                const data = await args.fetch(args.url);
                return data.msg == 'FlareSolverr is ready!';
        }
}
module.exports = Flaresolverr;
