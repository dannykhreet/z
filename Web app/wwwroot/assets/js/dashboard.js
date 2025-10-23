
$(document).ready(function(){
    config.load();
    if(!config.settings.showActivity){
        $("div[data-role='tile-button'] div.card-body").hide();
    };
    if(!config.settings.showHeaderImages){
        $("div[data-role='tile-button'] div.card-header").css('background-image', '');
    };
    $('#chk-activity').prop( "checked", config.settings.showActivity );
    $('#chk-headerimg').prop( "checked", config.settings.showHeaderImages );
    $("div[data-role='tile-button']").on('click', function(event){
        var listType = $(event.currentTarget).data('list');
        //window.console.log(listType);
        switch(listType){
            case 'checklist':
                location.href='checklist.html';
                break;
            case 'tasklist':
                location.href='tasklist.html';
                break;
        }
    });

    $('#chk-activity').on('change', function(){
        var status = $(this).is(':checked');
        config.settings.showActivity = status;
        config.save();
    });
    $('#chk-headerimg').on('change', function(){
        var status = $(this).is(':checked');
        config.settings.showHeaderImages = status;
        config.save();
    });

    
});