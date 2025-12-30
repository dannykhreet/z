/*
 * EZGOExport;
 * -> used controlers ExportController for back-end processing.
 * Currently available -> checklistaudits, tasktemplates
 * Export creates a popup near the pressed botton based on the data-exporttype property.
 * Depending on export a few options are set (e.g. start and end dates) and a export button will be available.
 * After setting the correct values the ExportController will handle a download of a file based on excel or csv (for now);
 * Front-end processing is fully done by JS.
 * */


var EZGOExportLanguages = {
    ChecklistAuditsTitle: 'Checklists and audits',
    ChecklistsAuditsTemplatesTitle: 'Checklist- and audit-templates',
    TaskTemplatesTitle: 'Task templates',
    TaskTemplatesTitleFormItemsText: 'Export all active tasktemplates',
    TasksTitle: 'Task tasks',
    TaskPropertiesTitle: 'Value registration',
    AuditTaskPropertiesTitle: 'Value registration',
    ChecklistTaskPropertiesTitle: 'Value registration',
    ChecklistsTitle: 'Checklists',
    AuditsTitle: 'Audits',
    AuditTemplatesTitle: 'Audit templates',
    AuditTemplatesFormItemsText: 'Export all active audittemplates',
    ChecklistTemplatesTitle: 'Checklist templates',
    ChecklistTemplatesFormItemsText: 'Export all active checklisttemplates',
    WorkInstructionTemplatesTitle: 'WI templates',
    WorkInstructionTemplatesFormItemsText: 'Export all active workinstructiontemplates',
    LanguagesTitle: 'Export translations',
    LanguagesFormItemsText: 'Export all language translations',
    LanguagesImportTitle: 'Import translations',
    LanguagesImportFormItemsText: 'Export language translations updates for import.',
    CompanyOverviewTitle: 'Company overview',
    ActionsTitle: 'Actions',
    CommentsTitle: 'Comments',
    ExportButtonTitle: 'Export',
    ValidationStartEarlierThanEndMessage: 'Start date must be earlier than the end date',
    ValidationStartTimeEarlierThanEndMessage: 'Start time must be earlier than the end date',
    BusyMessage: 'Busy with getting your file...',
    DoneMessage: 'Done!',
    StartDateTitle: 'Start date',
    EndDateTitle: 'End date',
    AssessmentsTitle: 'Skill assessments',
    CompanyAreasTitle: 'Areas',
    CompanyAreasText: 'Export all areas',
    AssessmentTemplatesTitle: 'Skill assessment templates',
    AssessmentTemplatesFormItemsText: 'Export all active assessmenttemplates',
    MatrixSkillScoreTitle: 'Export Matrix Scores',
    MatrixSkillScoreFormItemsText: 'Export matrix skill scores',
    WorkInstructionChangeNotificationsTitle: 'Export WI Changes'
}

