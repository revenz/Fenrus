class FenrusDriveCalendar
{
    initDone = false;
    calendar;
    selectedStart; selectedEnd;
    popupElement; popupEventId;
    
    constructor(){
        document.body.addEventListener('driveResizeEvent', (event) => {
            if(this.calendar)
                this.calendar.render();
        })
    }
    
    hide(){
        this.closeEvent();
    }

    show(){
        if(this.initDone)
            return;
        this.initDone = true;
        var calendarEl = document.getElementById('calendar-actual');
        let clearTimer = null;
        let doubleClick, clickTimer;
        let eventDoubleClick, eventClickTimer;
        let popupElement, mouseInPopUp;
        
        let view = localStorage.getItem('CALENDAR_VIEW') || 'timeGridWeek';
        let scrollTime = null;
        if(view === 'timeGridWeek' || view === 'timeGridDay') {
            let sdate= new Date().getHours() < 22 ? new Date(new Date().setHours(new Date().getHours() + 2)) : new Date();
            scrollTime = sdate.getHours() + ':' + (sdate.getMinutes() < 10 ? '0' : '') + sdate.getMinutes();
        }

        this.calendar = new FullCalendar.Calendar(calendarEl, {
            initialView: view,
            editable: true,
            selectable: true,
            businessHours: true,
            scrollTime: scrollTime,
            nowIndicator: true,
            navLinks: true, // can click day/week names to navigate views
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                //right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
                right: 'dgMonth,dgWeek,dgDay,dgList'
            },
            customButtons:
            {
                dgMonth: {
                    text: 'Month',
                    click: () => this.changeView('dayGridMonth')
                },
                dgWeek: {
                    text: 'Week',
                    click: () => this.changeView('timeGridWeek')
                },
                dgDay: {
                    text: 'Day',
                    click: () => this.changeView('timeGridDay')
                },
                dgList: {
                    text: 'List',
                    click: () => this.changeView('listWeek')
                },
            },
            datesSet: (info) => {
                this.closeEvent();  
            },
            events: {
                url: '/calendar'
            },
            eventContent: (arg) => {
                if (arg.view.type === 'dayGridMonth' && arg.event.start.getHours() === 0) {
                    return { html: htmlEncode(arg.event.title) };
                } else {
                    return { html: '<b>' + htmlEncode(arg.timeText) + '</b> ' + htmlEncode(arg.event.title) };
                }
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
                    this.showEvent(info);
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
                await this.saveEvent(model);
            },
            eventMouseLeave: (info) => {
                if (popupElement) {
                    setTimeout(() => {
                        if(!mouseInPopUp) {
                            popupElement.remove();
                            popupElement = null;
                        }
                    }, 100);
                }
            },
            eventMouseEnter: (info) => {
            }
        });
        this.calendar.render();
        this.setViewButton(view);
    }
    
    changeView(view){
        localStorage.setItem('CALENDAR_VIEW', view);
        this.calendar.changeView(view);
        this.setViewButton(view)
    }
    
    setViewButton(view)
    {
        let title = view === 'dayGridMonth' ? 'Month' :
            view === 'timeGridWeek' ? 'Week' :
                view === 'timeGridDay' ? 'Day' :
                    'List';
        let buttons = document.querySelectorAll('#fdrive-calendar .fc-header-toolbar .fc-toolbar-chunk:last-child .fc-button');
        for(let btn of buttons){
            btn.classList.remove('fc-button-active');
            if(btn.getAttribute('title') === title)
                btn.classList.add('fc-button-active');
        }
    }
    
    closeEvent(){
        if(this.popupElement) {
            this.popupElement.remove();
            this.popupElement = null;
        }
        this.popupEventId = null;
    }
    
    showEvent(info)
    {
        let hoverElement = info.el;

        if(this.popupElement)
        {
            this.popupElement.remove();
            if(this.popupEventId === info.event.id) 
            {
                this.popupEventId = null;
                return;
            }
        }
        this.popupEventId = info.event.id;
        // Create the popup element
        this.popupElement = document.createElement('div');
        this.popupElement.id = 'fdrive-calendar-popover';
        this.popupElement.style.width = '300px';
        this.popupElement.style.height = '106px';
        this.popupElement.style.position = 'absolute';

        // Create the triangle element
        const triangleElement = document.createElement('div');
        triangleElement.classList.add('pointer');
        triangleElement.style.position = 'absolute';
        this.popupElement.appendChild(triangleElement);

        // Create the content element
        const contentElement = document.createElement('div');
        contentElement.style.padding = '10px';
        //const options = { year: 'numeric', month: 'long', day: 'numeric' };
        //const readableDate = info.event.start.toLocaleDateString(navigator.language, options);
        const readableDate = this.formatDateRange(info.event.start, info.event.end);
        let editable = info.event.durationEditable !== false;
        contentElement.innerHTML = '<div>' +
            '<div class="title">' +
            `   <span class="name">${htmlEncode(info.event.title)}</span>` +
            `   <span class="day">${htmlEncode(readableDate)}</span>` +
            '</div>' +
            '<div class="controls">' +
            (editable ? ' <i class="edit fa-solid fa-pen"></i>' : '') +
            (editable ? ' <i class="delete fa-solid fa-trash"></i>' : '') +
            ' <i class="close fa-solid fa-xmark"></i>' +
            '</div>';
        this.popupElement.appendChild(contentElement);
        
        if(editable) {
            contentElement.querySelector('.edit').addEventListener('click', () => {
                this.editEvent({
                    Uid: info.event.id,
                    Name: info.event.title,
                    StartUtc: info.event.start,
                    EndUtc: info.event.end
                })
            });
            contentElement.querySelector('.delete').addEventListener('click', async () => {
                let result = await modalConfirm('Delete Event', `Are you sure you want to delete the event '${info.event.title}'?`);
                if(result)
                    await this.deleteEvent(info.event.id);
            });
        }

        contentElement.querySelector('.close').addEventListener('click', () => {
            this.closeEvent();
        });
        // Calculate the position of the popup element
        const hoverRect = hoverElement.getBoundingClientRect();
        const popupHeight = parseInt(this.popupElement.style.height);
        let top = info.jsEvent.y - 10;
        const popupTop = top - (popupHeight / 2);
        const popupLeft = hoverRect.right;
        this.popupElement.style.top = popupTop + 'px';
        this.popupElement.style.left = popupLeft + 'px';

        // Add the popup element to the body
        document.body.appendChild(this.popupElement);
    }
    
    formatDateRange(startDate, endDate) {
        const options = { year: 'numeric', month: 'long', day: 'numeric', hour: 'numeric', minute: 'numeric' };
        const startString = startDate.toLocaleDateString(navigator.language, options);

        let endString;
        if (endDate?.toDateString() === startDate?.toDateString())
            endString = endDate.toLocaleTimeString(navigator.language, { hour: 'numeric', minute: 'numeric' });
        else
            endString = endDate?.toLocaleDateString(navigator.language, options) || '';        
        return `${startString} - ${endString}`;
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
        this.closeEvent();
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
        await this.saveEvent(result);
    }
    
    async saveEvent(event) {
        let response = await fetch('/calendar', {
            method:'post',
            body: JSON.stringify(event),
            headers: {
                'Content-Type': 'application/json'
            }
        });
        this.refresh();
    }
    
    refresh(){
        this.closeEvent();
        this.calendar.refetchEvents();
    }
    
    async deleteEvent(uid) {
        let response = await fetch('/calendar/' + uid, {
            method:'delete'
        });
        if(response.status === 200)
            Toast.success(`Event deleted`);
        this.refresh();
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
        if(!date)
            return '';
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