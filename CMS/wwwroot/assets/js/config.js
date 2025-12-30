
var config = {
    settings: {
    showActivity: false,
    showHeaderImages: true},
    save: function(){
        setCookie('config',JSON.stringify(this.settings),1);
    },
    load: function(){
        var obj = JSON.parse(getCookie('config'));
        if(obj != null){
            this.settings = obj;
        }

        //this.showHeaderImages = obj.showHeaderImages == 'true';
    }
}