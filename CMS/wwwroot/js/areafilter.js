var areaid = 0;
var arealevel = 0;

var areaFilter = {
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
            success: function (data) {
                //area tree populating
                var tree = $('#areatree');
                var ul = $('<ul/>').addClass('list-group');
                $(data).each(function(index,area){
                    var sub = $('<li data-id="' + area.Id + '" data-parentid="' + area.ParentId + '" data-level="' + area.Level + '" />');
                    sub.addClass('list-group-item p-2 pr-0');
                    var child = areaFilter.subTree(area);
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
                    var child = areaFilter.subTreeInline(area);
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
                        areaFilter.selectAreaNode(el.currentTarget);

                        //find element in other tree
                        var otherEl = $(`#inlineAreaTree`).find(`a[data-id='${currentId}']`);
                        if (otherEl.length) {
                            areaFilter.selectAreaNode(otherEl[0]);
                        }
                    }
                    else {
                        areaFilter.deselectAreaNodes('#areatree');
                        areaFilter.deselectAreaNodes('#inlineAreaTree');
                    }

                    if (!areaFilter._enableExternalFiltering) {
                        areaFilter.doFilter('block');
                    }
                });

                //inline area tree event handler
                inlineTree.on('click', 'a', function (el) {
                    var currentId = $(el.currentTarget).parent('li').data('id');
                    if (currentId != areaid) {
                        areaFilter.selectAreaNode(el.currentTarget);

                        //find element in other tree
                        var otherEl = $('#areatree').find(`a[data-id='${currentId}'`);
                        if (otherEl.length) {
                            areaFilter.selectAreaNode(otherEl[0]);
                        }
                    }
                    else {
                        areaFilter.deselectAreaNodes('#inlineAreaTree');
                        areaFilter.deselectAreaNodes('#areatree');
                    }

                    if (!areaFilter._enableExternalFiltering) {
                        areaFilter.doFilter('block');
                    }
                });

                if (!areaFilter._enableExternalFiltering) {
                    //reset filters on left side
                    $("#instructionFilter").val('').trigger('change');
                    $("#roleFilter").val('').trigger('change');
                    $("#imageFilter").val('').trigger('change');
                    $("#videoFilter").val('').trigger('change');
                    $("#recurrenceFilter").val('').trigger('change');
                    $('#assessorFilter').val('').trigger('change');
                    $('#assesseeFilter').val('').trigger('change');
                    $('#workinstructionTypeFilter').val('').trigger('change');

                    //reset inline filters
                    $("#instructionFilterInline").val('').trigger('change');
                    $("#roleFilterInline").val('').trigger('change');
                    $("#imageFilterInline").val('').trigger('change');
                    $("#videoFilterInline").val('').trigger('change');
                    $("#recurrenceFilterInline").val('').trigger('change');
                    $('#assessorFilterInline').val('').trigger('change');
                    $('#assesseeFilterInline').val('').trigger('change');
                    $('#workinstructionTypeFilterInline').val('').trigger('change');

                    $('[data-role="roleFilter"],[data-role="instructionFilter"],[data-role="imageFilter"],[data-role="videoFilter"],[data-role="recurrenceFilter"],[data-role="assessorFilter"],[data-role="assesseeFilter"],[data-role="workinstructionTypeFilter"]').on('change', function (e, clickedIndex, isSelected, previousValue) {
                        var filterId = $(e.currentTarget).attr('id');

                        $(`#${filterId}Inline`).val($(`#${filterId}`).val()).trigger('change');

                        areaFilter.doFilter('block');
                    });

                    $('[data-role="roleFilterInline"],[data-role="instructionFilterInline"],[data-role="imageFilterInline"],[data-role="videoFilterInline"],[data-role="recurrenceFilterInline"],[data-role="assessorFilterInline"],[data-role="assesseeFilterInline"],[data-role="workinstructionTypeFilterInline"]').on('change', function (e, clickedIndex, isSelected, previousValue) {
                        var inlineFilterId = $(e.currentTarget).attr('id');

                        $(`#${inlineFilterId.replace("Inline", "")}`).val($(`#${inlineFilterId}`).val()).trigger('change');

                        $(`#${inlineFilterId.replace("Inline", "")}`).trigger('changed.bs.select');
                    });

                    var filters = localStorage.getItem(areaFilter._listtype + '_filters');
                    if (filters !== null) {
                        var filterObj = JSON.parse(filters);
                        if (filterObj.listtype === areaFilter._listtype) {
                            //v instructionFilter
                            //v roleFilter
                            //v imageFilter
                            //v videoFilter
                            //v recurrenceFilter
                            //assessorFilter
                            //assesseeFilter
                            //v workinstructionTypeFilter

                            areaid = parseInt(filterObj.areaid);
                            //console.log('FilterObj: ' + filterObj + ' -> ' + filters);
                            if ($("#instructionFilter")[0] != null) {
                                $("#instructionFilter").val(filterObj.hasinstruction).trigger('change');
                                $("#instructionFilterInline").val(filterObj.hasinstruction).trigger('change');
                            }

                            if ($('#roleFilter')[0] != null) {
                                if (filterObj.roles != null) {
                                    $('#roleFilter').val(filterObj.roles.split(',')).trigger('change');
                                    $('#roleFilterInline').val(filterObj.roles.split(',')).trigger('change');
                                }
                            }

                            if ($("#imageFilter")[0] != null) {
                                $("#imageFilter").val(filterObj.hasimage).trigger('change');
                                $("#imageFilterInline").val(filterObj.hasimage).trigger('change');
                            }

                            if ($("#videoFilter")[0] != null) {
                                $("#videoFilter").val(filterObj.hasvideo).trigger('change');
                                $("#videoFilterInline").val(filterObj.hasvideo).trigger('change');
                            }

                            $("#recurrenceFilter").val(filterObj.recurrences).trigger('change');
                            $("#recurrenceFilterInline").val(filterObj.recurrences).trigger('change');

                            if ($("#workinstructionTypeFilter")[0] != null) {
                                $("#workinstructionTypeFilter").val(filterObj.hasvideo).trigger('change');
                                $("#workinstructionTypeFilterInline").val(filterObj.hasvideo).trigger('change');
                            }

                            if ($("#tagsfilter")[0] != null && filterObj.tags != null && filterObj.tags.length) {
                                $('[id^="filter-tag-"]').each(function (index, elem) {
                                    if (filterObj.tags.includes(+$(elem).data('tag'))) {
                                        $(elem).show();
                                        toggleStyle(+$(elem).data('tag'));
                                    }
                                    
                                });
                            }

                            if (areaid != undefined && areaid !== 0) {
                                //open area tree sections
                                var level = $('#areatree li a[data-id="' + filterObj.areaid + '"]').parent('li').data('level') === undefined ? 0 : $('#areatree li a[data-id="' + filterObj.areaid + '"]').parent('li').data('level');
                                var pid = $('#areatree a[data-id="' + filterObj.areaid + '"]');
                                areaFilter.selectAreaNode(pid[0]);

                                for (i = 0; i < level; i++) {
                                    pid = areaFilter.nextSibling(pid);
                                }

                                //make areatree parent li's child anchors bold
                                var areaElem = $('#areatree a[data-id="' + filterObj.areaid + '"]');
                                var areaElemParentLi = areaElem.closest('ul').closest('li');
                                while (areaElemParentLi.length) {
                                    $(areaElemParentLi.children('a')[0]).css("font-weight", "700");
                                    areaElemParentLi = areaElemParentLi.closest('ul').closest('li');
                                }

                                //open inline area tree sections
                                var inlineLevel = $('#inlineAreaTree li a[data-id="' + filterObj.areaid + '"]').parent('li').data('level') === undefined ? 0 : $('#inlineAreaTree li a[data-id="' + filterObj.areaid + '"]').parent('li').data('level');
                                var inlinePid = $('#inlineAreaTree a[data-id="' + filterObj.areaid + '"]');

                                areaFilter.selectAreaNode(inlinePid[0]);
                                for (i = 0; i < inlineLevel; i++) {
                                    inlinePid = areaFilter.nextSibling(inlinePid);
                                }

                                //make inline area tree parent li's child anchors bold
                                var inlineAreaElem = $('#inlineAreaTree a[data-id="' + filterObj.areaid + '"]');
                                var inlineAreaElemParentLi = inlineAreaElem.closest('ul').closest('li');
                                while (inlineAreaElemParentLi.length) {
                                    $(inlineAreaElemParentLi.children('a')[0]).css("font-weight", "700");
                                    inlineAreaElemParentLi = inlineAreaElemParentLi.closest('ul').closest('li');
                                }
                            }

                            areaFilter.doFilter('block');
                        }
                        else
                        {
                            localStorage.removeItem(areaFilter._listtype + '_filters');
                        }
                    }
                }

                areaFilter.initialized = true;
            }
        });
    },

    nextSibling: function (element) {
        element.parent('li').closest('ul').show();
        return element.parent('li').closest('ul');
    },

    selectAreaNode: function (el) {
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
            var child = areaFilter.subTree(area);
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
            var child = areaFilter.subTreeInline(area);
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
        $("#instructionFilter").val('').trigger('change');
        $("#roleFilter").val('').trigger('change');
        $("#imageFilter").val('').trigger('change');
        $("#videoFilter").val('').trigger('change');
        $("#recurrenceFilter").val('').trigger('change');
        $('#assessorFilter').val('').trigger('change');
        $('#assesseeFilter').val('').trigger('change');
        $('#templateFilter').val('').trigger('change');
        $('#workinstructionTypeFilter').val('').trigger('change');
        
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
        $("#instructionFilterInline").val('').trigger('change');
        $("#roleFilterInline").val('').trigger('change');
        $("#imageFilterInline").val('').trigger('change');
        $("#videoFilterInline").val('').trigger('change');
        $("#recurrenceFilterInline").val('').trigger('change');
        $('#assessorFilterInline').val('').trigger('change');
        $('#assesseeFilterInline').val('').trigger('change');
        $('#workinstructionTypeFilterInline').val('').trigger('change');

        var tree = $('#inlineAreaTree');
        tree.find('a').css('font-weight', '400');
        tree.find('ul:not(:first)').hide();

        areaFilter.doFilter(displaytype);
        localStorage.removeItem(areaFilter._listtype + '_filters');
    },

    doFilter: function(displaytype) {
        var hasimage = $('#imageFilter').val();
        var hasvideo = $('#videoFilter').val();
        var hasinstruction = $('#instructionFilter').val();
        var workinstructiontype = $('#workinstructionTypeFilter').val();
        var tags = [];

        $('[id^="filter-tag-"]').each(function (index, elem) {
            if ($(elem).css("display") !== "none") {
                tags.push($(elem).data('tag'));
            }
        });

        var recurrences;

        var roles;

        var arr = $('#roleFilter').val();
        if (arr !== undefined) {
            roles = arr.join(",");
        }

        var arr_recurrence = $('#recurrenceFilter').val();
        if (arr_recurrence !== undefined) {
            recurrences = arr_recurrence.join(",");
        }

        var filterObj = {
            hasimage: hasimage,
            hasvideo: hasvideo,
            hasinstruction: hasinstruction,
            roles: roles,
            recurrences: recurrences,
            areaid: areaid !== undefined ? areaid.toString() : areaid,
            listtype: areaFilter._listtype,
            tags: tags,
            workinstructiontype: workinstructiontype
        }

        localStorage.setItem(areaFilter._listtype + '_filters', JSON.stringify(filterObj));

        $("#contentCol div.results").show();

        if (roles) {
            areaFilter.filterInRole(roles);
        }

        if (hasimage) {
            areaFilter.filterHasImages(hasimage);
        }

        if (hasvideo) {
            areaFilter.filterHasVideos(hasvideo);
        }

        if (hasinstruction) {
            areaFilter.filterHasInstructions(hasinstruction);
        }

        if (recurrences) {
            areaFilter.filterInRecurrence(recurrences);
        }

        if (workinstructiontype != null && (workinstructiontype == '0' || workinstructiontype == '1')) {
            areaFilter.filterInWorkInstructionType(workinstructiontype);
        }
        else if ($('#workinstructionTypeFilter').length || $('#workinstructionTypeFilterInline').length && $('#cardHeader').length) { //WI filter present but no option selected
            $('#cardHeader').html(areaFilter.language.workAndAssessmentInstructions);
            $('#cardHeader').append('<span id="counter"></span>');
        }

        if (tags != null && tags.length) {
            areaFilter.filterInTags(tags);
        }

        if (areaid != undefined && areaid !== 0) {
            areaFilter.filterInArea();
        }

        $("#contentCol").trigger('filterDone');
        ezgomediafetcher.preloadVisibleImagesAndVideos();
    },

    filterHasInstructions: function(value) {
        $("#contentCol div.results:visible").each(function (index, item) {
            var card = $(item);
            var hasInstructions = String(card.data('instructions')) !== '0';
            if (String(hasInstructions) === value) {
                var id = card.data('id')
                //if ($.inArray(id, filterResult) === -1) filterResult.push(id);
            }
            else {
                card.hide();
            }
        });
    },

    filterHasImages: function(value) {
        $("#contentCol div.results:visible").each(function (index, item) {
            var card = $(item);
            var hasImage = card.data('hasimage');
            var hasInstructions = card.data('hasimage') == '0';
            if (String(hasImage) === value) {
                var id = card.data('id')
            }
            else {
                card.hide();
            }
        });
    },

    filterHasVideos: function (value) {
        $("#contentCol div.results:visible").each(function (index, item) {
            var card = $(item);
            var hasVideo = card.data('hasvideo');
            var hasInstructions = card.data('hasimage') == '0';
            if (String(hasVideo) === value) {
                var id = card.data('id')
            }
            else {
                card.hide();
            }
        });
    },

    filterInRole: function(value) {

        $("#contentCol div.results").hide();
        if (value) {
            var roles = String(value).split(',');
            $(roles).each(function (index, role) {

                $("#contentCol div.results").each(function (index, item) {
                    var card = $(item);
                    var inRole = card.data('role') === role;

                    if (inRole) {
                        var id = card.data('id')
                        if ($.inArray(id, areaFilter.filterResult) === -1) areaFilter.filterResult.push(id);
                    }
                });
            });
            areaFilter.renderFilterResults();
        }
    },

    filterInRecurrence: function (value) {

        if (value) {
            var recurrences = String(value).split(',');
            $("#contentCol div.results:visible").each(function (index, item) {
                var card = $(item);
                var inRecurrence = recurrences.includes(card.data('recurrence'));

                if (!inRecurrence) {
                    card.hide();
                }
            });

        }
    },

    filterInWorkInstructionType: function (value) {
        if (value != null) {
            $("#contentCol div.results:visible").each(function (index, item) {
                var card = $(item);
                var inType = card.data('workinstructiontype').toString() === value;

                if (!inType) {
                    card.hide();
                }
            });
            if (value == '0' && $('#cardHeader').length) { //basic instruction
                $('#cardHeader').html(areaFilter.language.workInstructions);
                $('#cardHeader').append('<span id="counter"></span>');
            }
            else if (value == '1' && $('#cardHeader').length) { //skill instruction
                $('#cardHeader').html(areaFilter.language.assessmentInstructions);
                $('#cardHeader').append('<span id="counter"></span>');
            }
        }
    },

    filterInArea: function() {

        var area = areaid;
        var level = arealevel;

        if (area === 0) {
            return;
        }

        $("#contentCol div.results:visible").each(function (index, item) {
            var card = $(item);

            var ids = String(card.data('pathids')).split(',');
            if (ids[0] === 'undefined') {
                ids[0] = area;
            }
            if (parseInt(ids[level]) !== parseInt(area)) {
                card.hide();
            }
        });

    },

    filterInTags: function (selectedTags) {
        $("#contentCol div.results:visible").filter(function (index, elem) {
            var card = $(elem);
            var tags = String(card.data('tags')).split(',');
            if (!tags.some(item => selectedTags.includes(+item))) {
                card.hide();
            }
        });
    },

    renderFilterResults: function() {
        $("#contentCol div.results").hide();
        $(areaFilter.filterResult).each(function (index, id) {
            var id = areaFilter.filterResult[index];
            $('#contentCol [data-id="' + id + '"]:first').show();
        });

        areaFilter.filterResult = [];
    }


}
