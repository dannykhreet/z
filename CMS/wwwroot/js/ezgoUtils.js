(function ($) {
    $.utils = {
        // http://stackoverflow.com/a/8809472

        createUUID: function () {
            var d = new Date().getTime();
            if (window.performance && typeof window.performance.now === "function") {
                d += performance.now(); //use high-precision timer if available
            }
            var uuid = 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
                var r = (d + Math.random() * 16) % 16 | 0;
                d = Math.floor(d / 16);
                return (c == 'x' ? r : (r & 0x3 | 0x8)).toString(16);
            });
            return uuid;
        },

        dataURItoBlob: function (dataURI) {
            var mime = dataURI.split(',')[0].split(':')[1].split(';')[0];
            var binary = atob(dataURI.split(',')[1]);
            var array = [];
            for (var i = 0; i < binary.length; i++) {
                array.push(binary.charCodeAt(i));
            }
            return new Blob([new Uint8Array(array)], { type: mime });
        },

        arrayMove: function (arr, old_index, new_index) {
            while (old_index < 0) {
                old_index += arr.length;
            }
            while (new_index < 0) {
                new_index += arr.length;
            }
            arr.splice(new_index, 0, arr.splice(old_index, 1)[0]);
        }
    }

    $.fn.dialogue = function (options) {
        var defaults = {
            title: "", content: $("<p />"),
            closeIcon: false, id: $.utils.createUUID(), open: function () { }, buttons: []
        };
        var settings = $.extend(true, {}, defaults, options);

        // create the DOM structure
        var $modal = $("<div />").attr("id", settings.id).attr("role", "dialog").addClass("modal fade")
            .append($("<div />").addClass("modal-dialog modal-dialog-centered")
                .append($("<div />").addClass("modal-content")
                    .append($("<div />").addClass("modal-header")
                        .append($("<h4 />").addClass("modal-title").text(settings.title)))
                    .append($("<div />").addClass("modal-body")
                        .append(settings.content))
                    .append($("<div />").addClass("modal-footer")
                    )
                )
            );
        $modal.data('backdrop', 'static');
        $modal.shown = false;
        $modal.dismiss = function () {
            // loop until its shown
            // this is only because you can do $.fn.alert("utils.js makes this so easy!").dismiss(); in which case it will try to remove it before its finished rendering
            if (!$modal.shown) {
                window.setTimeout(function () {
                    $modal.dismiss();
                }, 50);
                return;
            }

            // hide the dialogue
            $modal.modal("hide");
            // remove the blanking
            $modal.prev().remove();
            // remove the dialogue
            $modal.empty().remove();

            $("body").removeClass("modal-open");
        }

        if (settings.closeIcon)
            $modal.find(".modal-header").prepend($("<button />").attr("type", "button").addClass("close").html("&times;").click(function () { $modal.dismiss() }));

        // add the buttons
        var $footer = $modal.find(".modal-footer");
        for (var i = 0; i < settings.buttons.length; i++) {
            (function (btn) {
                $footer.prepend($("<button />").addClass("btn btn-ezgo btn-sm")
                    .attr("id", btn.id)
                    .attr("type", "button")
                    .html(btn.text)
                    .click(function () {
                        btn.click($modal)
                    }))
            })(settings.buttons[i]);
        }

        settings.open($modal);

        $modal.on('shown.bs.modal', function (e) {
            $modal.shown = true;
        });
        // show the dialogue
        $modal.modal("show");

        return $modal;
    };
})(jQuery);


(function ($) {
    $.fn.alert = function (message) {
        return $.fn.dialogue({
            //title: "Alert",
            content: $("<p />").text(message),
            closeIcon: true,
            buttons: [
                { text: "Yes", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } },
                { text: "No", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
            ]
        });
    };

    $.fn.message = function (message) {
        return $.fn.dialogue({
            //title: "Alert",
            content: $("<p />").text(message),
            closeIcon: true,
            buttons: [
                { text: " Ok ", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
            ]
        });
    };
})(jQuery);