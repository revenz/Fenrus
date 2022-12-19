class Jeedom {
	async getDataUpdate(args)
	{
		return await args.fetch(
			{
				url: `${args.url}/core/api/jeeApi.php`,
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
				},
				body: JSON.stringify({
					'jsonrpc': '2.0',
					'method': 'update::nbNeedUpdate',
					'params': {
						'apikey': args.properties["apikey"],
					},
				}),
			},
		);
	}

	async getDataMessage(args)
	{
		return await args.fetch(
		{
			url: `${args.url}/core/api/jeeApi.php`,
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({
				jsonrpc: '2.0',
				method: 'message::all',
				params: {
					apikey: args.properties['apikey'],
				},
			}),
		});
	}

    async status(args) 
	{
        const [ dataUpdate, dataMessage ] = await Promise.all([
			await this.getDataUpdate(args),
			await this.getDataMessage(args)
		]);

		if(!dataUpdate?.result)
			return;
			
		const update = parseInt(dataUpdate.result, 10);
		const message = dataMessage?.result?.length || 0;
		
		return args.liveStats([
			['Update', update],
			['Message', message],
		]);
	}
		
	async test(args) {
		const data = await args.fetch(`${args.url}/core/api/jeeApi.php`);
		console.log(data.id);
		return data.id == '99999';
	}
}
module.exports = Jeedom;		
