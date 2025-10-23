var areaid = 0;
var arealevel = 0;

var areaFilter_v2 = {
    initialized: false,
    language: {
        workAndAssessmentInstructions: 'Work and assessment instructions',
        workInstructions: 'Work instructions',
        assessmentInstructions: 'Assessment instructions'
    },
    filterResult: new Array(),
    _listtype: '',
    _enableExternalFiltering: false,

    init(url, listtype, enableExternalFiltering) {
        this._listtype = listtype;
        this._enableExternalFiltering = enableExternalFiltering;
        $.ajax({
            url: url,
            dataType: 'json',
            async: false,
            success: function (data) {
                //area tree populating
                var tree = $('#areatree');
                var ul = $('<ul/>').addClass('list-group');
                $(data).each(function(index,area){
                    var sub = $('<li data-id="' + area.Id + '" data-parentid="' + area.ParentId + '" data-level="' + area.Level + '" />');
                    sub.addClass('list-group-item p-2 pr-0');
                    var child = areaFilter_v2.subTree(area);
                    var link = $('<a/>').attr('data-id', area.Id);
                    link.append('<span>' + area.Name + '</span>');
                    if(area.Children.length > 0){
                        link.addClass('d-flex align-items-center justify-content-between');
                        link.append('<i class="fas fa-chevron-down" />');
                    }
                    ul.append(sub.append(link.css('display','block')).append(child));
                });

                tree.append(ul);

                //inline area tree populating
                var inlineTree = $('#inlineAreaTree');
                var inlineUl = $('<ul/>').addClass('list-group-area-filter-inline');
                $(data).each(function (index, area) {
                    var sub = $('<li data-id="' + area.Id + '" data-parentid="' + area.ParentId + '" data-level="' + area.Level + '" />');
                    sub.addClass('list-group-item p-2 pr-0');
                    var child = areaFilter_v2.subTreeInline(area);
                    var link = $('<a/>').attr('data-id', area.Id);
                    link.append('<span>' + area.Name + '</span>');
                    if (area.Children.length > 0) {
                        link.addClass('d-flex align-items-center justify-content-between');
                        link.append('<i class="fas fa-chevron-down" />');
                    }
                    inlineUl.append(sub.append(link.css('display', 'block')).append(child));
                });

                inlineTree.append(inlineUl);

                //area tree event handler
                tree.on('click', 'a', function (el) {
                    var currentId = $(el.currentTarget).parent('li').data('id');
                    if (currentId != areaid) {
                        areaFilter_v2.selectAreaNode(el.currentTarget);

                        //find element in other tree
                        var otherEl = $(`#inlineAreaTree`).find(`a[data-id='${currentId}']`);
                        if (otherEl.length) {
                            areaFilter_v2.selectAreaNode(otherEl[0]);
                        }
                    }
                    else {
                        areaFilter_v2.deselectAreaNodes('#areatree');
                        areaFilter_v2.deselectAreaNodes('#inlineAreaTree');
                    }
                });

                //inline area tree event handler
                inlineTree.on('click', 'a', function (el) {
                    var currentId = $(el.currentTarget).parent('li').data('id');
                    if (currentId != areaid) {
                        areaFilter_v2.selectAreaNode(el.currentTarget);

                        //find element in other tree
                        var otherEl = $('#areatree').find(`a[data-id='${currentId}'`);
                        if (otherEl.length) {
                            areaFilter_v2.selectAreaNode(otherEl[0]);
                        }
                    }
                    else {
                        areaFilter_v2.deselectAreaNodes('#inlineAreaTree');
                        areaFilter_v2.deselectAreaNodes('#areatree');
                    }
                });

                if (!areaFilter_v2._enableExternalFiltering) {
                    //reset filters on left side
                    $("#instructionFilter").val('default').selectpicker("refresh");
                    $("#roleFilter").val('default').selectpicker("refresh");
                    $("#imageFilter").val('default').selectpicker("refresh");
                    $("#videoFilter").val('default').selectpicker("refresh");
                    $("#recurrenceFilter").val('default').selectpicker("refresh");
                    $('#assessorFilter').val('default').selectpicker("refresh");
                    $('#assesseeFilter').val('default').selectpicker("refresh");
                    $('#templateFilter').val('default').selectpicker("refresh");
                    $('#workinstructionTypeFilter').val('default').selectpicker("refresh");
                    $('#notificationAuthorFilter').val('default').selectpicker("refresh");

                    //reset inline filters
                    $("#instructionFilterInline").val('default').selectpicker("refresh");
                    $("#roleFilterInline").val('default').selectpicker("refresh");
                    $("#imageFilterInline").val('default').selectpicker("refresh");
                    $("#videoFilterInline").val('default').selectpicker("refresh");
                    $("#recurrenceFilterInline").val('default').selectpicker("refresh");
                    $('#assessorFilterInline').val('default').selectpicker("refresh");
                    $('#assesseeFilterInline').val('default').selectpicker("refresh");
                    $('#workinstructionTypeFilterInline').val('default').selectpicker("refresh");

                    $('[data-role="roleFilter"],[data-role="instructionFilter"],[data-role="imageFilter"],[data-role="videoFilter"],[data-role="recurrenceFilter"],[data-role="assessorFilter"],[data-role="assesseeFilter"],[data-role="workinstructionTypeFilter"]').on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) {
                        var filterId = $(e.currentTarget).attr('id');

                        $(`#${filterId}Inline`).val($(`#${filterId}`).val()).selectpicker("refresh");

                        //areaFilter_v2.doFilter('block');
                    });

                    $('[data-role="roleFilterInline"],[data-role="instructionFilterInline"],[data-role="imageFilterInline"],[data-role="videoFilterInline"],[data-role="recurrenceFilterInline"],[data-role="assessorFilterInline"],[data-role="assesseeFilterInline"],[data-role="workinstructionTypeFilterInline"]').on('changed.bs.select', function (e, clickedIndex, isSelected, previousValue) {
                        var inlineFilterId = $(e.currentTarget).attr('id');

                        $(`#${inlineFilterId.replace("Inline", "")}`).val($(`#${inlineFilterId}`).val()).selectpicker("refresh");

                        $(`#${inlineFilterId.replace("Inline", "")}`).trigger('changed.bs.select');
                    });

                }

                areaFilter_v2.initialized = true;
            }
        });
    },

    nextSibling: function (element) {
        element.parent('li').closest('ul').show();
        return element.parent('li').closest('ul');
    },

    selectAreaNode: function (el) {
        if (el == undefined) {
            return;
        }
        var currentId = $(el).parent('li').data('id');
        var currentLevel = $(el).parent('li').data('level');
        areaid = currentId;
        arealevel = currentLevel;
        $(el).closest('ul').find('li a').css('font-weight', '400');
        $(el).css('font-weight', '700');
        $(el).closest('ul').find('li ul').hide('slow');
        var ul = $(el).next('ul');
        ul.slideToggle(!ul.is(":visible"));
        if (!ul.is(":visible")) {
            ul.find('ul').hide('slow');
        }
    },

    deselectAreaNodes: function (selector) {
        var tree = $(selector);

        tree.find('a').css('font-weight', '400');
        tree.find('ul:not(:first)').hide('slow');

        areaid = 0;
        arealevel = 0;
    },

    subTree: function(area){
        var ul = $('<ul/>').addClass('list-group').css('display','none');
        $(area.Children).each(function(index,area){
            var sub = $('<li data-id="' + area.Id + '" data-parentid="' + area.ParentId + '" data-level="' + area.Level + '" />');
            sub.addClass('list-group-item p-0 mt-2 mb-2 ml-3 pr-0 border-0');
            var child = areaFilter_v2.subTree(area);
            var link = $('<a/>').attr('data-id',area.Id);
            link.append('<span>' + area.Name + '</span>');
            if(area.Children.length > 0){
                link.addClass('d-flex align-items-center justify-content-between');
                link.append('<i class="fas fa-chevron-down" />');
            }
            sub.append(link.css('display','block')).append(child);
            ul.append(sub);
        });
        return ul;
    },

    subTreeInline: function (area) {
        var ul = $('<ul/>').addClass('list-group-area-filter-inline').css('display', 'none');
        $(area.Children).each(function (index, area) {
            var sub = $('<li data-id="' + area.Id + '" data-parentid="' + area.ParentId + '" data-level="' + area.Level + '" />');
            sub.addClass('list-group-item p-0 mt-2 mb-2 ml-3 pr-0 border-0');
            var child = areaFilter_v2.subTreeInline(area);
            var link = $('<a/>').attr('data-id', area.Id);
            link.append('<span>' + area.Name + '</span>');
            if (area.Children.length > 0) {
                link.addClass('d-flex align-items-center justify-content-between');
                link.append('<i class="fas fa-chevron-down" />');
            }
            sub.append(link.css('display', 'block')).append(child);
            ul.append(sub);
        });
        return ul;
    },

    reset: function (displaytype) {
        areaid = 0;
        arealevel = 0;
        //reset filters on left side
        $("#instructionFilter").val('default').selectpicker("refresh");
        $("#roleFilter").val('default').selectpicker("refresh");
        $("#imageFilter").val('default').selectpicker("refresh");
        $("#videoFilter").val('default').selectpicker("refresh");
        $("#recurrenceFilter").val('default').selectpicker("refresh");
        $('#assessorsFilter').val('default').selectpicker("refresh");
        $('#assesseeFilter').val('default').selectpicker("refresh");
        $('#templateFilter').val('default').selectpicker("refresh");
        $('#iscompletedFilter').val('true').selectpicker("refresh");
        $('#workinstructionTypeFilter').val('default').selectpicker("refresh");
        $('#notificationAuthorFilter').val('default').selectpicker("refresh");
        
        $('[id^="filter-tag-"]').each(function (index, elem) {
            $(elem).hide();
        });

        $('.modal-filter-tag').each(function (index, elem) {
            var id = $(elem).data('id');
            if ($(elem).children('div')[0] != null && $($(elem).children('div')[0]).hasClass('selected-tag')) {
                toggleStyle($(elem).data('id'));
            }
        });

        var tree = $('#areatree');
        tree.find('a').css('font-weight', '400');
        tree.find('ul:not(:first)').hide();

        //reset inline filters
        $("#instructionFilterInline").val('default').selectpicker("refresh");
        $("#roleFilterInline").val('default').selectpicker("refresh");
        $("#imageFilterInline").val('default').selectpicker("refresh");
        $("#videoFilterInline").val('default').selectpicker("refresh");
        $("#recurrenceFilterInline").val('default').selectpicker("refresh");
        $('#assessorFilterInline').val('default').selectpicker("refresh");
        $('#assesseeFilterInline').val('default').selectpicker("refresh");
        $('#workinstructionTypeFilterInline').val('default').selectpicker("refresh");

        var tree = $('#inlineAreaTree');
        tree.find('a').css('font-weight', '400');
        tree.find('ul:not(:first)').hide();

        localStorage.removeItem(areaFilter_v2._listtype + '_filters');
    },
}
