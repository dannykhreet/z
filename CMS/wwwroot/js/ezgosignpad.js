class SignPad {
    constructor(canvas) {
        this.canvas = canvas
        this.context = canvas.getContext("2d")
    }
    isDrawing = false
    startX = 0
    startY = 0
    points = []
    blank = {}

    addEventListeners() {
        this.canvas.addEventListener("mousedown", this.mousedown);
        this.canvas.addEventListener("mousemove", this.mousemove);
        this.canvas.addEventListener("mouseup", this.mouseup);
        this.canvas.addEventListener("touchstart", this.touchstart);
        this.canvas.addEventListener("touchmove", this.touchmove);
        this.canvas.addEventListener("touchend", this.touchend);
    }

    initContext() {
        this.context.fillStyle = "#fff";
        this.context.fillRect(0, 0, this.canvas.width, this.canvas.height);

        this.initBlank();
    }

    initBlank() {
        this.blank = document.createElement('canvas');

        this.blank.width = this.canvas.width;
        this.blank.height = this.canvas.height;

        this.blank.getContext("2d").fillStyle = "#fff";
        this.blank.getContext("2d").fillRect(0, 0, this.blank.width, this.blank.height);
    }

    isCanvasEmpty() {
        return this.canvas.toDataURL() === this.blank.toDataURL();
    }

    touchstart = (e) => {
        e.preventDefault();

        const rect = this.canvas.getBoundingClientRect();
        const x = e.touches[0].clientX - rect.left;
        const y = e.touches[0].clientY - rect.top;
        this.isDrawing = true;
        this.startX = x;
        this.startY = y;
        this.points.push({
            x: x,
            y: y,
        });
    }

    touchmove = (e) => {
        e.preventDefault();

        const rect = this.canvas.getBoundingClientRect();
        const x = e.touches[0].clientX - rect.left;
        const y = e.touches[0].clientY - rect.top;
        if (this.isDrawing) {
            this.context.beginPath();
            this.context.moveTo(this.startX, this.startY);
            this.context.lineTo(x, y);
            this.context.lineWidth = 2;
            this.context.lineCap = "round";
            this.context.strokeStyle = "#9aa8b3";
            this.context.stroke();

            this.startX = x;
            this.startY = y;

            this.points.push({
                x: x,
                y: y,
            });
        }
    }

    touchend = (e) => {
        e.preventDefault();
        this.isDrawing = false;
    }

    mousedown = (e) => {
        var rect = this.canvas.getBoundingClientRect();
        var x = e.clientX - rect.left;
        var y = e.clientY - rect.top;

        this.isDrawing = true;
        this.startX = x;
        this.startY = y;
        this.points.push({
            x: x,
            y: y,
        });
    }

    mousemove = (e) => {
        var rect = this.canvas.getBoundingClientRect();
        var x = e.clientX - rect.left;
        var y = e.clientY - rect.top;

        if (this.isDrawing) {
            this.context.beginPath();
            this.context.moveTo(this.startX, this.startY);
            this.context.lineTo(x, y);
            this.context.lineWidth = 2;
            this.context.lineCap = "round";
            this.context.strokeStyle = "#9aa8b3";
            this.context.stroke();

            this.startX = x;
            this.startY = y;

            this.points.push({
                x: x,
                y: y,
            });
        }
    }
    mouseup = (e) => {
        this.isDrawing = false;
    }
    resetCanvas = () => {
        this.canvas.width = this.canvas.width;
        this.points.length = 0;
    }

}