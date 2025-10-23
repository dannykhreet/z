var announcements = {
    init: function () {
        if (document.querySelector('#announcement_popup_container') != null) {
            document.querySelector('#btn_announcements').addEventListener("click", function (e) {
                if (document.querySelector('#announcement_popup_container').style.display == 'block') {
                    document.querySelector('#announcement_popup_container').style.display = 'none';
                } else {
                    announcements.retrieve();
                    document.querySelector('#announcement_popup_container').style.display = 'block';
                }
            });
            document.querySelector('#announcement_popup_container').addEventListener("click", function (e) {
                document.querySelector('#announcement_popup_container').style.display = 'none';
            }); 
        }
    },
    retrieve: function () {
        let xhr = new XMLHttpRequest();
        xhr.open('GET', '/announcements/latest');
        xhr.send();
        xhr.onload = function () {
            if (xhr.status == 200) {
                document.querySelector('#annoucement_popup_content').innerHTML = xhr.response;
            } 
        };
        xhr.onerror = function () {
            //do nothign
        };
    }
}

announcements.init();