var language = {
    _version: '1.0',
    language: {
        switchSuccess: "Language switched!"
    },
    init: function () {
        
    },
    switch: function (local) {
        if (local != null) {
            toastr.remove();
            $('body').toggleClass('loaded');
            //add responses management from api (invalid password etc.)
            $.ajax({
                type: "POST",
                url: '/switchlanguage/' + local,
                data: '',
                success: function (data) {
                    //toastr.success(language.switchSuccess);
                    document.location.href = document.location.href.replace('#','');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $('body').toggleClass('loaded');
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });
        } else {
            //console.log('unable to switch language');
        }
    }
}

language.init();