var contextMenus = {};
const copyIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"></rect><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"></path></svg>`;
const cutIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><circle cx="6" cy="6" r="3"></circle><circle cx="6" cy="18" r="3"></circle><line x1="20" y1="4" x2="8.12" y2="15.88"></line><line x1="14.47" y1="14.48" x2="20" y2="20"></line><line x1="8.12" y1="8.12" x2="12" y2="12"></line></svg>`;
const pasteIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px; position: relative; top: -1px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2"></path><rect x="8" y="2" width="8" height="4" rx="1" ry="1"></rect></svg>`;
const downloadIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" style="margin-right: 7px; position: relative; top: -1px" fill="none" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><polyline points="7 10 12 15 17 10"></polyline><line x1="12" y1="15" x2="12" y2="3"></line></svg>`;
const deleteIcon = `<svg viewBox="0 0 24 24" width="13" height="13" stroke="currentColor" stroke-width="2.5" fill="none" style="margin-right: 7px" stroke-linecap="round" stroke-linejoin="round" class="css-i6dzq1"><polyline points="3 6 5 6 21 6"></polyline><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>`;
const infoIcon = `<span class="icon icon-info-circle" style="padding-right:0.5rem"></span>`;
const addIcon = `<span class="icon icon-plus" style="padding-right:0.5rem"></span>`;
const editIcon = `<span class="icon icon-edit" style="padding-right:0.5rem"></span>`;
const dashboardIcon = `<span class="icon icon-home" style="padding-right:0.5rem"></span>`;
const terminalIcon = `<span class="icon icon-globe" style="padding-right:0.5rem"></span>`;

function openDefaultContextMenu(event) {
    event?.preventDefault();
    event?.stopPropagation();
    if(!contextMenus['DEFAULT'])
    {
        let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');

        const menuItems = [
            {
                content: `${dashboardIcon}Edit Dashboard`,
                events: {
                    click: (e) => {
                        document.location = '/settings/dashboards/' + dashboardUid                
                    }
                }
            },
            {
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal()
                }
            }

            ];
            
            let menu = new ContextMenu({
                menuItems
            });
              
            menu.init();
            contextMenus['DEFAULT'] = menu;
        }
    
        contextMenus['DEFAULT'].open(event);
}

function openContextMenu(event, app){
    event?.preventDefault();
    event?.stopPropagation();
    if(typeof(app) === 'string')
        app = JSON.parse(app);

        
    let uid = app.Uid;
    let ele = document.getElementById(uid);
    let groupUid = ele.closest('.db-group').getAttribute('id');
    let dashboardUid = ele.closest('.dashboard').getAttribute('x-uid');
    let ssh = ele.getAttribute('x-ssh') === '1';
    let docker = ele.getAttribute('x-docker');
    if(!contextMenus[uid])
    {
        let menuItems = [];
        if(app._Type !== 'DashboardTerminal'){
            menuItems.push(
            {
                content: `${infoIcon}Up-Time`,
                events: {
                    click: (e) => openUpTime(app)                
                }
            });
        }

        menuItems = menuItems.concat([
        {
          content: `${deleteIcon}Delete`
        },
        {
            content: `${editIcon}Edit Group`,
            divider: "top", // top, bottom, top-bottom
            events: {
                click: (e) => {
                    document.location = '/settings/groups/' + groupUid                
                }
            }
        },
        {
            content: `${dashboardIcon}Edit Dashboard`,
            events: {
                click: (e) => {
                    document.location = '/settings/dashboards/' + dashboardUid                
                }
            }
        }
        ]);
        if(ssh){
            menuItems.push(
            {
                divider: "top",
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal(1, uid)
                }
            });
        }
        else if(docker){
            menuItems.push(
            {
                divider: "top",
                content: `${terminalIcon}Terminal`,
                events: {
                    click: (e) => openTerminal(2, uid)
                }
            });
        }
        
        let menu = new ContextMenu({
            menuItems
        });
          
        menu.init();
        contextMenus[uid] = menu;
    }

    contextMenus[uid].open(event);
}
