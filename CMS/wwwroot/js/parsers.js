var GlobalParser = {
    cleaner: function (txt) {
        let ele = document.createElement('div');
        ele.innerHTML = txt.replaceAll('"', '&quot;').replaceAll('\'', '&apos;').replaceAll('`', '&grave;');
        return ele.innerText; 
    },
    init: function () {
        GlobalParser.setHandlers();
    },
    setHandlers: function () {
        let items = document.querySelectorAll('input[type="text"], textarea');
        if (items != undefined && items != null) {
            for (let i = 0; i < items.length; i++) {
                items[i].addEventListener('input', function (e) {
                    var newVal = GlobalParser.cleaner(e.target.value);
                    if (newVal != e.target.value) {
                        e.target.value = newVal;
                    }
                });
                items[i].addEventListener('paste', function (e) {
                    var newVal = GlobalParser.cleaner(e.target.value);
                    if (newVal != e.target.value) {
                        e.target.value = newVal;
                    }
                });
            }
        }
    },
    escapeHtmlCharacters: function (text) {
        if (text != undefined && text != null) {
            return text.replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;').replaceAll('\'', '&#039;');
        } else {
            return text;
        }
    },
}

GlobalParser.init();