var contextMenus = {};
const deleteIcon = `<span class="icon fa-solid fa-trash" style="padding-right:0.5rem"></span>`;
const infoIcon = `<span class="icon fa-solid fa-circle-info" style="padding-right:0.5rem"></span>`;
const addIcon = `<span class="icon fa-solid fa-plus" style="padding-right:0.5rem"></span>`;
const editIcon = `<span class="icon fa-solid fa-pen-to-square" style="padding-right:0.5rem"></span>`;
const resizeIcon = `<span class="icon fa-solid fa-maximize" style="padding-right:0.5rem"></span>`;
const dashboardIcon = `<span class="icon fa-solid fa-house" style="padding-right:0.5rem"></span>`;
const terminalIcon = `<span class="icon fa-solid fa-terminal" style="padding-right:0.5rem"></span>`;
const logIcon = `<span class="icon fa-solid fa-file-lines" style="padding-right:0.5rem"></span>`;
const refreshIcon = `<span class="icon fa-solid fa-rotate-right" style="padding-right:0.5rem"></span>`;

function openDefaultContextMenu(event) {
    event?.preventDefault();
    event?.stopPropagation();
    if(!contextMenus['DEFAULT'])
    {
        let dashboardUid = document.querySelector('.dashboard').getAttribute('x-uid');
        const menuItems = [
            {
                content: `${dashboardIcon} ${Translations.EditDashboard}`,
                events: {
                    click: (e) => {
                        document.location = '/settings/dashboards/' + dashboardUid                
                    }
                }
            },
            {
                content: `${terminalIcon} ${Translations.Terminal}`,
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
    
    console.log('contextMenus[\'DEFAULT\']', contextMenus['DEFAULT']);
    
    contextMenus['DEFAULT'].open(event);
}

function openContextMenu(event, app){
    event?.preventDefault();
    event?.stopPropagation();
    if(typeof(app) === 'string')
        app = JSON.parse(app);
        
    let uid = app.Uid;
    let ele = document.getElementById(uid);
    let group = ele.closest('.db-group');
    let groupUid = group.getAttribute('id');
    let dashboardUid = ele.closest('.dashboard').getAttribute('x-uid');
    let ssh = ele.getAttribute('x-ssh') === '1';
    let monitor = ele.getAttribute('x-monitor') === '1';
    let docker = ele.getAttribute('x-docker');
    let systemGroup = group.className.indexOf('system-group') > 0;
    let smart = ele.getAttribute('class').indexOf('db-smart');
    if(!contextMenus[uid])
    {
        let menuItems = [];
        if(monitor)
        {
            menuItems.push(
            {
                content: `${infoIcon}${Translations.UpTime}`,
                events: {
                    click: (e) => openUpTime(app)                
                }
            });
        }
        if(ssh){
            menuItems.push(
            {
                divider: "top",
                content: `${terminalIcon}${Translations.Terminal}`,
                events: {
                    click: (e) => openTerminal(1, uid)
                }
            });
        }
        else if(docker){
            menuItems.push(
            {
                content: `${terminalIcon}${Translations.Terminal}`,
                events: {
                    click: (e) => openTerminal(2, uid)
                }
            });
            menuItems.push(
            {
                content: `${logIcon}${Translations.Log}`,
                events: {
                    click: (e) => openDockerLog(uid)
                }
            });
        }

        if(!systemGroup)
        {
            let sizes = [Translations.Size_Small, Translations.Size_Medium, Translations.Size_Large, Translations.Size_Larger,
                Translations.Size_XLarge, Translations.Size_XXLarge];
            menuItems = menuItems.concat([
            {
                content: `${resizeIcon}${Translations.Resize}`,
                divider: "top",
                submenu: sizes.map((x) =>
                {
                    return { 
                        content: x,
                        events: {
                            click: (e) => {
                                let enumSizes = ['small', 'medium', 'large', 'larger', 'x-Large', 'xx-large'];
                                for(let s of enumSizes){
                                    ele.classList.remove(s.toLowerCase());
                                }
                                let index = sizes.indexOf(x);
                                let size = enumSizes[index].replace('-', '');

                                ele.classList.add(x.toLowerCase());
                                document.dispatchEvent(new CustomEvent('fenrus-item-resized', {
                                    detail: { element: ele }
                                }));

                                fetch(`/settings/groups/${groupUid}/resize/${uid}/${size}`, { method: 'POST'});
                            }
                        }
                    };
                })
            }]);
        }
        
        if(smart)
        {
            menuItems.push(
            {
                content: `${refreshIcon}${Translations.Refresh}`,
                events: {
                    click: (e) => {
                        let smartApp = SmartAppInstances[uid];
                        if(smartApp)
                            smartApp.refresh();
                    }
                }
            });
        }

        menuItems = menuItems.concat([
        {
            content: `${editIcon}${Translations.EditGroup}`,
            divider: "top",
            events: {
                click: (e) => {
                    document.location = systemGroup ? '/settings/system/groups/' + groupUid : '/settings/groups/' + groupUid                
                }
            }
        },
        {
            content: `${dashboardIcon}${Translations.EditDashboard}`,
            events: {
                click: (e) => {
                    document.location = '/settings/dashboards/' + dashboardUid                
                }
            }
        }]);


        if(!systemGroup) {
            menuItems = menuItems.concat([
                {
                    divider: "top",
                    content: `${deleteIcon}${Translations.Delete}`,
                    events: {
                        click: async (e) => {
                            if (await modalConfirm(Translations.Delete, `${Translations.Delete} ${app.Name}?`)) {
                                ele.remove();
                                let items = group.querySelectorAll('.items .db-item');
                                if (items.length === 0) {
                                    group.remove();
                                } else {
                                    document.dispatchEvent(new CustomEvent('fenrus-item-deleted', {
                                        detail: {group: group}
                                    }));
                                }
                                fetch(`/settings/groups/${groupUid}/delete/${uid}`, {method: 'DELETE'});
                            }
                        }
                    }
                },
            ]);
        }
        
        let menu = new ContextMenu({
            menuItems
        });
          
        menu.init();
        contextMenus[uid] = menu;
    }

    contextMenus[uid].open(event);
}
