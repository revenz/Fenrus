class DailyReminder
{
    async status(args) {
		let reminderType = args.properties['reminderType'] ?? 'days';
		let typeIncrement = parseInt(args.properties['typeIncrement'] ?? 1);
		let dayOffset = parseInt(args.properties['dayOffset'] ?? 0);
		let hourOffset = parseInt(args.properties['hourOffset'] ?? 0);
		let todayIcon = args.properties['todayIcon'];
		let notTodayIcon = args.properties['notTodayIcon'];
		let test = args.properties['test'] ?? false;
		let displayText = args.properties['displayText'] ?? 'in {days} days';
		
		let today = new Date();
		
		let loopCount = 1;
		let startDateParamName = "startDate" + loopCount;
		let startDateParam;
		let closestDayDifference;
		
		while((startDateParam = args.properties[startDateParamName]) != null && startDateParam.length > 0) {
			
			let startDate = new Date(startDateParam);
			startDate.setDate(startDate.getDate()+dayOffset);
			startDate.setTime(startDate.getTime() + (hourOffset * 60 * 60 * 1000));
			let resultantNextDay; 
			if(reminderType == "months") {
				resultantNextDay = this.xmonths(today, startDate, typeIncrement);
			} else if(reminderType == "weeks") {
				resultantNextDay = this.xdays(today, startDate, typeIncrement*7);
			} else {
				resultantNextDay = this.xdays(today, startDate, typeIncrement);
			}
			let daysDifference = this.getDaysBetween(today, resultantNextDay);
			if(isNaN(daysDifference)){
				return;
			}
			
			if(closestDayDifference == null || (closestDayDifference > daysDifference)) {
				closestDayDifference = daysDifference;
			}
			
			startDateParamName = "startDate" + ++loopCount;
		}
		
		
		if(closestDayDifference == 0) {
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
			if (closestDayDifference == 1) {
				return args.liveStats([
					['Tomorrow']
				]);
			} else {
				return args.liveStats([
					[displayText.replace("{days}",closestDayDifference ?? 'Unknown')]
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
 
}

