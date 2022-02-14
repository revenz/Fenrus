
class Utils {

    newGuid() {
        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    }

    htmlEncode(text) {
        if(text === undefined) 
            return '';
        if(typeof(text) !== 'string')
            text = '' + text;
        return text.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\"/g, "&#34;").replace(/\'/g, "&#39;");
    }
        
    formatBytes(bytes) {
        if (typeof (bytes) === 'string') {
            bytes = parseFloat(bytes, 10);
        }

        if (isNaN(bytes))
            return '';

        let order = 0;
        const sizes = ["B", "KB", "MB", "GB", "TB"];
        while (bytes >= 1000 && order < sizes.length - 1) {
            ++order;
            bytes /= 1000; 
        }
        return bytes.toFixed(2) + ' ' +sizes[order];
    }

    formatTime(date, showSeconds) {
        let minute = date.getMinutes();
        if (minute < 10)
            minute = '0' + minute;
        let hour = date.getHours();
        let meridian = 'am';
        if (hour >= 12) {
            meridian = 'pm';
            hour -= hour == 12 ? 0 : 12;
        }
        if (hour == 0)
            hour = 12;

        if (showSeconds) {
            let seconds = date.getSeconds();
            if (seconds < 10)
                seconds = '0' + seconds;
            return hour + ':' + minute + ':' + seconds + ' ' + meridian;
            
        }
        return hour + ':' + minute + ' ' + meridian;
    }
    
    formatTime(date, showSeconds) {
        let minute = date.getMinutes();
        if (minute < 10)
            minute = '0' + minute;
        let hour = date.getHours();
        let meridian = 'am';
        if (hour >= 12) {
            meridian = 'pm';
            hour -= hour == 12 ? 0 : 12;
        }
        if (hour == 0)
            hour = 12;

        if (showSeconds) {
            let seconds = date.getSeconds();
            if (seconds < 10)
                seconds = '0' + seconds;
            return hour + ':' + minute + ':' + seconds + ' ' + meridian;
            
        }
        return hour + ':' + minute + ' ' + meridian;
    }


    formatDate(date) {
        if (!date)
            return '';
        if (typeof (date) === 'string')
            date = new Date(date);
        let now = new Date();
        if (date.getTime() > (now.getTime() - (24 * 60 * 60 * 1000)))
        {
            // within last 24 hours
            if (date.getDate() == now.getDate()) {
                // today, so return time
                return this.formatTime(date);
            }
        }
        let day = date.getDate();
        let month = date.getMonth() + 1; // zero based
        let year = date.getFullYear();
        if (month < 10)
            month = '0' + month;
        if (day < 10)
            day = '0' + day;
        return year + '-' + month + '-' + day;
    }
}

module.exports = Utils;