class ValetudoRE {

	async status(args)
	{

        let consumable = await args.fetch(`${args.url}/api/consumable_status`);
        let state = await args.fetch(`${args.url}/api/current_status`);
        if(!consumable?.consumables.main_brush_work_time || !state?.human_state)
		return args.liveStats([['Status','Error, no data'],]);

		return args.barInfo([
			{
				label:'Status',
				value:state.human_state,
			},
			{
				label:'Main brush',
				percent: Math.round(100-((consumable.consumables.main_brush_work_time*100)/1080000))  || 0,
			},
			{
				label:'Side brush',
				percent: Math.round(100-((consumable.consumables.side_brush_work_time*100)/720000)) || 0,
			},
			{
				label:'Filter',
				percent: Math.round(100-((consumable.consumables.filter_work_time*100)/540000))  || 0,
			},
			{
				label:'Sensor',
				percent: Math.round(100-((consumable.consumables.sensor_dirty_time*100)/108000)) || 0,
			},
		]);
	}

	async test(args) {
		const data = await args.fetch(`${args.url}/api/current_status`);
		return data.error_code == '0';
	}
}
module.exports = ValetudoRE;
