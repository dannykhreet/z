var ezgoprops = {
    propertiesEnabled: true,
    language: {
        addproperty: 'Add property',
    },
    proptype: 'task',
    templateid: 0,
    data: null,
    properties: new Array(),
    _template: undefined,
    _value1: undefined,
    _value2: undefined,
    _propid: undefined,
    _propvalueid: undefined,
    _displayvalue: undefined,
    _displayfooter: undefined,
    _propertyStructureVersion: 1,

    init: function (params) {
        ezgoprops.proptype = params.proptype;
        ezgoprops.templateid = params.templateid;
        if (params !== undefined && params != null && params.propertiesEnabled !== undefined && params.propertiesEnabled != null) {
            ezgoprops.propertiesEnabled = params.propertiesEnabled;
            ezgoprops._propertyStructureVersion = params.propertyStructureVersion;
            if (!ezgoprops.propertiesEnabled) {
                ezgoprops.disablePropertyAdd();
            }
        }
        // Set property limit from settings (default to 5 if not provided)
        if (params !== undefined && params != null && params.propertyLimit !== undefined && params.propertyLimit != null) {
            ezgoprops.propertyLimit = params.propertyLimit;
        } else {
            ezgoprops.propertyLimit = 5;
        }
        ezgoprops.load();
    },

    disablePropertyAdd: function () {
        $('#ph-1').parent().hide();
    },

    load: function () {
        $.get('/config/properties', function (data) {
            ezgoprops.data = JSON.parse(data);
            ezgoprops.render();
            ezgoprops.loadTemplateProps();

            $('[data-selector="valuesymbol"]').on('click', 'a', function () {
                ezgoprops._propvalueid = $(this).attr('data-pvid');
                $(this).parent().parent().find('[data-element="symbolbutton"]:eq(1)').html($(this).html()).attr('data-pvid', ezgoprops._propvalueid);
                $(this).closest('[role="tabpanel"]').find('[data-element="symbolbutton"]:eq(0)').html($(this).html()).attr('data-pvid', ezgoprops._propvalueid);
                $(this).closest('[role="tabpanel"]').find('[data-element="symbolbutton"]:eq(1)').html($(this).html()).attr('data-pvid', ezgoprops._propvalueid);
            });

            $('[data-selector="addbutton"]').on('click', function (e) {
                ezgoprops._setProperty(e);
            });

            $('input[id^="element_"]').blur(function (event) {
                var isvalid = event.target.checkValidity();
                if (isvalid) {
                    $(event.target).css('border-color', '#ced4da');
                }
            }).bind('invalid', function (event) {
                setTimeout(function () {
                    $(event.target).focus();
                    $(event.target).css('border-color', 'red');
                    if (!$(event.target).attr('id').includes('_singlevalue')) {
                        var buttonidpart = $(event.target).attr('id').replaceAll('element_', '').replaceAll('_first', '').replaceAll('_second', '');
                        $('#addPropertyButton-' + buttonidpart)[0].disabled = true;
                    }
                }, 50);
            });

            $('#valueProps').on('click', '.x-btn', function (e) {
                e.stopPropagation();
                var propindex = parseInt($(e.currentTarget).closest('li').data('index'));
                ezgoprops.deleteProperty(propindex);
            })

            $('#valueProps').on('click', 'li[data-selector="property"]', function (e) {
                var index = parseInt($(e.currentTarget).data('index'));
                $('#valueProps li').removeClass('active');
                if (e !== undefined) {
                    var len = index * (e.currentTarget.offsetWidth + 15);
                    var left = (len - (e.currentTarget.offsetWidth + 15) / 2) - 28;
                    var propertyid = parseInt($(e.currentTarget).data('propid'));

                    if ($('#valuePanel').hasClass('.active')) {
                        $('#valuePanel').hide();
                    }
                }

                $('#valuePanel').attr('data-selected', index);
            });
        });
    },

    loadTemplateProps: function (template) {
        var index = 0;
        if (!template) {
            return;
        }

        this._template = template;
        $('#valuePanel').hide();
        $('#valueProps').empty();

        $(template.Properties).each(function (index, prop) {
            var value = '<div>' + prop.PrimaryDecimalValue + '-' + prop.SecondaryDecimalValue + '</div>';
            switch (prop.FieldType) {
                case 1:
                case 3:
                case 4:
                case 5:
                    var val = ezgoprops.getValues(prop);
                    value = '<div>' + ((prop.ValueType == 1 || prop.ValueType == 0) ? (isNaN(val.value1) || val.value1 == null ? '' : val.value1) : val.value1) + '</div>';
                    break;
                case 2:
                    var val = ezgoprops.getValues(prop);
                    value = '<div>' + ((prop.ValueType == 1 || prop.ValueType == 0) ? ((isNaN(val.value1) || val.value1 == null ? '' : val.value1) + '-' + (isNaN(val.value2) || val.value2 == null ? '' : val.value2)) : (val.value1 + '-' + val.value2)) + '</div>';
                    break;
            }

            if (prop.Property.PropertyValueKind !== undefined) {
                var pv = prop.Property.PropertyValueKind.PropertyValues.filter(function (propertyValue) {
                    return propertyValue.Id === prop.PropertyValueId;
                })[0];
            }

            var title = prop.TitleDisplay ? prop.TitleDisplay : prop.Property.ShortName;
            var subtitle = prop.PropertyValueDisplay ? prop.PropertyValueDisplay : ((pv !== undefined) ? pv.ValueSymbol : '');
            var footer = '<div>' + ((pv !== undefined) ? pv.ValueSymbol : '') + '</div>';
            var li;
            var requiredAsterisk = "";

            if (prop.IsRequired === true) {
                requiredAsterisk = '<i class="fa-solid fa-asterisk m-1" style="color: red; position: absolute; top: 0px;"></i>';
            }

            li = $('<li class="shadow-sm" data-selector="property" data-index="' + index + '" data-propid="' + prop.Property.Id + '"><div class="bigbutton"><div>' + title + '</div>' + value + '<div>' + subtitle + '</div>' + requiredAsterisk + '<div class="x-btn"><i class="fa fa-trash"></i></div>' + '<div class="edit-circle" onclick="showEditPropertyModal(' + prop.Id + ')"><i class="fa-solid fa-pencil" style="margin-top: 6px;"></i></div></div></li>');
            
            $('#valueProps').append(li);
            index++;
        });

        if ($('#valueProps li').length < ezgoprops.propertyLimit) {
            $('#valueProps').append(ezgoprops._renderAddButton());
        }
    },

    getValues: function (prop) {
        var output = {
            value1: undefined,
            value2: undefined
        };

        //    Integer = 0,
        //    Decimal = 1,
        //    String = 2,
        //    Date = 3,
        //    Time = 4,
        //    DateTime = 5,
        //    Boolean = 6

        var isRange = prop.FieldType == 2;
        switch (prop.ValueType) {
            case 0:
                output.value1 = prop.PrimaryIntValue;
                output.value2 = isRange ? prop.SecondaryIntValue : undefined;
                break;
            case 1:
                output.value1 = prop.PrimaryDecimalValue;
                output.value2 = isRange ? prop.SecondaryDecimalValue : undefined;
                break;
            case 2:
                output.value1 = prop.PrimaryStringValue;
                output.value2 = isRange ? prop.SecondaryStringValue : undefined;
                break;
            case 3:
                var current_date = new Date(prop.PrimaryDateTimeValue);
                var formatted_date = ezgoprops._appendLeadingZero(current_date.getDate()) + "-" + ezgoprops._appendLeadingZero(current_date.getMonth() + 1) + "-" + current_date.getFullYear();
                output.value1 = current_date.getUTCFullYear() !== 1970 && prop.PrimaryDateTimeValue !== undefined ? formatted_date : '';
                output.value2 = isRange ? prop.SecondaryDateTimeValue : undefined;
                break;
            case 4:
                output.value1 = prop.PrimaryTimeValue;
                output.value2 = isRange ? prop.SecondaryTimeValue : undefined;
                break;
            case 5:
                var current_datetimevalue = new Date(prop.PrimaryDateTimeValue);
                var formatted_datetimevalue = ezgoprops._appendLeadingZero(current_datetimevalue.getDate()) + "-" + ezgoprops._appendLeadingZero(current_datetimevalue.getMonth() + 1) + "-" + current_datetimevalue.getFullYear() + " " + ezgoprops._appendLeadingZero(current_datetimevalue.getHours()) + ":" + ezgoprops._appendLeadingZero(current_datetimevalue.getMinutes());
                output.value1 = current_datetimevalue.getUTCFullYear() !== 1970 && prop.PrimaryDateTimeValue !== undefined ? formatted_datetimevalue : '';
                output.value2 = isRange ? prop.SecondaryDateTimeValue : undefined;
                break;
            case 6:
                output.value1 = prop.PrimaryBoolValue;
                output.value2 = isRange ? prop.SecondaryBoolValue : undefined;
                break;
            default:
                output.value1 = prop.PrimaryStringValue;
                output.value2 = isRange ? prop.SecondaryStringValue : undefined;
                break;
        }
        return output;
    },

    render: function () {
        var app = ezgoprops._renderAppContainer();
        $('#app').html(app);
        let valuetypeid = 0;
        $(this.data).each(function (index, property) {
            var content = ezgoprops._renderContentContainer(
                {
                    isRange: property.FieldType === 2,
                    valueType: property.ValueType,
                    property: property,
                });

            var displayControls = ezgoprops._renderDisplayControls();
            var prop = $('<a id="v-pills-tab-' + property.Id + '" href="#v-pills-' + property.Id + '-' + property.FieldType + '">' + property.Name.replaceAll('(~)', 'range') + '</a>')
                .attr('role', 'tab')
                .attr('data-bs-toggle', 'pill')
                .attr('area-selected', 'false')
                .attr('data-propid', property.Id)
                .addClass('nav-link');

            if (index === 0) {
                prop.addClass('active');
            }

            $('#props').append(prop);

            var panel = $('<div id="v-pills-' + property.Id + '-' + property.FieldType + '" role="tabpanel" aria-labelledby="#v-pills-tab-' + property.Id + '"><h4>' + property.Name.replace('(~)', 'range').replace('(>)', 'larger than').replace('(<)', 'smaller than') + '</h4><span>' + property.Description + '</span></div>').addClass('tab-pane fade text-muted')
                .attr('data-pid', property.Id)
                .attr('data-valuetype', property.ValueType)
                .attr('data-fieldtype', property.FieldType);

            if (index === 0) {
                panel.addClass('show active');
            }

            displayControls = displayControls.replaceAll('{{changedisplay}}', 'displayControl' + property.Id)
            content = content.replaceAll('{{controls}}', displayControls);

            switch (property.FieldType) {
                case 1:
                case 3:
                case 4:
                case 5:
                    content = content
                        .replaceAll('{{fromelement}}', ezgoprops._renderInputElement({
                            valueType: property.ValueType,
                            fieldType: property.FieldType,
                            elementId: 'element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_singlevalue',
                            dataElement: 'fromelement',
                            disabled: false
                        }));
                    break;

                case 2:
                    content = content
                        .replaceAll('{{fromelement}}', ezgoprops._renderInputElement({
                            valueType: property.ValueType,
                            fieldType: property.FieldType,
                            elementId: 'element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_first',
                            dataElement: 'fromelement',
                            disabled: true
                        }))
                        .replaceAll('{{toelement}}', ezgoprops._renderInputElement({
                            valueType: property.ValueType,
                            fieldType: property.FieldType,
                            elementId: 'element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_second',
                            dataElement: 'toelement',
                            disabled: true
                        }));
                    break;

                default:
                    break;
            }
            panel.append(content);

            $('#props-tabcontent').append(panel);

            if ($('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_first').length && $('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_second').length) {
                $('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_first').off('input').on('input', function () {
                    if ($(this).val() != '' && $('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_second').val() != '') {
                        if (Number($(this).val()) < Number($('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_second').val())) {
                            $('#addPropertyButton-' + property.Id + '_' + property.ValueType + '_' + property.FieldType).removeAttr('disabled', '');
                            $(this).css('border-color', '#ced4da');
                        }
                        else {
                            $('#addPropertyButton-' + property.Id + '_' + property.ValueType + '_' + property.FieldType)[0].disabled = true;;
                            $(this).css('border-color', 'red');
                        }
                    }
                });

                $('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_second').off('input').on('input', function () {
                    if ($(this).val() != '' && $('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_first').val() != '') {
                        if (Number($(this).val()) > Number($('#element_' + property.Id + '_' + property.ValueType + '_' + property.FieldType + '_first').val())) {
                            $('#addPropertyButton-' + property.Id + '_' + property.ValueType + '_' + property.FieldType).removeAttr('disabled', '');
                            $(this).css('border-color', '#ced4da');
                        }
                        else {
                            $('#addPropertyButton-' + property.Id + '_' + property.ValueType + '_' + property.FieldType)[0].disabled = true;
                            $(this).css('border-color', 'red');
                        }
                    }
                });
            }
        });
    },

    toggleValuePanel: function (e) {
        if (e !== undefined) {
            var len = $('#valueProps li').length * (e.offsetWidth + 17);
            var left = (len - (e.offsetWidth + 15) / 2) - 28;
        }

        $('#valuePanel').attr('data-selected', 0);
        $('#valuePanel .arrow').css('left', left + 'px');
        $('#props a:first').click();
        $('#valuePanel').toggle();
    },

    deleteProperty: function (propindex) {
        ezgoprops._template.Properties.splice(propindex, 1);
        ezgoprops.loadTemplateProps(ezgoprops._template);
    },

    showValuePropPanel: function () {
        $('#valuePanel').removeClass('d-none');
    },

    hideValuePropPanel: function () {
        $('#valuePanel').addClass('d-none');
    },

    _renderInputElement: function (params) {
        let required = (params.fieldType == 0 || params.fieldType == 1) ? '' : ' required ';
        var el_int = '<input id="' + params.elementId + '" type="number" step="1" max="2147483647" class="form-control" data-selector="input" data-element="' + params.dataElement + '" onkeypress="return event.charCode >= 48 && event.charCode <= 57" ' + required + ' />';
        var el_dec = '<input id="' + params.elementId + '" type="number" step=".01" max="999999999999" class="form-control" data-selector="input" data-element="' + params.dataElement + '"  ' + required + '/>';
        var el_date = '<input id="' + params.elementId + '" type="date" class="form-control" data-selector="input" data-element="' + params.dataElement + '"  ' + required + '/>';
        var el_datetime = '<input id="' + params.elementId + '" type="datetime-local" class="form-control" data-selector="input" data-element="' + params.dataElement + '"  ' + required + '/>';
        var el_time = '<input id="' + params.elementId + '" type="time" class="form-control" data-selector="input" data-element="' + params.dataElement + '"  ' + required + '/>';
        var el_string = '<input type="text" maxlength="26" class="form-control" data-selector="input" data-element="' + params.dataElement + '"  ' + required + '/>';

        switch (params.valueType) {
            case 0:
                return el_int;
            case 1:
                return el_dec;
            case 2:
                return el_string;
            case 3:
                return el_date;
            case 4:
                return el_time;
            case 5:
                return el_datetime;
            case 6:
                var el = $('<div class="input-group"/>');
                var prependEl = $('<div class="input-group-prepend" />');
                var group = $('<div class="input-group-text">');
                group.append('<input type="radio" name="radioBool" aria-label="Radio button for following text input">');
                prependEl.append(group);
                el.append(prependEl);
                el.append('<input type="text" id="' + params.elementId + '" class="form-control" placeholder="' + params.placeHolder + '" aria-label="Text input with radio button">');
                return el.html();
            default:
        }
    },

    _renderInputGroup: function (property) {
        var filteredProp = (property.PropertyValueKind !== undefined) ? property.PropertyValueKind.PropertyValues.filter(function (prop) {
            return prop.Id === property.PropertyValueId;
        }) : [];

        var inputgroup = $('<div/>').addClass('input-group-append').attr('data-selector', 'valuesymbol');
        var button = $('<button/>').addClass('btn btn-outline-secondary1 btn-ezgo dropdown-toggle').attr('type', 'button').attr('data-bs-toggle', 'dropdown').attr('data-pvid', filteredProp.length > 0 ? filteredProp[0].Id : '').attr('data-element', 'symbolbutton').html(filteredProp.length > 0 ? filteredProp[0].ValueSymbol : '');
        var dropdown = $('<div/>').addClass('dropdown-menu');
        var singleAppend = $('<div><span class="input-group-text">' + ((property.PropertyValue !== undefined && property.PropertyValue.ValueSymbol) ? property.PropertyValue.ValueSymbol : '') + '</span></div>').attr('data-element', 'symbolbutton').attr('data-pvid', filteredProp.length > 0 ? filteredProp[0].Id : '').addClass('input-group-append');

        if (property.PropertyValueKind !== undefined) {
            $(property.PropertyValueKind.PropertyValues).each(function (index, pValues) {
                var item = $('<a/>').html(pValues.ValueSymbol).addClass('dropdown-item')
                    .attr('data-pvid', pValues.Id);
                dropdown.append(item);
            });
        };

        if ((property.ValueType === 0 || property.ValueType === 1) && property.PropertyValueKind !== undefined && property.PropertyValueKind.PropertyValues.length > 1) {
            return inputgroup.append(button).append(dropdown);
        }
        else {
            return singleAppend;
        }
    },

    _renderContentContainer: function (params) {
        var output = $('<div/>');
        var el = $('<div />').addClass('form-row pt-2');
        var title = $('<small>Define default value for ' + params.property.Name.toLowerCase() + '</small>');
        var group = $('<div />').addClass('form-group form-group-sm col-md-6');
        var inputGroup = $('<div><span class="input-group-text">' + ((params.property.PropertyValue !== undefined && params.property.PropertyValue.ValueSymbol) ? params.property.PropertyValue.ValueSymbol : '') + '</span></div>').addClass('input-group-append');
        inputGroup = ezgoprops._renderInputGroup(params.property);
        var inputGroupContainer = $('<div />').append('{{element}}').addClass('input-group input-group-sm');
        var addButton = $('<div/>').addClass('pt-2').append($('<button id="addPropertyButton-' + params.property.Id + '_' + params.property.ValueType + '_' + params.property.FieldType + '">' + ezgoprops.language.addproperty + '</button>').attr('data-selector', 'addbutton').addClass('btn btn-ezgo btn-sm'));

        if (params.valueType !== 2 && params.valueType !== 6) {
            inputGroupContainer.append(inputGroup);
        }

        var valueTypeStringVisibleStyle = 'style="display:none"';
        var valueTypeDateTimeVisibleStyle = 'style="display:none"';
        var valuetypeSelector = ezgoprops._propertyStructureVersion == 2 && params.property.PropertyGroupId !== 1 ? '<div class="form-row pt-2"><div class="form-group col-md-6"><small>User value type</small><div class="input-group"><select data-selector="value_type" class="form-control form-control-sm" style="width:100px;"><option value="1" title="Number with decimals">Number with decimals</option><option value="0" title="Full numbers">Number without decimals</option><option value="2" title="Text" ' + valueTypeStringVisibleStyle + '>String</option><option value="4" title="Text" ' + valueTypeDateTimeVisibleStyle + '>Time</option><option value="5" title="Text" ' + valueTypeDateTimeVisibleStyle + '>Date time</option></select></div></div></div>' : '';
        var firstGroup = group;
        var firstInputGroup = inputGroupContainer;
        var firstCol = $('<div class="form-group col-md-6" >');
        firstGroup.append(title).append(firstInputGroup);
        firstCol.append(firstGroup.html().replaceAll('{{element}}', '{{fromelement}}'));
        el.append(firstCol);

        if (params.isRange) {
            var secondGroup = group;
            var secondInputGroup = inputGroupContainer;
            var secondCol = $('<div class="form-group col-md-6" >');
            secondGroup.append(title).append(secondInputGroup);
            secondCol.append(secondGroup.html().replaceAll('{{element}}', '{{toelement}}'));
            el.append(secondCol);
        }
        return output.append(el).append(valuetypeSelector).append('{{controls}}').append(addButton).html();
    },

    _renderDisplayControls: function () {
        var controls = $('<div/>');
        var link = $('<a data-bs-toggle="collapse" href="#{{changedisplay}}"><small>change display format</small></a>');
        var display = $('<div id="{{changedisplay}}">').addClass('form-row collapse').attr('role', 'button')
            .append(
                $('<div/>').addClass('form-group col-md-3')
                    .append('<small>display title</small>')
                    .append($('<div class="input-group">')
                        .append('<input type="text" class="form-control form-control-sm" data-selector="displaytitle">')
                    )
            ).append(
                $('<div/>').addClass('form-group col-md-3')
                    .append('<small>display footer</small>')
                    .append($('<div class="input-group">')
                        .append('<input type="text" class="form-control form-control-sm" data-selector="displayfooter">')
                    )
            );

        return controls.append(link).append(display).html();
    },

    _renderAppContainer: function () {
        var container = $('<div/>');
        var row = $('<div/>')
            .append($('<div/>').addClass('row p-0')
                .append(
                    $('<div class="col-4 d-none d-md-block pt-2" style="height: 325px;overflow: auto;border-right: dashed 1px #93C54B;">')
                        .append($('<div id="props" class="nav flex-column nav-pills" role="tablist" aria-orientation="vertical"></div>'))
                )
                .append(
                    $('<div class="col-12 col-md-6">')
                        .append($('<div class="d-sm-block d-md-none pt-1"/>')
                            .append($('<button/>').addClass('"btn btn-ezgo btn-sm btn-block').attr('type', 'button').html('Properties'))
                        )
                        .append($('<div id="props-tabcontent" class="tab-content p-1"></div>'))
                        .append($('<div class="p-1"><div class="custom-control custom-switch"><input type="checkbox" class="custom-control-input" id="isRequired"><label class="custom-control-label tab-content text-muted" style="font-weight: 400 !important;" for="isRequired">make property required</label></div></div>'))
                )
            )
        return container.append(row).html();
    },

    _renderAddButton: function () {
        if (ezgoprops.propertiesEnabled) {
            return $('<li/>').addClass('ui-state-sorting-disabled').append(
                $('<div id="ph-1" style="user-select: none;" data-item="value-ph" onclick="ezgoprops.toggleValuePanel(this)">').addClass('bigbutton-plus d-flex justify-content-center text-center').append(
                    $('<div class="addvalue">')
                        .append('<i class="fa fa-plus-circle"></i>')
                        .append('<div style="line-height:16px;">' + ezgoprops.language.addproperty + '</div>')
                )
            );
        } else { return ''; }
    },

    _setProperty: function (event) {
        var prop = $('#props-tabcontent [role="tabpanel"].active.show');
        var isvalid = prop.find('[data-selector="input"]').length > 1 ? prop.find('[data-selector="input"]')[0].checkValidity() && prop.find('[data-selector="input"]')[1].checkValidity() : prop.find('[data-selector="input"]')[0].checkValidity();

        if (!isvalid) {
            return;
        }

        var propid = prop.data('pid');
        var valuetypeselect = prop.find('[data-selector="value_type"]').val();
        var valuetype = valuetypeselect !== undefined && valuetypeselect !== null ? valuetypeselect : prop.attr('data-valuetype');
        var fieldType = prop.attr('data-fieldtype');
        var propvalue1 = prop.find('[data-selector="input"]:eq(0)').val();
        var propvalue2 = prop.find('[data-selector="input"]:eq(1)').val();
        var propvalueid = prop.find('[data-element="symbolbutton"]').attr('data-pvid');
        var displaytitle = prop.find('[data-selector="displaytitle"]').val();
        var displayfooter = prop.find('[data-selector="displayfooter"]').val();
        var isRequired = document.getElementById('isRequired').checked;
        var propertyDetails = ezgoprops.data.filter(function (propdetails) {
            return propdetails.Id === parseInt(propid);
        });

        if (!ezgoprops._template.Properties) {
            ezgoprops._template.Properties = new Array();
        }

        var propertyItem = {
            TaskTemplateId: ezgoprops._template.Id,
            Id: ezgoprops._template.Properties.length,
            PropertyId: parseInt(propid),
            PropertyValueId: parseInt(propvalueid),
            FieldType: parseInt(fieldType),
            IsRequired: isRequired,
            TitleDisplay: displaytitle ? displaytitle : null,
            PropertyValueDisplay: displayfooter ? displayfooter : null,
            Index: ezgoprops._template.Properties.length,
            Property: propertyDetails[0],
            ValueType: parseInt(valuetype),
            isNew: true
        }

        switch (parseInt(valuetype)) {
            case 0:
                propertyItem.PrimaryIntValue = parseInt(propvalue1);
                propertyItem.SecondaryIntValue = propvalue2 ? parseInt(propvalue2) : null;
                break;

            case 1:
                propertyItem.PrimaryDecimalValue = parseFloat(propvalue1);
                propertyItem.SecondaryDecimalValue = propvalue2 ? parseFloat(propvalue2) : null;
                break;

            case 2:
                propertyItem.PrimaryStringValue = propvalue1;
                propertyItem.SecondaryStringValue = propvalue2;
                break;

            case 3:
                propertyItem.PrimaryDateTimeValue = propvalue1 !== '' ? propvalue1 : null;
                propertyItem.SecondaryDateTimeValue = propvalue2;
                break;

            case 4:
                propertyItem.PrimaryTimeValue = propvalue1;
                propertyItem.SecondaryTimeValue = propvalue2;
                break;

            case 5:
                propertyItem.PrimaryDateTimeValue = propvalue1 !== '' ? propvalue1 : null;
                propertyItem.SecondaryDateTimeValue = propvalue2;
                break;

            case 6:
                propertyItem.BoolValue = propvalue1;
                break;

            default:
                break;
        }

        ezgoprops._template.Properties.push(propertyItem);
        ezgoprops.loadTemplateProps(ezgoprops._template);
        $('#valuePanel').hide();
        $('input[data-selector="input"]').val('');
        $('input[data-selector="displaytitle"]').val('');
        $('input[data-selector="displayfooter"]').val('');
    },

    _appendLeadingZero: function (n) {
        if (n <= 9) {
            return "0" + n;
        }
        return n
    }
}
