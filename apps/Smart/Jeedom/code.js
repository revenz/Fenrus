class Jeedom {
    async status(args) {
		const dataUpdate = await args.fetch(
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
		
		const dataMessage = await args.fetch(
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
			},
			
			const update = parseInt(dataUpdate.result);
			const message = dataMessage.result.length;
			
			return args.liveStats([
				['Update', update],
				['Message', message],
			]);
		}
		
		async test(args) {
			const data = await args.fetch(`${args.url}/core/api/jeeApi.php`);
			console.error(data.id);
			if (data.id == '99999'){
                return true;
				} else {
			return false;			