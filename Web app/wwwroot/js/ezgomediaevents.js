var ezgomediaevents = {
    tryInitEventHandlers: function() {
        if (window.jQuery) {
            ezgomediaevents.initEventHandlers();
        } else {
            setTimeout(function() { ezgomediaevents.tryInitEventHandlers() }, 50);
        }
    },
    initEventHandlers: function() {
        //const img = document.querySelectorAll("img");
        //img.forEach((el) => {
        //    el.addEventListener('load', (e) => {
        //        console.log("LOAD EVENT WITHOUT JQUERY");
        //        console.log(e);
        //        console.log(el);
        //        ezgomediafetcher.getAndSetImage(e);
        //    })
        //});

        //doesn't work
        $('body').on('error', 'img', function(e) {
            //console.log("IMG mouseover EVENT TRIGGERED");
            console.log(e);
            ezgomediafetcher.getAndSetImage(e);
        });
        
        /*console.log("event handlers added!");*/
    },

};

ezgomediaevents.tryInitEventHandlers();