class DailyReminder
{
    async status(args) {
		let startDate = new Date(args.properties['startDate']);
		let reminderType = args.properties['reminderType'];
		let typeIncrement = parseInt(args.properties['typeIncrement']);
		let dayOffset = parseInt(args.properties['dayOffset'] ?? 0);
		let todayIcon = args.properties['todayIcon'];
		let notTodayIcon = args.properties['notTodayIcon'];
		let test = args.properties['test'] ?? false;
		let today = new Date();
		
		startDate.setDate(startDate.getDate()+dayOffset);
		
		let resultantNextDay; 
		if(reminderType == "months") {
			resultantNextDay = this.xmonths(today, startDate, typeIncrement);
		} else if(reminderType == "weeks") {
			resultantNextDay = this.xdays(today, startDate, typeIncrement*7);
		} else {
			resultantNextDay = this.xdays(today, startDate, typeIncrement);
		}
		let displayText = args.properties['displayText'] ?? 'in {days} days';
		
		let daysDifference = this.getDaysBetween(today, resultantNextDay);
		if(isNaN(daysDifference)){
			return;
		}
		
		if(daysDifference == 0) {
			if(todayIcon != null && todayIcon.length > 0 && !test){
				args.changeIcon(todayIcon);
			}
			return args.liveStats([
				['Today']
			]);
		} else {
			if(notTodayIcon != null && notTodayIcon.length > 0 && !test){
				args.changeIcon(notTodayIcon);
			}
			if (daysDifference == 1) {
				return args.liveStats([
					['Tomorrow']
				]);
			} else {
				return args.liveStats([
					[displayText.replace("{days}",daysDifference)]
				]);
			}
		}
    }


	xmonths(today, startDate, increment) {
		while(this.getDaysBetween(today, startDate) < 0) {
			startDate.setMonth(startDate.getMonth()+increment);
		}
		return startDate;
	}
	
	
	xdays(today, startDate, increment) {
		while(this.getDaysBetween(today, startDate) < 0) {
			startDate.setDate(startDate.getDate()+increment);
		}
		return startDate;
	}
	
	getDaysBetween(date1, date2){
		let timeDif = date2.getTime() - date1.getTime();
		return Math.ceil(timeDif / (1000 * 3600 * 24));
	}
	
	
    async test(args) {
		//set a flag for test as changeIcon does not support being called from test func
		args.properties['test'] = true;
        let mainStatus = await this.status(args);
		console.log("mainStatus",mainStatus);
        return mainStatus != null;
    }
}

module.exports = DailyReminder;