class Toast {
    constructor(type, message, timeout = 5000) {
        type = (type || 'success').toLowerCase();
        this.type = type;
        this.message = message;
        this.timeout = timeout;
        this.toast = document.createElement('div');
        this.toast.classList.add('toast');
        this.toast.classList.add(type);
        this.toast.innerHTML = `<i class="fa ${this.getIconClass()}"></i><div class="message">${message}</div><span class="close"><i class="fa fa-times"></i></span>`;
        document.body.appendChild(this.toast);
        setTimeout(() => {
            this.close();
        }, this.timeout);
        this.slideToTop();
        this.toast.querySelector('.close').addEventListener('click', () => {
            this.close();
        });
    }

    close() {
        this.toast.classList.add('fade-out');
        setTimeout(() => {
            this.toast.remove();
            Toast.slideUp();
        }, 500);
    }

    slideToTop() {
        let currentTop = 0;
        document.querySelectorAll('.toast').forEach(toast => {
            const height = toast.offsetHeight;
            toast.style.top = currentTop + 'px';
            currentTop += height + 10;
        });
    }

    getIconClass() {
        switch (this.type) {
            case 'success':
                return 'fa-check-circle';
            case 'warning':
                return 'fa-exclamation-triangle';
            case 'info':
                return 'fa-info-circle';
            case 'error':
                return 'fa-times-circle';
            default:
                return '';
        }
    }

    static slideUp() {
        let currentTop = 10;
        document.querySelectorAll('.toast').forEach(toast => {
            toast.style.top = currentTop + 'px';
            currentTop += toast.offsetHeight + 10;
        });
    }

    static success(message, timeout) {
        new Toast('success', message, timeout);
    }

    static warning(message, timeout) {
        new Toast('warning', message, timeout);
    }

    static info(message, timeout) {
        new Toast('info', message, timeout);
    }

    static error(message, timeout) {
        new Toast('error', message, timeout);
    }
}
