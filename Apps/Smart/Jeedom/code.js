class Jeedom {
	getDataUpdate(args)
	{
		return args.fetch(
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
		).data;
	}

	getDataMessage(args)
	{
		return args.fetch(
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
		}).data;
	}

    status(args) 
	{
		let dataUpdate = this.getDataUpdate(args);
		let dataMessage = this.getDataMessage(args);

		if(!dataUpdate?.result)
			return;
			
		const update = parseInt(dataUpdate.result, 10);
		const message = dataMessage?.result?.length || 0;
		
		return args.liveStats([
			['Update', update],
			['Message', message],
		]);
	}
		
	test(args) {
		const data = args.fetch(`${args.url}/core/api/jeeApi.php`).data;
		args.log(data.id);
		return data.id == '99999';
	}
}		
