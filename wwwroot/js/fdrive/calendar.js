class FenrusDriveCalendar
{
    initDone = false;
    calendar;
    selectedStart; selectedEnd;
    constructor(){
    }

    show(){
        if(this.initDone)
            return;
        this.initDone = true;
        var calendarEl = document.getElementById('calendar-actual');
        let clearTimer = null;
        let doubleClick, clickTimer;
        let eventDoubleClick, eventClickTimer;
        this.calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: 'timeGridWeek',
            editable: true,
            selectable: true,
            businessHours: true,
            nowIndicator: true,
            navLinks: true, // can click day/week names to navigate views
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
            },
            events: {
                url: '/calendar'
            },
            dateClick: (info) => {
                let singleClick = info.dateStr;

                if(doubleClick === singleClick){
                    console.log('Double-click!');
                    doubleClick = null;
                    this.addActual(info.start, info.end);
                }else{
                    doubleClick = singleClick;
                    clearInterval(clickTimer);
                    clickTimer = setInterval(() =>{
                        doubleClick = null;
                        clearInterval(clickTimer);
                    }, 500);
                }
            },
            eventClick: (info) => {
                let event = info.event;
                let singleClick = event.id;

                if(eventDoubleClick === singleClick){
                    console.log('event Double-click!', event);
                    eventDoubleClick = null;
                    this.editEvent({
                        Uid: event.id,
                        Name: event.title,
                        StartUtc: event.start,
                        EndUtc: event.end
                    });
                }else{
                    eventDoubleClick = singleClick;
                    clearInterval(eventClickTimer);
                    eventClickTimer = setInterval(() =>{
                        eventDoubleClick = null;
                        clearInterval(eventClickTimer);
                    }, 500);
                }
            },
            select: (info) => {
                if(clearTimeout) {
                    clearTimeout(clearTimer);
                    clearTimer = null;
                }
                this.selectedStart = info.start;
                this.selectedEnd = info.end;
            },
            unselect: () => {
                clearTimer = setTimeout(() => {
                    this.selectedStart = null;
                    this.selectedEnd = null;                    
                }, 250);
            },
            eventDrop: async (info) => {
                console.log('event dropped', info);                
                let model = {
                    Uid: info.event.id,
                    Name: info.event.title,
                    StartUtc: new Date(info.event.start).toISOString(),
                    EndUtc: new Date(info.event.end).toISOString()
                };
                let response = await fetch('/calendar', {
                    method:'post',
                    body: JSON.stringify(model),
                    headers: {
                        'Content-Type': 'application/json'
                    }
                });
                this.calendar.refetchEvents();
            }
        });
        this.calendar.render();
    }

    async add() {
        await this.addActual();
    }

    async addActual(start, end) {
        start = start || this.selectedStart || this.neareast30mins(new Date());
        end = end || this.selectedEnd || this.addMinutesToDate(start);
        await this.editEvent({
            Name: '',
            StartUtc: start,
            EndUtc: end
        });
    }
    
    async editEvent(event)
    {
        let editing = !!event.Uid;
        console.log('event', event);
        let form = modalFormInput({ label: 'Name', name: 'Name', type: 'text', value: event.Name }) +
            modalFormInput({ label: 'Start', name: 'StartUtc', type: 'datetime', value: this.dateToString(event.StartUtc) }) +
            modalFormInput({ label: 'End', name: 'EndUtc', type: 'datetime', value: this.dateToString(event.EndUtc) });
        let result = await modalForm(editing ? 'Edit Calendar Event' : 'New Calendar Event', form);
        if(!result)
            return;
        if(editing)
        {
            // check if the edit changed
            result.Uid = event.Uid;
            if(JSON.stringify(event) === JSON.stringify(result))
                return;
        }
        
        result.StartUtc = new Date(result.StartUtc).toISOString();
        result.EndUtc = new Date(result.EndUtc).toISOString();
        let response = await fetch('/calendar', {
            method:'post',
            body: JSON.stringify(result),
            headers: {
                'Content-Type': 'application/json'
            }
        });
        this.calendar.refetchEvents();
        
    }

    createElement(readOnly) {
        let ele = document.createElement('div');
        ele.className = 'note'  + (readOnly ? ' readonly' : '');
        ele.innerHTML = '<div class="controls">' +
            '<i class="up fa-solid fa-caret-up"></i>' +
            (readOnly ? '' : '<i class="delete fa-sharp fa-solid fa-trash"></i>') +
            '<i class="down fa-solid fa-caret-down"></i>' +
            '</div>' +
            `<input type="text" placeholder="Note Title" ${(readOnly ? 'disabled' : '')}  />` +
            `<div class="content-editor" contenteditable="true" spellcheck="false" ` +
            (readOnly ? ' oncut="return false" onpaste="return false" onkeydown="if(event.metaKey) return true; return false;" ' : '') +
            '></div>';
        ele.querySelector('.up').addEventListener('click', (event) => this.move(event.target, true));
        ele.querySelector('.down').addEventListener('click', (event) => this.move(event.target, false));
        if(!readOnly) {
            ele.querySelector('.delete').addEventListener('click', (event) => this.deleteNote(event.target));
            ele.querySelector('input').addEventListener('change', (event) => this.onChange(event));
            let contentEditor = ele.querySelector('.content-editor');
            contentEditor.addEventListener('paste', (event) => this.onPaste(event));
            contentEditor.addEventListener('focusout', (event) => this.onChange(event));
        }
        return ele;
    }
    
    
    neareast30mins(date){
        date = date || new Date();

        // get the current time components
        let hours = date.getHours();
        let minutes = date.getMinutes();

        // round the minutes to the nearest 30
        minutes = Math.round(minutes / 30) * 30;

        // adjust the hours if necessary
        if (minutes === 60) {
            minutes = 0;
            hours += 1;
        }

        // create a new Date object with the rounded time
        return new Date(date.getFullYear(), date.getMonth(), date.getDate(), hours, minutes);
    }
    addMinutesToDate(date, minutes = 30) 
    {
        // create a new Date object with the same date and time as the input
        let newDate = new Date(date.getTime());
    
        // add the specified number of minutes to the new Date object
        newDate.setMinutes(newDate.getMinutes() + minutes);
    
        // return the new Date object
        return newDate;
    }



    dateToString(date){
        // get the date components
        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0'); // add leading zero if necessary
        const day = String(date.getDate()).padStart(2, '0'); // add leading zero if necessary
        const hours = String(date.getHours()).padStart(2, '0'); // add leading zero if necessary
        const minutes = String(date.getMinutes()).padStart(2, '0'); // add leading zero if necessary
        const seconds = String(date.getSeconds()).padStart(2, '0'); // add leading zero if necessary

        // concatenate the components into the desired format
        return `${year}-${month}-${day}T${hours}:${minutes}:${seconds}`;
    }
}