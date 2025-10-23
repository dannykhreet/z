function addMinutes(date, minutes) { const dateCopy = new Date(date); dateCopy.setMinutes(date.getMinutes() + minutes); return dateCopy; }

function isScrolledIntoView(elem)
{
    var docViewTop = $(window).scrollTop();
    var docViewBottom = docViewTop + $(window).height();

    var elemTop = $(elem).offset().top;
    var elemBottom = elemTop + $(elem).height();


    return ((elemBottom <= (docViewBottom + 500)) && (elemBottom >= docViewTop)) || ((elemTop <= (docViewBottom + 500)) && (elemTop >= docViewTop));
}

var ezgomediafetcher = {
    usingPathStyleURLs: false,
    apiConnectionBroken: false,
    objectURLs: {},
    s3: null,
    initializing: false,
    expirationDate: addMinutes(new Date(), 1),
    tryInit: function() {
        if (window.jQuery) {
            ezgomediafetcher.init();
        } else {
            setTimeout(function() { ezgomediafetcher.tryInit() }, 50);
        }
    },
    init: function () {
        if (ezgomediafetcher.initializing) {
            return;
        }
        ezgomediafetcher.initializing = true;
        //console.log("Initializing ezgomediafetcher...");
        //without jquery (xhr)
        $.ajax({
            type: "GET",
            url: '/fetchmediatoken',
            success: function (data) {
                //data.Expiration 
                var parsedData = JSON.parse(data);
                
                var creds = new AWS.Credentials({
                    accessKeyId: parsedData.AccessKeyId,
                    secretAccessKey: parsedData.SecretAccessKey,
                    sessionToken: parsedData.SessionToken
                });

                AWS.config.update({
                    region: "eu-central-1",
                    credentials: creds
                });

                ezgomediafetcher.s3 = new AWS.S3({region: "eu-central-1"});
                ezgomediafetcher.expirationDate = new Date(parsedData.Expiration);
                
                //console.log("Done initializing ezgomediafetcher!");
                ezgomediafetcher.initializing = false;
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR);
                console.log(jqXHR.status);
                //if fetching media token went wrong, prevent entering a loop where getAndSetImage keeps calling tryInit and itself after
                ezgomediafetcher.apiConnectionBroken = true;
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                ezgomediafetcher.initializing = false;
            },
            contentType: "application/json; charset=utf-8"
        });
        
    },
    setVisibilityAttributes: function () {
        $('img, video').each(function (index, item) {
            if (isScrolledIntoView(item) && $(item).is(":visible")) {
                $(item).attr('is-visible', true);
            }
            else {
                $(item).attr('is-visible', false);
            }
        });
    },
    preloadFancyboxAnchor: function (item) {
        var now = new Date();

        var url = $(item).data('href');
        var src = $(item).attr('href');
        if (url == undefined || url == "") {
            //skip if src is already set and data-src is empty
            if (src != undefined && src != "") {
                return true;
            }
            $(item).attr('href', '/assets/img/normal_unavailable_image.png');
            return true;
        }
        if (url.startsWith("blob:") || url.startsWith("/")) {
            $(item).attr('href', url);

            return true;
        }

        //href is an id
        if (url.startsWith("#")) {
            return true;
        }

        //try get from cache
        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            if (ezgomediafetcher.objectURLs[url] == '') {
                setTimeout(function () { ezgomediafetcher.preloadFancyboxAnchor(item) }, 50);
                return true;
            }

            if (src != ezgomediafetcher.objectURLs[url]) {
                $(item).attr('href', ezgomediafetcher.objectURLs[url]);
            }

            return true;
        }

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('href', '/assets/img/normal_unavailable_image.png');
                return true;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadFancyboxAnchor(item) }, 50);
            return true;
        }

        ezgomediafetcher.objectURLs[url] = '';

        var tempUrl = url.replace("https://", "");

        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);

        var params = {
            Bucket: bucketname,
            Key: key
        };

        if (bucketname == "" || key == "") {
            let imgUrlPart = '/assets/img/normal_unavailable_image.png';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('href', imgUrlPart);
            return true;
        }

        if (key == "emptyprofile") {
            let imgUrlPart = '/images/user-placeholder.jpg';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('href', imgUrlPart);
            return true;
        }

        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(item).attr('href', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    $(item).attr('href', objectURL);

                    return true;
                }
            });
        }
        catch (error) {
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('href', '/assets/img/normal_unavailable_image.png');
                return true;
            }

            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadFancyboxAnchor(item) }, 50);
            return true;
        };
    },
    preloadFancyboxAnchors: function () {
        $('a[data-fancybox]').each(function (index, item) {
            return ezgomediafetcher.preloadFancyboxAnchor(item);
        });
    },
    preloadFancyboxAnchorsExceptVideos: function () {
        $('a[data-fancybox]').each(function (index, item) {
            return ezgomediafetcher.preloadFancyboxAnchorExceptVideos(item);
        });
    },
    preloadFancyboxAnchorExceptVideos: function (item) {
        var now = new Date();

        var url = $(item).data('href');
        var src = $(item).attr('href');

        if (url == undefined || url == "") {
            //skip if src is already set and data-src is empty
            if (src != undefined && src != "") {
                return true;
            }
            $(item).attr('href', '/assets/img/normal_unavailable_image.png');
            return true;
        }
        if (url.startsWith("blob:") || url.startsWith("/")) {
            $(item).attr('href', url);

            return true;
        }

        //href is an id
        //we use this for videos so videos get skipped because of this if statement
        if (url.startsWith("#")) {
            return true;
        }

        //try get from cache
        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            if (ezgomediafetcher.objectURLs[url] == '') {
                setTimeout(function () { ezgomediafetcher.preloadFancyboxAnchorExceptVideos(item) }, 50);
                return true;
            }

            if (src != ezgomediafetcher.objectURLs[url]) {
                $(item).attr('href', ezgomediafetcher.objectURLs[url]);
            }

            return true;
        }

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('href', '/assets/img/normal_unavailable_image.png');
                return true;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadFancyboxAnchorExceptVideos(item) }, 50);
            return true;
        }

        ezgomediafetcher.objectURLs[url] = '';

        var tempUrl = url.replace("https://", "");

        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);

        var params = {
            Bucket: bucketname,
            Key: key
        };

        if (bucketname == "" || key == "") {
            let imgUrlPart = '/assets/img/normal_unavailable_image.png';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('href', imgUrlPart);
            return true;
        }

        if (key == "emptyprofile") {
            let imgUrlPart = '/images/user-placeholder.jpg';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('href', imgUrlPart);
            return true;
        }

        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(item).attr('href', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    $(item).attr('href', objectURL);

                    return true;
                }
            });
        }
        catch (error) {
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('href', '/assets/img/normal_unavailable_image.png');
                return true;
            }

            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadFancyboxAnchorExceptVideos(item) }, 50);
            return true;
        };
    },
    preloadVisibleImagesAndVideos: function () {
        ezgomediafetcher.setVisibilityAttributes();

        $('img:visible[is-visible="true"], video:visible[is-visible="true"]').each(function (index, item) {
            return ezgomediafetcher.preloadVisibleImageOrVideo(item);
        });
    },
    preloadVisibleImageOrVideo: function (item) {
        var now = new Date();

        var url = $(item).data('src');
        var src = $(item).attr('src');
        if (url == undefined || url == "") {
            //skip if src is already set and data-src is empty
            if (src != undefined && src != "") {
                return true;
            }
            $(item).attr('src', '/assets/img/normal_unavailable_image.png');
            return true;
        }
        if (url.startsWith("blob:") || url.startsWith("/")) {
            $(item).attr('src', url);

            return true;
        }

        //try get from cache
        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            if (ezgomediafetcher.objectURLs[url] == '') {
                setTimeout(function () { ezgomediafetcher.preloadVisibleImageOrVideo(item) }, 50);
                return true;
            }

            if (src != ezgomediafetcher.objectURLs[url]) {
                $(item).attr('src', ezgomediafetcher.objectURLs[url]);
            }

            return true;
        }

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                return true;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadVisibleImageOrVideo(item) }, 50);
            return true;
        }

        ezgomediafetcher.objectURLs[url] = '';

        var tempUrl = url.replace("https://", "");

        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);

        var params = {
            Bucket: bucketname,
            Key: key
        };

        if (bucketname == "" || key == "") {
            let imgUrlPart = '/assets/img/normal_unavailable_image.png';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        if (key == "emptyprofile") {
            let imgUrlPart = '/images/user-placeholder.jpg';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    $(item).attr('src', objectURL);

                    //check if parent element is an anchor element and has data-fancybox attribute
                    let parentElement = $(item).parent();
                    if (parentElement.length) {
                        if (parentElement.is('a') && parentElement[0].hasAttribute('data-fancybox') && parentElement[0].hasAttribute('data-href')) {
                            ezgomediafetcher.preloadFancyboxAnchor(parentElement[0]);
                        }
                    }

                    return true;
                }
            });
        }
        catch (error) {
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                return true;
            }

            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadVisibleImageOrVideo(item) }, 50);
            return true;
        };
    },
    preloadVisibleVideos: function () {
        $('video:visible[is-visible="true"]').each(function (index, item) {
            return ezgomediafetcher.preloadVisibleVideo(item);
        });
    },
    preloadVisibleVideo: function (item) {
        var now = new Date();

        var url = $(item).data('src');
        var src = $(item).attr('src');
        if (url == undefined || url == "") {
            //skip if src is already set and data-src is empty
            if (src != undefined && src != "") {
                return true;
            }
            $(item).attr('src', '/assets/img/normal_unavailable_image.png');
            return true;
        }
        if (url.startsWith("blob:") || url.startsWith("/")) {
            $(item).attr('src', url);
            return true;
        }

        //try get from cache
        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            if (ezgomediafetcher.objectURLs[url] == '') {
                setTimeout(function () { ezgomediafetcher.preloadVisibleVideo(item) }, 50);
                return true;
            }

            if (src != ezgomediafetcher.objectURLs[url]) {
                $(item).attr('src', ezgomediafetcher.objectURLs[url]);
            }

            return true;
        }

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');

                return true;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadVisibleVideo(item) }, 50);

            return true;
        }

        ezgomediafetcher.objectURLs[url] = '';

        var tempUrl = url.replace("https://", "");

        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);

        var params = {
            Bucket: bucketname,
            Key: key
        };

        if (bucketname == "" || key == "") {
            let imgUrlPart = '/assets/img/normal_unavailable_image.png';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        if (key == "emptyprofile") {
            let imgUrlPart = '/images/user-placeholder.jpg';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    $(item).attr('src', objectURL);
                    return true;
                }
            });
        }
        catch (error) {
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                return true;
            }

            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadVisibleVideo(item) }, 50);

            return true;
        };
    },
    preloadImagesAndVideos: function () {
        $('img, video').each(function (index, item) {
            return ezgomediafetcher.preloadImageOrVideo(item);
        });
    },
    preloadImageOrVideo: function (item) {
        var now = new Date();
        
        var url = $(item).data('src');
        var src = $(item).attr('src');
        if (url == undefined || url == "") {
            //skip if src is already set and data-src is empty
            if (src != undefined && src != "") {
                return true;
            }
            $(item).attr('src', '/assets/img/normal_unavailable_image.png');
            return true;
        }
        if (url.startsWith("blob:") || url.startsWith("/")) {
            $(item).attr('src', url);

            return true;
        }

        //try get from cache
        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            if (ezgomediafetcher.objectURLs[url] == '') {
                setTimeout(function () { ezgomediafetcher.preloadImageOrVideo(item) }, 250);

                return true;
            }

            if (src != ezgomediafetcher.objectURLs[url]) {
                $(item).attr('src', ezgomediafetcher.objectURLs[url]);
            }

            return true;
        }

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                return true;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadImageOrVideo(item) }, 250);
            return true;
        }

        ezgomediafetcher.objectURLs[url] = '';

        var tempUrl = url.replace("https://", "");

        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);

        //Fix for video bucket which has URL like https://s3.eu-central-1.amazonaws.com/ezfactory-media/media/tasks/30798/671c713f-e1f6-4496-a363-615b48eaa2a6.MOV
        //Path-style url
        if (bucketname.includes(".com")) {
            bucketname = tempUrl.split('/')[1];
            key = tempUrl.split(bucketname + '/')[1];
            AWS.config.update({
                s3ForcePathStyle: true
            });
            ezgomediafetcher.s3 = new AWS.S3({ region: "eu-central-1" });
            ezgomediafetcher.usingPathStyleURLs = true;
        }

        var params = {
            Bucket: bucketname,
            Key: key
        };

        if (bucketname == "" || key == "") {
            let imgUrlPart = '/assets/img/normal_unavailable_image.png';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        if (key == "emptyprofile") {
            let imgUrlPart = '/images/user-placeholder.jpg';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    $(item).attr('src', objectURL);
                    
                    return true;
                }
            });
            if (ezgomediafetcher.usingPathStyleURLs) {
                AWS.config.update({
                    s3ForcePathStyle: false
                });
                ezgomediafetcher.s3 = new AWS.S3({ region: "eu-central-1" });
                ezgomediafetcher.usingPathStyleURLs = false;
            }
        }
        catch (error) {
            if (ezgomediafetcher.usingPathStyleURLs) {
                AWS.config.update({
                    s3ForcePathStyle: false
                });
                ezgomediafetcher.s3 = new AWS.S3({ region: "eu-central-1" });
                ezgomediafetcher.usingPathStyleURLs = false;
            }
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                return true;
            }

            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadImageOrVideo(item) }, 50);
            return true;
        }
    },
    preloadImages: function () {
        $('img').each(function (index, item) {
            return ezgomediafetcher.preloadImage(item);
        });
    },
    preloadImage: function (item) {
        var now = new Date();

        var url = $(item).data('src');
        var src = $(item).attr('src');
        if (url == undefined || url == "") {
            //skip if src is already set and data-src is empty
            if (src != undefined && src != "") {
                return true;
            }
            $(item).attr('src', '/assets/img/normal_unavailable_image.png');
            return true;
        }
        if (url.startsWith("blob:") || url.startsWith("/")) {
            $(item).attr('src', url);

            return true;
        }

        //try get from cache
        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            if (ezgomediafetcher.objectURLs[url] == '') {
                setTimeout(function () { ezgomediafetcher.preloadImage(item) }, 50);
                return true;
            }

            if (src != ezgomediafetcher.objectURLs[url]) {
                $(item).attr('src', ezgomediafetcher.objectURLs[url]);
            }

            return true;
        }

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                return true;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadImage(item) }, 50);
            return true;
        }

        ezgomediafetcher.objectURLs[url] = '';

        var tempUrl = url.replace("https://", "");

        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);

        //Fix for video bucket which has URL like https://s3.eu-central-1.amazonaws.com/ezfactory-media/media/tasks/30798/671c713f-e1f6-4496-a363-615b48eaa2a6.MOV
        //Path-style url
        if (bucketname.includes(".com")) {
            bucketname = tempUrl.split('/')[1];
            key = tempUrl.split(bucketname + '/')[1];
            AWS.config.update({
                s3ForcePathStyle: true
            });
            ezgomediafetcher.s3 = new AWS.S3({ region: "eu-central-1" });
            ezgomediafetcher.usingPathStyleURLs = true;
        }

        var params = {
            Bucket: bucketname,
            Key: key
        };

        if (bucketname == "" || key == "") {
            let imgUrlPart = '/assets/img/normal_unavailable_image.png';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        if (key == "emptyprofile") {
            let imgUrlPart = '/images/user-placeholder.jpg';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('src', imgUrlPart);
            return true;
        }

        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    $(item).attr('src', objectURL);

                    return true;
                }
            });
            if (ezgomediafetcher.usingPathStyleURLs) {
                AWS.config.update({
                    s3ForcePathStyle: false
                });
                ezgomediafetcher.s3 = new AWS.S3({ region: "eu-central-1" });
                ezgomediafetcher.usingPathStyleURLs = false;
            }
        }
        catch (error) {
            if (ezgomediafetcher.usingPathStyleURLs) {
                AWS.config.update({
                    s3ForcePathStyle: false
                });
                ezgomediafetcher.s3 = new AWS.S3({ region: "eu-central-1" });
                ezgomediafetcher.usingPathStyleURLs = false;
            }
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('src', '/assets/img/normal_unavailable_image.png');
                return true;
            }

            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadImage(item) }, 50);
            return true;
        }
    },
    preloadAreaChartSvgImages: function() {
        $('#orgchart svg image').each(function (index, item) {
            ezgomediafetcher.preloadAreaChartSvgImage(item);
        });
    },
    preloadAreaChartSvgImage: function (item) {
        var now = new Date();

        var url = $(item).data('src');
        var src = $(item).attr('xlink:href');

        if (url == undefined || url == "") {
            //skip if src is already set and data-src is empty
            if (src != undefined && src != "") {
                return true;
            }
            $(item).attr('xlink:href', '/assets/img/normal_unavailable_image.png');
            return true;
        }
        if (url.startsWith("blob:") || url.startsWith("/")) {
            $(item).attr('xlink:href', url);
            return true;
        }

        //try get from cache
        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            if (ezgomediafetcher.objectURLs[url] == '') {
                setTimeout(function () { ezgomediafetcher.preloadAreaChartSvgImage(item) }, 50);
                return true;
            }

            if (src != ezgomediafetcher.objectURLs[url]) {
                $(item).attr('xlink:href', ezgomediafetcher.objectURLs[url]);
            }
            return true;
        }

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('xlink:href', '/assets/img/normal_unavailable_image.png');
                return true;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadAreaChartSvgImage(item) }, 50);
            return true;
        }

        ezgomediafetcher.objectURLs[url] = '';

        var tempUrl = url.replace("https://", "");

        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);

        var params = {
            Bucket: bucketname,
            Key: key
        };

        if (bucketname == "" || key == "") {
            let imgUrlPart = '/assets/img/normal_unavailable_image.png';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('xlink:href', imgUrlPart);
            return true;
        }

        if (key == "emptyprofile") {
            let imgUrlPart = '/images/user-placeholder.jpg';
            ezgomediafetcher.objectURLs[url] = imgUrlPart;
            $(item).attr('xlink:href', imgUrlPart);
            return true;
        }

        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(item).attr('xlink:href', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    $(item).attr('xlink:href', objectURL);

                    var fancyboxattr = $(item).closest('a').attr("data-fancybox");
                    // For some browsers, `attr` is undefined; for others,
                    // `attr` is false.  Check for both.
                    if (typeof fancyboxattr !== 'undefined' && fancyboxattr !== false) {
                        $(item).closest('a').attr('href', objectURL);
                    }
                    return true;
                }
            });
        }
        catch (error) {
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(item).attr('xlink:href', '/assets/img/normal_unavailable_image.png');
                return true;
            }

            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.preloadAreaChartSvgImage(item) }, 50);
            return true;
        };
    },
    getAndSetImage: function (e) { //https://ezfactory-test-storage.s3.eu-central-1.amazonaws.com/136/lists/0/35e5867a-f739-41f6-a549-429fc6c1a0c7.jpg
        var now = new Date();

        //initialize ezgomediafetcher if necessary
        if(!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(e.target).attr('src', '/assets/img/normal_unavailable_image.png');
                return;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function() { ezgomediafetcher.getAndSetImage(e) }, 50);
            return;
        }

        //if url is blob dont do anything
        var url = $(e.target).attr('src');
        if(url == undefined || url == "" || url.startsWith("blob:") || url.startsWith("/"))
        {
            return;
        }

        if(ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            ezgomediafetcher.updateImageAndRelatedHtml(e, ezgomediafetcher.objectURLs[url]);
        }

        var tempUrl = url.replace("https://", "");
        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/')+1, tempUrl.length);
        var params = {
            Bucket: bucketname,
            Key: key
        };
        try {
            ezgomediafetcher.s3.getObject(params, function(err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(e.target).attr('src', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;
                    
                    //update html
                    ezgomediafetcher.updateImageAndRelatedHtml(e, objectURL);
                }
            });
        }
        catch(error) {
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(e.target).attr('src', '/assets/img/normal_unavailable_image.png');
                return;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function() { ezgomediafetcher.getAndSetImage(e) }, 50);
        };
    },
    updateImageAndRelatedHtml: function(e, objectURL) {
        $(e.target).attr('src', objectURL);

        var fancyboxattr = $(e.target).closest('a').attr("data-fancybox");
        // For some browsers, `attr` is undefined; for others,
        // `attr` is false.  Check for both.
        if (typeof fancyboxattr !== 'undefined' && fancyboxattr !== false) {
            $(e.target).closest('a').attr('href', objectURL);
        }
    },
    getAndSetSvgImage: function (e) {
        var now = new Date();

        //initialize ezgomediafetcher if necessary
        if (!ezgomediafetcher.s3 || ezgomediafetcher.s3 == null || now > ezgomediafetcher.expirationDate) {
            if (ezgomediafetcher.apiConnectionBroken) {
                $(e.target).attr('xlink:href', '/assets/img/normal_unavailable_image.png');
                return;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.getAndSetImage(e) }, 50);
            return;
        }

        //if url is blob dont do anything
        var url = $(e.target).attr('xlink:href');
        if (url == undefined || url == "" || url.startsWith("blob:") || url.startsWith("/")) {
            return;
        }

        if (ezgomediafetcher.objectURLs != undefined && ezgomediafetcher.objectURLs[url] != undefined) {
            ezgomediafetcher.updateImageAndRelatedHtml(e, ezgomediafetcher.objectURLs[url]);
        }

        var tempUrl = url.replace("https://", "");
        var bucketname = tempUrl.split('.s3')[0];
        var key = tempUrl.substring(tempUrl.indexOf('/') + 1, tempUrl.length);
        var params = {
            Bucket: bucketname,
            Key: key
        };
        try {
            ezgomediafetcher.s3.getObject(params, function (err, data) {
                if (err) {
                    console.log(err, err.stack);
                    $(e.target).attr('xlink:href', '/assets/img/normal_unavailable_image.png');
                }
                else {
                    var blob = new Blob([new Uint8Array(data.Body)], { type: data.ContentType });
                    var objectURL = URL.createObjectURL(blob);

                    //update cache
                    ezgomediafetcher.objectURLs[url] = objectURL;

                    //update html
                    ezgomediafetcher.updateSvgImageAndRelatedHtml(e, objectURL);
                }
            });
        }
        catch (error) {
            console.log(error);
            delete ezgomediafetcher.objectURLs[url];
            if (ezgomediafetcher.apiConnectionBroken) {
                $(e.target).attr('xlink:href', '/assets/img/normal_unavailable_image.png');
                return;
            }
            ezgomediafetcher.tryInit();
            setTimeout(function () { ezgomediafetcher.getAndSetImage(e) }, 50);
        };
    },
    updateSvgImageAndRelatedHtml: function (e, objectURL) {
        $(e.target).attr('xlink:href', objectURL);
    },
};
