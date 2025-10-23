
var matrixAreas = {
    renderAdd: function () {
        $.ajax({
            url: '/config/getareas',
            dataType: 'json',
            success: function (data) {

                $('#areascardadd').html('');
                var area = data.filter(obj => {
                    return obj.Id === 5495;
                })[0];
                for (var i = 0; i < 10; i++) {
                    var level = data.filter(obj => {
                        return obj.Level === i;
                    });
                    if (level.length !== 0) {
                        var sub_ul = $('<ul class="list-group col-3" />')
                        $(level).each(function (index, item) {
                            var sub_li = $('<li data-id="' + item.Id + '" data-parentid="' + item.ParentId + '" data-level="' + item.Level + '" />');
                            sub_li.html(item.Name);
                            sub_li.addClass('list-group-item');
                            sub_li.data('id', item.Id);
                            sub_li.data('parentid', item.ParentId);
                            if (i !== 0) {
                                sub_li.css('display', 'none');
                            }
                            sub_ul.append(sub_li);
                        });
                        $('#areascardadd').append(sub_ul);
                    }
                }
                $('#areascardadd ul').on('click', 'li', function (el) {
                    var currentId = $(el.currentTarget).data('id');
                    var currentLevel = $(el.currentTarget).data('level');
                    matrixOverview.currentlySelectedArea = currentId;
                    matrixAreas.hideOthersAdd(currentLevel);
                    $('#areascardadd li[data-parentid="' + currentId + '"]').css('display', 'block');
                    $('#areascardadd li[data-level="' + currentLevel + '"]').removeClass('active');
                    $(el.currentTarget).addClass('active');
                });

                if (matrixOverview.currentlySelectedArea > 0) {
                    var area = data.filter(obj => {
                        return obj.Id === parseInt(matrixOverview.currentlySelectedArea);
                    })[0];
                    $('#areascardadd ul li[data-level!="0"]').hide();
                    $('#areascardadd ul li').removeClass('active');
                    var parents = area.FullDisplayIds.split(' -> ');
                    $(parents).each(function (index, item) {
                        $('#areascardadd li[data-parentid="' + item + '"]').show();
                        $('#areascardadd li[data-id="' + item + '"]').addClass('active');
                    });
                }

            }
        });
    },
    renderChange: function () {
        $.ajax({
            url: '/config/getareas',
            dataType: 'json',
            success: function (data) {

                $('#areascardchange').html('');
                var area = data.filter(obj => {
                    return obj.Id === 5495;
                })[0];
                for (var i = 0; i < 10; i++) {
                    var level = data.filter(obj => {
                        return obj.Level === i;
                    });
                    if (level.length !== 0) {
                        var sub_ul = $('<ul class="list-group col-3" />')
                        $(level).each(function (index, item) {
                            var sub_li = $('<li data-id="' + item.Id + '" data-parentid="' + item.ParentId + '" data-level="' + item.Level + '" />');
                            sub_li.html(item.Name);
                            sub_li.addClass('list-group-item');
                            sub_li.data('id', item.Id);
                            sub_li.data('parentid', item.ParentId);
                            if (i !== 0) {
                                sub_li.css('display', 'none');
                            }
                            sub_ul.append(sub_li);
                        });
                        $('#areascardchange').append(sub_ul);
                    }
                }
                $('#areascardchange ul').on('click', 'li', function (el) {
                    var currentId = $(el.currentTarget).data('id');
                    var currentLevel = $(el.currentTarget).data('level');
                    matrixOverview.currentlySelectedArea = currentId;
                    matrixAreas.hideOthersChange(currentLevel);
                    $('#areascardchange li[data-parentid="' + currentId + '"]').css('display', 'block');
                    $('#areascardchange li[data-level="' + currentLevel + '"]').removeClass('active');
                    $(el.currentTarget).addClass('active');
                });

                if (matrixOverview.currentlySelectedArea > 0) {
                    var area = data.filter(obj => {
                        return obj.Id === parseInt(matrixOverview.currentlySelectedArea);
                    })[0];
                    $('#areascardchange ul li[data-level!="0"]').hide();
                    $('#areascardchange ul li').removeClass('active');
                    var parents = area.FullDisplayIds.split(' -> ');
                    $(parents).each(function (index, item) {
                        $('#areascardchange li[data-parentid="' + item + '"]').show();
                        $('#areascardchange li[data-id="' + item + '"]').addClass('active');
                    });
                }

            }
        });
    },
    hideOthersAdd: function (level) {
        var list = $("#areascardadd > ul li").filter(function () {
            return $(this).attr("data-level") > level;
        });
        list.hide().removeClass('active');
    },
    hideOthersChange: function (level) {
        var list = $("#areascardchange > ul li").filter(function () {
            return $(this).attr("data-level") > level;
        });
        list.hide().removeClass('active');
    },

};