var ezgomediaoptimizer = {
    selectors: {
        TemplateImage: "#TemplateImage",
        StepImages: "[id^=StepImage-]",
        InstructionImages: "[id^=InstructionImage-]",

        TemplateBubble: "#TemplateBubble",
        StepBubbles: "[id^=StepBubble-]",
        InstructionBubbles: "[id^=InstructionBubble-]",

        MediaOptimizeHeader: "#MediaOptimizeHeader",
    },

    language: {
        HeaderTitle: "Template optimizations available",
        HeaderDescription: "We are here to help... We have detected media files that are not yet optimized. You can do them one by one yourself or we can help you do this all at once by clicking the button to the right.",
        HeaderOptimizeAll: "Optimize all images",
        HeaderNotNow: "Not now, maybe later...",
        HeaderSaveMBs: "Save MBs",

        ModalTitle: "Media files to be optimised",
        ModalHeader: "Important",
        ModalTemplateImage: "Template image",
        ModalTemplateItemImage: "Template item image",
        ModalInstructionItemImage: "Instruction item image",
        ModalOptimizeButton: "Optimize now",

        BubbleHeader: "Important",
        BubbleDescription: "This process will replace the original image used in this template with a new compressed image. If you want to keep the original, please use the download button to download this image. This process cannot be reversed.",
        BubbleDownloadButtonAlt: "Download image",
        BubbleOptimizeButton: "Optimize now"
    },

    //single image compress functionality 
    //e is a button in a popover
    compressImage: async function (e) {
        //find bubble from popover Optimize now button
        var imageBubbleId = $(`[aria-describedby='${$(e).closest("div.popover").attr("id")}']`).attr("id");

        var pictureElement = $(ezgomediaoptimizer.getImageSelectorFromBubbleId(imageBubbleId));
        var picture = pictureElement.get(0);

        const response = await fetch(picture.src, { 'mode': 'cors' });

        var contentType = response.headers.get('Content-Type');

        if (response.status == 200) {
            var imageData = await response.arrayBuffer();
            if (imageData.byteLength / 1024 / 1024 > ezgolist.maxImageSizeMB || picture.naturalWidth > ezgolist.maxImageWidthOrHeight || picture.naturalHeight > ezgolist.maxImageWidthOrHeight) {
                var originalFile = new File([imageData], picture.src.split('/').pop(), { type: contentType });

                const options = {
                    maxWidthOrHeight: ezgolist.maxImageWidthOrHeight,
                    useWebWorker: true,
                    maxIteration: 1
                };

                //First only resize image
                var compressedFile = await imageCompression(originalFile, options);
                //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);

                //Then compress image if necessary
                while (compressedFile.size / 1024 / 1024 > ezgolist.maxImageSizeMB) {
                    options.maxSizeMB = ezgolist.maxImageSizeMB;
                    compressedFile = await imageCompression(compressedFile, options);
                    //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);
                }

                var url = URL.createObjectURL(compressedFile);
                pictureElement.attr('src', url);
                ezgomediaoptimizer.updateEzgolistTmpl(pictureElement.attr('id'), url, compressedFile.name.split('.').pop());
                ezgolist._hasChanged = true;
            }
        }

        $('.popover').popover('hide');
        $(`#${imageBubbleId}`).hide();
    },

    //update anchor href and download attributes where e is an anchor element in the popover
    updateDownloadLink: async function (e) {
        //find bubble from popover Download button
        var imageBubbleId = $(`[aria-describedby='${$(e).closest("div.popover").attr("id")}']`).attr("id");

        var pictureElement = $(ezgomediaoptimizer.getImageSelectorFromBubbleId(imageBubbleId));

        var picture = pictureElement.get(0);

        var imageFile = await fetch('/download/image', {
            method: 'POST',
            body: JSON.stringify(picture.src),
            headers: {
                'Accept': 'application/json; charset=utf-8',
                'Content-Type': 'application/json; charset=UTF-8'
            }
        });

        imageFile.blob().then(blob => {
            const url = window.URL.createObjectURL(blob);
            e.setAttribute("href", url);
            e.setAttribute("download", picture.src.split('/').pop());
            e.removeAttribute("onclick");
            e.click();
        });
    },

    updateDownloadLinkWithUrl: async function (e, externalUrl) {
        var imageFile = await fetch('/download/image', {
            method: 'POST',
            body: JSON.stringify(externalUrl),
            headers: {
                'Accept': 'application/json; charset=utf-8',
                'Content-Type': 'application/json; charset=UTF-8'
            }
        });

        imageFile.blob().then(blob => {
            const url = window.URL.createObjectURL(blob);
            e.setAttribute("href", url);
            e.setAttribute("download", externalUrl.split('/').pop());
            e.removeAttribute("onclick");
            e.click();
        });
    },

    //show warnings for audit and checklist detail pages
    showWarnings: async function () {
        if (this.storageAvailable('localStorage')) {
            if (!localStorage.getItem('Existing image compression')) {
                localStorage.setItem('Existing image compression', 'enabled');
            }
            else if (localStorage.getItem('Existing image compression') !== 'enabled') {
                this.hideWarnings();
                return;
            }
        }

        let showHeader = false;
        //check image filesizes and show appropriate warnings

        //try enable image template warning
        showHeader |= await this.tryEnableWarning(ezgomediaoptimizer.selectors.TemplateImage, ezgomediaoptimizer.selectors.TemplateBubble);

        //try enable steps warnings
        var stepImageSelectors = [];
        $(ezgomediaoptimizer.selectors.StepImages).each(function () { stepImageSelectors.push(`#${this.id}`); });

        var stepBubbleSelectors = [];
        $(ezgomediaoptimizer.selectors.StepBubbles).each(function () { stepBubbleSelectors.push(`#${this.id}`); });

        if (stepImageSelectors.length == stepBubbleSelectors.length) {
            for (var i = 0; i < stepImageSelectors.length; i++) {
                showHeader |= await this.tryEnableWarning(stepImageSelectors[i], stepBubbleSelectors[i]);
            }
        }

        if (ezgolist.tmpl.TaskTemplates !== undefined) {
            for (var i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
                if (ezgolist.tmpl.TaskTemplates[i].Steps !== undefined) {
                    for (var j = 0; j < ezgolist.tmpl.TaskTemplates[i].Steps.length; j++) {
                        var pictureUrl = ezgolist.tmpl.TaskTemplates[i].Steps[j].Picture;
                        if (ezgolist.tmpl.TaskTemplates[i].Steps[j].Picture !== undefined && !ezgolist.tmpl.TaskTemplates[i].Steps[j].Picture.toString().startsWith('data:') && !ezgolist.tmpl.TaskTemplates[i].Steps[j].Picture.toString().startsWith('blob:')) {
                            pictureUrl = ezgolist.mediaUrl + ezgolist.tmpl.TaskTemplates[i].Steps[j].Picture;
                            showHeader |= await this.imageNeedsResizingOrCompressionByUrl(pictureUrl);
                        }
                    }
                }
            }
        }

        if (ezgolist.tmpl.Type == "task") {
            for (var j = 0; j < ezgolist.tmpl.Steps.length; j++) {
                var pictureUrl = ezgolist.tmpl.Steps[j].Picture;
                if (ezgolist.tmpl.Steps[j].Picture !== undefined && !ezgolist.tmpl.Steps[j].Picture.toString().startsWith('data:') && !ezgolist.tmpl.Steps[j].Picture.toString().startsWith('blob:')) {
                    pictureUrl = ezgolist.mediaUrl + ezgolist.tmpl.Steps[j].Picture;
                    showHeader |= await this.imageNeedsResizingOrCompressionByUrl(pictureUrl);
                }
            }
        }

        //hide header based on showHeader bool
        if (showHeader) {
            $(ezgomediaoptimizer.selectors.MediaOptimizeHeader).show();
        }
        else {
            $(ezgomediaoptimizer.selectors.MediaOptimizeHeader).hide();
        }
        ezgomediaoptimizer.reInitializePopovers();
    },

    //show warnings for tasktemplate steps dialog
    showInstructionWarnings: async function () {
        if (this.storageAvailable('localStorage')) {
            if (!localStorage.getItem('Existing image compression')) {
                localStorage.setItem('Existing image compression', 'enabled');
            }
            else if (localStorage.getItem('Existing image compression') !== 'enabled') {
                this.hideWarnings();
                return;
            }
        }
        var showHeader = false;
        //try enable step instructions warnings
        var instructionImageSelectors = [];
        $(ezgomediaoptimizer.selectors.InstructionImages).each(function () { instructionImageSelectors.push(`#${this.id}`); });

        var instructionBubbleSelectors = [];
        $(ezgomediaoptimizer.selectors.InstructionBubbles).each(function () { instructionBubbleSelectors.push(`#${this.id}`); });

        if (instructionImageSelectors.length == instructionBubbleSelectors.length) {
            for (var i = 0; i < instructionImageSelectors.length; i++) {
                showHeader |= await this.tryEnableWarning(instructionImageSelectors[i], instructionBubbleSelectors[i]);
            }
        }
        return showHeader;
    },

    //try enable warning based on imageselector and warningselector
    tryEnableWarning: async function (imageSelector, warningSelector) {
        //get image filesize
        if (this.isEzgolistTmplPicture(imageSelector) && await this.imageNeedsResizingOrCompression(imageSelector)) {
            //if image filesize or dimensions too large, enable warning bubble
            $(warningSelector).removeProp('hidden');
            $(warningSelector).show();
            return true;
        }
        else {
            $(warningSelector).prop('hidden', 'hidden');
            $(warningSelector).hide();
        }
        return false;
    },

    //optimize images of entire checklist or audit
    //edits ezgolist.tmpl
    optimizeTemplate: async function () {
        //try optimize template image
        var requestUrl = ezgolist.tmpl.Picture;

        if (ezgolist.tmpl.Picture !== undefined && !ezgolist.tmpl.Picture.toString().startsWith('data:') && !ezgolist.tmpl.Picture.toString().startsWith('blob:')) {
            requestUrl = ezgolist.mediaUrl + ezgolist.tmpl.Picture;

            const response = await fetch(requestUrl, { 'mode': 'cors' });

            var contentType = response.headers.get('Content-Type');

            if (response.status == 200) {
                var imageData = await response.arrayBuffer();
                if (imageData.byteLength / 1024 / 1024 > ezgolist.maxImageSizeMB || this.getExternalImageWidthOrHeight(requestUrl).naturalWidth > ezgolist.maxImageWidthOrHeight) {
                    var originalFile = new File([imageData], ezgolist.tmpl.Picture.split('/').pop(), { type: contentType });

                    const options = {
                        maxWidthOrHeight: ezgolist.maxImageWidthOrHeight,
                        useWebWorker: true,
                        maxIteration: 1
                    };

                    //First only resize image
                    var compressedFile = await imageCompression(originalFile, options);
                    //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);

                    //Then compress image if necessary
                    while (compressedFile.size / 1024 / 1024 > ezgolist.maxImageSizeMB) {
                        options.maxSizeMB = ezgolist.maxImageSizeMB;
                        compressedFile = await imageCompression(compressedFile, options);
                        //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);
                    }
                    var url = URL.createObjectURL(compressedFile);
                    ezgolist.tmpl.Picture = url;
                    ezgolist.tmpl.PictureType = compressedFile.name.split('.').pop();
                }
            }
        }
        //try optimize tasktemplates
        for (var i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
            ezgolist.tmpl.TaskTemplates[i] = await this.optimizeTaskTemplate(ezgolist.tmpl.TaskTemplates[i], false);
        }
        ezgolist._hasChanged = true;
        this.hideWarnings();
        $("#mediaOptimizeHeader").modal('hide');
    },

    //optimize images of a tasktemplate
    //edits input variable (can be ezgolist.tmpl or ezgolist.tmpl.TaskTemplates[i], same object)
    optimizeTaskTemplate: async function (taskTemplate, updateUI) {
        //try optimize tasktemplates
        var requestUrl = taskTemplate.Picture;

        if (taskTemplate.Picture !== undefined && !taskTemplate.Picture.toString().startsWith('data:') && !taskTemplate.Picture.toString().startsWith('blob:')) {
            requestUrl = ezgolist.mediaUrl + taskTemplate.Picture;

            const response = await fetch(requestUrl, { 'mode': 'cors' });

            var contentType = response.headers.get('Content-Type');

            if (response.status == 200) {
                var imageData = await response.arrayBuffer();
                if (imageData.byteLength / 1024 / 1024 > ezgolist.maxImageSizeMB || this.getExternalImageWidthOrHeight(requestUrl).naturalWidth > ezgolist.maxImageWidthOrHeight) {
                    var originalFile = new File([imageData], taskTemplate.Picture.split('/').pop(), { type: contentType });

                    const options = {
                        maxWidthOrHeight: ezgolist.maxImageWidthOrHeight,
                        useWebWorker: true,
                        maxIteration: 1
                    };

                    //First only resize image
                    var compressedFile = await imageCompression(originalFile, options);
                    //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);

                    //Then compress image if necessary
                    while (compressedFile.size / 1024 / 1024 > ezgolist.maxImageSizeMB) {
                        options.maxSizeMB = ezgolist.maxImageSizeMB;
                        compressedFile = await imageCompression(compressedFile, options);
                        //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);
                    }
                    var url = URL.createObjectURL(compressedFile);
                    taskTemplate.Picture = url;
                    taskTemplate.PictureType = compressedFile.name.split('.').pop();
                }

            }
        }

        //try optimize tasktemplates' steps
        if (taskTemplate.Steps != null) {
            for (var j = 0; j < taskTemplate.Steps.length; j++) {
                var requestUrl = taskTemplate.Steps[j].Picture;

                if (taskTemplate.Steps[j].Picture !== undefined && !taskTemplate.Steps[j].Picture.toString().startsWith('data:') && !taskTemplate.Steps[j].Picture.toString().startsWith('blob:')) {
                    requestUrl = ezgolist.mediaUrl + taskTemplate.Steps[j].Picture;

                    const response = await fetch(requestUrl, { 'mode': 'cors' });

                    var contentType = response.headers.get('Content-Type');

                    if (response.status == 200) {
                        var imageData = await response.arrayBuffer();
                        if (imageData.byteLength / 1024 / 1024 > ezgolist.maxImageSizeMB || this.getExternalImageWidthOrHeight(requestUrl).naturalWidth > ezgolist.maxImageWidthOrHeight) {
                            var originalFile = new File([imageData], taskTemplate.Steps[j].Picture.split('/').pop(), { type: contentType });

                            const options = {
                                maxWidthOrHeight: ezgolist.maxImageWidthOrHeight,
                                useWebWorker: true,
                                maxIteration: 1
                            };

                            //First only resize image
                            var compressedFile = await imageCompression(originalFile, options);
                            //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);

                            //Then compress image if necessary
                            while (compressedFile.size / 1024 / 1024 > ezgolist.maxImageSizeMB) {
                                options.maxSizeMB = ezgolist.maxImageSizeMB;
                                compressedFile = await imageCompression(compressedFile, options);
                                //console.log(`Successfully compressed file to ${compressedFile.size / 1024} KB.`);
                            }
                            var url = URL.createObjectURL(compressedFile);
                            taskTemplate.Steps[j].Picture = url;
                            taskTemplate.Steps[j].PictureType = compressedFile.name.split('.').pop();
                        }

                    }
                }
            }
        }
        if (updateUI) {
            ezgolist._hasChanged = true;
            this.hideWarnings();
            $("#mediaOptimizeHeader").modal('hide');
        }
        return taskTemplate;
    },

    //method for convenience purposes only
    getImageSelectorFromBubbleId: function (bubbleId) {
        if (bubbleId.includes("TemplateBubble")) {
            return ezgomediaoptimizer.selectors.TemplateImage;
        }
        else if (bubbleId.includes("StepBubble-")) {
            return `#StepImage-${bubbleId.split("-").pop()}`;
        }
        else if (bubbleId.includes("InstructionBubble-")) {
            return `#InstructionImage-${bubbleId.split("-").pop()}`;
        }
    },

    //force hide all media optimization warnings
    hideWarnings: function () {
        $(ezgomediaoptimizer.selectors.TemplateBubble).hide();
        $(ezgomediaoptimizer.selectors.StepBubbles).hide();
        $(ezgomediaoptimizer.selectors.InstructionBubbles).hide();
        $(ezgomediaoptimizer.selectors.MediaOptimizeHeader).hide();
    },

    //check if an image needs resizing/compression based on its jQuery selector
    imageNeedsResizingOrCompression: async function (imageSelector) {
        var queryResult = $(imageSelector);
        if (queryResult.length <= 0) {
            return false;
        }

        var picture = queryResult.get(0);
        var response = {}
        try {
            response = await fetch(picture.src, { 'mode': 'cors' });
        }
        catch (error) {
            //console.log(error);
            caches.open('v1').then(function (cache) {
                cache.delete(picture.src).then(function (result) {
                    //console.log(`Picture ${picture.src} cleared from cache`);
                });
            });

            response = await fetch(picture.src, {
                'mode': 'cors',
                headers: {
                    'Cache-Control': 'no-cache'
                }
            });
            window.location.reload(true);
        }
        if (response.status == 200) {
            var imageData = await response.arrayBuffer();
            if (imageData.byteLength / 1024 / 1024 > ezgolist.maxImageSizeMB || picture.naturalWidth > ezgolist.maxImageWidthOrHeight || picture.naturalHeight > ezgolist.maxImageWidthOrHeight) {
                return true;
            }
        }
        return false;
    },

    //check if an image needs resizing/compression based on an url
    imageNeedsResizingOrCompressionByUrl: async function (pictureUrl) {
        var response = {}
        try {
            response = await fetch(pictureUrl, { 'mode': 'cors' });
        }
        catch (error) {
            //console.log(error);
            caches.open('v1').then(function (cache) {
                cache.delete(pictureUrl).then(function (result) {
                    //console.log(`Picture ${pictureUrl} cleared from cache`);
                });
            });

            response = await fetch(pictureUrl, {
                'mode': 'cors',
                headers: {
                    'Cache-Control': 'no-cache'
                }
            });
            window.location.reload(true);
        }
        if (response.status == 200) {
            var imageData = await response.arrayBuffer();
            if (imageData.byteLength / 1024 / 1024 > ezgolist.maxImageSizeMB) {
                return true;
            }
            var imageWidthOrHeight = await this.getExternalImageWidthOrHeight(pictureUrl);
            if (imageWidthOrHeight > ezgolist.maxImageWidthOrHeight) {
                return true;
            }
        }
        return false;
    },

    //get image width or height based on an url
    getExternalImageWidthOrHeight: async function (pictureUrl) {
        const image = await new Promise((resolve, reject) => {
            const img = new Image();
            img.onload = () => resolve(img);
            img.onerror = reject;
            img.src = pictureUrl;
        })

        return image.naturalWidth > image.naturalHeight ? image.naturalWidth : image.naturalHeight;
    },

    //update the ezgolist.tmpl object based on the img selector (with url and ext)
    updateEzgolistTmpl: function (imgSelector, url, ext) {
        if (imgSelector.includes("TemplateImage")) {
            ezgolist.tmpl.Picture = url;
            ezgolist.tmpl.PictureType = ext;
        }
        else if (imgSelector.includes("StepImage-")) {
            if (ezgolist.tmpl.TaskTemplates != null) {
                for (var i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
                    if (ezgolist.tmpl.TaskTemplates[i].Id == imgSelector.split("-").pop()) {
                        ezgolist.tmpl.TaskTemplates[i].Picture = url;
                        ezgolist.tmpl.TaskTemplates[i].PictureType = ext;
                    }
                }
            }
        }
        else if (imgSelector.includes("InstructionImage-")) {
            if (ezgolist.tmpl.Type == "task") {
                var instructionId = imgSelector.split("-").pop();
                if (ezgolist.tmpl.Steps != null) {
                    for (var j = 0; j < ezgolist.tmpl.Steps.length; j++) {
                        if (ezgolist.tmpl.Steps[j] != null && ezgolist.tmpl.Steps[j].Id == instructionId) {
                            ezgolist.tmpl.Steps[j].Picture = url;
                            ezgolist.tmpl.Steps[j].PictureType = ext;
                        }
                    }
                }
            }
            else {
                var instructionId = imgSelector.split("-").pop();
                if (ezgolist.tmpl.TaskTemplates != null) {
                    for (var i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
                        if (ezgolist.tmpl.TaskTemplates[i] != null && ezgolist.tmpl.TaskTemplates[i].Steps != null) {
                            for (var j = 0; j < ezgolist.tmpl.TaskTemplates[i].Steps.length; j++) {
                                if (ezgolist.tmpl.TaskTemplates[i].Steps[j] != null && ezgolist.tmpl.TaskTemplates[i].Steps[j].Id == instructionId) {
                                    ezgolist.tmpl.TaskTemplates[i].Steps[j].Picture = url;
                                    ezgolist.tmpl.TaskTemplates[i].Steps[j].PictureType = ext;
                                }
                            }
                        }
                    }
                }
            }
        }
    },

    isEzgolistTmplPicture: function (imgSelector) {
        if (imgSelector.includes("TemplateImage")) {
            return ezgolist.tmpl.Picture !== undefined;
        }
        else if (imgSelector.includes("StepImage-")) {
            if (ezgolist.tmpl.TaskTemplates != null) {
                for (var i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
                    if (ezgolist.tmpl.TaskTemplates[i].Id == imgSelector.split("-").pop()) {
                        return ezgolist.tmpl.TaskTemplates[i].Picture !== undefined;
                    }
                }
            }
        }
        else if (imgSelector.includes("InstructionImage-")) {
            if (ezgolist.tmpl.Type == "task") {
                var instructionId = imgSelector.split("-").pop();
                if (ezgolist.tmpl.Steps != null) {
                    for (var j = 0; j < ezgolist.tmpl.Steps.length; j++) {
                        if (ezgolist.tmpl.Steps[j] != null && ezgolist.tmpl.Steps[j].Id == instructionId) {
                            return ezgolist.tmpl.Steps[j].Picture !== undefined;
                        }
                    }
                }
            }
            else {
                var instructionId = imgSelector.split("-").pop();
                if (ezgolist.tmpl.TaskTemplates != null) {
                    for (var i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
                        if (ezgolist.tmpl.TaskTemplates[i] != null && ezgolist.tmpl.TaskTemplates[i].Steps != null) {
                            for (var j = 0; j < ezgolist.tmpl.TaskTemplates[i].Steps.length; j++) {
                                if (ezgolist.tmpl.TaskTemplates[i].Steps[j] != null && ezgolist.tmpl.TaskTemplates[i].Steps[j].Id == instructionId) {
                                    return ezgolist.tmpl.TaskTemplates[i].Steps[j].Picture !== undefined;
                                }
                            }
                        }
                    }
                }
            }
        }
    },

    //reinitialize all popovers on the page
    reInitializePopovers: function () {
        $('[data-toggle=popover]').popover('hide');
        $('[data-toggle=popover]').popover('dispose');

        //enable any popovers that arent enabled yet
        $("[data-toggle=popover]").popover({
            html: true,
            sanitize: false,
            content: compressionPopover,
            trigger: 'click'
        });
    },

    storageAvailable: function (type) {
        var storage;
        try {
            storage = window[type];
            var x = '__storage_test__';
            storage.setItem(x, x);
            storage.removeItem(x);
            return true;
        }
        catch (e) {
            return e instanceof DOMException && (
                // everything except Firefox
                e.code === 22 ||
                // Firefox
                e.code === 1014 ||
                // test name field too, because code might not be present
                // everything except Firefox
                e.name === 'QuotaExceededError' ||
                // Firefox
                e.name === 'NS_ERROR_DOM_QUOTA_REACHED') &&
                // acknowledge QuotaExceededError only if there's something already stored
                (storage && storage.length !== 0);
        }
    },

    showHeader: function () {
        $(ezgomediaoptimizer.selectors.MediaOptimizeHeader).show();
    },

    hideHeader: function () {
        $(ezgomediaoptimizer.selectors.MediaOptimizeHeader).hide();
    },
};

