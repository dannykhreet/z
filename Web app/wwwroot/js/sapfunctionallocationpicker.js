var functionalLocationPicker = {
    locationRequestTimeout: null,
    lastLocationRequest: null,
    functionalLocationInfo: null,
    selectedFunctionalLocationId: null,
    savedFunctionalLocationInfo: null,

    init: function () {
        $('#modalSelectFunctionalLocation').on('show.bs.modal', function () {
            functionalLocationPicker.resetSelectedFunctionalLocation();
            if (functionalLocationPicker.savedFunctionalLocationInfo?.functionalLocationId && $('#functionalLocation-' + functionalLocationPicker.savedFunctionalLocationInfo.functionalLocationId).length > 0) {
                $('#functionalLocation-' + functionalLocationPicker.savedFunctionalLocationInfo.functionalLocationId).find('.functionalLocationSelectElement').click();
            }
        });

        $('#functionalLocationSearchBox').on('keyup', function (e) {
            $('#functionalLocationMainList').html(functionalLocationsTableHeader);

            if (functionalLocationPicker.locationRequestTimeout != null) {
                clearTimeout(functionalLocationPicker.locationRequestTimeout);
            }
            if (functionalLocationPicker.lastLocationRequest != null) {
                functionalLocationPicker.lastLocationRequest.abort();
            }

            functionalLocationPicker.resetSelectedFunctionalLocation();

            let functionalLocationSearchValue = $(this).val().toLowerCase().trim();

            var queryParams = [];
            var funcLocationBaseUrl = '';

            if (functionalLocationSearchValue != undefined && functionalLocationSearchValue != null && functionalLocationSearchValue != '' && functionalLocationSearchValue.length > 2) {
                queryParams.push('filterText=' + functionalLocationSearchValue);
                funcLocationBaseUrl = '/config/functionallocations/search';
            }
            else {
                queryParams.push('parentLevel=' + 0);
                funcLocationBaseUrl = '/config/functionallocations';
            }

            if (functionalLocationSearchValue.length < 3) {
                $('#functionalLocationMainList').html('Please enter at least 3 characters to search. Showing root functional locations...');
            }

            var funcLocationRequestUrl = funcLocationBaseUrl + (queryParams.length ? '?' + queryParams.join('&') : '');

            //loading image from completed audits
            $('#functionalLocationMainList').append(functionalLocationLoader);
            functionalLocationPicker.locationRequestTimeout = setTimeout(() => {
                functionalLocationPicker.lastLocationRequest = $.ajax({
                    type: "GET",
                    url: funcLocationRequestUrl,
                    success: function (data) {
                        $('#functionalLocationMainList').html(functionalLocationsTableHeader);
                        $('#functionalLocationMainList').append(data);
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        if (jqXHR.statusText == "abort") {
                            return;
                        }
                        else {
                            toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                        }
                    },
                    contentType: "application/json; charset=utf-8"
                });
            }, 500);
        });

        //move this and use proper css
        var firstBorderElementToFix = $('.borderRightElement').first();
        var lastBorderElementToFix = $('.borderRightElement').last();
        if (firstBorderElementToFix.length > 0) {
            $('.borderRightElement').first().css('border-image', 'linear-gradient(to bottom, rgba(0,0,0,0) 5px,rgba(255,255,255,1) 25%,rgba(255,255,255,1) 100%)');
            $('.borderRightElement').first().css('border-image-slice', '1');
        }
        if (lastBorderElementToFix.length > 0) {
            $('.borderRightElement').last().css('border-image', 'linear-gradient(to top, rgba(0,0,0,0) 5px,rgba(255,255,255,1) 25%,rgba(255,255,255,1) 100%)');
            $('.borderRightElement').last().css('border-image-slice', '1');
        }

        $('#functionalLocationMainList').on('click', '.functionalLocationSelectElement', functionalLocationPicker.selectFunctionalLocation);
        $('#functionalLocationMainList').on('click', '.functionalLocationExpand', functionalLocationPicker.expandFunctionalLocation);
    },

    resetSelectedFunctionalLocation: function () {
        $('.functionalLocation').each(function (index, elem) {
            $(elem).find('.functionalLocationSelectElement').removeClass('locationSelected');
        });
        functionalLocationPicker.functionalLocationInfo = null;
        $('#modal_select_functional_location_save_button').attr('disabled', '');

        functionalLocationPicker.selectedFunctionalLocationId = null;
    },

    selectFunctionalLocation: function (e) {
        if ($(e.target).hasClass('functionalLocationExpand') || $(e.target).is('i') || $(e.currentTarget).hasClass('locationSelectionDisabled')) {
            return;
        }
        else if ($(this).length > 0) {
            let functionalLocationSelected = $(this).hasClass('locationSelected');
            $('.functionalLocation').each(function (index, elem) {
                $(elem).find('.functionalLocationSelectElement').removeClass('locationSelected');
            });

            if (!functionalLocationSelected) {
                let functionalLocationName = $(this).closest('[id^="functionalLocation-"]').data('functionallocationname');
                let functionalLocation = $(this).closest('[id^="functionalLocation-"]').data('functionallocation');
                let functionalLocationId = $(this).closest('[id^="functionalLocation-"]').data('id');
                $(this).addClass('locationSelected');
                functionalLocationPicker.functionalLocationInfo = {
                    functionalLocationName: functionalLocationName,
                    functionalLocation: functionalLocation,
                    functionalLocationId: functionalLocationId
                }
                $('#modal_select_functional_location_save_button').removeAttr('disabled');
            }
            else {
                functionalLocationPicker.resetSelectedFunctionalLocation();
            }
        }
    },

    saveSelectedFunctionalLocationId: function () {
        functionalLocationPicker.selectedFunctionalLocationId = functionalLocationPicker.functionalLocationInfo.functionalLocationId;
        functionalLocationPicker.savedFunctionalLocationInfo = functionalLocationPicker.functionalLocationInfo;
        $('#modalSelectFunctionalLocation').modal('hide');
    },

    resetSelectedFunctionalLocationId: function () {
        functionalLocationPicker.selectedFunctionalLocationId = null;
        functionalLocationPicker.savedFunctionalLocationInfo = null;
        $('#modalSelectFunctionalLocation').trigger('hidden.bs.modal');
    },

    expandFunctionalLocation: function (e) {
        if ($(e.currentTarget).closest('.functionalLocation').length > 0) {
            let parentElement = $(e.currentTarget).closest('.functionalLocation')[0];
            let functionalLocationId = $(e.currentTarget).closest('.functionalLocation').data('id');
            let functionalLocationIndentationLevel = $(e.currentTarget).closest('.functionalLocation').data('indentationlevel');

            if ($(parentElement).data('childrenloaded') != true) {
                parentElement.insertAdjacentHTML("beforeend", functionalLocationLoader);

                var queryParams = [];

                if (functionalLocationId != undefined && functionalLocationId != null && functionalLocationId != '') {
                    queryParams.push('functionalLocationId=' + functionalLocationId);
                }

                if (functionalLocationIndentationLevel != undefined && functionalLocationIndentationLevel != null && functionalLocationIndentationLevel != '') {
                    queryParams.push('parentLevel=' + functionalLocationIndentationLevel);
                }

                if (functionalLocationPicker.locationRequestTimeout != null) {
                    clearTimeout(functionalLocationPicker.locationRequestTimeout);
                }
                if (functionalLocationPicker.lastLocationRequest != null) {
                    functionalLocationPicker.lastLocationRequest.abort();
                }

                functionalLocationPicker.locationRequestTimeout = setTimeout(() => {
                    functionalLocationPicker.lastLocationRequest = $.ajax({
                        type: "GET",
                        url: '/config/functionallocations' + (queryParams.length ? '?' + queryParams.join('&') : ''),
                        success: function (data) {
                            $('.functionalLocationLoader').remove();
                            $(parentElement).find('.functionalLocationExpand').html('<i class="fa-solid fa-caret-down"></i>')
                            parentElement.insertAdjacentHTML("beforeend", data);
                            $(parentElement).data('childrenloaded', true);
                            $(parentElement).data('expanded', true);
                        },
                        error: function (jqXHR, textStatus, errorThrown) {
                            if (jqXHR.statusText == "abort") {
                                return;
                            }
                            else {
                                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                            }
                        },
                        contentType: "application/json; charset=utf-8"
                    });
                }, 500);
            }
            else {
                if ($(parentElement).data('expanded') == true) {
                    //hide
                    $(parentElement).find('ul').first().hide();
                    $(parentElement).data('expanded', false);
                    $(parentElement).find('.functionalLocationExpand').first().html('<i class="fa-solid fa-caret-right"></i>');
                }
                else {
                    //show
                    $(parentElement).find('ul').first().show();
                    $(parentElement).data('expanded', true);
                    $(parentElement).find('.functionalLocationExpand').first().html('<i class="fa-solid fa-caret-down"></i>');
                }
            }
        }
    }
}