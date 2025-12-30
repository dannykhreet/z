var deeplink = {

    init: function () {
        this.hideAll();
        $("#tagsChecklist").autocomplete({
            source: '/task/getchecklists',
            select: function (event, ui) {
                $('#deeplinkChecklistTag').removeClass('d-none');
                $('#deeplinkChecklistLabel').html(ui.item.label);
                $('#deeplinkChecklistLabel').data('checklistid', ui.item.value);
                $('#tagsChecklist').hide();
                ezgolist.setDetails();
            },
            close: function (event, ui) {
                //console.log(event);
                $("#tagsChecklist").val('').trigger('change');
                $('#deeplinkChecklistTag').show();
            }
        });
        $("#tagsAudit").autocomplete({
            source: '/task/getaudits',
            select: function (event, ui) {
                //console.log(ui.item.value);
                $('#deeplinkAuditTag').removeClass('d-none');
                $('#deeplinkAuditLabel').html(ui.item.label);
                $('#deeplinkAuditLabel').data('auditid', ui.item.value);
                $('#tagsAudit').hide();
                ezgolist.setDetails();
            },
            close: function (event, ui) {
                //console.log(event);
                $("#tagsAudit").val('').trigger('change');
                $('#deeplinkAuditTag').show();
            }
        });
        $("#deepLinkChecklistIsRequired").change(function () {
            ezgolist.setDetails();
        });
        $("#deepLinkAuditIsRequired").change(function () {
            ezgolist.setDetails();
        });
    },

    render: function (id, type, isRequired) {
        //console.log(id + ' => ' + type);
        switch (type) {
            case 'audit':
                $.get('/audit/getname/' + id, function (name) {
                    $('#deeplinkAuditTag').removeClass('d-none');
                    $('#deeplinkAuditLabel').html(name);
                    $('#deeplinkAuditLabel').data('auditid', id);
                    $('#deepLinkAuditIsRequired').prop('checked', isRequired);
                    $('#deepLinkAuditRemoveBtn').show();
                    $('#tagsAudit').hide();
                });
                break;
            case 'checklist':
                $.get('/checklist/getname/' + id, function (name) {
                    $('#deeplinkChecklistTag').removeClass('d-none');
                    $('#deeplinkChecklistLabel').html(name);
                    $('#deeplinkChecklistLabel').data('checklistid', id);
                    $('#deepLinkChecklistIsRequired').prop('checked', isRequired);
                    $('#deepLinkChecklistRemoveBtn').show();
                    $('#tagsChecklist').hide();
                });
                break;
        }
        
    },

    hideAll: function () {
        $('#rowDeeplinkAudit').hide();
        $('#rowDeeplinkChecklist').hide();
        if (ezgolist._firstLoadDone) {
            ezgolist.tmpl.DeepLinkTo = 'none';
        }
    },

    showChecklist: function () {
        this.hideAll();
        $('#rowDeeplinkChecklist').show();

    },

    showAudit: function () {
        this.hideAll();
        $('#rowDeeplinkAudit').show();

    },

    removeAuditTag: function () {
        $('#deeplinkAuditTag').hide();
        $('#tagsAudit').show();
    },

    removeChecklistTag: function () {
        $('#deeplinkChecklistTag').hide();
        $('#tagsChecklist').show();
    }
}