//old popover html
//var compressionPopover = '<table><tr><td>' +
//    '<h3><b>Important</b></h3>' +
//    '</td>' +
//    '<td style="vertical-align: top">' +
//    '<a class="border rounded-circle btn btn-secondary btn-sm btn-outline shadow-sm mt-auto" title="Download image" style="float: right; margin: 0; color: white" onclick="return ezgomediaoptimizer.updateDownloadLink(this);" target="_blank">' +
//    '<i class="fas fa-download"></i>' +
//    '</a></td></tr>' +
//    '<tr><td colspan=2>' +
//    '<p style="width: 90%; float: left">This process will replace the original image used in this template with a new compressed image. ' +
//    'If you want to keep the original, please use the download button to download this image. ' +
//    'This process cannot be reversed.</p></td></tr>' +
//    '<tr><td colspan=2 style="text-align: center">' +
//    '<button type="button" title="Optimize now" class="btn btn-ezgo btn-sm btn-outline mr-2 shadow-sm mt-auto" style="background-color: #E07200 !important; color: black" onclick="ezgomediaoptimizer.compressImage(this)">Optimize now</button>' +
//    '</td></tr></table>';


//new popover html
var compressionPopover = `<table><tr><td>
    <h3><b>${ezgomediaoptimizer.language.BubbleHeader}</b></h3>
    </td>
    <td style="vertical-align: top">
    <a class="border rounded-circle btn btn-secondary btn-sm btn-outline shadow-sm mt-auto" title="${ezgomediaoptimizer.language.BubbleDownloadButtonAlt}" style="float: right; margin: 0; color: white" onclick="return ezgomediaoptimizer.updateDownloadLink(this);" target="_blank">
    <i class="fas fa-download"></i>
    </a></td></tr>
    <tr><td colspan=2>
    <p style="width: 90%; float: left">${ezgomediaoptimizer.language.BubbleDescription}</p></td></tr>
    <tr><td colspan=2 style="text-align: center">
    <button type="button" title="${ezgomediaoptimizer.language.BubbleOptimizeButton}" class="btn btn-ezgo btn-sm btn-outline mr-2 shadow-sm mt-auto" onclick="ezgomediaoptimizer.compressImage(this)">${ezgomediaoptimizer.language.BubbleOptimizeButton}</button>
    </td></tr></table>`;