var EZGOExport = {
    _uid: '',
    _maxdaysexport: 32,
    _maxdaysexportstart: 370,
    activate: function () {
        EZGOExport.setHandlers();
        //console.log('EZGOExport tooling initiated');
    },
    enableExportButton: function () {
        if (document.querySelector('#export_button') != null) {
            document.querySelector('#export_button').disabled = false;
        }
       
    },
    disableExportButton: function () {
        if (document.querySelector('#export_button') != null) { 
            document.querySelector('#export_button').disabled = true;
        }
    },
    generateTimeInput: function (id, title, selected) {
        let label = '<div><label for="' + id + '">' + title + '</label></div>';
        let input = '<select id="' + id + '">';
        for (var ti = 0; ti < 24; ti++) {
            let h = ti.toString().padStart(2, '0');
            input = input + '<option ' + (selected !== null && selected == 'first' && ti == 0 ? ' selected ' : '') + ' value="' + h + ':00">' + h + ':00</option>';
            input = input + '<option value="' + h + ':15">' + h + ':15</option>';
            input = input + '<option value="' + h + ':30">' + h + ':30</option>';
            input = input + '<option ' + (selected !== null && selected == 'last' && ti == 23 ? ' selected ' : '') + 'value="' + h + ':45">' + h + ':45</option>';
        }
        input = input + '</select>';
        return title !== null && title != undefined ? label + input : input;
    },
    generateDateInput: function (id, title, selected, coupledfield) {
        let label = '<div><label for="' + id + '">' + title + '</label></div>';
        let ev = coupledfield != null ? ' onchange="EZGOExport.regenerateDateInput(\'' + coupledfield + '\',document.getElementById(\'' + id + '\').value);"' : '';
        let input = '<select id="' + id + '" ' + ev + ' style="width:100px;">';
        for (var ti = 0; ti < EZGOExport._maxdaysexport; ti++) {
            let d = new Date();
            let selectedoption = '';
            if ((selected !== null && selected == 'first' && ti == 0) || (selected !== null && selected == 'last' && ti == 13)) {
                selectedoption = ' selected ';
            };
            d.setDate(d.getDate() - ti);
            input = input + '<option ' + selectedoption + ' value="' + d.toDateString() + '">' + d.toLocaleDateString() + '</option>';
        }
        input = input + '</select>';
        return title !== null && title != undefined ? label + input : input;
    },
    generateDateSelectorInput: function (id, title, selected, coupledfield) {
        let value = '';
        let min = '';
        let max = '';
        if (selected == 'now' || selected == 'now-32') {
            let minDate = new Date(Date.now());
            minDate.setDate(minDate.getDate() - 3650);
            value = ' value="' + EZGOExport.formatDate(new Date(Date.now()), '00') + '" ';
            min = ' min="' + EZGOExport.formatDate(new Date(minDate), '00') + '" ';
            max = ' max="' + EZGOExport.formatDate(new Date(Date.now()), '00') + '" ';
        }
        let label = '<div><label for="' + id + '">' + title + '</label></div>';
        let ev = coupledfield != null ? ' onchange="EZGOExport.regenerateDateSelectorInput(\'' + coupledfield + '\',document.getElementById(\'' + id + '\').value);"' : '';
        let input = '<input type="datetime-local" id="' + id + '" ' + ev + ' ' + value + ' ' + min + ' ' + max + ' style="width:160px;" />';
        return title !== null && title != undefined ? label + input : input;
    },
    regenerateDateInput: function (id, startdate) {
        let currentVal = document.getElementById(id).value;
        let currentDate = new Date();
        let input = '';

        for (var ti = 0; ti < EZGOExport._maxdaysexport; ti++) {
            let d = new Date(startdate);
            d.setDate(d.getDate() + ti);
            if (d <= currentDate) {
                let selectedoption = '';
                if (d.toDateString() === currentVal) {
                    selectedoption = ' selected ';
                };
                input = input + '<option ' + selectedoption + ' value="' + d.toDateString() + '">' + d.toLocaleDateString() + '</option>';
            }
        }
        document.getElementById(id).innerHTML = input;
    },
    regenerateDateSelectorInput: function (id, startdate) {
        let newDate = new Date(startdate);
        newDate.setDate(newDate.getDate() + EZGOExport._maxdaysexport);
        if (newDate > new Date(Date.now())) { newDate = new Date(Date.now()); }

        document.getElementById(id).value = EZGOExport.formatDate(newDate, '00');
        document.getElementById(id).min = startdate;
        document.getElementById(id).max = EZGOExport.formatDate(newDate, '00');
    },
    initModal: function (sender, exportName) {
        EZGOExport.removeModal(); //first remove;
        var formitems = '';
        var title = '';
        var height = 125;
        switch (exportName) {
            case 'checklistsaudits':
                title = EZGOExportLanguages.ChecklistAuditsTitle; //'Checklists and audits'
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now','export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'checklistsauditstemplates':
                title = EZGOExportLanguages.ChecklistsAuditsTemplatesTitle; //'Checklist- and audit-templates'
                break;
            case 'tasktemplates':
                title = EZGOExportLanguages.TaskTemplatesTitle; //'Task templates';
                height = 50;
                formitems = EZGOExportLanguages.TaskTemplatesTitleFormItemsText; //'Export all active tasktemplates';
                break;
            case 'tasks':
                title = EZGOExportLanguages.TasksTitle; //'Task tasks';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'taskproperties':
                title = EZGOExportLanguages.TaskPropertiesTitle; //'Value registration';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'audittaskproperties':
                title = EZGOExportLanguages.AuditTaskPropertiesTitle; //'Value registration';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'checklisttaskproperties':
                title = EZGOExportLanguages.ChecklistTaskPropertiesTitle; //'Value registration';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'checklists':
                title = EZGOExportLanguages.ChecklistsTitle; //'Checklists';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'audits':
                title = EZGOExportLanguages.AuditsTitle; //'Audits';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now','export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'audittemplates':
                title = EZGOExportLanguages.AuditTemplatesTitle; //'Audit templates';
                height = 50;
                formitems = EZGOExportLanguages.AuditTemplatesFormItemsText;//'Export all active audittemplates';
                break;
            case 'checklisttemplates':
                title = EZGOExportLanguages.ChecklistTemplatesTitle; // 'Checklist templates';
                height = 50;
                formitems = EZGOExportLanguages.ChecklistTemplatesFormItemsText; //'Export all active checklisttemplates';
                break;
            case 'assessmenttemplates':
                title = EZGOExportLanguages.AssessmentTemplatesTitle; // 'Skill assessment templates';
                height = 50;
                formitems = EZGOExportLanguages.AssessmentTemplatesFormItemsText; //'Export all active assessmenttemplates';
                break;
            case 'workinstructiontemplates':
                title = EZGOExportLanguages.WorkInstructionTemplatesTitle; // 'Checklist templates';
                height = 50;
                formitems = EZGOExportLanguages.WorkInstructionTemplatesFormItemsText; //'Export all active checklisttemplates';
                break;
            case 'languages':
                title = EZGOExportLanguages.LanguagesTitle; //'Export translations';
                height = 50;
                formitems = EZGOExportLanguages.LanguagesFormItemsText; //'Export all language translations';
                break;
            case 'languagesimport':
                title = EZGOExportLanguages.LanguagesImportTitle;//'Import translations';
                height = 50;
                formitems = EZGOExportLanguages.LanguagesImportFormItemsText; //'Export language translations updates for import.';
                break;
            case 'companyoverview':
                title = EZGOExportLanguages.CompanyOverviewTitle; //'Company overview';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'wichangenotifications':
                title = EZGOExportLanguages.WorkInstructionChangeNotificationsTitle; //'Company overview';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'actions':
                title = EZGOExportLanguages.ActionsTitle; //'Actions';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                            EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'comments':
                title = EZGOExportLanguages.CommentsTitle; //'Comments';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'auditinglog':
                title = EZGOExportLanguages.CompanyOverviewTitle; //'auditing log';
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                    EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'assessments':
                title = EZGOExportLanguages.AssessmentsTitle;
                formitems = EZGOExport.generateDateSelectorInput('export_start_date', EZGOExportLanguages.StartDateTitle, 'now', 'export_end_date') + '<div style="height:2px;"></div>' +
                            EZGOExport.generateDateSelectorInput('export_end_date', EZGOExportLanguages.EndDateTitle, 'now-32');
                break;
            case 'companyareas':
                title = EZGOExportLanguages.CompanyAreasTitle; // Company active areas ;
                height = 50;
                formitems = EZGOExportLanguages.CompanyAreasText; //'Export all active areas';
                break;
            case 'matrixskillscores':
                title = EZGOExportLanguages.MatrixSkillScoreTitle; 
                height = 50;
                formitems = EZGOExportLanguages.MatrixSkillScoreFormItemsText; 
                break;
        }
        var modal = '<div style="position:relative;display:inline;" id="export_container">' +
            '<div style="position:absolute; left:-200px; right:0px; width: 200px; height:200px; z-index:9999;">' +
            '<div style="position:relative;text-align:center; border-radius: 5px; border:1px solid #C0C0C0; background-color: #ffffff; box-shadow: 1 1 4px rgba(255, 255, 255, .5);">' +
            '<div style="position:relative;height:50px;text-align:center;border-bottom:1px solid #C0C0C0; vertical-align:bottom"><br /><span style="font-weight:bolder;font-size:16px;">' + title + '</span></div>' +
            '<div style="position:absolute; right: 2px; width:20px; height: 20px; top: 0px; cursor: pointer;" onclick="EZGOExport.removeModal();">x</div>' +
            '<div style="position:relative;height:' + height + 'px;margin-top:5px;" id="export_displayform_container">' +
            formitems +
            '</div>' +
            '<div style="position:relative;height:40px;text-align:right;border-top:1px solid #C0C0C0">' +
            '<div><button class="btn btn-ezgo btn-sm" style="margin:2px;" id="export_button" onclick="EZGOExport.export(\'' + exportName + '\');" disabled>' + EZGOExportLanguages.ExportButtonTitle + '</button></div>' +
            '</div>' +
            '<a id="ezgo_target"></a>' +
            '</div>' +
            '</div>' +
            '</div>';
        sender.insertAdjacentHTML('afterend', modal);

        EZGOExport.enableExportButton(); 
        
    },
    removeModal: function () {
        var modal = document.getElementById('export_container');
        if (modal != null && modal !== undefined) {
            modal.parentNode.removeChild(modal);
            if(event!=null && event !== undefined) event.stopPropagation();
            return false;
        }
    },
    setHandlers: function () {
        //TODO add handlers
        var exportButtons = document.querySelectorAll("[data-export]");
        if (exportButtons != null) {
            for (var i = 0; i < exportButtons.length; i++) {
                exportButtons[i].addEventListener("click", function () { EZGOExport.initModal(this, this.getAttribute('data-export')); })
            }
        }
    },
    validate: function (exportName) {
        let succes = true;
        switch (exportName) {
            case 'assessments':
            case 'tasks':
            case 'taskproperties':
            case 'audittaskproperties':
            case 'checklisttaskproperties':
            case 'checklists':
            case 'audits':
            case 'companyoverview':
            case 'companyareas':
            case 'comments':
            case 'actions':
            case 'wichangenotifications':
                if (document.getElementById('export_start_date').value == '' || document.getElementById('export_end_date').value == '') {
                    let message = EZGOExportLanguages.ValidationStartEarlierThanEndMessage; //'Start date must be earlier than the end date'
                    if (toastr !== null) {
                        toastr.error(message);
                    } else {
                        alert(message);
                    }
                    succes = false;
                } else if (new Date(document.getElementById('export_start_date').value) > new Date(document.getElementById('export_end_date').value)) {
                    let message = EZGOExportLanguages.ValidationStartEarlierThanEndMessage; //'Start date must be earlier than the end date'
                    if (toastr !== null) {
                        toastr.error(message);
                    } else {
                        alert(message);
                    }
                    succes = false;
                }
            break;
        }
        return succes;
    },
    export: function (exportName) {
        switch (exportName) {
            case 'checklistsaudits'://why is this implemented but used nowhere? TODO REMOVE!
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/checklistsandaudits/',
                        'application/json', 'export_checklistaudit_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'assessments':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/assessments/',
                        'application/json', 'export_assessments_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'checklists':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/checklists/',
                        'application/json', 'export_checklist_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'checklisttemplates':
                EZGOExport.exportRequest('/export/checklisttemplates/',
                    'application/json', 'export_checklisttemplates_company_' + Date.now().toString() + '.xlsx',
                    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    'export_displayform_container', 'xlsx', false);
                break;
            case 'assessmenttemplates':
                EZGOExport.exportRequest('/export/assessmenttemplates/',
                    'application/json', 'export_assessmenttemplates_company_' + Date.now().toString() + '.xlsx',
                    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    'export_displayform_container', 'xlsx', false);
                break;
            case 'workinstructiontemplates':
                EZGOExport.exportRequest('/export/workinstructiontemplates/',
                    'application/json', 'export_workinstructiontemplates_company_' + Date.now().toString() + '.xlsx',
                    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    'export_displayform_container', 'xlsx', false);
                break;
            case 'audits':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/audits/',
                        'application/json', 'export_audits_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'audittemplates':
                EZGOExport.exportRequest('/export/audittemplates/',
                    'application/json', 'export_audittemplates_company_' + Date.now().toString() + '.xlsx',
                    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    'export_displayform_container', 'xlsx', false);
                break;
            case 'tasktemplates':
                EZGOExport.exportRequest('/export/tasktemplates/',
                    'application/json', 'export_templates_company_' + Date.now().toString() + '.xlsx',
                    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    'export_displayform_container', 'xlsx', false);
                break;
            case 'tasks':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/tasks/',
                        'application/json', 'export_tasks_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'taskproperties':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/taskproperties/',
                        'application/json', 'export_taskproperties_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'checklisttaskproperties':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/checklisttaskproperties/',
                        'application/json', 'export_taskproperties_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'audittaskproperties':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/audittaskproperties/',
                        'application/json', 'export_taskproperties_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'languages':
                 EZGOExport.exportRequest('/export/languageresources/',
                     'application/json', 'export_languages_' + Date.now().toString() + '.xlsx',
                     'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                     'export_displayform_container', 'xlsx', false);
                break;
            case 'languagesimport':
                EZGOExport.exportRequest('/export/languageimport/',
                    'application/json', 'export_languageimport_' + Date.now().toString() + '.csv',
                    'text/csv',
                    'export_displayform_container', 'csv', false);
                break;
            case 'companyoverview':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/companyoverview/',
                        'application/json', 'export_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'companyareas':
                    EZGOExport.exportRequest('/export/companyareas/',
                        'application/json', 'export_company_areas_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', false);
                break;
            case 'actions':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/actions/',
                        'application/json', 'export_actions_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'comments':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/comments/',
                        'application/json', 'export_comments_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'wichangenotifications':
                if (EZGOExport.validate(exportName)) {
                    EZGOExport.exportRequest('/export/wichangenotifications/',
                        'application/json', 'export_wichangenotifications_company_' + Date.now().toString() + '.xlsx',
                        'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                        'export_displayform_container', 'xlsx', true);
                }
                break;
            case 'auditinglog':
                //console.log('auditinglog');
                if (EZGOExport.validate(exportName)) {
                    //console.log('auditinglog is in');
                    EZGOExport.exportRequest('/export/auditinglog/',
                        'application/json', 'export_auditinglog_' + Date.now().toString() + '.csv',
                        'text/csv',
                        'export_displayform_container', 'csv', true);
                }
                break;
            case 'matrixskillscores':
                EZGOExport.exportRequest('/export/matrixskillscores/',
                    'application/json', 'export_matrix_skill_scores_' + Date.now().toString() + '.xlsx',
                    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
                    'export_displayform_container', 'xlsx', false);
                break;

        }
    },
    exportData: function (useForms, exporttype) {
        let startDateTime = null;
        let endDateTime = null;
        let startDateNumber = null;
        let endDateNumber = null;
        let startTime = null;
        let endTime = null;
        let exDate = null;
        let exType = exporttype;
        //console.log(exType);
        if (useForms) {
            startDateTime = document.getElementById('export_start_date').value;
            endDateTime = document.getElementById('export_end_date').value;
            //startTime = document.getElementById('export_start_time').value;
            //endTime = document.getElementById('export_end_time').value;

            //startDateTime = EZGOExport.formatDate(startDateNumber, startTime);
            //endDateTime = EZGOExport.formatDate(endDateNumber, endTime);
            //console.log(endDateTime);
        }

        if (useForms) {
            let d = {
                FromDate: startDateTime,
                ToDate: endDateTime,
                Date: exDate,
                ExportType: exType
            }
            return d;
        } else {
            let d = {
                ExportType: exType

            }
            return d
        }
    },
    exportRequest: function (uriPart, contenttype, fileName, fileContentType, loaderId, exporttype, useForms) {
        //console.log('Starting export...');
        var exportData = EZGOExport.exportData(useForms, exporttype);
       
        try {
            EZGOExport.setLoaderStart(loaderId);
            var xhttp = new XMLHttpRequest();
            xhttp.open('POST', uriPart);
            xhttp.responseType = 'blob';
            //Setting correct headers.
            xhttp.setRequestHeader("Content-type", contenttype);
            xhttp.onload = function (event) {
                if (xhttp.status == 200) {
                    var blob = xhttp.response; //Get the blob... which should be returning from call.

                    EZGOExport.handleFile(blob, fileName, fileContentType);
                    EZGOExport.setLoaderEnd(loaderId);
                    //console.log('Export...done');
                } else {
                    if (xhttp.status == 401 || xhttp.status == 405) {
                        console.log('rights error occurred, please contact the system administrator: ', xhttp.status);
                    } else {
                        console.log('error occurred, please contact the system administrator: ',xhttp.status);
                    }
                }
            }
            xhttp.send(JSON.stringify(exportData));
        } catch (ex) {
            //console.log(ex);
            EZGOExport.setLoaderEnd(loaderId);
            //EZGOExport.showErrorMessage('An error occurred. Please try again later. If this problem is not solved, please contact the system administrator.');
        }
    },
    handleFile: function (fileBlob, fileName, fileContentType) {
        if (window.navigator.msSaveOrOpenBlob) {
            // Internet Explorer....because of reasons..
            var contentType = fileContentType;
            window.navigator.msSaveOrOpenBlob(new Blob([fileBlob], { type: contentType }), fileName);
        } else {
            var el = document.getElementById("ezgo_target");
            el.href = window.URL.createObjectURL(fileBlob);
            el.download = fileName;
            el.click();
        }
    },
    setLoaderStart: function (elementId) {
        var loader = document.getElementById(elementId);
        loader.innerHTML = EZGOExportLanguages.BusyMessage + '<br /><img src="data:image/gif;base64,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" />';
    },
    setLoaderEnd: function (elementId) {
        var loader = document.getElementById(elementId);
        loader.innerHTML = EZGOExportLanguages.DoneMessage;//'Done!';
        setTimeout(function () { EZGOExport.removeModal(); }, 2000);
    },
    formatDate: function (dateValue, time) {
        var d = new Date(dateValue);
        date = [
            d.getFullYear(),
            ('0' + (d.getMonth() + 1)).slice(-2),
            ('0' + d.getDate()).slice(-2)
        ].join('-');
        date = date + 'T' + time + ':00';
        return date;
    }
}

EZGOExport.activate();