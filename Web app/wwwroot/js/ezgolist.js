var ezgolist = {
    language: {
        listtype: 'item',
        listtypeId: 'ID: ',
        addItem: 'Add item',
        addStage: 'Add Stage',
        addItemLower: 'add item',
        addStage: 'add stage',
        nextItem: 'next item',
        addStep: 'add step',
        nextStep: 'next step',
        addpdf: 'Add PDF',
        addlink: 'Add link',
        assignButton: 'Assign',
        addinstruction: 'Add instruction',
        addworkinstruction: 'Add WorkInstruction',
        scoringsystem: 'Scoring system',
        scoringdisabled: 'disabled, has completed audits',
        confirmationleavemessage: 'It looks like you have been editing something.If you leave before saving, your changes will be lost.',
        ppEnable: 'Enable', //pp = picture proof
        ppDisable: 'Disable',
        ppForAllItems: 'picture proof for all items?',
        instructionCounterLabel: 'Instruction',
        stageTemplate: 'Stage template',

        optionYes: 'Yes',
        optionNo: 'No',

        deleteAuditItem: 'Delete audit item?',
        deleteChecklistItem: 'Delete checklist item?',
        deleteWorkInstructionItem: 'Delete workinstruction item?',

        deleteAuditInstruction: 'Delete instruction from this audit item?',
        deleteChecklistInstruction: 'Delete instruction from this checklist item?',
        deleteTaskInstruction: 'Delete instruction from this task item?',

        deleteAuditTemplate: 'Delete this audit template?',
        deleteChecklistTemplate: 'Delete this checklist template?',
        deleteSkillAssessmentTemplate: 'Delete this skillassessment template?',
        deleteTaskTemplate: 'Delete this task template?',
        deleteWorkInstructionTemplate: 'Delete this workinstruction template?',

        selectTypeTitle: 'Select a type for this instruction',
        selectTypeDisabled: 'disabled, is part of an assessment'
    },
    currentWorkInstructionTemplates: {},
    workInstructionsEnabled: false,
    taskTemplateAttachmentsEnabled: false,
    transferableChecklistsEnabled: false,
    itemListSelector: '#itemlist',
    itemListItemSelector: 'li[data-type="item"]',
    openFieldsSelector: '#openfieldslist',
    listType: '',
    templateNameSelector: '#txtTemplateName',
    templateAreaIdSelector: 'div[data-type="areaid"]',
    templateSignatureSelector: 'input[name="signature"]:checked',
    templateRoleSelector: 'input[name="role"]:checked',
    placeholderImageUrl: '/img/default-placeholder-image.png',
    mediaUrl: 'https://ezgo.accapi.ezfactory.nl/media/',
    useSlim: false,
    stagesEnabled: false,
    tmpl: null,
    initialTmpl: null,
    lastId: 0,
    templateId: 0,
    sharedTemplateId: 0,
    companyId: 0,
    getListUrl: '',
    initDataObject: '',
    maxUploadFileSize: 52428800,
    maxImageWidthOrHeight: 1920,
    maxImageSizeMB: 0.5,
    sendWiChangeNotification: false,
    _currentTemplateId: 0,
    _currentTemplateStepId: 0,
    _currentStageTemplateId: 0,
    _mediaarray: new Array(),
    _mediaCntr: 0,
    _hasChanged: false,
    _firstLoadDone: false,
    _propertyVersion: 1,
    _isNewTemplate: false,
    _lastNewItemId: 0,
    _lastNewStageId: 0,
    _itemStepLastIndex: {},

    //ezgolist.init:
    //initializes/sets a lot of properties of ezgolist
    //adds event handlers for some html elements
    //calls render() for a new list or 
    //load() for an existing list, based on the list id
    init: function (id, companyid, listtype, propertyVersion) {
        if (typeof analytics !== 'undefined') {
            if (analytics != null) {
                analytics.logEvent('load_templates', { name: listtype, companyid: companyid });
            }
        }

        ezgolist.tmpl = Object.create(templateObj);
        ezgolist.tmpl.Id = id;
        ezgolist.tmpl.Tags = new Array();
        ezgolist.templateId = id;
        ezgolist.tmpl.CompanyId = companyid;
        ezgolist.companyId = companyid;
        ezgolist.listType = listtype;
        ezgolist.lastId = $('#itemlist').find('li[data-type="item"]').length;
        $(ezgolist.itemListSelector).empty();

        //determine if background for stages should be displayed
        ezgolist.liClass = (ezgolist.listType == 'checklist') ? 'template-item' : 'list-inline-item pb-3';

        cardTemplateImage = cardTemplateImage.replaceAll('{{liClass}}', ezgolist.liClass);
        cardTemplateVideo = cardTemplateVideo.replaceAll('{{liClass}}', ezgolist.liClass);

        $('#editTemplateStepModal').on('hide.bs.modal', function (e) {
            ezgolist.updateItem(ezgolist._currentTemplateId);
        });

        $('#editTemplateStepDetailModal').on('hide.bs.modal', function (e) {
            ezgolist.updateItemStep(ezgolist._currentTemplateStepId);
        });

        $('#editStageTemplateStepModal').on('hide.bs.modal', function (e) {
            ezgolist.updateStageTemplate(ezgolist._currentStageTemplateId)
        });

        $('#AddWorkInstructionModal').on('keyup', '[data-action="search_workinstructions"]', function () {
            ezgolist.searchWorkInstructions($(this).val());
        });

        $('#enablePictureProof').on('change', function (e) {
            var isChecked = $('#enablePictureProof').is(':checked');
            ezgolist._hasChanged = true;
            if (ezgolist.listType == 'task') {
                ezgolist.tmpl.HasPictureProof = isChecked;
            }
            else if (ezgolist.listType == 'checklist' || ezgolist.listType == 'audit') {
                objIndex = ezgolist.tmpl.TaskTemplates.findIndex((obj => obj.Id == ezgolist._currentTemplateId));
                ezgolist.tmpl.TaskTemplates[objIndex].HasPictureProof = isChecked;

                if (!isChecked) {
                    //turn off general switch
                    $('#enablePictureProofForAllItems').prop('checked', false);
                }
                else {
                    //if all tasktemplates have pictureproof, turn on general toggle
                    ezgolist.updateGeneralPictureProofToggle();
                }
            }
            $('body').trigger('ezgolistChanged');
        });

        if (ezgolist.listType === 'checklist' || ezgolist.listType === 'audit') {
            $('#enablePictureProofForAllItems').on('click', function (e) {
                e.preventDefault();
                e.stopPropagation();
                e.stopImmediatePropagation();
                var isChecked = $('#enablePictureProofForAllItems').is(':checked');
                var enableDisable = isChecked ? ezgolist.language.ppEnable : ezgolist.language.ppDisable;

                $.fn.dialogue({
                    content: $("<p />").text(enableDisable + ' ' + ezgolist.language.ppForAllItems),
                    closeIcon: true,
                    buttons: [{
                        text: "Yes", id: $.utils.createUUID(), click: function ($modal) {
                            $('#enablePictureProofForAllItems').prop('checked', isChecked);
                            $('#enablePictureProofForAllItems').is(':checked');
                            $modal.dismiss();
                            ezgolist._hasChanged = true;
                            $(ezgolist.tmpl.TaskTemplates).each(function (index, taskTemplate) {
                                taskTemplate.HasPictureProof = isChecked;
                            });
                            ezgolist.render();
                            $('body').trigger('ezgolistChanged');
                        }
                    },
                    {
                        text: "No", id: $.utils.createUUID(), click: function ($modal) {
                            $('#enablePictureProofForAllItems').prop('checked', !isChecked);
                            $modal.dismiss();
                        }
                    }]
                });
            });
        }

        if (ezgolist.listType === 'workinstruction') {
            $('#availableForAllAreas').on('change', function (e) {
                ezgolist.tmpl.IsAvailableForAllAreas = this.checked;
            });
        }

        ezgolist._propertyVersion = propertyVersion;
        if (id === 0 && ezgolist.sharedTemplateId === 0) {
            ezgolist.tmpl.TaskTemplates = new Array();
            ezgolist.tmpl.ScoreType = "thumbs";
            ezgolist.render();

            if (ezgolist.listType === 'task') {
                ezgolist.tmpl.Type = 'task';
                ezgolist.tmpl.Steps = new Array();
            }
        } else {
            ezgolist.load();
            var min = 0
            var max = 10;

            if (ezgolist.tmpl.MinScore !== undefined) {
                min = parseInt(ezgolist.tmpl.MinScore)
            }

            if (ezgolist.tmpl.MaxScore !== undefined) {
                max = parseInt(ezgolist.tmpl.MaxScore)
            }
        }

        $(".js-range-slider").ionRangeSlider({
            type: "double",
            skin: "round",
            grid: false,
            min: 0,
            max: 10,
            from: 0,
            to: 10,
            prefix: "",
            onChange: ezgolist.setDetails
        });

        window.addEventListener("beforeunload", function (e) {
            if (!ezgolist._hasChanged) {
                return undefined;
            }
            e.preventDefault();
            var confirmationMessage = ezgolist.language.confirmationleavemessage;
            (e || window.event).returnValue = confirmationMessage; //Gecko + IE
            return confirmationMessage; //Gecko + Webkit, Safari, Chrome etc.
        });

        $(document).on('click', 'a[data-selector="deletemedia"]', function (e) {
            ezgolist.deleteMedia(e);
        });

        //fancybox
        $(document).on('afterShow.fb', function () {
            //load videos
            ezgomediafetcher.preloadVisibleVideos();
        });

        // ****** EZ DROPZONE ******
        function preventDefault(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        document.querySelectorAll('[data-drop="true"]').forEach(item => {
            item.addEventListener('dragenter', preventDefault, false)
            item.addEventListener('dragleave', preventDefault, false)
            item.addEventListener('dragover', preventDefault, false)
            item.addEventListener('drop', preventDefault, false)
        });

        function handleDrop(e) {
            var dt = e.dataTransfer,
                files = dt.files;

            if (files.length) {
                ezgolist.dropPreviewFileEx('#' + e.currentTarget.id + ' input[type="file"]', files);
            }
        }

        document.querySelectorAll('[data-drop="true"]').forEach(item => {
            item.addEventListener('drop', handleDrop, false);
        });

        // ****** EZ DROPZONE ******
        ezgolist._hasChanged = false;
        ezgolist.setOpenFieldHandlers(); //add openfiel handlers

        if (ezgolist.listType === 'skillassessment') {
            ezgolist.extension.assessments.init();
        }

        if (ezgolist.listType === 'task') {
            ezgolist.fixTemplateDefaultData('norecurrency');
            $('[data-actiontype="recurrencytype_change"]').on('click', function () {
                ezgolist.fixTemplateDefaultData($(this).attr('data-value'));
            });
        }
    },

    //ezgolist.setDetails:
    // sets miscellaneous properties in the ezgolist.tmpl field
    setDetails: function (element) {
        if (element != null && element.id != null) {
            //don't do anything if called from name or description fields and nothing was changed
            if ((element.id == 'txtTemplateName' && element.value == ezgolist.tmpl.Name)
                || (element.id == 'txtTemplateDescription' && element.value == ezgolist.tmpl.Description)) {
                return;
            }

            //don't do anything if called from plannedtime field and nothing was changed
            if (element.id == 'plannedtime') {
                let plannedTime = parseInt(element.value);
                if (isNaN(plannedTime) && isNaN(ezgolist.tmpl.PlannedTime) || plannedTime == ezgolist.tmpl.PlannedTime) {
                    return;
                }
            }
        }

        var signatures = $('input[name="signature"]:checked').val();
        ezgolist.tmpl.Role = $('input[name="role"]:checked').val();
        ezgolist.tmpl.Name = $('#txtTemplateName').val();

        if ($('#plannedtime').val() !== '' && ezgolist._firstLoadDone) {
            var parsedValue = parseInt($('#plannedtime').val());
            ezgolist.tmpl.PlannedTime = isNaN(parsedValue) ? null : parsedValue;

            if (isNaN(parsedValue) && ezgolist._firstLoadDone) {
                ezgolist.tmpl.PlannedTime = null;
            }
        }

        switch (signatures) {
            case 'none':
                ezgolist.tmpl.IsSignatureRequired = false;
                ezgolist.tmpl.IsDoubleSignatureRequired = false;
                break;

            case 'single':
                ezgolist.tmpl.IsSignatureRequired = true;
                ezgolist.tmpl.IsDoubleSignatureRequired = false;
                break;

            case 'double':
                ezgolist.tmpl.IsSignatureRequired = true;
                ezgolist.tmpl.IsDoubleSignatureRequired = true;
                break;
        }

        if (ezgolist.listType === 'audit' && $('#ScoreRangeSlider').data().from !== undefined && ezgolist._firstLoadDone) {
            var score = $('input[name="score"]:checked').val();

            if (score !== undefined) {
                ezgolist.tmpl.ScoreType = score;

                if (ezgolist.tmpl.ScoreType == 'score') {
                    ezgolist.tmpl.MinScore = $('#ScoreRangeSlider').data().from;
                    ezgolist.tmpl.MaxScore = $('#ScoreRangeSlider').data().to;
                } else {
                    ezgolist.tmpl.MinScore = 0;
                    ezgolist.tmpl.MaxScore = 10;
                }
            }
        }

        if (ezgolist.listType === 'task' && ezgolist._firstLoadDone) {
            var machinestatus = $('input[name="machinestatus"]:checked').val();
            ezgolist.tmpl.Description = $('#txtTemplateDescription').val();
            ezgolist.tmpl.MachineStatus = machinestatus;
            var deeplinkto = $('input[name="deeplink"]:checked').val();

            if (deeplinkto !== undefined) {
                ezgolist.tmpl.DeepLinkTo = deeplinkto;
                switch (deeplinkto) {
                    case 'audit':
                        ezgolist.tmpl.DeepLinkId = parseInt($('#deeplinkAuditLabel').data('auditid'));
                        ezgolist.tmpl.DeepLinkCompletionIsRequired = $('#deepLinkAuditIsRequired').is(":checked");
                        break;

                    case 'checklist':
                        ezgolist.tmpl.DeepLinkId = parseInt($('#deeplinkChecklistLabel').data('checklistid'));
                        ezgolist.tmpl.DeepLinkCompletionIsRequired = $('#deepLinkChecklistIsRequired').is(":checked");
                        break;

                    default:
                        ezgolist.tmpl.DeepLinkId = null;
                        ezgolist.tmpl.DeepLinkCompletionIsRequired = undefined;
                        break;
                }
            }
        }

        if ((ezgolist.listType === 'workinstruction' || ezgolist.listType === 'skillassessment') && ezgolist._firstLoadDone) {
            ezgolist.tmpl.WorkInstructionType = parseInt($('input[name="workinstructiontype"]:checked').val());
            ezgolist.tmpl.Description = $('#txtTemplateDescription').val();
        }

        if (ezgolist._firstLoadDone) {
            ezgolist._hasChanged = true;
            $('body').trigger('ezgolistChanged');
        }
    },

    fixTemplateDefaultData: function (templateType) {
        ezgolist.tmpl.RecurrencyType = (templateType === 'norecurrency' ? 'no recurrency' : templateType);

        if (ezgolist.tmpl.Recurrency != null && ezgolist.tmpl.Recurrency != undefined) {
            ezgolist.tmpl.Recurrency.RecurrencyType = ezgolist.tmpl.RecurrencyType;
        }
    },

    addItem: function () {
        ezgolist._hasChanged = true;
        var taskTmpl = Object.create(taskTemplateObj);
        let itemIndex = ezgolist.getNextIndex();
        taskTmpl.Id = ezgolist._lastNewItemId + 1;
        ezgolist._lastNewItemId = taskTmpl.Id;
        $('#txtItemName').val('');
        $('#txtItemDesc').val('');
        taskTmpl.Type = ezgolist.listType;
        taskTmpl.CompanyId = ezgolist.companyId;
        taskTmpl.Name = $('#txtItemName').val();
        taskTmpl.Description = $('#txtItemDesc').val();
        taskTmpl.Index = itemIndex;
        taskTmpl.isNew = true;
        taskTmpl.Steps = new Array();
        taskTmpl.HasPictureProof = $("#enablePictureProofForAllItems").is(':checked');
        taskTmpl.Tags = new Array();
        taskTmpl.Attachments = new Array();
        ezgolist.tmpl.TaskTemplates.push(taskTmpl);
        ezgolist.render();
        $('body').trigger('ezgolistChanged');
        return taskTmpl.Id;
    },

    updateItem: function (id) {
        ezgolist._hasChanged = true;
        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
            return obj.Id === id;
        })[0];
        if (template !== undefined) {
            template.Name = $('#txtItemName').val();
            template.Description = $('#txtItemDesc').val();
            template.Weight = parseFloat($('#txtWeight').val());
            ezgolist.render();
        }
        $('body').trigger('ezgolistChanged');
    },

    deleteItem: function () {
        let deleteItemDialogueTranslation = '';

        switch (ezgolist.listType) {
            case 'audit':
                deleteItemDialogueTranslation = ezgolist.language.deleteAuditItem;
                break;
            case 'checklist':
                deleteItemDialogueTranslation = ezgolist.language.deleteChecklistItem;
                break;
            case 'workinstruction':
                deleteItemDialogueTranslation = ezgolist.language.deleteWorkInstructionItem;
                break;
            default:
                break;
        }

        $.fn.dialogue({
            content: $("<p />").text(deleteItemDialogueTranslation),
            closeIcon: true,
            buttons: [{
                text: ezgolist.language.optionYes,
                id: $.utils.createUUID(),
                click: function ($modal) {
                    ezgolist._hasChanged = true;
                    var templateItemId = $('#btnDeleteItem').data('templateid');
                    objIndex = ezgolist.tmpl.TaskTemplates.findIndex((obj => obj.Id == templateItemId));
                    ezgolist.tmpl.TaskTemplates.splice(objIndex, 1);
                    ezgolist.render();
                    $modal.dismiss();
                    ezgolist.reindexItemsAndStages();
                    $('#editTemplateStepModal').modal('hide');
                    $('body').trigger('ezgolistChanged');
                }
            },
            {
                text: ezgolist.language.optionNo,
                id: $.utils.createUUID(),
                click: function ($modal) {
                    $modal.dismiss();
                }
            }]
        });
    },

    deleteStageTemplate: function () {
        $.fn.dialogue({
            content: $("<p />").text('Delete ' + ezgolist.listType + ' stage?'),
            closeIcon: true,
            buttons: [{
                text: "Yes",
                id: $.utils.createUUID(),
                click: function ($modal) {
                    ezgolist._hasChanged = true;
                    var templateId = parseInt($('#currentStageTemplateId').val());
                    objIndex = ezgolist.tmpl.StageTemplates.findIndex((obj => obj.Id == templateId));
                    ezgolist.tmpl.StageTemplates.splice(objIndex, 1);
                    ezgolist.render();
                    $modal.dismiss();
                    ezgolist.reindexItemsAndStages();
                    $('#editStageTemplateStepModal').modal('hide');
                    $('body').trigger('ezgolistChanged');
                }
            },
            {
                text: "No",
                id: $.utils.createUUID(),
                click: function ($modal) {
                    $modal.dismiss();
                }
            }]
        });
    },

    nextTemplate: function () {
        var templateItemId = parseInt($('#currentTemplateItemId').val());
        objIndex = ezgolist.tmpl.TaskTemplates.findIndex((obj => obj.Id == templateItemId));

        if (objIndex < ezgolist.tmpl.TaskTemplates.length - 1) {
            ezgolist.updateItem(templateItemId);
            templateItemId = ezgolist.tmpl.TaskTemplates[objIndex + 1].Id;
            ezgolist.renderDialogContent(templateItemId);
        } else {
            ezgolist.updateItem(templateItemId);
            ezgolist.addItem();
            ezgolist.renderDialogContent(ezgolist.tmpl.TaskTemplates[objIndex + 1].Id);
        }
    },

    previousTemplate: function () {
        var templateItemId = parseInt($('#currentTemplateItemId').val());
        objIndex = ezgolist.tmpl.TaskTemplates.findIndex((obj => obj.Id == templateItemId));

        if (objIndex > 0) {
            ezgolist.updateItem(templateItemId);
            templateItemId = ezgolist.tmpl.TaskTemplates[objIndex - 1].Id;
            ezgolist.renderDialogContent(templateItemId);
        }
    },

    addItemStep: function (taskTemplateId) {
        ezgolist._hasChanged = true;
        var template;

        if (ezgolist.listType !== 'task') {
            template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                return obj.Id === taskTemplateId;
            })[0];
        } else {
            template = ezgolist.tmpl;
        }

        let itemStepIndex = template.Steps === undefined ? 1 : ezgolist._itemStepLastIndex[taskTemplateId] === undefined ? template.Steps.length + 1 : ezgolist._itemStepLastIndex[taskTemplateId] + 1;
        ezgolist.updateItem(taskTemplateId);
        var taskTmplStep = Object.create(taskTemplateStepObj);
        taskTmplStep.Id = itemStepIndex;
        taskTmplStep.Description = '';
        taskTmplStep.isNew = true

        if (template.Steps === undefined) {
            template.Steps = new Array()
        }

        ezgolist._itemStepLastIndex[taskTemplateId] = itemStepIndex;
        template.Steps.push(taskTmplStep);
        template.StepsCount++;

        if (ezgolist.listType !== 'task') {
            ezgolist.renderDialogContent(taskTemplateId);
        } else {
            ezgolist.renderTaskSteps(taskTemplateId);
        }

        $('body').trigger('ezgolistChanged');
        showTemplateItemStepDialog(taskTmplStep.Id);
    },

    getNextIndex: function () {
        let taskTemplatesCount = ezgolist.tmpl.TaskTemplates === undefined ? 0 : ezgolist.tmpl.TaskTemplates.length;
        let stagesCount = ezgolist.tmpl.StageTemplates === undefined ? 0 : ezgolist.tmpl.StageTemplates.length;
        return taskTemplatesCount + stagesCount + 1;
    },

    reindexItemsAndStages: function () {
        $(ezgolist.itemListSelector + ' li').each(function (index) {
            let cid = parseInt($(this).attr('data-cid'));

            if (isNaN(cid)) {
                return;
            }

            if ($(this).attr('data-type') == 'item') {
                let taskTemplate;

                if (ezgolist.listType === 'skillassessment') {
                    taskTemplate = ezgolist.tmpl.TaskTemplates.find(obj => obj.WorkInstructionTemplateId == cid);
                }
                else {
                    taskTemplate = ezgolist.tmpl.TaskTemplates.find(obj => obj.Id == cid);
                }
                if (taskTemplate != undefined) taskTemplate.Index = index + 1;
            } else if ($(this).attr('data-type') == 'stage') {
                let stage = ezgolist.tmpl.StageTemplates.find(obj => {
                    return obj.Id == cid;
                });
                if (stage != undefined) stage.Index = index + 1;
            }
        });

        //sort arrays
        if (ezgolist.tmpl.TaskTemplates != undefined) {
            ezgolist.tmpl.TaskTemplates.sort(function (a, b) {
                if (a.Index < b.Index) {
                    return -1;
                }
                if (a.Index > b.Index) {
                    return 1;
                }
                return 0;
            });
        }
        if (ezgolist.tmpl.StageTemplates != undefined) {
            ezgolist.tmpl.StageTemplates.sort(function (a, b) {
                if (a.Index < b.Index) {
                    return -1;
                }
                if (a.Index > b.Index) {
                    return 1;
                }
                return 0;
            });
        }
    },

    addStage: function () {
        ezgolist._hasChanged = true;
        ezgolist.tmpl.StageTemplates ??= new Array();

        let stageTmpl = Object.create(stageTemplateObj);

        ezgolist.reindexItemsAndStages();

        stageTmpl.Id = ezgolist._lastNewStageId + 1;
        ezgolist._lastNewStageId = stageTmpl.Id;

        stageTmpl.Name = '';
        stageTmpl.Description = '';
        stageTmpl.BlockNextStagesUntilCompletion = false;
        stageTmpl.LockStageAfterCompletion = false;
        stageTmpl.UseShiftNotes = false;
        stageTmpl.NumberOfSignaturesRequired = 0;

        $('#txtStageName').val('');
        $('#txtStageDescription').val('');
        $('#stageSignature1').click();
        $('#do_not_lock_stages').click();
        $('#enable_shift_notes').prop('checked', false);

        stageTmpl.isNew = true;

        ezgolist.tmpl.StageTemplates.push(stageTmpl);

        ezgolist.render();

        showStageTemplateDialog(undefined, stageTmpl.Id);
        return stageTmpl.Id;
    },

    addItemWorkInstruction: function (templateId) {
        $('#AddWorkInstructionModalBody').html('');
        ezgolist.initWorkInstructionModal(ezgolist.currentWorkInstructionTemplates, templateId);
        $('#searchworkinstructions').val('')
        $('#AddWorkInstructionModal').modal('show');
    },

    addItemLink: function (taskTemplateId) {
        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
            return obj.Id === ezgolist._currentTemplateId;
        })[0];

        if (template != undefined && template) {
            template.Name = $('#txtItemName').val();
            template.Description = $('#txtItemDesc').val();
        }

        if (ezgolist.listType == 'task') {
            var link = $('#AddTaskTemplateLinkAttachmentInput').val('');
            $('#AddTaskTemplateLinkAttachmentModal').modal('show');
        }
        else if (ezgolist.listType == 'checklist') {
            var link = $('#AddChecklistItemLinkAttachmentInput').val('');
            $('#AddChecklistItemLinkAttachmentModal').modal('show');
        }
        else if (ezgolist.listType = 'audit') {
            var link = $('#AddAuditItemLinkAttachmentInput').val('');
            $('#AddAuditItemLinkAttachmentModal').modal('show');
        }
    },
    addLinkAttachmentToChecklistItem: function () {
        var currentItemId = ezgolist._currentTemplateId;
        var link = $('#AddChecklistItemLinkAttachmentInput').val();

        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
            return obj.Id === +currentItemId;
        })[0];

        if (template !== undefined) {
            template.Attachments = [];
        }

        if ($('#AddChecklistItemLinkAttachmentInput').is(":valid")) {
            var newAttachment = checklistItemAttachmentTemplateLink
                .replaceAll('{{url}}', link)
                .replaceAll('{{encodedUrl}}', encodeURIComponent(link))
                .replaceAll('{{itemid}}', template.Id);

            $('#checklistItemAttachmentsPreviewContainer').html(newAttachment);
            $('#checklistItemAttachmentsPreviewContainer').show();
            $('#checklistItemAttachmentsSelectionContainer').hide();

            var lastModified = new Date().toISOString();

            var newAttachment = {
                AttachmentType: "Link",
                FileExtension: "",
                FileName: "",
                FileType: "",
                LastModified: lastModified,
                Size: 0,
                Uri: link
            };

            template.Attachments[0] = newAttachment;
            $('#AddChecklistItemLinkAttachmentModal').modal('hide');

            ezgolist.renderDialogContent(ezgolist._currentTemplateId);

            //handle remove
            $('#deleteChecklistItemAttachmentLink-' + ezgolist._currentTemplateId).off('click').on('click', function (elem) {
                var currentItemId = $(this).attr('id').replace('deleteChecklistItemAttachmentLink-', '');

                var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                    return obj.Id === +currentItemId;
                })[0];

                if (template != undefined && template) {
                    template.Name = $('#txtItemName').val();
                    template.Description = $('#txtItemDesc').val();
                }

                template.Attachments = [];
                ezgolist.renderDialogContent(ezgolist._currentTemplateId);
            });
        }
        else {
            $('#ChecklistItemLinkAttachmentInvalidError').show();
        }
    },
    addLinkAttachmentToAuditItem: function () {
        var currentItemId = ezgolist._currentTemplateId;
        var link = $('#AddAuditItemLinkAttachmentInput').val();

        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
            return obj.Id === +currentItemId;
        })[0];

        if (template !== undefined) {
            template.Attachments = [];
        }

        if ($('#AddAuditItemLinkAttachmentInput').is(":valid")) {
            var newAttachment = auditItemAttachmentTemplateLink
                .replaceAll('{{url}}', link)
                .replaceAll('{{encodedUrl}}', encodeURIComponent(link))
                .replaceAll('{{itemid}}', template.Id);

            $('#auditItemAttachmentsPreviewContainer').html(newAttachment);
            $('#auditItemAttachmentsPreviewContainer').show();

            $('#auditItemAttachmentsSelectionContainer').hide();

            var lastModified = new Date().toISOString();

            var newAttachment = {
                AttachmentType: "Link",
                FileExtension: "",
                FileName: "",
                FileType: "",
                LastModified: lastModified,
                Size: 0,
                Uri: link
            };

            template.Attachments[0] = newAttachment;
            $('#AddAuditItemLinkAttachmentModal').modal('hide');

            ezgolist.renderDialogContent(ezgolist._currentTemplateId);

            //handle remove
            $('#deleteAuditItemAttachmentLink-' + ezgolist._currentTemplateId).off('click').on('click', function (elem) {
                var currentItemId = $(this).attr('id').replace('deleteAuditItemAttachmentLink-', '');

                var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                    return obj.Id === +currentItemId;
                })[0];

                if (template != undefined && template) {
                    template.Name = $('#txtItemName').val();
                    template.Description = $('#txtItemDesc').val();
                }

                template.Attachments = [];
                ezgolist.renderDialogContent(ezgolist._currentTemplateId);
            });
        }
        else {
            $('#AuditItemLinkAttachmentInvalidError').show();
        }
    },
    addLinkAttachmentToTaskTemplate: function () {
        var link = $('#AddTaskTemplateLinkAttachmentInput').val();

        var template = ezgolist.tmpl;

        if (template !== undefined) {
            template.Attachments = [];
        }

        if ($('#AddTaskTemplateLinkAttachmentInput').is(":valid")) {
            var newAttachment = taskTemplateAttachmentTemplateLink
                .replaceAll('{{url}}', link)
                .replaceAll('{{encodedUrl}}', encodeURIComponent(link))
                .replaceAll('{{itemid}}', template.Id);

            $('#taskTemplateAttachmentsPreviewContainer').html(newAttachment);
            $('#taskTemplateAttachmentsPreviewContainer').show();

            $('#taskTemplateAttachmentsSelectionContainer').hide();

            var lastModified = new Date().toISOString();

            var newAttachment = {
                AttachmentType: "Link",
                FileExtension: "",
                FileName: "",
                FileType: "",
                LastModified: lastModified,
                Size: 0,
                Uri: link
            };

            template.Attachments[0] = newAttachment;
            $('#AddTaskTemplateLinkAttachmentModal').modal('hide');

            ezgolist.renderTaskSteps(ezgolist.tmpl.Id);

            //handle remove
            $('#deleteTaskTemplateAttachmentLink-' + ezgolist.tmpl.Id).off('click').on('click', function (elem) {
                var currentItemId = $(this).attr('id').replace('deleteTaskTemplateAttachmentLink-', '');

                ezgolist.tmpl.Attachments = [];
                ezgolist.renderTaskSteps(ezgolist.tmpl.Id);
            });
        }
        else {
            $('#TaskTemplateLinkAttachmentInvalidError').show();
        }
    },

    initWorkInstructionModal: function (data, templateId) {
        $.each(data, function (index, elem) {
            var image = ezgolist.placeholderImageUrl;

            if (elem.Picture != "" && elem.Picture != undefined) {
                image = ezgolist.mediaUrl + elem.Picture;
            }

            $('#AddWorkInstructionModalBody').append('<div class="row m-1" style="border: 1px solid #eaecef" id="workinstruction-' + elem.Id + '">' +
                '<div class="col-3">' +
                '<img id="testTemplateImage" class="pb-2" style="width:100%;object-fit: cover; object-position: center; height:7rem;" class="img-fluid" alt="..." data-src=' + image + '>' +
                '</div>' +
                '<div class="col-9">' +
                '<div class="row">' +
                '<div class="col-9">' +
                '<h5>' + elem.Name + '</h5>' +
                '<h6>' + elem.Description + '</h6>' +
                '<span>' + elem.AreaPath + '</span>' +
                '</div>' +
                '<div class="col-3" style="display: flex; align-items: center">' +
                '<button data-workinstructionid="' + elem.Id + '" data-templateid="' + templateId + '"  class="btn btn-ezgo btn-sm shadow-sm" onclick="ezgolist.assignItemWorkInstruction(this)">' + ezgolist.language.assignButton + '</button>' +
                '</div>' +
                '</div>' +
                '</div>' +
                '</div>');
        });
        ezgomediafetcher.preloadImagesAndVideos();
        ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
        //ezgomediafetcher.preloadImagesAndVideos();
    },

    searchWorkInstructions: function (term) {
        var toShow = ezgolist.currentWorkInstructionTemplates.filter(w => w.Name.toLowerCase().includes(term.toLowerCase()));
        var toHide = ezgolist.currentWorkInstructionTemplates.filter(w => !w.Name.toLowerCase().includes(term.toLowerCase()));

        $.each(toShow, function (index, elem) {
            $('#workinstruction-' + elem.Id).show();
        });

        $.each(toHide, function (index, elem) {
            $('#workinstruction-' + elem.Id).hide();
        });
    },

    assignItemWorkInstruction: function (elem) {
        if (ezgolist.listType == "audit" || ezgolist.listType == "checklist") {
            var taskTemplate = ezgolist.tmpl.TaskTemplates.filter(t => t.Id == $(elem).data('templateid'))[0];
            var index = ezgolist.tmpl.TaskTemplates.indexOf(taskTemplate);

            ezgolist.tmpl.TaskTemplates[index].WorkInstructionRelations = [];
            ezgolist.tmpl.TaskTemplates[index].WorkInstructionRelations[0] = {};
            ezgolist.tmpl.TaskTemplates[index].WorkInstructionRelations[0].WorkInstructionTemplateId = $(elem).data('workinstructionid');
            ezgolist.tmpl.TaskTemplates[index].WorkInstructionRelations[0].TaskTemplateId = $(elem).data('templateid');

            if (ezgolist.listType == "checklist") {
                ezgolist.tmpl.TaskTemplates[index].WorkInstructionRelations[0].ChecklistTemplateId = ezgolist.tmpl.Id;
            } else if (ezgolist.listType == "audit") {
                ezgolist.tmpl.TaskTemplates[index].WorkInstructionRelations[0].AuditTemplateId = ezgolist.tmpl.Id;
            }
        } else if (ezgolist.listType == "task") {
            ezgolist.tmpl.WorkInstructionRelations = [];
            ezgolist.tmpl.WorkInstructionRelations[0] = {};
            ezgolist.tmpl.WorkInstructionRelations[0].WorkInstructionTemplateId = $(elem).data('workinstructionid');
            ezgolist.tmpl.WorkInstructionRelations[0].TaskTemplateId = $(elem).data('templateid');
        }

        var itemName = $('#txtItemName').val();
        var itemDesc = $('#txtItemDesc').val();
        ezgolist._hasChanged = true;

        if (ezgolist.listType !== 'task') {
            ezgolist.renderDialogContent($(elem).data('templateid'));
        } else {
            ezgolist.renderTaskSteps($(elem).data('templateid'));
        }

        $('#txtItemName').val(itemName);
        $('#txtItemDesc').val(itemDesc);
        $('#AddWorkInstructionModal').modal('hide');
        $('body').trigger('ezgolistChanged');
    },

    removeWorkInstructionRelations: function (templateId) {
        if (ezgolist.listType == "task") {
            ezgolist.tmpl.WorkInstructionRelations = [];
        } else if (ezgolist.listType == "audit" || ezgolist.listType == "checklist") {
            var step = ezgolist.tmpl.TaskTemplates.filter(obj => {
                return obj.Id === templateId;
            })[0];
            step.WorkInstructionRelations = [];
        }

        var itemName = $('#txtItemName').val();
        var itemDesc = $('#txtItemDesc').val();
        ezgolist._hasChanged = true;

        if (ezgolist.listType !== 'task') {
            ezgolist.renderDialogContent(templateId);
        } else {
            ezgolist.renderTaskSteps(templateId);
        }

        $('#txtItemName').val(itemName);
        $('#txtItemDesc').val(itemDesc);
        $('body').trigger('ezgolistChanged');
    },

    updateItemStep: function (templateStepId) {
        ezgolist._hasChanged = true;
        var templateId = parseInt($('#currentTemplateItemId').val());
        var template;

        if (ezgolist.listType !== 'task') {
            template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                return obj.Id === templateId;
            })[0];
        } else {
            template = ezgolist.tmpl;
        }

        var step = template.Steps.filter(obj => {
            return obj.Id === templateStepId;
        })[0];

        if (step !== undefined) {
            step.Description = $('#txtStepDescription').val();
            if (ezgolist.listType !== 'task') {
                ezgolist.renderDialogContent(templateId);
            } else {
                ezgolist.renderTaskSteps(templateId);
            }
        }
        $('body').trigger('ezgolistChanged');
    },

    updateStageTemplate: function (stageTemplateId) {
        ezgolist._hasChanged = true;

        ezgolist.reindexItemsAndStages();

        var templateId = parseInt($('#currentStageTemplateId').val());

        if (templateId != undefined && templateId > 0) {

            var stageTemplate = ezgolist.tmpl.StageTemplates.filter(obj => {
                return obj.Id === templateId;
            })[0];
            if (stageTemplate !== undefined) {
                stageTemplate.Name = $('#txtStageName').val();
                stageTemplate.Description = $('#txtStageDescription').val();

                var numberOfSignaturesRequired = 0;

                if ($('#stageSignature2:checked').val() != undefined || $('#stageSignature2Label').hasClass('active')) {
                    numberOfSignaturesRequired = 1;
                }
                else if ($('#stageSignature3:checked').val() != undefined || $('#stageSignature3Label').hasClass('active')) {
                    numberOfSignaturesRequired = 2;
                }

                stageTemplate.NumberOfSignaturesRequired = numberOfSignaturesRequired;

                var lockAndBlockStage = ($('#lock_next_stages:checked').val() == undefined && !$('#lock_next_stages_label').hasClass('active')) ? false : true;

                stageTemplate.BlockNextStagesUntilCompletion = lockAndBlockStage;
                stageTemplate.LockStageAfterCompletion = lockAndBlockStage;

                stageTemplate.UseShiftNotes = ($('#enable_shift_notes:checked').val() == undefined && !$('#enable_shift_notes_label').hasClass('active')) ? false : true;

                ezgolist.render();
            }
        }
    },

    deleteItemStep: function () {
        var templateId = parseInt($('#currentTemplateItemId').val());
        var stepid = $('#btnDeleteStep').data('stepid');
        var template;

        if (ezgolist.listType !== 'task') {
            template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                return obj.Id === templateId;
            })[0];
        } else {
            template = ezgolist.tmpl;
        }

        var step = template.Steps.filter(obj => {
            return obj.Id === stepid;
        })[0];

        let deleteItemInstructionDialogueTranslation = '';

        switch (ezgolist.listType) {
            case 'audit':
                deleteItemInstructionDialogueTranslation = ezgolist.language.deleteAuditInstruction;
                break;
            case 'checklist':
                deleteItemInstructionDialogueTranslation = ezgolist.language.deleteChecklistInstruction;
                break;
            case 'task':
                deleteItemInstructionDialogueTranslation = ezgolist.language.deleteTaskInstruction;
                break;
            default:
                break;
        }

        $.fn.dialogue({
            content: $("<p />").text(deleteItemInstructionDialogueTranslation),
            closeIcon: true,
            buttons: [{
                text: ezgolist.language.optionYes,
                id: $.utils.createUUID(),
                click: function ($modal) {
                    ezgolist._hasChanged = true;
                    stepIndex = template.Steps.findIndex((obj => obj.Id == stepid));
                    template.Steps.splice(stepIndex, 1);
                    template.StepsCount--;

                    if (ezgolist.listType !== 'task') {
                        ezgolist.renderDialogContent(templateId);
                    } else {
                        ezgolist.renderTaskSteps(templateId);
                    }

                    $modal.dismiss();
                    $('#editTemplateStepDetailModal').modal('hide');
                    $('body').trigger('ezgolistChanged');
                }
            },
            {
                text: ezgolist.language.optionNo,
                id: $.utils.createUUID(),
                click: function ($modal) {
                    $modal.dismiss();
                }
            }]
        });
    },

    nextTemplateStep: function () {
        var templateItemId = parseInt($('#currentTemplateItemId').val());
        var templateItemStepId = parseInt($('#currentTemplateItemStepId').val());
        objIndex = ezgolist.tmpl.TaskTemplates.findIndex((obj => obj.Id == templateItemId));
        var template;

        if (ezgolist.listType !== 'task') {
            template = ezgolist.tmpl.TaskTemplates[objIndex];
        } else {
            template = ezgolist.tmpl;
        }

        stepIndex = template.Steps.findIndex((obj => obj.Id == templateItemStepId));

        if (stepIndex < template.Steps.length - 1) {
            ezgolist.updateItemStep(templateItemStepId);
            templateItemStepId = template.Steps[stepIndex + 1].Id;
            ezgolist.renderStepDialogContent(templateItemStepId);
        } else {
            ezgolist.updateItemStep(templateItemStepId);
            ezgolist.addItemStep(templateItemId);
            ezgolist.renderDialogContent(templateItemId);
            ezgolist.renderStepDialogContent(template.Steps[template.Steps.length - 1].Id);
        }
    },

    previousTemplateStep: function () {
        var templateItemId = parseInt($('#currentTemplateItemId').val());
        var templateItemStepId = parseInt($('#currentTemplateItemStepId').val());
        objIndex = ezgolist.tmpl.TaskTemplates.findIndex((obj => obj.Id == templateItemId));
        var template;

        if (ezgolist.listType !== 'task') {
            template = ezgolist.tmpl.TaskTemplates[objIndex];
        } else {
            template = ezgolist.tmpl;
        }

        stepIndex = template.Steps.findIndex((obj => obj.Id == templateItemStepId));

        if (stepIndex > 0) {
            ezgolist.updateItemStep(templateItemStepId);
            templateItemStepId = template.Steps[stepIndex - 1].Id;
            ezgolist.renderStepDialogContent(templateItemStepId);
        }
    },

    deleteMedia: function (e) {
        var type = $(e.currentTarget).data('type');
        var scrollTop = $('#mainCol').scrollTop();
        switch (type) {
            case 'template':
                delete ezgolist.tmpl.Picture;
                delete ezgolist.tmpl.PictureType;
                delete ezgolist.tmpl.Video;
                delete ezgolist.tmpl.VideoThumbnail;
                delete ezgolist.tmpl.VideoType;
                break;

            case 'item':
                var templateId = parseInt($(e.currentTarget).data('templateid'));
                var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                    return obj.Id === templateId;
                })[0];
                delete template.Picture;
                delete template.PictureType;
                delete template.VideoThumbnail;
                delete template.Video;
                delete template.VideoType;
                //get current item and save basic info before render;
                let currentTemplateItemId = parseInt($('#currentTemplateItemId').val());
                ezgolist.updateItem(currentTemplateItemId);
                ezgolist.renderDialogContent(currentTemplateItemId);
                break;

            case 'step':
                var templateId = parseInt($(e.currentTarget).data('templateid'));
                var templateStepId = parseInt($(e.currentTarget).data('templatestepid'));
                var template;

                if (ezgolist.listType === 'task') {
                    template = ezgolist.tmpl;
                } else {
                    template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                        return obj.Id === templateId;
                    })[0];
                }

                var step = template.Steps.filter(obj => {
                    return obj.Id === templateStepId;
                })[0];
                delete step.Picture;
                delete step.PictureType;
                delete step.VideoThumbnail;
                delete step.Video;
                delete step.VideoType;
                let currentTemplateItemStepId = parseInt($('#currentTemplateItemStepId').val());
                ezgolist.updateItemStep(currentTemplateItemStepId);

                if (ezgolist.listType === 'task') {
                    ezgolist.renderTaskSteps(templateId);
                } else {
                    ezgolist.renderDialogContent(templateId);
                }

                ezgolist.renderStepDialogContent(templateStepId);
                break;

            case 'pdf':
                var templateId = parseInt($(e.currentTarget).data('templateid'));
                if (ezgolist.listType === 'task') {
                    delete ezgolist.tmpl.DescriptionFile;
                    ezgolist.tmpl.Attachments = [];

                    ezgolist.renderTaskSteps(templateId);
                } else {
                    var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                        return obj.Id === templateId;
                    })[0];

                    if (template != undefined && template) {
                        template.Name = $('#txtItemName').val();
                        template.Description = $('#txtItemDesc').val();
                    }

                    delete template.DescriptionFile;
                    template.Attachments = [];
                    ezgolist.renderDialogContent(templateId);
                }
                break;
        }
        ezgolist.render();
        $('#mainCol').scrollTop(scrollTop);
    },

    uploadMedia: function (url, ext, delay) {
        var xhr = new XMLHttpRequest();
        xhr.open('GET', url, true);
        xhr.responseType = 'blob';
        xhr.onload = function (e) {
            if (this.status == 200) {
                ezgolist._hasChanged = true;
                var myBlob = this.response;
                // myBlob is now the blob that the object URL pointed to.
                var file = new File([myBlob], $.utils.createUUID() + ext);
                var formdata = new FormData();
                formdata.append("file", file);
                $('body').trigger('ezgolistChanged');

                return (new Promise(resolve => { setTimeout(() => resolve(delay), delay) }))
                    .then(d => `Waited ${d} seconds`);
            }
        };
        xhr.send();
    },

    uploadMediaEx: async function (spec) {
        let blob = await fetch(spec.url).then(
            r => r.blob()
        );
        var file = new File([blob], $.utils.createUUID() + spec.ext);
        var formData = new FormData();
        formData.append("file", file);
        formData.append("filekind", spec.kind);
        formData.append("itemtype", spec.type);

        return fetch('/' + ezgolist.listType + '/upload', {
            method: 'POST',
            body: formData,
        }).then(async function (response) {
            var responsetext = await response.text();
            if (!response.ok || responsetext == "" || responsetext.startsWith("An error occurred while connecting or getting data from the API")) {
                throw Error(response.statusText);
            }
            return responsetext;
        }).then(path => {
            ezgolist._hasChanged = true;
            var newPath = String(path).replaceAll('"', '');
            switch (spec.type) {
                case 'template':
                    switch (spec.kind) {
                        case 'pic':
                            ezgolist.tmpl.Picture = newPath;
                            break;

                        case 'thumb':
                            ezgolist.tmpl.VideoThumbnail = newPath;
                            break;

                        case 'video':
                            ezgolist.tmpl.Video = newPath;
                            break;
                    }
                    break;

                case 'item':
                    switch (spec.kind) {
                        case 'pic':
                            ezgolist.tmpl.TaskTemplates[spec.index].Picture = newPath;
                            break;

                        case 'thumb':
                            ezgolist.tmpl.TaskTemplates[spec.index].VideoThumbnail = newPath;
                            break;

                        case 'video':
                            ezgolist.tmpl.TaskTemplates[spec.index].Video = newPath;
                            break;

                        case 'doc':
                            ezgolist.tmpl.TaskTemplates[spec.index].DescriptionFile = newPath;
                            break;
                    }
                    break;

                case 'step':
                    switch (spec.kind) {
                        case 'pic':
                            if (ezgolist.listType === 'task') {
                                ezgolist.tmpl.Steps[spec.stepindex].Picture = newPath;
                            } else {
                                ezgolist.tmpl.TaskTemplates[spec.index].Steps[spec.stepindex].Picture = newPath;
                            }
                            break;

                        case 'thumb':
                            if (ezgolist.listType === 'task') {
                                ezgolist.tmpl.Steps[spec.stepindex].VideoThumbnail = newPath;
                            } else {
                                ezgolist.tmpl.TaskTemplates[spec.index].Steps[spec.stepindex].VideoThumbnail = newPath;
                            }
                            break;

                        case 'video':
                            if (ezgolist.listType === 'task') {
                                ezgolist.tmpl.Steps[spec.stepindex].Video = newPath;
                            } else {
                                ezgolist.tmpl.TaskTemplates[spec.index].Steps[spec.stepindex].Video = newPath;
                            }
                            break;
                    }
                    break;

                case 'doc':
                    ezgolist.tmpl.DescriptionFile = newPath;
                    break;

                case 'wiattachment':
                    ezgolist.tmpl.TaskTemplates[spec.index].Attachments[0].Uri = newPath;

                default:
                    break;
            }
        }).catch(function (error) {
            throw Error(error);
        });
        $('body').trigger('ezgolistChanged');
    },

    delete: function () {
        let deleteTemplateDialogueTranslation = '';

        switch (ezgolist.listType) {
            case 'audit':
                deleteTemplateDialogueTranslation = ezgolist.language.deleteAuditTemplate;
                break;
            case 'checklist':
                deleteTemplateDialogueTranslation = ezgolist.language.deleteChecklistTemplate;
                break;
            case 'skillassessment':
                deleteTemplateDialogueTranslation = ezgolist.language.deleteSkillAssessmentTemplate;
                break;
            case 'task':
                deleteTemplateDialogueTranslation = ezgolist.language.deleteTaskTemplate;
                break;
            case 'workinstruction':
                deleteTemplateDialogueTranslation = ezgolist.language.deleteWorkInstructionTemplate;
                break;
            default:
                break;
        }

        $.fn.dialogue({
            content: $("<p />").text(deleteTemplateDialogueTranslation),
            closeIcon: true,
            buttons: [{
                text: ezgolist.language.optionYes,
                id: $.utils.createUUID(),
                click: function ($modal) {
                    ezgolist._hasChanged = false;
                    $('#frmDelete').submit();
                    $modal.dismiss();
                }
            },
            {
                text: ezgolist.language.optionNo,
                id: $.utils.createUUID(),
                click: function ($modal) {
                    $modal.dismiss();
                }
            }]
        });
    },

    //When executeDelayedSave is true, will set it to false and then save().
    //Is called when postrecurrency(event) is completed.
    tryDelayedSave: function () {
        if (ezgolist.executeDelayedSave) {
            ezgolist.executeDelayedSave = false;
            toastr.remove();
            ezgolist.save();
        }
    },

    save: async function () {
        ezgolist.reindexItemsAndStages();

        // For skillassessment, sync TaskTemplates array with current DOM order
        if (ezgolist.listType === 'skillassessment') {
            var currentDOMOrder = [];
            $(ezgolist.itemListSelector + ' li[data-type="item"]').each(function () {
                var cid = parseInt($(this).attr('data-cid'));
                if (!isNaN(cid)) {
                    currentDOMOrder.push(cid);
                }
            });

            if (currentDOMOrder.length > 0 && currentDOMOrder.length === ezgolist.tmpl.TaskTemplates.length) {
                var reordered = [];
                var usedIndices = new Set();

                currentDOMOrder.forEach(function (cid) {
                    // For skillassessment, data-cid is WorkInstructionTemplateId
                    var templateIndex = ezgolist.tmpl.TaskTemplates.findIndex(function (t) {
                        return t.WorkInstructionTemplateId === cid && !usedIndices.has(ezgolist.tmpl.TaskTemplates.indexOf(t));
                    });

                    if (templateIndex !== -1) {
                        usedIndices.add(templateIndex);
                        reordered.push(ezgolist.tmpl.TaskTemplates[templateIndex]);
                    }
                });

                // Only apply reorder if we matched all items
                if (reordered.length === ezgolist.tmpl.TaskTemplates.length) {
                    ezgolist.tmpl.TaskTemplates = reordered;
                }

                // Re-index
                ezgolist.tmpl.TaskTemplates.forEach(function (template, index) {
                    template.Index = index + 1;
                });
            }
        }

        if (!ezgolist.validateTemplate()) {
            return;
        }

        //When postrecurrency(event) has not returned a value yet, delay the save function.
        //tryDelayedSave() is called at the end of postrecurrency(event).
        if (ezgolist.recurrencyIsUpToDate == false) {
            if (ezgolist.executeDelayedSave) {
                //if already waiting for a delayed save, exit and keep waiting.
                return;
            }
            //set executeDelayedSave tp true and exit function. tryDelayedSave() will be called from postrecurrency(event).
            ezgolist.executeDelayedSave = true;
            toastr.success('Waiting for server response.');
            return;
        }

        toastr.remove();
        $('body').toggleClass('loaded');
        ezgolist._mediaarray.splice(0, ezgolist._mediaarray.length);

        if (ezgolist.tmpl.Picture !== undefined) {
            if (String(ezgolist.tmpl.Picture).startsWith('blob:')) {
                var url = ezgolist.tmpl.Picture;
                var ext = '.' + ezgolist.tmpl.PictureType;
                ezgolist._mediaarray.push({ type: 'template', url: url, ext: ext, delay: 1000, index: 0, stepindex: 0, kind: 'pic' });
            }
        }

        if (ezgolist.listType === 'task') {
            ezgolist.tmpl.Recurrency.AreaId = ezgolist.tmpl.AreaId;
            if (ezgolist.tmpl.DeepLinkTo === 'none') {
                ezgolist.tmpl.DeepLinkTo = '';
                ezgolist.tmpl.DeepLinkId = null;
                ezgolist.tmpl.DeepLinkCompletionIsRequired = undefined;
            }

            if ($('#plannedtime').val() == undefined || $('#plannedtime').val() == '') {
                ezgolist.tmpl.PlannedTime = null;
            } //added check if no planned time then reset field so it will reset in db

            if (ezgolist.tmpl.Steps.length > 0) {
                $(ezgolist.tmpl.Steps).each(function (stepindex, step) {
                    if (step.Picture !== undefined) {
                        if (String(step.Picture).startsWith('blob:') || String(step.Picture).startsWith('data:')) {
                            var url = step.Picture;
                            var ext = '.' + step.PictureType;
                            ezgolist._mediaarray.push({ type: 'step', url: url, ext: ext, delay: 1000, index: 0, stepindex: stepindex, kind: 'pic' });
                        }
                    }

                    if (step.VideoThumbnail !== undefined) {
                        if (String(step.VideoThumbnail).startsWith('blob:') || String(step.VideoThumbnail).startsWith('data:')) {
                            var url = step.VideoThumbnail;
                            var ext = '.jpg';
                            ezgolist._mediaarray.push({ type: 'step', url: url, ext: ext, delay: 1000, index: 0, stepindex: stepindex, kind: 'thumb' });
                        }
                    }

                    if (step.Video !== undefined) {
                        if (String(step.Video).startsWith('blob:') || String(step.Video).startsWith('data:')) {
                            var url = step.Video;
                            var ext = '.' + step.VideoType;
                            ezgolist._mediaarray.push({ type: 'step', url: url, ext: ext, delay: 1000, index: 0, stepindex: stepindex, kind: 'video' });
                        }
                    }
                });
            }

            if (ezgolist.tmpl.DescriptionFile !== undefined) {
                if (String(ezgolist.tmpl.DescriptionFile).startsWith('blob:')) {
                    var url = ezgolist.tmpl.DescriptionFile;
                    var ext = '.pdf';
                    ezgolist._mediaarray.push({ type: 'doc', url: url, ext: ext, delay: 1000, index: 0, stepindex: 0, kind: 'doc' });
                }
            }

            if (ezgolist.tmpl.VideoThumbnail !== undefined) {
                if (String(ezgolist.tmpl.VideoThumbnail).startsWith('blob:') || String(ezgolist.tmpl.VideoThumbnail).startsWith('data:')) {
                    var url = ezgolist.tmpl.VideoThumbnail;
                    var ext = '.jpg';
                    ezgolist._mediaarray.push({ type: 'template', url: url, ext: ext, delay: 1000, index: 0, stepindex: 0, kind: 'thumb' });
                }
            }

            if (ezgolist.tmpl.Video !== undefined) {
                if (String(ezgolist.tmpl.Video).startsWith('blob:') || String(ezgolist.tmpl.Video).startsWith('data:')) {
                    var url = ezgolist.tmpl.Video;
                    var ext = '.' + ezgolist.tmpl.VideoType;
                    ezgolist._mediaarray.push({ type: 'template', url: url, ext: ext, delay: 1000, index: 0, stepindex: 0, kind: 'video' });
                }
            }
        }

        $(ezgolist.tmpl.TaskTemplates).each(function (itemindex, item) {
            if (item.Picture !== undefined) {
                if (String(item.Picture).startsWith('blob:') || String(item.Picture).startsWith('data:')) {
                    var url = item.Picture;
                    var ext = '.' + item.PictureType;
                    ezgolist._mediaarray.push({ type: 'item', url: url, ext: ext, delay: 1000, index: itemindex, stepindex: 0, kind: 'pic' });
                }
            }

            if (item.VideoThumbnail !== undefined) {
                if (String(item.VideoThumbnail).startsWith('blob:') || String(item.VideoThumbnail).startsWith('data:')) {
                    var url = item.VideoThumbnail;
                    var ext = '.jpg';
                    ezgolist._mediaarray.push({ type: 'item', url: url, ext: ext, delay: 1000, index: itemindex, stepindex: 0, kind: 'thumb' });
                }
            }

            if (item.Video !== undefined) {
                if (String(item.Video).startsWith('blob:') || String(item.Video).startsWith('data:')) {
                    var url = item.Video;
                    var ext = '.' + item.VideoType;
                    ezgolist._mediaarray.push({ type: 'item', url: url, ext: ext, delay: 1000, index: itemindex, stepindex: 0, kind: 'video' });
                }
            }

            if (item.DescriptionFile !== undefined) {
                if (String(item.DescriptionFile).startsWith('blob:')) {
                    var url = item.DescriptionFile;
                    var ext = '.pdf';
                    ezgolist._mediaarray.push({ type: 'item', url: url, ext: ext, delay: 1000, index: itemindex, stepindex: 0, kind: 'doc' });
                }
            }

            if (!Array.isArray(item.Steps)) {
                item.Steps = [];
            }

            if (ezgolist.listType === 'workinstruction') {
                if (item.Attachments != null && item.Attachments.length) {
                    $(item.Attachments).each(function (index, attachment) {
                        if (String(attachment.Uri).startsWith('blob:')) {
                            ezgolist._mediaarray.push({ type: 'wiattachment', url: attachment.Uri, delay: 1000, index: itemindex, ext: ".pdf", kind: 'doc' });
                        }
                    })
                }
            }

            if (item.Steps.length > 0) {
                $(item.Steps).each(function (stepindex, step) {
                    if (step.Picture !== undefined) {
                        if (String(step.Picture).startsWith('blob:') || String(step.Picture).startsWith('data:')) {
                            var url = step.Picture;
                            var ext = '.' + step.PictureType;
                            ezgolist._mediaarray.push({ type: 'step', url: url, ext: ext, delay: 1000, index: itemindex, stepindex: stepindex, kind: 'pic' });
                        }
                    }

                    if (step.VideoThumbnail !== undefined) {
                        if (String(step.VideoThumbnail).startsWith('blob:') || String(step.VideoThumbnail).startsWith('data:')) {
                            var url = step.VideoThumbnail;
                            var ext = '.jpg';
                            ezgolist._mediaarray.push({ type: 'step', url: url, ext: ext, delay: 1000, index: itemindex, stepindex: stepindex, kind: 'thumb' });
                        }
                    }

                    if (step.Video !== undefined) {
                        if (String(step.Video).startsWith('blob:') || String(step.Video).startsWith('data:')) {
                            var url = step.Video;
                            var ext = '.' + step.VideoType;
                            ezgolist._mediaarray.push({ type: 'step', url: url, ext: ext, delay: 1000, index: itemindex, stepindex: stepindex, kind: 'video' });
                        }
                    }
                });
            }
        });

        const starterPromise = Promise.resolve(null);
        const log = result => console.log(result);
        var uploadError = false;

        await ezgolist._mediaarray.reduce(
            (p, spec) => p.then(() => ezgolist.uploadMediaEx(spec).catch(() => uploadError = true)),
            starterPromise
        );

        if (ezgolist.sharedTemplateId > 0) {
            ezgolist.tmpl.SharedTemplateId = ezgolist.sharedTemplateId;
        }

        var endpoint = '/' + ezgolist.listType + '/settemplate';
        if (uploadError) {
            toastr.error('Error uploading media! Template not saved!');

            $('body').toggleClass('loaded');
            ezgolist._hasChanged = true;
            $('body').trigger('ezgolistChanged');
        }
        else if (ezgolist.listType === 'workinstruction') {
            var wiTemplateChangesNotifComment = $('#workInstructionChangesNotificationComment').val();
            var wiExtendedToPost = {
                WorkInstructionTemplate: ezgolist.tmpl,
                NotificationComment: wiTemplateChangesNotifComment
            };
            var sendNotificationParam = '';
            if (ezgolist.sendWiChangeNotification) {
                sendNotificationParam = '?sendChangesNotification=true';
            }
            $.ajax({
                type: "POST",
                url: endpoint + sendNotificationParam,
                data: JSON.stringify(wiExtendedToPost),
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    ezgolist.tmpl = JSON.parse(data);

                    if (ezgolist.tmpl.TaskTemplates === undefined || ezgolist.tmpl.TaskTemplates === null) {
                        ezgolist.tmpl.TaskTemplates = new Array();
                    }

                    ezgolist.render();
                    ezgolist.fixRecurrencyEmptyFields(); //add fix for .net model state issue. 
                    toastr.success('Template saved successfully')
                    $('body').toggleClass('loaded');
                    ezgolist._hasChanged = false;
                    $('body').trigger('ezgolistChanged');

                    if (ezgolist._isNewTemplate) {
                        if (ezgolist.listType == 'workinstruction' || ezgolist.listType == 'skillassessment') {
                            location.href = '/' + ezgolist.listType + 's/details/' + ezgolist.tmpl.Id;
                        }
                        else {
                            location.href = '/' + ezgolist.listType + '/details/' + ezgolist.tmpl.Id;
                        }
                    }

                    if (auditingLog != undefined) {
                        auditingLog.resetChangeLog();
                    }
                    $('#WIChangesModal').modal('hide');
                    ezgolist.initialTmpl = structuredClone(ezgolist.tmpl);
                    $('#workInstructionChangesNotificationComment').val('');
                },
                error: function (ex) {
                    $('body').toggleClass('loaded');
                    ezgolist._hasChanged = false;
                    $('body').trigger('ezgolistChanged');
                    toastr.error('Issue occurred while saving, please try again later.');
                }
            });
        }
        else {
            $.ajax({
                type: "POST",
                url: endpoint,
                data: JSON.stringify(ezgolist.tmpl),
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    ezgolist.tmpl = JSON.parse(data);

                    if (ezgolist.tmpl.TaskTemplates === undefined || ezgolist.tmpl.TaskTemplates === null) {
                        ezgolist.tmpl.TaskTemplates = new Array();
                    }

                    ezgolist.render();
                    ezgolist.fixRecurrencyEmptyFields(); //add fix for .net model state issue. 
                    toastr.success('Template saved successfully')
                    $('body').toggleClass('loaded');
                    ezgolist._hasChanged = false;
                    $('body').trigger('ezgolistChanged');

                    if (ezgolist._isNewTemplate) {
                        if (ezgolist.listType == 'workinstruction' || ezgolist.listType == 'skillassessment') {
                            location.href = '/' + ezgolist.listType + 's/details/' + ezgolist.tmpl.Id;
                        }
                        else {
                            location.href = '/' + ezgolist.listType + '/details/' + ezgolist.tmpl.Id;
                        }
                    }

                    if (auditingLog != undefined) {
                        auditingLog.resetChangeLog();
                    }
                },
                error: function (ex) {
                    $('body').toggleClass('loaded');
                    ezgolist._hasChanged = false;
                    $('body').trigger('ezgolistChanged');
                    toastr.error('Issue occurred while saving, please try again later.');
                }
            });
        }
    },

    fixRecurrencyEmptyFields: function () {
        if (ezgolist.listType === 'task') {
            $('#recurrence-form #Id').val(ezgolist.tmpl.Recurrency.Id);
            $('#CompanyId').val(ezgolist.tmpl.Recurrency.CompanyId);
            $('#AreaId').val(ezgolist.tmpl.Recurrency.AreaId);
            $('#TemplateId').val(ezgolist.tmpl.Recurrency.TemplateId);
        }
    },

    duplicate: function () {
        $('#frmDuplicate').submit();
    },

    share: function () {
        var companyIds = [];
        $('[id^=checkbox-share-company]').each(function (index, element) {
            if (element.checked)
                companyIds.push(element.dataset.id);
        });

        if (ezgolist._hasChanged) {
            toastr.warning('Please save changes to template before sharing');
        }
        else if (companyIds.length > 0) {
            var endpoint = '/' + ezgolist.listType + '/share/' + ezgolist.tmpl.Id;

            $('body').toggleClass('loaded');
            $('#btn-share-template').prop('disabled', true);

            $.ajax({
                type: "POST",
                url: endpoint,
                data: JSON.stringify(companyIds),
                contentType: 'application/json; charset=utf-8',
                success: function (data) {
                    $('body').toggleClass('loaded');
                    toastr.success(ezgolist.language.sharingSuccess ??= 'Template shared successfully');
                    $('#template-share-modal').modal('hide');
                    $('#btn-share-template').prop('disabled', false);
                },
                error: function (ex) {
                    $('body').toggleClass('loaded');
                    toastr.error(ezgolist.language.sharingFailed ??= 'Template sharing failed');
                    $('#btn-share-template').prop('disabled', false);
                }
            });
        }
        else {
            toastr.warning('No companies selected');
        }
    },

    validateTemplate: function () {
        var val = true;
        // Global validation
        if ((ezgolist.tmpl.AreaId === undefined || ezgolist.tmpl.AreaId === null) || ezgolist.tmpl.AreaId < 1) {
            toastr.error('Area is a required field.');
            val = false;
        }

        if ((ezgolist.tmpl.Name === undefined || ezgolist.tmpl.Name === null) || ezgolist.tmpl.Name === '') {
            toastr.error('Template name is a required field.');
            val = false;
        }

        if (ezgolist.tmpl.TaskTemplates !== undefined || ezgolist.tmpl.TaskTemplates !== null) {
            $(ezgolist.tmpl.TaskTemplates).each(function (index, item) {
                if (item.Name === '') {
                    toastr.error('Names in ' + ezgolist.listType + ' items are required.');
                    val = false;
                }
            });
        }

        if (ezgolist.listType == "skillassessment" && ezgolist.tmpl.TaskTemplates.length == 0) {
            toastr.error('One or more assessment instructions required.')
            val = false;
        }

        if (ezgolist.listType === 'task' && ezgolist.tmpl.Recurrency === undefined) {
            toastr.error('Select a recurrency type.');
            val = false;
        }
        else if (ezgolist.listType === 'task' && ezgolist.tmpl.Recurrency !== undefined) {
            if (ezgolist.tmpl.RecurrencyType === undefined || ezgolist.tmpl.RecurrencyType === '') {
                toastr.error('Select a recurrency type.');
            }

            if (ezgolist.tmpl.RecurrencyType === 'week') {
                if (ezgolist.tmpl.Recurrency.Schedule.Weekday0 == false && ezgolist.tmpl.Recurrency.Schedule.Weekday1 == false && ezgolist.tmpl.Recurrency.Schedule.Weekday2 == false && ezgolist.tmpl.Recurrency.Schedule.Weekday3 == false && ezgolist.tmpl.Recurrency.Schedule.Weekday4 == false && ezgolist.tmpl.Recurrency.Schedule.Weekday5 == false && ezgolist.tmpl.Recurrency.Schedule.Weekday6 == false) {
                    toastr.error('No week day selected.');
                    val = false;
                }
                else if (!$('#Schedule_Weekday0,#Schedule_Weekday1,#Schedule_Weekday2,#Schedule_Weekday3,#Schedule_Weekday4,#Schedule_Weekday5,#Schedule_Weekday6').is(':checked')) {
                    toastr.error('No week day selected.');
                    val = false;
                }
            }

            if (ezgolist.tmpl.RecurrencyType === 'shifts') {
                if (ezgolist.tmpl.Recurrency.Shifts.length < 1) {
                    toastr.error('No shifts present.');
                    val = false;
                }
                else if (!$('[data-group-id = "Shifts"').is(':checked')) {
                    toastr.error('No shifts present.');
                    val = false;
                }
            }

            if (ezgolist.tmpl.RecurrencyType === 'no recurrency') {
                if (ezgolist.tmpl.Recurrency.Shifts.length < 1) {
                    toastr.error('No shifts present.');
                    val = false;
                }
                else if (!$('[data-group-id = "Shifts_once"').is(':checked')) {
                    toastr.error('No shifts present.');
                    val = false;
                }
            }

            if (ezgolist.tmpl.RecurrencyType === 'periodday' || ezgolist.tmpl.RecurrencyType === 'dynamicday') {
                if (ezgolist.tmpl.Recurrency.Schedule.Day < 1) {
                    toastr.error('Number of days must be positive.');
                    val = false;
                }
                else if ($('#Schedule_Day').val() < 1) {
                    toastr.error('Number of days must be positive.');
                    val = false;
                }
            }
        }

        if (ezgolist.listType === 'task' && ezgolist.tmpl.DeepLinkTo !== undefined && ezgolist.tmpl.DeepLinkTo !== 'none') {
            if (ezgolist.tmpl.DeepLinkId === undefined || ezgolist.tmpl.DeepLinkId === 0) {
                toastr.error('Select a connected item.');
                val = false;
            }
        }

        if (ezgolist.listType === 'audit' && ezgolist.tmpl.ScoreType === 'score') {
            if (ezgolist.tmpl.MaxScore == ezgolist.tmpl.MinScore) {
                toastr.error('Minimal score value and maximal score value can not be the same.');
                val = false;
            }
        }

        if (ezgolist.transferableChecklistsEnabled &&
            ezgolist.listType === 'checklist' &&
            (ezgolist.tmpl.StageTemplates !== undefined && ezgolist.tmpl.StageTemplates !== null && ezgolist.tmpl.StageTemplates.length > 0) &&
            (ezgolist.tmpl.TaskTemplates !== undefined && ezgolist.tmpl.TaskTemplates !== null && ezgolist.tmpl.TaskTemplates.length > 0)) {
            let highestStageIndex = Math.max(...ezgolist.tmpl.StageTemplates.map(elem => elem.Index));
            let highestTaskIndex = Math.max(...ezgolist.tmpl.TaskTemplates.map(elem => elem.Index));
            if (highestStageIndex < highestTaskIndex) {
                toastr.error('Checklist items found after last stage, please make sure all items are in a stage.');
                val = false;
            }
        }

        switch (ezgolist.listType) {
            case "audit":
                if ((ezgolist.tmpl.TaskTemplates !== undefined || ezgolist.tmpl.TaskTemplates !== null)) {
                    $(ezgolist.tmpl.TaskTemplates).each(function (index, item) {
                        if (item.Weight === 0) {
                            toastr.error('Template item weight cannot be 0.');
                            val = false;
                        }
                    });
                }
                break;
        }
        return val;
    },

    load: function () {
        if (ezgolist.listType == 'skillassessment' || ezgolist.listType == 'workinstruction') {
            ezgolist.tmpl = ezgolist.initDataObject; //use direct object already supplied.
            ezgolist.tmpl.Tags ??= new Array();
            ezgolist.tmpl.Description ??= '';
            ezgolist.render();

            if (ezgolist._firstLoadDone) {
                ezgolist.initialTmpl = structuredClone(ezgolist.tmpl);
            }
        }
        else {
            var result;
            var isShared = ezgolist.sharedTemplateId > 0;
            $.ajax({
                url: ezgolist.getListUrl + (isShared ? ezgolist.sharedTemplateId : ezgolist.templateId),
                dataType: 'json',
                success: function (data) {
                    result = data;
                    ezgolist.tmpl = result;
                    if (ezgolist.tmpl.TaskTemplates === undefined) {
                        ezgolist.tmpl.TaskTemplates = new Array();
                    }

                    if (isShared) {
                        ezgolist.tmpl.TaskTemplates.forEach(function (item) {
                            item.isNew = true;
                            if (item.Steps != null && item.Steps != undefined) {
                                item.Steps.forEach(function (step) {
                                    step.isNew = true;
                                });
                            }
                        });

                        if (ezgolist.listType === 'checklist') {
                            if (ezgolist.tmpl.StageTemplates != undefined)
                                ezgolist.tmpl.StageTemplates.forEach(function (stage) {
                                    stage.isNew = true;
                                });
                        }

                        if (ezgolist.listType === 'task') {
                            ezgolist.tmpl.Recurrency.Shifts ??= new Array();
                            ezgolist.tmpl.Steps.forEach(function (step) {
                                step.isNew = true;
                            });
                        }
                    }

                    if (ezgolist.listType === 'task') { ezgolist.tmpl.Description ??= ''; }
                    ezgolist.render();
                    if (ezgolist.listType === 'task') {
                        deeplink.render(ezgolist.tmpl.DeepLinkId, ezgolist.tmpl.DeepLinkTo, ezgolist.tmpl.DeepLinkCompletionIsRequired ?? false);
                    }

                    if (ezgolist._firstLoadDone) {
                        ezgolist.initialTmpl = structuredClone(ezgolist.tmpl);
                    }
                }
            });
        }
    },

    renderAreas: function () {
        $.ajax({
            url: '/config/getareas',
            dataType: 'json',
            success: function (data) {
                $('#areascard').html('');
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
                            var sub_li = $('<li data-id="' + item.Id + '" data-parentid="' + item.ParentId + '" data-level="' + item.Level + '" style="border-right: 1px solid rgba(0,0,0,.125);border-left: 1px solid rgba(0,0,0,.125);" />');
                            sub_li.html(item.Name);
                            sub_li.addClass('list-group-item');
                            sub_li.data('id', item.Id);
                            sub_li.data('parentid', item.ParentId);
                            if (i !== 0) {
                                sub_li.css('display', 'none');
                            }
                            sub_ul.append(sub_li);
                        });
                        $('#areascard').append(sub_ul);
                    }
                }

                $('#areascard ul').on('click', 'li', function (el) {
                    var currentId = $(el.currentTarget).data('id');
                    var currentLevel = $(el.currentTarget).data('level');
                    ezgolist.tmpl.AreaId = currentId;
                    ezgolist.hideOthers(currentLevel);
                    $('li[data-parentid="' + currentId + '"]').css('display', 'block');
                    $('li[data-level="' + currentLevel + '"]').removeClass('active');
                    $(el.currentTarget).addClass('active');
                });

                if (ezgolist.tmpl.AreaId > 0) {
                    var area = data.filter(obj => {
                        return obj.Id === parseInt(ezgolist.tmpl.AreaId);
                    })[0];
                    $('#areascard ul li[data-level!="0"]').hide();
                    $('#areascard ul li').removeClass('active');
                    var parents = area.FullDisplayIds.split(' -> ');
                    $(parents).each(function (index, item) {
                        $('li[data-parentid="' + item + '"]').show();
                        $('li[data-id="' + item + '"]').addClass('active');
                    });
                }
            }
        });
    },

    hideOthers: function (level) {
        var list = $("ul li").filter(function () {
            return $(this).attr("data-level") > level;
        });
        list.hide().removeClass('active');
    },

    //render all items to the dom based on the load of the json data.
    render: function () {
        var initialScrollTop = $('#mainCol').scrollTop();
        var initialScrollLeft = $('#mainCol').scrollLeft();
        $(this.itemListSelector).empty();
        var instance = $('.js-range-slider').data("ionRangeSlider");

        if (instance !== undefined) {
            instance.update({
                from: ezgolist.tmpl.MinScore,
                to: ezgolist.tmpl.MaxScore
            });
        }

        if (ezgolist.tmpl.Id === 0) {
            $('#btnDeleteTemplate').attr('disabled', 'disabled');
            $('#btnDuplicateTemplate').attr('disabled', 'disabled');
            $('#btnShareTemplate').attr('disabled', 'disabled');
        }

        var signature = 'none';
        if (ezgolist.tmpl.IsSignatureRequired) {
            signature = ezgolist.tmpl.IsDoubleSignatureRequired ? 'double' : 'single';
        }

        $(ezgolist.templateNameSelector).val(ezgolist.tmpl.Name);
        $('#txtTemplateDescription').val(ezgolist.tmpl.Description);
        $('input[name="role"][value="' + ezgolist.tmpl.Role + '"]').prop('checked', 'checked').click();
        $('input[name="signature"][value="' + signature + '"]').attr('checked', 'checked').click();

        if (ezgolist.listType === 'workinstruction') {
            $('input[name="workinstructiontype"][value="' + ezgolist.tmpl.WorkInstructionType + '"]').prop('checked', 'checked').click();
        }

        $('[data-type="cid"]').html(ezgolist.language.listtypeId + ': ' + ezgolist.tmpl.Id);
        $('#uploaderTemplate').data('templateid', ezgolist.tmpl.Id);

        if (ezgolist.listType === 'audit') {
            $('input[name="score"][value="' + ezgolist.tmpl.ScoreType + '"]').prop('checked', 'checked').click();
        }

        if (ezgolist.listType === 'task') {
            var dl = ezgolist.tmpl.DeepLinkTo ?? "none"
            $('input[name="deeplink"][value="' + dl + '"]').prop('checked', 'checked').click();
            $('input[name="machinestatus"][value="' + ezgolist.tmpl.MachineStatus + '"]').attr('checked', 'checked').click();
            $("#enablePictureProof").prop('checked', ezgolist.tmpl.HasPictureProof);

            if (ezgolist.tmpl.PlannedTime !== undefined) {
                $('#plannedtime').val(ezgolist.tmpl.PlannedTime);
            }

            if (!ezgolist.tmpl.MachineStatus) {
                $('input[name="machinestatus"][value="not_applicable"]').attr('checked', 'checked').click();
            }
        }

        ezgolist.updateGeneralPictureProofToggle();

        if (ezgolist.tmpl.Picture !== undefined) {
            if (String(ezgolist.tmpl.Picture).startsWith('data:') || String(ezgolist.tmpl.Picture).startsWith('blob:')) {
                $('#TemplateImage').data('src', ezgolist.tmpl.Picture);

                $('#TemplateImage').parent('a').data('href', ezgolist.tmpl.Picture);
                $('#TemplateImage').parent('a').data('caption', ezgolist.tmpl.Name);
            }
            else {
                $('#TemplateImage').data('src', ezgolist.mediaUrl + ezgolist.tmpl.Picture);
                $('#TemplateImage').parent('a').data('href', ezgolist.mediaUrl + ezgolist.tmpl.Picture);
                $('#TemplateImage').parent('a').data('caption', ezgolist.tmpl.Name);
            }
        }
        else {
            $('#TemplateImage').data('src', '/assets/img/normal_unavailable_image.png').parent('a').data('href', '/assets/img/normal_unavailable_image.png').data('caption', ezgolist.tmpl.Name);
        }

        $(ezgolist.tmpl.TaskTemplates).each(function (index, item) {
            if (item === undefined) return;

            var picture = ezgolist.placeholderImageUrl
            if (item.Picture != undefined && !item.Picture.toString().startsWith('data:') && !item.Picture.toString().startsWith('blob:')) {
                picture = ezgolist.mediaUrl + item.Picture;
            }

            var pictureProof = 'none';
            if (item.HasPictureProof === true) pictureProof = 'flex';

            if (item.VideoThumbnail !== undefined && item.VideoThumbnail !== undefined) {
                if (!item.VideoThumbnail.toString().startsWith('data:') && !item.VideoThumbnail.toString().startsWith('blob:')) {
                    picture = ezgolist.mediaUrl + item.VideoThumbnail;
                }
                else {
                    picture = item.VideoThumbnail;
                }

                // For skillassessment, use WorkInstructionTemplateId as the unique identifier for DOM tracking
                var itemCid = (ezgolist.listType === 'skillassessment' && item.WorkInstructionTemplateId)
                    ? item.WorkInstructionTemplateId
                    : item.Id;

                var li = cardTemplateVideo
                    .replaceAll('{{cid}}', itemCid)
                    .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(item.Name))
                    .replaceAll('{{desc}}', GlobalParser.escapeHtmlCharacters(item.Description))
                    .replaceAll('{{picture}}', picture)
                    .replaceAll('{{pictureproof}}', pictureProof)
                    .replaceAll('{{video}}', item.Video)
                    .replaceAll('{{videoid}}', 'video' + item.Id)
                    .replaceAll('{{fancy}}', 'items');
                $(ezgolist.itemListSelector).append(li)
            }
            else {
                if (String(item.Picture).match("^data:") || String(item.Picture).match("^blob:")) {
                    picture = item.Picture;
                }

                // For skillassessment, use WorkInstructionTemplateId as the unique identifier for DOM tracking
                var itemCid = (ezgolist.listType === 'skillassessment' && item.WorkInstructionTemplateId)
                    ? item.WorkInstructionTemplateId
                    : item.Id;

                var li = cardTemplateImage
                    .replaceAll('{{cid}}', itemCid)
                    .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(item.Name))
                    .replaceAll('{{desc}}', GlobalParser.escapeHtmlCharacters(item.Description))
                    .replaceAll('{{picture}}', picture)
                    .replaceAll('{{pictureproof}}', pictureProof)
                    .replaceAll('{{fancy}}', 'items');
                $(ezgolist.itemListSelector).append(li);
            }
        });

        //render stages
        $(ezgolist.tmpl.StageTemplates).each(function (index, stage) {
            if (stage === undefined) return;

            var li = cardTemplateStage
                .replaceAll('{{cid}}', stage.Id)
                .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(stage.Name))
                .replaceAll('{{desc}}', GlobalParser.escapeHtmlCharacters(stage.Description));

            //insert the stage after the item or stage with the index before
            if (stage.Index > 1 && ezgolist.tmpl.TaskTemplates != undefined) {
                let itemOrStageBeforeStage;
                let elementBeforeStage = { length: 0 };
                let iterationCount = 0;

                while (elementBeforeStage.length == 0) {
                    iterationCount++;
                    if (stage.Index - iterationCount == 0) break;

                    itemOrStageBeforeStage = ezgolist.tmpl.TaskTemplates.find(obj => {
                        return obj.Index === stage.Index - iterationCount;
                    });
                    if (itemOrStageBeforeStage === undefined) {
                        itemOrStageBeforeStage = ezgolist.tmpl.StageTemplates.find(obj => {
                            return obj.Index === stage.Index - iterationCount;
                        });
                    }

                    if (itemOrStageBeforeStage != undefined) {
                        elementBeforeStage = $(ezgolist.itemListSelector + " [data-cid=" + itemOrStageBeforeStage.Id + "]");
                    }
                }
                if (elementBeforeStage.length > 0) {
                    //if there are multiple elements before the stage, only insert stage element after the last one

                    elementBeforeStage = elementBeforeStage[elementBeforeStage.length - 1];

                    $(elementBeforeStage).after(li);
                }
                else {
                    $(ezgolist.itemListSelector).append(li);
                }
            } else if (stage.Index == 1) {
                $(ezgolist.itemListSelector).prepend(li);
            } else {
                $(ezgolist.itemListSelector).append(li);
            }
        });

        let addNewButtonTemplate = newTemplateItem.replaceAll('{{listtype}}', ezgolist.language.addItem);
        if (ezgolist.listType == 'checklist' && ezgolist.stagesEnabled) addNewButtonTemplate = newTemplateItemOrStage.replaceAll('{{listtype}}', ezgolist.language.addItem).replaceAll('{{addstage}}', ezgolist.language.addStage);//TODO translate addStage
        $(ezgolist.itemListSelector).append(addNewButtonTemplate);

        if (ezgolist.listType === 'task') {
            ezgolist.renderTaskSteps(ezgolist.tmpl.Id);
        }

        if (ezgolist.listType === 'audit' && ezgolist.tmpl.HasDerivedItems) {
            $('#scoringControls label').attr('disabled', true).addClass('disabled');
            $('#scoringControls :input[name="score"]').attr('disabled', true);
            $('#scoringTitle').html(ezgolist.language.scoringsystem + ' (' + ezgolist.language.scoringdisabled + ')');
            $('.js-range-slider').data("ionRangeSlider").update({ disable: true });
        }

        if (ezgolist.listType === 'workinstruction' && ezgolist.tmpl.IsWITemplateLinkedToAssessment) {
            // Disable the Work button and uncheck its radio
            $('label:has(input[name="workinstructiontype"][value="0"])')
                .addClass('disabled')
                .removeClass('active')
                .find('input')
                .prop('disabled', true)
                .prop('checked', false);

            $('label:has(input[name="workinstructiontype"][value="1"])')
                .addClass('disabled');

            // Update the title
            $('#selectTypeTitle').text(
                ezgolist.language.selectTypeTitle +
                ' (' + ezgolist.language.selectTypeDisabled + ')'
            );
        }

        ezgolist._firstLoadDone = true;
        ezgolist.renderAreas();
        ezgolist.renderOpenFields(); //render open fields
        ezgolist.enableQrCode();
        ezgomediafetcher.preloadImagesAndVideos();
        ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
        $('#mainCol')[0].scrollTo(initialScrollLeft, initialScrollTop);
    },

    renderDialogContent: function (templateId) {
        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
            return obj.Id === templateId;
        })[0];

        var tagsToDeselect = document.getElementsByClassName("item-tag");
        if (tagsToDeselect.length > 0) {
            tagsToDeselect.forEach(function (element) {
                element.classList.remove("btn-ezgo");
                element.classList.add("btn-outline-ezgo");
                element.classList.remove("selected-tag");
            })
        }

        if (template === undefined) return;

        ezgoprops.loadTemplateProps(template);
        objIndex = ezgolist.tmpl.TaskTemplates.findIndex((obj => obj.Id == templateId));
        $('#txtItemName').val(template.Name);
        $('#txtItemDesc').val(template.Description);
        $("#enablePictureProof").prop('checked', template.HasPictureProof);
        $('#txtWeight').val(template.Weight === 0 ? 1 : template.Weight);
        $('#instructionlist').empty();
        $('#currentTemplateItemId').val(templateId);
        $('#btnDeleteItem').data('templateid', templateId);
        $('#uploaderItem').data('templateid', templateId);
        $('[data-selector="deletemedia"]').data('templateid', templateId);
        ezgolist._currentTemplateId = templateId;

        $(template.Steps).each(function (index, step) {
            var picture = ezgolist.placeholderImageUrl
            if (step.Picture !== undefined && !String(step.Picture).toString().startsWith('data:') && !String(step.Picture).toString().startsWith('blob:')) {
                picture = ezgolist.mediaUrl + step.Picture;
            }

            if (step.VideoThumbnail != undefined) {
                picture = '';
                if (step.VideoThumbnail.startsWith('data:') || step.VideoThumbnail.startsWith('blob:')) {
                    picture = step.VideoThumbnail;
                }
                else {
                    picture = ezgolist.mediaUrl + step.VideoThumbnail;
                }

                var li = cardTemplateStepVideo
                    .replaceAll('{{cid}}', step.Id)
                    .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{desc}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{picture}}', picture)
                    .replaceAll('{{video}}', step.Video)
                    .replaceAll('{{videoid}}', 'videoid' + step.Id)
                    .replaceAll('{{fancy}}', 'steps');
                $('#instructionlist').append(li)
            } else {
                if (String(step.Picture).match("^data:") || String(step.Picture).match("^blob:")) {
                    picture = step.Picture;
                }

                var li = cardTemplateStepImage
                    .replaceAll('{{cid}}', step.Id)
                    .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{desc}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{picture}}', picture)
                    .replaceAll('{{videoid}}', 'videoid' + step.Id)
                    .replaceAll('{{fancy}}', 'steps');
                $('#instructionlist').append(li);
            }

            if (String(step.Picture).match("^data:") || String(step.Picture).match("^blob:")) {
                picture = step.Picture;
            }
        }); //end each

        if (template.Tags?.length > 0) {
            template.Tags.forEach(function (tag) {
                var element = document.getElementById('i' + tag.Id);
                if (element?.classList != null) {
                    element.classList.add("btn-ezgo");
                    element.classList.remove("btn-outline-ezgo");
                    element.classList.add("selected-tag");
                }
            })
        }

        if (template.DescriptionFile !== undefined) {
            var file = ezgolist.mediaUrl + template.DescriptionFile;

            if (String(template.DescriptionFile).match("^blob:")) {
                file = template.DescriptionFile;
            }

            var attachment = newTemplateItemAttachment
                .replaceAll('{{filename}}', template.DescriptionFile)
                .replaceAll('{{url}}', file)
                .replaceAll('{{fancy}}', 'steps')
                .replaceAll('{{templateid}}', template.Id);
            $('#instructionlist').append(attachment);
        }
        else if (template.Attachments !== undefined && template.Attachments.length > 0 && ezgolist.taskTemplateAttachmentsEnabled) {
            var attachment = template.Attachments[0];
            var attachmentUri = attachment.Uri;

            if (ezgolist.listType == 'checklist') {
                if (attachment.AttachmentType.toLowerCase() == "pdf") {
                    attachmentUri = ezgolist.mediaUrl + attachmentUri;

                    if (attachment.Uri.startsWith("blob:")) {
                        attachmentUri = attachment.Uri;
                    }

                    var attachmentHtml = checklistItemAttachmentTemplatePdf
                        .replaceAll('{{url}}', attachmentUri)
                        .replaceAll('{{itemid}}', template.Id)
                        .replaceAll('{{filename}}', attachment.FileName);

                    $('#instructionlist').append(attachmentHtml);

                    //handle remove
                    $('#deleteChecklistItemAttachmentPdf-' + ezgolist._currentTemplateId).off('click').on('click', function (elem) {
                        var currentItemId = $(this).attr('id').replace('deleteChecklistItemAttachmentPdf-', '');

                        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                            return obj.Id === +currentItemId;
                        })[0];

                        if (template != undefined && template) {
                            template.Name = $('#txtItemName').val();
                            template.Description = $('#txtItemDesc').val();
                        }

                        template.Attachments = [];
                        ezgolist.renderDialogContent(ezgolist._currentTemplateId);
                    });
                }
                else if (attachment.AttachmentType.toLowerCase() == "link") {
                    var attachmentHtml = checklistItemAttachmentTemplateLink
                        .replaceAll('{{url}}', attachmentUri)
                        .replaceAll('{{encodedUrl}}', encodeURIComponent(attachmentUri))
                        .replaceAll('{{itemid}}', template.Id);

                    $('#instructionlist').append(attachmentHtml);

                    //handle remove
                    $('#deleteChecklistItemAttachmentLink-' + ezgolist._currentTemplateId).off('click').on('click', function (elem) {
                        var currentItemId = $(this).attr('id').replace('deleteChecklistItemAttachmentLink-', '');

                        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                            return obj.Id === +currentItemId;
                        })[0];

                        if (template != undefined && template) {
                            template.Name = $('#txtItemName').val();
                            template.Description = $('#txtItemDesc').val();
                        }

                        template.Attachments = [];
                        ezgolist.renderDialogContent(ezgolist._currentTemplateId);
                    });
                }
            }
            else if (ezgolist.listType == 'audit') {
                if (attachment.AttachmentType.toLowerCase() == "pdf") {
                    attachmentUri = ezgolist.mediaUrl + attachmentUri;

                    if (attachment.Uri.startsWith("blob:")) {
                        attachmentUri = attachment.Uri;
                    }

                    var attachmentHtml = auditItemAttachmentTemplatePdf
                        .replaceAll('{{url}}', attachmentUri)
                        .replaceAll('{{itemid}}', template.Id)
                        .replaceAll('{{filename}}', attachment.FileName);

                    $('#instructionlist').append(attachmentHtml);

                    //handle remove
                    $('#deleteAuditItemAttachmentPdf-' + ezgolist._currentTemplateId).off('click').on('click', function (elem) {
                        var currentItemId = $(this).attr('id').replace('deleteAuditItemAttachmentPdf-', '');

                        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                            return obj.Id === +currentItemId;
                        })[0];

                        if (template != undefined && template) {
                            template.Name = $('#txtItemName').val();
                            template.Description = $('#txtItemDesc').val();
                        }

                        template.Attachments = [];
                        ezgolist.renderDialogContent(ezgolist._currentTemplateId);
                    });
                }
                else if (attachment.AttachmentType.toLowerCase() == "link") {
                    var attachmentHtml = auditItemAttachmentTemplateLink
                        .replaceAll('{{url}}', attachmentUri)
                        .replaceAll('{{encodedUrl}}', encodeURIComponent(attachmentUri))
                        .replaceAll('{{itemid}}', template.Id);

                    $('#instructionlist').append(attachmentHtml);

                    //handle remove
                    $('#deleteAuditItemAttachmentLink-' + ezgolist._currentTemplateId).off('click').on('click', function (elem) {
                        var currentItemId = $(this).attr('id').replace('deleteAuditItemAttachmentLink-', '');

                        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                            return obj.Id === +currentItemId;
                        })[0];

                        if (template != undefined && template) {
                            template.Name = $('#txtItemName').val();
                            template.Description = $('#txtItemDesc').val();
                        }

                        template.Attachments = [];
                        ezgolist.renderDialogContent(ezgolist._currentTemplateId);
                    });
                }
            }
        }

        if (template.WorkInstructionRelations !== undefined && template.WorkInstructionRelations.length > 0) {
            var workInstructionRelationId = template.WorkInstructionRelations[0].WorkInstructionTemplateId;
            var workInstruction = ezgolist.currentWorkInstructionTemplates.filter(w => w.Id == workInstructionRelationId)[0];
            var picture = ezgolist.placeholderImageUrl;

            if (workInstruction?.Picture) {
                picture = ezgolist.mediaUrl + workInstruction.Picture;
            }  

            var attachment = newTemplateItemAttachmentWorkInstruction
                .replaceAll('{{cid}}', template.Id)
                .replaceAll('{{picture}}', picture)
                .replaceAll('{{name}}', workInstruction != undefined ? GlobalParser.escapeHtmlCharacters(workInstruction.Name) : 'work instruction not found');
            $('#instructionlist').append(attachment);
        }

        var picture = ezgolist.placeholderImageUrl

        if (template.Picture != undefined && !template.Picture.toString().startsWith('data:')) {
            picture = ezgolist.mediaUrl + template.Picture;
        }

        if (template.VideoThumbnail !== undefined && (!String(template.VideoThumbnail).startsWith("blob:"))) {
            picture = ezgolist.mediaUrl + template.VideoThumbnail;
        }

        if (template.VideoThumbnail !== undefined && String(template.VideoThumbnail).startsWith("blob:")) {
            picture = template.VideoThumbnail;
        }

        if (String(template.Picture).match("^blob:")) {
            picture = template.Picture;
        }

        $('#dialogImg').attr('src', '/images/media-loading.gif');
        $('#dialogImg').data('src', picture);
        ezgomediafetcher.preloadImagesAndVideos();
        ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
        $('#dialogImg').parent('a').data('caption', template.Name);

        //check if feature WI is on and use newTemplateItemChooserWI if its on
        if (template.StepsCount === 0 && template.DescriptionFile === undefined && (template.Attachments === undefined || template.Attachments.length == 0) && (template.WorkInstructionRelations === undefined || template.WorkInstructionRelations.length == 0)) {
            var chooseButton = '';
            if (ezgolist.workInstructionsEnabled && ezgolist.taskTemplateAttachmentsEnabled) {
                chooseButton = newTemplateItemChooserWI
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addworkinstruction}}', ezgolist.language.addworkinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf)
                    .replaceAll('{{addlink}}', ezgolist.language.addlink);
            }
            else if (!ezgolist.workInstructionsEnabled && ezgolist.taskTemplateAttachmentsEnabled) {
                chooseButton = newTemplateItemChooser
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf)
                    .replaceAll('{{addlink}}', ezgolist.language.addlink);
            }
            else if (ezgolist.workInstructionsEnabled && !ezgolist.taskTemplateAttachmentsEnabled) {
                chooseButton = newTemplateItemChooserWIWithoutLink
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addworkinstruction}}', ezgolist.language.addworkinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf);
            }
            else {
                chooseButton = newTemplateItemChooserWithoutLink
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf);
            }
            $('#instructionlist').append(chooseButton);
        }

        if (template.StepsCount > 0) {
            var addButton = newTemplateItemStep
                .replaceAll('{{cid}}', templateId)
                .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction);
            $('#instructionlist').append(addButton);
        }

        $('#dialogImg').parent('a').data('href', picture);

        if (template.VideoThumbnail !== undefined && template.Video !== undefined) {
            $('#dialogVideo').data('src', template.Video);
            $('#dialogImg').data('src', picture);
            $('#dialogImg').parent('a').data('href', '#dialogVideo').attr('href', '#dialogVideo');

            ezgomediafetcher.preloadImagesAndVideos();
            ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
        }

        if (ezgolist.listType === 'workinstruction') {
            if (template.Attachments != null && template.Attachments.length) {
                var attachment = template.Attachments[0];
                if (attachment.AttachmentType.toLowerCase() == "pdf") {
                    var attachmentUri = ezgolist.mediaUrl + attachment.Uri;

                    if (attachment.Uri.startsWith('blob:')) {
                        attachmentUri = attachment.Uri;
                    }

                    var newAttachment = workInstructionItemAttachmentTemplatePdf
                        .replaceAll('{{url}}', attachmentUri)
                        .replaceAll('{{itemid}}', template.Id)
                        .replaceAll('{{filename}}', attachment.FileName)
                        .replaceAll('{{fancy}}', "workInstruction-" + template.Id);

                    $('#workInstructionItemAttachmentsPreviewContainer').html(newAttachment);
                    $('#workInstructionItemAttachmentsPreviewContainer').show();
                    $('#workInstructionItemAttachmentsSelectionContainer').hide();
                }
                else if (attachment.AttachmentType.toLowerCase() == "link") {
                    var attachmentUri = attachment.Uri;

                    var newAttachment = workInstructionItemAttachmentTemplateLink
                        .replaceAll('{{url}}', attachmentUri)
                        .replaceAll('{{encodedUrl}}', encodeURIComponent(attachmentUri))
                        .replaceAll('{{itemid}}', template.Id);

                    $('#workInstructionItemAttachmentsPreviewContainer').html(newAttachment);
                    $('#workInstructionItemAttachmentsPreviewContainer').show();
                    $('#workInstructionItemAttachmentsSelectionContainer').hide();
                }

                ezgomediafetcher.preloadImagesAndVideos();
                ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
            }
            else {
                $('[id^="workInstructionItemAttachment-"]').remove();
                $('#workInstructionItemAttachmentsSelectionContainer').show();
            }
        }

        $('#editTemplateStepModal .modal-header h4').html(ezgolist.language.listtype + ' (' + (objIndex + 1) + '/' + ezgolist.tmpl.TaskTemplates.length + ')');

        if ((objIndex + 1) === ezgolist.tmpl.TaskTemplates.length) {
            $('#btnNextTemplate').html(ezgolist.language.addItemLower);
        }
        else {
            $('#btnNextTemplate').html(ezgolist.language.nextItem);
        }

        if (objIndex === 0) {
            $('#btnPreviousTemplate').attr('disabled', 'disabled');
        }
        else {
            $('#btnPreviousTemplate').prop('disabled', false);
        }

        ezgomediafetcher.preloadImagesAndVideos();
        ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
    },

    renderStepDialogContent: function (templateStepId) {
        var templateId = parseInt($('#currentTemplateItemId').val());
        var template;

        if (ezgolist.listType !== 'task') {
            template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                return obj.Id === templateId;
            })[0];
        }
        else {
            template = ezgolist.tmpl;
        }

        var step;
        step = template.Steps.filter(obj => {
            return obj.Id === templateStepId;
        })[0];

        objIndex = template.Steps.findIndex((obj => obj.Id == step.Id));

        $('#btnDeleteStep').data('stepid', templateStepId);
        $('#currentTemplateItemStepId').val(templateStepId);
        $('#txtStepDescription').val(step.Description);
        $('#uploaderItemStep').data('templateid', templateId);
        $('#uploaderItemStep').data('templatestepid', templateStepId);
        $('[data-selector="deletemedia"]').data('templateid', templateId);
        $('[data-selector="deletemedia"]').data('templatestepid', templateStepId);

        var picture = ezgolist.placeholderImageUrl;
        if (step.Picture !== undefined) {
            if (String(step.Picture).startsWith('data:') || String(step.Picture).startsWith('blob:')) {
                picture = step.Picture;
            }
            else {
                picture = ezgolist.mediaUrl + step.Picture;
            }
        }

        if (step.VideoThumbnail !== undefined) {
            if (String(step.VideoThumbnail).startsWith('data:') || String(step.VideoThumbnail).startsWith('blob:')) {
                picture = step.VideoThumbnail;
            }
            else {
                picture = ezgolist.mediaUrl + step.VideoThumbnail;
            }
        }

        $('#stepImage').data('src', picture);
        $('#editTemplateStepDetailModal h4').html(ezgolist.language.instructionCounterLabel + ' (' + (objIndex + 1) + '/' + template.Steps.length + ')');

        ezgolist._currentTemplateStepId = templateStepId;

        if (objIndex + 1 === template.Steps.length) {
            $('#btnNextStep').html(ezgolist.language.addStep);
        }
        else {
            $('#btnNextStep').html(ezgolist.language.nextStep);
        }

        if (objIndex === 0) {
            $('#btnPreviousStep').attr('disabled', 'disabled');
        }
        else {
            $('#btnPreviousStep').prop('disabled', false);
        }
        $('#txtStepDescription').focus();

        ezgomediafetcher.preloadImagesAndVideos();
        ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
    },

    renderTaskSteps: function (templateId) {
        $('#currentTemplateItemId').val(templateId);
        $('#instructionlist').empty();
        var template = ezgolist.tmpl;
        ezgoprops.loadTemplateProps(template);
        $(ezgolist.tmpl.Steps).each(function (index, step) {
            var picture = ezgolist.placeholderImageUrl
            if (step.Picture !== undefined && !String(step.Picture).toString().startsWith('data:') && !String(step.Picture).toString().startsWith('blob:')) {
                picture = ezgolist.mediaUrl + step.Picture;
            }

            if (step.VideoThumbnail != undefined) {
                picture = '';
                if (step.VideoThumbnail.startsWith('data:') || step.VideoThumbnail.startsWith('blob:')) {
                    picture = step.VideoThumbnail;
                }
                else {
                    picture = ezgolist.mediaUrl + step.VideoThumbnail;
                }

                var li = cardTemplateStepVideo
                    .replaceAll('{{cid}}', step.Id)
                    .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{desc}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{picture}}', picture)
                    .replaceAll('{{video}}', step.Video)
                    .replaceAll('{{videoid}}', 'videoid' + step.Id)
                    .replaceAll('{{fancy}}', 'steps');
                $('#instructionlist').append(li)
            }
            else {
                if (String(step.Picture).match("^data:") || String(step.Picture).match("^blob:")) {
                    picture = step.Picture;
                }

                var li = cardTemplateStepImage
                    .replaceAll('{{cid}}', step.Id)
                    .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{desc}}', GlobalParser.escapeHtmlCharacters(step.Description))
                    .replaceAll('{{picture}}', picture)
                    .replaceAll('{{videoid}}', 'videoid' + step.Id)
                    .replaceAll('{{fancy}}', 'steps');
                $('#instructionlist').append(li);

                ezgomediafetcher.preloadImagesAndVideos();
                ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
            }

            if (String(step.Picture).match("^data:") || String(step.Picture).match("^blob:")) {
                picture = step.Picture;
            }
        });

        if (template.DescriptionFile !== undefined && template.DescriptionFile !== null) {
            var file = ezgolist.mediaUrl + template.DescriptionFile;

            if (String(template.DescriptionFile).match("^blob:")) {
                file = template.DescriptionFile;
            }

            var attachment = newTemplateItemAttachment
                .replaceAll('{{filename}}', template.DescriptionFile)
                .replaceAll('{{url}}', file)
                .replaceAll('{{fancy}}', 'steps')
                .replaceAll('{{templateid}}', template.Id);
            $('#instructionlist').append(attachment);
        }
        else if (template.Attachments !== undefined && template.Attachments.length > 0 && ezgolist.taskTemplateAttachmentsEnabled) {
            var attachment = template.Attachments[0];
            var attachmentUri = attachment.Uri;

            if (attachment.AttachmentType.toLowerCase() == "pdf") {
                attachmentUri = ezgolist.mediaUrl + attachmentUri;

                if (attachment.Uri.startsWith("blob:")) {
                    attachmentUri = attachment.Uri;
                }

                var attachmentHtml = taskTemplateAttachmentTemplatePdf
                    .replaceAll('{{url}}', attachmentUri)
                    .replaceAll('{{itemid}}', template.Id)
                    .replaceAll('{{filename}}', attachment.FileName);

                $('#instructionlist').append(attachmentHtml);

                //handle remove
                $('#deleteTaskTemplateAttachmentPdf-' + ezgolist.tmpl.Id).off('click').on('click', function (elem) {
                    var currentItemId = $(this).attr('id').replace('deleteTaskTemplateAttachmentPdf-', '');
                    ezgolist.tmpl.Attachments = [];
                    ezgolist.renderTaskSteps(ezgolist.tmpl.Id);
                });
            }
            else if (attachment.AttachmentType.toLowerCase() == "link") {
                var attachmentHtml = taskTemplateAttachmentTemplateLink
                    .replaceAll('{{url}}', attachmentUri)
                    .replaceAll('{{encodedUrl}}', encodeURIComponent(attachmentUri))
                    .replaceAll('{{itemid}}', template.Id);

                $('#instructionlist').append(attachmentHtml);

                //handle remove
                $('#deleteTaskTemplateAttachmentLink-' + ezgolist.tmpl.Id).off('click').on('click', function (elem) {
                    var currentItemId = $(this).attr('id').replace('deleteTaskTemplateAttachmentLink-', '');
                    ezgolist.tmpl.Attachments = [];
                    ezgolist.renderTaskSteps(ezgolist.tmpl.Id);
                });
            }
        }

        if (template.WorkInstructionRelations !== undefined && template.WorkInstructionRelations.length > 0) {
            var workInstructionRelationId = template.WorkInstructionRelations[0].WorkInstructionTemplateId;
            var workInstruction = ezgolist.currentWorkInstructionTemplates.filter(w => w.Id == workInstructionRelationId)[0];
            var picture = ezgolist.placeholderImageUrl;

            if (workInstruction?.Picture) {
                picture = ezgolist.mediaUrl + workInstruction.Picture;
            }

            var attachment = newTemplateItemAttachmentWorkInstruction
                .replaceAll('{{picture}}', picture)
                .replaceAll('{{cid}}', template.Id)
                .replaceAll('{{name}}', workInstruction != undefined ? GlobalParser.escapeHtmlCharacters(workInstruction.Name) : 'work instruction not found');
            $('#instructionlist').append(attachment);
        }
        //check if feature WI is on and use newTemplateItemChooserWI if its on
        if ((template.StepsCount === undefined || template.StepsCount === 0) && template.DescriptionFile === undefined && (template.Attachments === undefined || template.Attachments.length == 0) && (template.WorkInstructionRelations === undefined || template.WorkInstructionRelations.length == 0)) {
            var chooseButton = '';
            if (ezgolist.workInstructionsEnabled && ezgolist.taskTemplateAttachmentsEnabled) {
                chooseButton = newTemplateItemChooserWI
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addworkinstruction}}', ezgolist.language.addworkinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf)
                    .replaceAll('{{addlink}}', ezgolist.language.addlink);
            }
            else if (!ezgolist.workInstructionsEnabled && ezgolist.taskTemplateAttachmentsEnabled) {
                chooseButton = newTemplateItemChooser
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf)
                    .replaceAll('{{addlink}}', ezgolist.language.addlink);
            }
            else if (ezgolist.workInstructionsEnabled && !ezgolist.taskTemplateAttachmentsEnabled) {
                chooseButton = newTemplateItemChooserWIWithoutLink
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addworkinstruction}}', ezgolist.language.addworkinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf);
            }
            else {
                chooseButton = newTemplateItemChooserWithoutLink
                    .replaceAll('{{cid}}', templateId)
                    .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction)
                    .replaceAll('{{addpdf}}', ezgolist.language.addpdf);
            }
            $('#instructionlist').append(chooseButton);
        }

        if (template.StepsCount > 0) {
            var addButton = newTemplateItemStep
                .replaceAll('{{cid}}', templateId)
                .replaceAll('{{addinstruction}}', ezgolist.language.addinstruction);
            $('#instructionlist').append(addButton);
        }

        var picture = ezgolist.placeholderImageUrl
        if (template.Picture != undefined && !template.Picture.toString().startsWith('data:')) {
            picture = ezgolist.mediaUrl + template.Picture;
        }

        if (template.VideoThumbnail !== undefined && (!String(template.VideoThumbnail).startsWith("blob:"))) {
            picture = ezgolist.mediaUrl + template.VideoThumbnail;
        }

        if (template.VideoThumbnail !== undefined && String(template.VideoThumbnail).startsWith("blob:")) {
            picture = template.VideoThumbnail;
        }

        if (String(template.Picture).match("^data:") || String(template.Picture).match("^blob:")) {
            picture = template.Picture;
        }

        $('#dialogImg').data('src', picture).parent('a').data('href', picture).data('caption', template.Name);

        if (template.VideoThumbnail !== undefined && template.Video !== undefined) {
            $('#dialogVideo').data('src', template.Video);
            $('#TemplateImage').data('src', picture);
            $('#TemplateImage').parent('a').data('href', '#dialogVideo').attr('href', '#dialogVideo');
        }

        ezgomediafetcher.preloadImagesAndVideos();
        ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
    },

    previewFile: function (input) {
        var file = $(input).get(0).files[0];
        var previewElement = $(input).data('preview');
        var templateId = $(input).data('templateid');
        var templateStepId = $(input).data('templatestepid');
        var uploadType = $(input).data('type');

        if (file) {
            var reader = new FileReader();

            reader.onload = function () {
                ezgolist._hasChanged = true;
                $(previewElement).attr("src", reader.result);

                switch (uploadType) {
                    case 'template':
                        ezgolist.tmpl.Picture = reader.result;
                        break;

                    case 'item':
                        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                            return obj.Id === templateId;
                        })[0];
                        template.Picture = reader.result;
                        ezgolist.render();
                        ezgolist.renderDialogContent(templateId);
                        break;

                    case 'instruction':
                        var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                            return obj.Id === templateId;
                        })[0];
                        var step = template.Steps.filter(obj => {
                            return obj.Id === templateStepId;
                        })[0];
                        step.Picture = reader.result;
                        ezgolist.renderStepDialogContent(templateStepId);
                        break;

                    case 'attachment':
                        if (!file.name.toLowerCase().endsWith('.pdf')) {
                            toastr.error('Attachment is not of type pdf! Attachment not added.');
                            return;
                        }
                        var template;
                        if (ezgolist.listType !== 'task') {
                            template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                                return obj.Id === templateId;
                            })[0];
                        }
                        else {
                            template = ezgolist.tmpl;
                        }

                        ezgolist.updateItem(template.Id);
                        var url = URL.createObjectURL(file);
                        template.DescriptionFile = url;

                        if (ezgolist.listType !== 'task') {
                            ezgolist.renderDialogContent(templateId);
                        }
                        else {
                            ezgolist.renderTaskSteps(templateId);
                        }
                        break;
                }
                ezgolist.render();
                $('body').trigger('ezgolistChanged');
            }
            reader.readAsDataURL(file);
        }
    },

    previewFileEx: async function (input) {
        var previewElement = $(input).data('preview');
        var templateId = $(input).data('templateid');
        var templateStepId = $(input).data('templatestepid');
        var file = event.target.files[0];
        var ext = file.name.split('.').pop();
        var fileReader = new FileReader();
        var uploadType = $(input).data('type');

        //Filesize validation check
        var elementId = '#fileSizeAlert-' + uploadType;
        $(elementId).hide();

        if (file.size > ezgolist.maxUploadFileSize) {
            $(elementId).removeClass('d-none').fadeIn();
            input.value = "";
            setTimeout(function () {
                $(elementId).fadeOut();
            }, 3000);
            return;
        };

        if (file.type.match('image')) {
            const options = {
                maxWidthOrHeight: ezgolist.maxImageWidthOrHeight,
                useWebWorker: true,
                maxIteration: 1
            };
            //First only resize file
            var compressedFile = await imageCompression(file, options);
            //Then compress image if necessary
            while (compressedFile.size / 1024 / 1024 > ezgolist.maxImageSizeMB) {
                options.maxSizeMB = ezgolist.maxImageSizeMB;
                compressedFile = await imageCompression(compressedFile, options);
            }

            fileReader.onload = function () {
                ezgolist._hasChanged = true;
                var url = URL.createObjectURL(compressedFile, { oneTimeOnly: true });
                var template;

                if (ezgolist.listType !== 'task') {
                    template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                        return obj.Id === templateId;
                    })[0];
                }
                else {
                    template = ezgolist.tmpl;
                }

                if (uploadType === 'template') {
                    ezgolist.tmpl.Picture = url;
                    ezgolist.tmpl.PictureType = ext;
                }

                if (uploadType === 'item') {
                    template.Video = undefined;
                    template.VideoThumbnail = undefined;
                    template.VideoType = undefined;

                    template.Picture = url;
                    template.PictureType = ext;
                }

                if (uploadType === 'instruction') {
                    var step = template.Steps.filter(obj => {
                        return obj.Id === templateStepId;
                    })[0];

                    step.Video = undefined;
                    step.VideoThumbnail = undefined;
                    step.VideoType = undefined;

                    step.Picture = url; // fileReader.result;
                    step.PictureType = ext;
                }

                $(previewElement).attr("src", url).parent('a').attr('href', url);
                $(previewElement).data("src", url).parent('a').data('href', url);
                $('body').trigger('ezgolistChanged');
            };
            fileReader.readAsDataURL(compressedFile);
        }
        else {
            fileReader.onload = function () {
                var url = URL.createObjectURL(file);
                var video = document.createElement('video');
                var timeupdate = function () {
                    if (snapImage()) {
                        video.removeEventListener('timeupdate', timeupdate);
                        video.pause();
                    }
                };
                video.addEventListener('loadeddata', function () {
                    if (snapImage()) {
                        video.removeEventListener('timeupdate', timeupdate);
                    }
                });
                var snapImage = function () {
                    var canvas = document.createElement('canvas');
                    canvas.width = video.videoWidth;
                    canvas.height = video.videoHeight;
                    canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
                    var image = canvas.toDataURL();
                    var success = image.length > 100000;

                    if (success) {
                        var objecturl = URL.createObjectURL($.utils.dataURItoBlob(image));
                        var template;

                        if (ezgolist.listType !== 'task') {
                            template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                                return obj.Id === templateId;
                            })[0];
                        }
                        else {
                            template = ezgolist.tmpl;
                        }

                        if (uploadType === 'item') {
                            template.Picture = undefined;
                            template.PictureType = undefined;

                            template.VideoThumbnail = objecturl;
                            template.Video = url;
                            template.VideoType = ext;
                            $(previewElement).data("src", objecturl);
                            $('#dialogVideo').data('src', url);
                            $(previewElement).parent('a').data('href', '#dialogVideo').attr('href', '#dialogVideo');
                            ezgomediafetcher.preloadImagesAndVideos();
                            ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
                        }

                        if (uploadType === 'instruction') {
                            var step = template.Steps.filter(obj => {
                                return obj.Id === templateStepId;
                            })[0];

                            step.Picture = undefined;
                            step.PictureType = undefined;

                            step.VideoThumbnail = objecturl;
                            step.Video = url;
                            step.VideoType = ext;
                            $(previewElement).attr("src", objecturl);
                        }
                    }
                    return success;
                };

                video.addEventListener('timeupdate', timeupdate);
                video.preload = 'metadata';
                video.src = url;
                //Load video in Safari / IE11
                video.muted = true;
                video.playsInline = true;
                video.play();
            };
            fileReader.readAsArrayBuffer(file);
        }
    },

    dropPreviewFileEx: async function (input, files) {
        var previewElement = $(input).data('preview');
        var templateId = $(input).data('templateid');
        var templateStepId = $(input).data('templatestepid');
        var file = files[0];
        var ext = file.name.split('.').pop();
        var fileReader = new FileReader();
        var uploadType = $(input).data('type');

        //Filesize validation check
        var elementId = '#fileSizeAlert-' + uploadType;
        $(elementId).hide();

        if (file.size > ezgolist.maxUploadFileSize) {
            $(elementId).removeClass('d-none').fadeIn();
            input.value = "";
            setTimeout(function () {
                $(elementId).fadeOut();
            }, 3000);
            return;
        };

        if (file.type.match('image')) {
            const options = {
                maxWidthOrHeight: ezgolist.maxImageWidthOrHeight,
                useWebWorker: true,
                maxIteration: 1
            };
            //First only resize file
            var compressedFile = await imageCompression(file, options);
            //Then compress image if necessary
            while (compressedFile.size / 1024 / 1024 > ezgolist.maxImageSizeMB) {
                options.maxSizeMB = ezgolist.maxImageSizeMB;
                compressedFile = await imageCompression(compressedFile, options);
            }

            fileReader.onload = function () {
                ezgolist._hasChanged = true;
                var url = URL.createObjectURL(compressedFile, { oneTimeOnly: true });
                var template;

                if (ezgolist.listType !== 'task') {
                    template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                        return obj.Id === templateId;
                    })[0];
                }
                else {
                    template = ezgolist.tmpl;
                }

                if (uploadType === 'template') {
                    ezgolist.tmpl.Picture = url;
                    ezgolist.tmpl.PictureType = ext;
                }

                if (uploadType === 'item') {
                    template.Video = undefined;
                    template.VideoThumbnail = undefined;
                    template.Picture = url;
                    template.PictureType = ext;
                }

                if (uploadType === 'instruction') {
                    var step = template.Steps.filter(obj => {
                        return obj.Id === templateStepId;
                    })[0];
                    step.Picture = url;
                    step.Video = undefined;
                    step.VideoThumbnail = undefined;
                    step.PictureType = ext;
                }

                $(previewElement).attr("src", url).parent('a').attr('href', url);
                $('body').trigger('ezgolistChanged');
            };
            fileReader.readAsDataURL(compressedFile);
        }
        else {
            fileReader.onload = function () {
                var url = URL.createObjectURL(file);
                var video = document.createElement('video');
                var timeupdate = function () {
                    if (snapImage()) {
                        video.removeEventListener('timeupdate', timeupdate);
                        video.pause();
                    }
                };
                video.addEventListener('loadeddata', function () {
                    if (snapImage()) {
                        video.removeEventListener('timeupdate', timeupdate);
                    }
                });
                var snapImage = function () {
                    var canvas = document.createElement('canvas');
                    canvas.width = video.videoWidth;
                    canvas.height = video.videoHeight;
                    canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
                    var image = canvas.toDataURL();
                    var success = image.length > 100000;

                    if (success) {
                        var objecturl = URL.createObjectURL($.utils.dataURItoBlob(image));
                        var template;

                        if (ezgolist.listType !== 'task') {
                            template = ezgolist.tmpl.TaskTemplates.filter(obj => {
                                return obj.Id === templateId;
                            })[0];
                        }
                        else {
                            template = ezgolist.tmpl;
                        }

                        if (uploadType === 'item') {
                            template.VideoThumbnail = objecturl;
                            template.Video = url;
                            template.Picture = undefined;
                            template.VideoType = ext;
                            $(previewElement).data("src", objecturl);
                            $('#dialogVideo').data('src', url);
                            $(previewElement).parent('a').data('href', '#dialogVideo').attr('href', '#dialogVideo');
                            ezgomediafetcher.preloadImagesAndVideos();
                            ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
                        }

                        if (uploadType === 'instruction') {
                            var step = template.Steps.filter(obj => {
                                return obj.Id === templateStepId;
                            })[0];

                            step.picture = undefined;
                            step.VideoThumbnail = objecturl;
                            step.Video = url;
                            step.VideoType = ext;
                            $(previewElement).attr("src", objecturl);
                        }
                    }
                    return success;
                };
                video.addEventListener('timeupdate', timeupdate);
                video.preload = 'metadata';
                video.src = url;
                //Load video in Safari / IE11
                video.muted = true;
                video.playsInline = true;
                video.play();
            };
            fileReader.readAsArrayBuffer(file);
        }
    },

    openDialog: function () {
        var isOpen = $('#myModal').is(':visible');
    },

    toJsonString: function () {
        $('#output').html(JSON.stringify(this.tmpl, null, 4));
    },

    //** openfields logic **//
    renderOpenFields: function () {
        if ($(ezgolist.openFieldsSelector) != null && $(ezgolist.openFieldsSelector) !== undefined) {
            $(ezgolist.openFieldsSelector).children('[data-type="open_field"]').remove();

            if (ezgolist.tmpl.OpenFieldsProperties != null) {
                let openfieldsHtmlTemplate = '<li class="list-inline-item pb-3" data-pid="{{propertyid}}" data-tpid="{{templatepropertyid}}" data-type="open_field" data-datatype="{{valuetype}}" data-display="{{displaytitle}}" data-isrequiredbyuser="{{requiredbyuser}}" data-index="{{propertyindex}}"><div class="card h-100 shadow-sm" style="border: 1px solid #edebeb;min-height:100px;"><div style="margin:5px;text-align:right;"><span data-action="move_open_field_previous"><i class="fas fa-chevron-left" style="cursor:pointer;" title="Move item previous"></i></span> <span data-action="move_open_field_next"><i class="fas fa-chevron-right" style="cursor:pointer;"  title="Move item next"></i></span> <span data-action="edit_open_field"><i class="fa fa-pencil-alt" style="cursor:pointer;" title="Edit item"></i></span> <span data-action="remove_open_field"><i class="fa fa-trash" style="cursor:pointer;color:red;"  title="Remove item"></i></span><div class="card-body p-0" style="text-align:left"><div><p class="view p-2">{{displaytitle}}</p></div></div></div><div style="position:absolute; bottom: 0px; right:0px; vertical-align:bottom;text-align:right;margin:5px; font-size:10px; color: #C0C0C0"><span>{{requiredtext}}</span> <span>{{typetext}}</span></div></div></li>';
                let allfields = '';

                $(ezgolist.tmpl.OpenFieldsProperties).each(function (index, item) {
                    let openFieldHtml = openfieldsHtmlTemplate.replaceAll('{{propertyid}}', item.PropertyId).replaceAll('{{templatepropertyid}}', item.Id).replaceAll('{{valuetype}}', item.ValueType).replaceAll('{{displaytitle}}', item.TitleDisplay).replaceAll('{{requiredbyuser}}', item.IsRequired).replaceAll('{{propertyindex}}', item.Index);
                    openFieldHtml = openFieldHtml.replaceAll('{{requiredtext}}', item.IsRequired ? 'required' : '');
                    openFieldHtml = openFieldHtml.replaceAll('{{typetext}}', ezgolist.generateOpenFieldsTypeDisplayText(item.ValueType));
                    allfields = allfields + openFieldHtml;
                });

                $(ezgolist.openFieldsSelector).prepend(allfields);

                if (ezgolist.tmpl.OpenFieldsProperties.length == 10) {
                    $('[data-containertype="add_open_field_container"]').hide();
                }
                else {
                    $('[data-containertype="add_open_field_container"]').show();
                }
            };
        }
    },

    generateOpenFieldsTypeDisplayText: function (fieldtype) {
        if (fieldtype == 5) {
            return 'date-time';
        };

        if (fieldtype == 2) {
            return 'text';
        };

        if (fieldtype == 0) {
            return 'number';
        };

        if (fieldtype == 1) {
            return 'decimal number';
        };

        return '';
    },

    setOpenFieldHandlers: function () {
        $(ezgolist.openFieldsSelector + ',#editOpenFieldModal').on('click', '[data-action="remove_open_field"],[data-action="delete_close_open_field"]', function () {
            ezgolist.deleteOpenField($(this).attr('data-action') == 'delete_close_open_field' ? $('#modal_openfield_tpid').val() : $(this).closest('li').attr('data-tpid'));
        });

        $(ezgolist.openFieldsSelector).on('click', '[data-action="edit_open_field"]', function () {
            ezgolist.editOpenField(this);
        });

        $(ezgolist.openFieldsSelector).on('click', '[data-action="add_open_field"]', function () {
            ezgolist.addNewOpenField(this);
        });

        $(ezgolist.openFieldsSelector).on('click', '[data-action="move_open_field_next"]', function () {
            ezgolist.moveOpenFields($(this).closest('li').attr('data-tpid'), 'next');
        });

        $(ezgolist.openFieldsSelector).on('click', '[data-action="move_open_field_previous"]', function () {
            ezgolist.moveOpenFields($(this).closest('li').attr('data-tpid'), 'previous');
        });

        $('#editOpenFieldModal').on('click', '[data-action="save_open_field"],[data-action="save_and_close_open_field"]', function () {
            ezgolist.saveOpenField($(this).attr('data-action') == 'save_and_close_open_field');
        });
    },

    addNewOpenField: function (item) {
        ezgolist._hasChanged = true;
        $('#btnDeleteOpenField').hide();
        $('#btnSaveOpenField').show();
        $('#btnSaveCloseOpenField').show();
        $('#editOpenFieldModal').modal('show');
        ezgolist.clearOpenFieldModal();
        $('body').trigger('ezgolistChanged');
    },

    deleteOpenField: function (id) {
        ezgolist._hasChanged = true;
        ezgolist.tmpl.OpenFieldsProperties = ezgolist.tmpl.OpenFieldsProperties.filter(function (item) {
            return item.Id !== parseInt(id);
        });
        ezgolist.reIndexOpenFields();
        ezgolist.renderOpenFields();
        $('#editOpenFieldModal').modal('hide');
        $('body').trigger('ezgolistChanged');
    },

    editOpenField: function (item) {
        ezgolist.clearOpenFieldModal();
        let container = $(item).closest('li');
        $('#modal_openfield_select').val(container.attr('data-pid'));
        $('#modal_openfield_select').attr('disabled', 'disabled');
        $('#modal_openfield_tpid').val(container.attr('data-tpid'));
        $('#modal_openfield_name').val(container.attr('data-display'));
        $('#modal_openfield_required').prop('checked', container.attr('data-isrequiredbyuser') == 'true');
        $('#btnDeleteOpenField').show();
        $('#btnSaveOpenField').hide();
        $('#btnSaveCloseOpenField').show();
        $('#editOpenFieldModal').modal('show');
    },

    saveOpenField: function (autoClose) {
        ezgolist._hasChanged = true;
        let propertyvalue = $('#modal_openfield_select').val();
        let displayvalue = $('#modal_openfield_name').val();
        let requiredvalue = $('#modal_openfield_required').is(':checked');
        let valuetype = $('#modal_openfield_select option:selected').attr('data-datatype');
        let templatepropertyid = $('#modal_openfield_tpid').val();

        if (templatepropertyid != null && templatepropertyid !== '') {
            ezgolist.updateExistingOpenFieldObject(templatepropertyid, propertyvalue, displayvalue, requiredvalue, valuetype);
        }
        else if (propertyvalue !== '') {
            ezgolist.generateNewOpenFieldObject(propertyvalue, displayvalue, requiredvalue, valuetype);
        }

        if (autoClose) {
            $('#editOpenFieldModal').modal('hide');
        }
        else {
            ezgolist.clearAndContinueOpenFieldModal();
            if (ezgolist.tmpl.OpenFieldsProperties.length == 10) {
                $('#btnSaveCloseOpenField').hide();
                $('#btnSaveOpenField').hide();
                $('#editOpenFieldModal').modal('hide');
            }
        }

        $('body').trigger('ezgolistChanged');
    },

    clearOpenFieldModal: function () {
        $('#modal_openfield_select').val('920');
        $('#modal_openfield_name').val('');
        $('#modal_openfield_tpid').val('');
        $('#modal_openfield_select').removeAttr('disabled');
        $('#modal_openfield_required').prop("checked", false);
    },

    clearAndContinueOpenFieldModal: function () {
        $('#modal_openfield_name').val('');
        $('#modal_openfield_tpid').val('');
        $('#modal_openfield_required').prop("checked", false);
        $('#modal_openfield_select').removeAttr('disabled');
        $('#modal_openfield_name').focus();
    },

    generateNewOpenFieldObject: function (propertyId, titleDisplay, isRequired, valueType) {
        if (propertyId != null && propertyId !== '' && titleDisplay != null && titleDisplay !== '') {
            var openfield = {
                PropertyId: parseInt(propertyId),
                TitleDisplay: titleDisplay,
                IsRequired: isRequired,
                ValueType: parseInt(valueType),
                Index: ezgolist.tmpl.OpenFieldsProperties != null ? ezgolist.tmpl.OpenFieldsProperties.length : 0,
                Id: ezgolist.tmpl.OpenFieldsProperties != null ? ezgolist.tmpl.OpenFieldsProperties.length * -1 : 0, //generate a negative non existing id, now based on index.
            }

            if (ezgolist.tmpl != null) {
                if (ezgolist.tmpl.OpenFieldsProperties == undefined) ezgolist.tmpl['OpenFieldsProperties'] = [];
                if (ezgolist.tmpl.OpenFieldsProperties == null) ezgolist.tmpl.OpenFieldsProperties = [];
                ezgolist.tmpl.OpenFieldsProperties.push(openfield);
                ezgolist.setMessageOpenFieldsModal('<span style="color:green">Item added</span>');
                ezgolist.renderOpenFields();
            }
        }
    },

    updateExistingOpenFieldObject: function (templatePropertyId, propertyId, titleDisplay, isRequired, valueType) {
        for (var i = 0; i < ezgolist.tmpl.OpenFieldsProperties.length; i++) {
            if (ezgolist.tmpl.OpenFieldsProperties[i].Id == parseInt(templatePropertyId)) {
                ezgolist.tmpl.OpenFieldsProperties[i].TitleDisplay = titleDisplay;
                ezgolist.tmpl.OpenFieldsProperties[i].IsRequired = isRequired;
            };
        }
        ezgolist.renderOpenFields();
    },

    moveOpenFields: function (id, direction) {
        let changed = false;
        if (direction == 'next') {
            changed = ezgolist.moveOpenFieldDown(id);
        }
        else if (direction == 'previous') {
            changed = ezgolist.moveOpenFieldUp(id);
        }

        if (changed) {
            ezgolist._hasChanged = true;
            ezgolist.reIndexOpenFields();
            ezgolist.renderOpenFields();
            $('body').trigger('ezgolistChanged');
        }
    },

    moveOpenFieldUp: function (id) {
        let foundItem = ezgolist.tmpl.OpenFieldsProperties.filter(function (item) {
            return item.Id == parseInt(id);
        });

        if (foundItem != null && foundItem.length > 0) {
            //make sure no negative index occurs.
            if (foundItem[0].Index > 0) {
                ezgolist.deleteOpenField(id); //remove original item
                ezgolist.tmpl.OpenFieldsProperties.splice(foundItem[0].Index - 1, 0, foundItem[0]);
            }
            return true;
        }
        return false;
    },

    moveOpenFieldDown: function (id) {
        let foundItem = ezgolist.tmpl.OpenFieldsProperties.filter(function (item) {
            return item.Id == parseInt(id);
        });

        if (foundItem != null && foundItem.length > 0) {
            ezgolist.deleteOpenField(id); //remove original item
            ezgolist.tmpl.OpenFieldsProperties.splice(foundItem[0].Index + 1, 0, foundItem[0]);
            return true;
        }
        return false;
    },

    setMessageOpenFieldsModal: function (message) {
        $('#modal_openfield_message').html(message);
        setTimeout(function () { $('#modal_openfield_message').html('') }, 1000);
    },

    reIndexOpenFields: function () {
        for (var i = 0; i < ezgolist.tmpl.OpenFieldsProperties.length; i++) {
            ezgolist.tmpl.OpenFieldsProperties[i].Index = i;
        }
    },

    reIndexTaskTemplates: function () {
        // Try to sync with DOM order first
        var domItems = $(ezgolist.itemListSelector + ' li[data-type="item"]');

        if (domItems.length > 0 && domItems.length === ezgolist.tmpl.TaskTemplates.length) {
            var newOrder = [];
            domItems.each(function (index) {
                var cid = parseInt($(this).attr('data-cid'));
                var template = ezgolist.tmpl.TaskTemplates.find(function (obj) {
                    return obj.Id === cid || obj.WorkInstructionTemplateId === cid;
                });
                if (template != undefined) {
                    template.Index = index + 1;
                    newOrder.push(template);
                }
            });

            if (newOrder.length === ezgolist.tmpl.TaskTemplates.length) {
                ezgolist.tmpl.TaskTemplates = newOrder;
                return;
            }
        }

        // Fallback: just re-index the existing array order
        for (var i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
            ezgolist.tmpl.TaskTemplates[i].Index = i + 1;
        }
    }, 
    reIndexTaskTemplatesFromDOM: function () {
        // Sync TaskTemplates array order with the current DOM order
        var newOrder = [];
        $(ezgolist.itemListSelector + ' li[data-type="item"]').each(function (index) {
            var cid = parseInt($(this).attr('data-cid'));
            var template = ezgolist.tmpl.TaskTemplates.find(function (obj) {
                return obj.Id === cid || obj.WorkInstructionTemplateId === cid;
            });
            if (template != undefined) {
                template.Index = index + 1;
                newOrder.push(template);
            }
        });

        // If we found items in the DOM, use that order
        // Otherwise keep the existing array (for cases where DOM isn't rendered yet)
        if (newOrder.length > 0) {
            ezgolist.tmpl.TaskTemplates = newOrder;
        } else {
            // Just re-index the existing array
            ezgolist.tmpl.TaskTemplates.forEach(function (template, index) {
                template.Index = index + 1;
            });
        }
    },
    updateGeneralPictureProofToggle: function () {
        if (ezgolist.listType == 'checklist' || ezgolist.listType == 'audit') {
            var tasksHavePictureProof = false;
            $(ezgolist.tmpl.TaskTemplates).each(function (index, elem) {
                if (elem.HasPictureProof) {
                    tasksHavePictureProof = true;
                }
                else {
                    tasksHavePictureProof = false;
                    return false;
                }
            });

            $("#enablePictureProofForAllItems").prop('checked', tasksHavePictureProof);

            $(ezgolist.tmpl.TaskTemplates).each(function (index, elem) {
                if (!elem.HasPictureProof) {
                    $("#enablePictureProofForAllItems").prop('checked', false);
                    return false;
                }
            });
        }
    },

    //** end openfields logic **//
    //** start qr code logic **//
    enableQrCode: function () {
        if ($('#btnGenerateQRCode') != undefined) {
            if (ezgolist.tmpl != null && ezgolist.tmpl.Id != null && ezgolist.tmpl.Id > 0) {
                if (ezgolist.tmpl.Id > 0 && ($('#btnGenerateQRCode').attr('data-templateid') == null || $('#btnGenerateQRCode').attr('data-templateid') == 0)) {
                    $('#btnGenerateQRCode').attr('data-templateid', ezgolist.tmpl.Id);
                }

                if (ezgolist.tmpl.AreaId > 0 && ($('#btnGenerateQRCode').attr('data-areaid') == null || $('#btnGenerateQRCode').attr('data-areaid') == 0)) {
                    $('#btnGenerateQRCode').attr('data-areaid', ezgolist.tmpl.AreaId);
                }

                if ($('#btnGenerateQRCode').attr('data-areaid') != null && $('#btnGenerateQRCode').attr('data-templateid') != null) {
                    if ($('#btnGenerateQRCode').attr('data-areaid') != 0 && $('#btnGenerateQRCode').attr('data-templateid') != 0) {
                        $('#btnGenerateQRCode').removeAttr('disabled');
                    }
                }
            }
        }
    },

    //** end qr code logic **//
    //** assessment specific logic **//
    extension: {
        assessments: {
            selectionOrderCounter: 0,
            init: function () {
                ezgolist.extension.assessments.initHandlers();
                ezgolist.extension.assessments.initModal();
            },

            initHandlers: function () {
                $('[data-action="assessment_choice"]').on('click', function () {
                    if ($(this).attr('data-selected') == 'true') {
                        ezgolist.extension.assessments.setAssessmentRowDisplayOff(this, null);
                    }
                    else {
                        ezgolist.extension.assessments.setAssessmentRowDisplayOn(this);
                    }
                });

                $('[data-action="search_assessment"]').on('keyup', function () {
                    ezgolist.extension.assessments.searchModal($(this).val());
                });

                $('[data-action="close"]').on('click', function () {
                    ezgolist.extension.assessments.resetModal();
                });

                $('#clear_search_btn').on('click', function () {
                    $('#searchassessment').val('');
                    ezgolist.extension.assessments.searchModal('');
                });

                $('[data-action="addinstruction"]').on('click', function () {
                    ezgolist.extension.assessments.setAssessmentsToMainObject();
                });
            },

            initModal: function () {
                $('[data-containertype="assessment_choice"]').show();
                $('[data-containertype="assessment_choice_check"]').hide();
                ezgolist.extension.assessments.selectionOrderCounter = 0;
                ezgolist.extension.assessments.resetModalAssessmentChoice();
                ezgolist.extension.assessments.setAssessments();
            },

            resetModal: function () {
                ezgolist.extension.assessments.resetModalAssessmentChoice();
                $('#searchassessment').val('');
            },

            resetModalAssessmentChoice: function () {
                $('[data-containertype="assessment_choice_name"]').removeClass('font-weight-bold');
                $('[data-containertype="assessment_choice_check"]').hide();
            },

            setAssessments: function () {
                ezgolist.extension.assessments.selectionOrderCounter = 0;
                // Clear all existing selections first
                $('[data-containertype="assessment_choice"]').each(function () {
                    ezgolist.extension.assessments.setAssessmentRowDisplayOff(this);
                });

                // Set selections based on current TaskTemplates order
                if (ezgolist.tmpl.TaskTemplates != null && ezgolist.tmpl.TaskTemplates.length > 0) {
                    for (let i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
                        let template = ezgolist.tmpl.TaskTemplates[i];
                        let foundAssessment = $('[data-id="' + template.WorkInstructionTemplateId + '"][data-containertype="assessment_choice"]');

                        if (foundAssessment.length > 0) {
                            // Use i+1 as the display order in modal
                            ezgolist.extension.assessments.setAssessmentRowDisplayOn(foundAssessment[0], i + 1);
                        }
                    }
                }
            },

            setAssessmentRowDisplayOn: function (row, index) {
                $(row).find('[data-containertype="assessment_choice_name"]').addClass('font-weight-bold');
                $(row).find('[data-containertype="assessment_choice_check"]').show();
                var resolvedIndex = index;

                if (resolvedIndex == null || isNaN(resolvedIndex)) {
                    ezgolist.extension.assessments.selectionOrderCounter++;
                    resolvedIndex = ezgolist.extension.assessments.selectionOrderCounter;
                }
                else {
                    ezgolist.extension.assessments.selectionOrderCounter = Math.max(
                        ezgolist.extension.assessments.selectionOrderCounter,
                        resolvedIndex
                    );
                }

                $(row).attr('data-index', resolvedIndex);
                $(row).attr('data-selected', true);
            },

            setAssessmentRowDisplayOff: function (row) {
                $(row).find('[data-containertype="assessment_choice_name"]').removeClass('font-weight-bold');
                $(row).find('[data-containertype="assessment_choice_check"]').hide();
                $(row).removeAttr('data-index');
                $(row).removeAttr('data-selected');
            },

            setAssessments: function () {
                ezgolist.extension.assessments.selectionOrderCounter = 0;
                // Clear all existing selections first
                $('[data-containertype="assessment_choice"]').each(function () {
                    ezgolist.extension.assessments.setAssessmentRowDisplayOff(this);
                });

                // Set selections based on current TaskTemplates order
                if (ezgolist.tmpl.TaskTemplates != null && ezgolist.tmpl.TaskTemplates.length > 0) {
                    for (let i = 0; i < ezgolist.tmpl.TaskTemplates.length; i++) {
                        let template = ezgolist.tmpl.TaskTemplates[i];
                        let foundAssessment = $('[data-id="' + template.WorkInstructionTemplateId + '"][data-containertype="assessment_choice"]');

                        if (foundAssessment.length > 0) {
                            // Use i+1 as the display order in modal
                            ezgolist.extension.assessments.setAssessmentRowDisplayOn(foundAssessment[0], i + 1);
                        }
                    }
                }
            },

            setAssessmentsToMainObject: function () {
                // First, capture the current DOM order of items BEFORE processing modal selections
                // This preserves any manual drag-and-drop reordering
                var currentDOMOrder = [];
                $(ezgolist.itemListSelector + ' li[data-type="item"]').each(function () {
                    var cid = parseInt($(this).attr('data-cid'));
                    if (!isNaN(cid)) {
                        currentDOMOrder.push(cid);
                    }
                });

                // Get all selected assessments from modal
                let selectedAssessments = $('[data-containertype="assessment_choice"][data-selected="true"]');
                let selectedAssessmentsOrdered = selectedAssessments.toArray().sort(function (a, b) {
                    let aIndex = parseInt($(a).attr('data-index'));
                    let bIndex = parseInt($(b).attr('data-index'));

                    if (isNaN(aIndex)) aIndex = Number.MAX_SAFE_INTEGER;
                    if (isNaN(bIndex)) bIndex = Number.MAX_SAFE_INTEGER;

                    if (aIndex === bIndex) {
                        return parseInt($(a).attr('data-id')) - parseInt($(b).attr('data-id'));
                    }

                    return aIndex - bIndex;
                });

                // Build a set of currently selected WorkInstructionTemplateIds
                let selectedWIIds = new Set();
                selectedAssessmentsOrdered.forEach(function (elem) {
                    let wiId = parseInt($(elem).attr('data-id'));
                    selectedWIIds.add(wiId);
                });

                // Build a map of WorkInstructionTemplateId -> existing TaskTemplate
                // but only retain entries that remain selected. This prevents deselected
                // items from lingering in the client-side list until a page refresh.
                let existingByWIId = new Map();
                if (ezgolist.tmpl.TaskTemplates && ezgolist.tmpl.TaskTemplates.length > 0) {
                    ezgolist.tmpl.TaskTemplates.forEach(function (template) {
                        if (selectedWIIds.has(template.WorkInstructionTemplateId)) {
                            existingByWIId.set(template.WorkInstructionTemplateId, template);
                        }
                    });
                }

                // Step 1: Reorder existing items based on current DOM order
                let reorderedExisting = [];
                let usedTemplates = new Set();

                currentDOMOrder.forEach(function (cid) {
                    // For skillassessment, data-cid is WorkInstructionTemplateId
                    let template = ezgolist.tmpl.TaskTemplates.find(function (t) {
                        return t.WorkInstructionTemplateId === cid && !usedTemplates.has(t);
                    });
                    if (template && selectedWIIds.has(template.WorkInstructionTemplateId)) {
                        usedTemplates.add(template);
                        reorderedExisting.push(template);
                    }
                });

                // Get IDs of items we've already added to reorderedExisting
                let addedWIIds = new Set(reorderedExisting.map(function (t) {
                    return t.WorkInstructionTemplateId;
                }));

                // Step 2: Find newly selected items (not in DOM yet)
                let newItems = [];
                selectedAssessmentsOrdered.forEach(function (elem) {
                    let wiId = parseInt(elem.getAttribute('data-id'));

                    // Skip if already added from DOM order
                    if (addedWIIds.has(wiId)) {
                        return;
                    }

                    // Check if it exists but wasn't in DOM (edge case)
                    if (existingByWIId.has(wiId)) {
                        reorderedExisting.push(existingByWIId.get(wiId));
                        addedWIIds.add(wiId);
                        return;
                    }

                    // Truly new item - parse from modal data
                    let dataitem = elem.querySelector('[data-type="datastore"]');
                    if (dataitem != null && $(dataitem).val() != null) {
                        let data = JSON.parse($(dataitem).val());

                        // Preserve the WorkInstructionTemplateId so DOM tracking remains stable after re-render
                        // even when Id is swapped out for a temporary client-side value.
                        data.WorkInstructionTemplateId = data.WorkInstructionTemplateId || data.Id;

                        // Generate unique negative ID for new items to enable DOM tracking
                        ezgolist._lastNewItemId = (ezgolist._lastNewItemId || 0) - 1;
                        data.Id = ezgolist._lastNewItemId;
                        data.isNew = true;
                        newItems.push(data);
                    }
                });

                // Step 3: Combine - existing items in DOM order, then new items at the end
                ezgolist.tmpl.TaskTemplates = reorderedExisting.concat(newItems);

                // Step 4: Re-index sequentially
                ezgolist.tmpl.TaskTemplates.forEach(function (template, index) {
                    template.Index = index + 1;
                });

                // Mark as changed
                ezgolist._hasChanged = true;
                $('body').trigger('ezgolistChanged');

                ezgolist.render();
                $('#assignInstructionModal').modal('hide');
            },

            searchModal: function (searchValue) {
                searchValue = searchValue.replaceAll(',', '');
                searchValue = searchValue.replaceAll(';', '');
                var searchTerms = searchValue.split(' ');
                if (searchValue != null && searchValue != '') {
                    $('[data-containertype="assessment_choice"]').hide();
                    $('[data-containertype="assessment_choice"]').each(function () {
                        var elem = this;
                        searchTerms.forEach(function (s) {
                            if ($(elem).attr('data-name').toLowerCase().includes(searchValue.toLowerCase())) {
                                $(elem).show();
                            }
                        });
                    });
                }
                else {
                    $('[data-containertype="assessment_choice"]').show();
                }
            }
        }
    }
    //** end assessment specific logic **//
}

function showEditTemplateItemDialog(templateId) {
    var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
        return obj.Id === templateId;
    })[0];

    if (ezgolist.listType === 'workinstruction') {
        if (template.Attachments != null && template.Attachments.length) {
            var attachment = template.Attachments[0];
            if (attachment.AttachmentType.toLowerCase() == "pdf") {
                var attachmentUri = ezgolist.mediaUrl + attachment.Uri;

                if (attachment.Uri.startsWith('blob:')) {
                    attachmentUri = attachment.Uri;
                }

                var newAttachment = workInstructionItemAttachmentTemplatePdf
                    .replaceAll('{{url}}', attachmentUri)
                    .replaceAll('{{itemid}}', template.Id)
                    .replaceAll('{{filename}}', attachment.FileName)
                    .replaceAll('{{fancy}}', "workInstruction-" + template.Id);

                $('#workInstructionItemAttachmentsPreviewContainer').html(newAttachment);
                $('#workInstructionItemAttachmentsPreviewContainer').show();
                $('#workInstructionItemAttachmentsSelectionContainer').hide();
            }
            else if (attachment.AttachmentType.toLowerCase() == "link") {
                var attachmentUri = attachment.Uri;

                var newAttachment = workInstructionItemAttachmentTemplateLink
                    .replaceAll('{{url}}', attachmentUri)
                    .replaceAll('{{encodedUrl}}', encodeURIComponent(attachmentUri))
                    .replaceAll('{{itemid}}', template.Id);

                $('#workInstructionItemAttachmentsPreviewContainer').html(newAttachment);
                $('#workInstructionItemAttachmentsPreviewContainer').show();
                $('#workInstructionItemAttachmentsSelectionContainer').hide();
            }

            ezgomediafetcher.preloadImagesAndVideos();
            ezgomediafetcher.preloadFancyboxAnchorsExceptVideos();
        }
        else {
            $('[id^="workInstructionItemAttachment-"]').remove();
            $('#workInstructionItemAttachmentsSelectionContainer').show();
        }
    }

    if (ezgolist.listType === 'skillassessment') {
        $('#assignInstructionModal').modal('show');
    }
    else {
        ezgolist.renderDialogContent(templateId);
        $('#editTemplateStepModal').modal('show');
    }
}

function showTemplateItemDialog() {

    if (ezgolist.listType === 'skillassessment') {
        $('#assignInstructionModal').modal('show');
    }
    else {
        $(".add-button").remove();
        var templateId = ezgolist.addItem();
        ezgolist.renderDialogContent(templateId);
        $('#editTemplateStepModal').modal('show');
    }

    if (ezgolist.listType === 'workinstruction') {
        $('[id^="workInstructionItemAttachment-"]').remove();
        $('#workInstructionItemAttachmentsSelectionContainer').show();
    }
}

function showStageTemplateDialog(event, stageTemplateId) {
    //if element is being dragged, don't show stage template dialog
    if (event != undefined) {
        if ($(event.originalTarget).closest('li').length > 0 && $(event.originalTarget).closest('li').data('dragging')) {
            $(event.originalTarget).closest('li').data('dragging', false);
            return;
        }
    }
    //get stage template from ezgolist.tmpl
    //use it to set modal values
    var stageTemplate = ezgolist.tmpl.StageTemplates.filter(obj => {
        return obj.Id === stageTemplateId;
    })[0];

    var tagsToDeselect = document.getElementsByClassName("stage-tag");
    if (tagsToDeselect.length > 0) {
        tagsToDeselect.forEach(function (element) {
            element.classList.remove("btn-ezgo");
            element.classList.add("btn-outline-ezgo");
            element.classList.remove("selected-tag");
        })
    }

    if (stageTemplate != null && stageTemplate != undefined) {

        var stageObjectIndex = ezgolist.tmpl.StageTemplates.findIndex((obj => obj.Id == stageTemplateId));
        $('#editStageTemplateStepModal .modal-header h4').html(ezgolist.language.stageTemplate + ' (' + (stageObjectIndex + 1) + '/' + ezgolist.tmpl.StageTemplates.length + ')'); //TODO Translate Stage Template

        //for firefox remove active class from labels
        $('#stageSignature1Label').removeClass('active');
        $('#stageSignature2Label').removeClass('active');
        $('#stageSignature3Label').removeClass('active');

        $('#do_not_lock_stages_label').removeClass('active');
        $('#lock_next_stages_label').removeClass('active');

        if ($('#enable_shift_notes').is(":checked")) {
            $('#enable_shift_notes').click();
        }

        $('#enable_shift_notes_label').removeClass('active');

        $('#enable_shift_notes').prop('checked', false);

        if (stageTemplate.Id != undefined && stageTemplate.Id != null) {
            $('#currentStageTemplateId').val(stageTemplate.Id);
        }

        if (stageTemplate.Name != '' && stageTemplate.Name != null && stageTemplate.Name != undefined) {
            $('#txtStageName').val(stageTemplate.Name);
        }
        else {
            $('#txtStageName').val('');
        }

        if (stageTemplate.Description != '' && stageTemplate.Description != null && stageTemplate.Description != undefined) {
            $('#txtStageDescription').val(stageTemplate.Description);
        }
        else {
            $('#txtStageDescription').val('');
        }

        if (stageTemplate.NumberOfSignaturesRequired != undefined && stageTemplate.NumberOfSignaturesRequired != null) {
            if (stageTemplate.NumberOfSignaturesRequired == 0) {
                $('#stageSignature1').click();
            }
            else if (stageTemplate.NumberOfSignaturesRequired == 1) {
                $('#stageSignature2').click();
            }
            else if (stageTemplate.NumberOfSignaturesRequired == 2) {
                $('#stageSignature3').click();
            }
        }
        else {
            $('#stageSignature1').click();
        }

        if (stageTemplate.BlockNextStagesUntilCompletion != undefined && stageTemplate.BlockNextStagesUntilCompletion != null &&
            stageTemplate.LockStageAfterCompletion != undefined && stageTemplate.LockStageAfterCompletion != null) {
            if (stageTemplate.BlockNextStagesUntilCompletion && stageTemplate.LockStageAfterCompletion) {
                $('#lock_next_stages').click();
            }
            else {
                $('#do_not_lock_stages').click();
            }
        }
        else {
            $('#do_not_lock_stages').click();
        }

        if (stageTemplate.UseShiftNotes != undefined && stageTemplate.UseShiftNotes != null) {
            if (stageTemplate.UseShiftNotes != $('#enable_shift_notes_label').hasClass("active")) {
                $('#enable_shift_notes').click();
            }
        }
        else {
            if ($('#enable_shift_notes_label').hasClass("active") == true) {
                $('#enable_shift_notes').click();
            }
        }

        if (stageTemplate.Tags?.length > 0) {
            stageTemplate.Tags.forEach(function (tag) {
                var element = document.getElementById('s' + tag.Id);
                if (element?.classList != null) {
                    element.classList.add("btn-ezgo");
                    element.classList.remove("btn-outline-ezgo");
                    element.classList.add("selected-tag");
                }
            })
        }
    }
    else {
        $('#txtStageName').val('');

        $('#txtStageDescription').val('');

        $('#stageSignature1').click();

        $('#do_not_lock_stages').click();

        if ($('#enable_shift_notes').is(":checked")) {
            $('#enable_shift_notes').click();
        }

        $('#enable_shift_notes').prop('checked', false);
    }

    if (ezgolist.listType == 'checklist') {
        $('#editStageTemplateStepModal').modal('show');
    }
}

function showTemplateItemStepDialog(templateStepId) {
    var template = ezgolist.tmpl.TaskTemplates.filter(obj => {
        return obj.Id === ezgolist._currentTemplateId;
    })[0];

    if (template != undefined && template) {
        template.Name = $('#txtItemName').val();
        template.Description = $('#txtItemDesc').val();
    }

    ezgolist.renderStepDialogContent(templateStepId);
    $('#txtStepDescription').focus();
    $('#editTemplateStepDetailModal').modal('show');
    setTimeout(function () { $('#txtStepDescription').focus(); }, 500);
}

//NOTE: PLEASE KEEP TEMPLATE INDENTATION IN TACT (makes it so much more readable)

var cardTemplateImage =
    '<li class="{{liClass}}" data-cid="{{cid}}" data-type="item" data-name="{{name}}" data-desc="{{desc}}">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="card-body p-0">' +
    '<div class="justify-content-lg-center" style="display: {{pictureproof}}; position: absolute; color: white; background-color: #93C54B; height: 1.5rem; width: 1.5rem; border-radius: .75rem; z-index: 10; right: .2rem; top: .2rem; box-shadow: 0px 0px .2rem darkslategray;">' +
    '<i class="fa-solid fa-camera d-lg-flex justify-content-center align-items-center align-content-center p-auto m-auto" style="font-size: 15px;">' +
    '</i>' +
    '</div>' +
    '<div>' +
    '<div data-item="edit-action" data-counter="0" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil d-print-none" data-template-id="" data-step-id="0" data-type="pencil" onclick="showEditTemplateItemDialog({{cid}})">' +
    '<i class="fa fa-pencil-alt d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</div>' +
    '</div>' +
    '<a class="card-link" href="/images/media-loading.gif" data-href="{{picture}}" data-fancybox="{{fancy}}" data-caption="{{name}}">' +
    '<img loading="lazy" class="img-fluid" src="/images/media-loading.gif" data-src="{{picture}}" />' +
    '</a>' +
    '<div>' +
    '<p id="view" class="view p-2" >{{name}}</p>' +
    '</div >' +
    '</div>' +
    '</div>' +
    '</li>';

var cardTemplateStage =
    '<li class="template-stage" style="cursor: pointer" data-cid="{{cid}}" data-type="stage" data-name="{{name}}" data-desc="{{desc}}" onclick="showStageTemplateDialog(event, {{cid}})" >' +
    '<div class="card h-100 shadow-sm" style="min-height:209px; background-color: #93C54B;">' +
    '<div class="card-body p-0 stage-body">' +
    '<h3>{{name}}</h3>' +
    '</div>' +
    '</div>' +
    '</li>';

var cardTemplateVideo =
    '<li class="{{liClass}}" data-cid="{{cid}}" data-type="item" data-name="{{name}}" data-desc="{{desc}}">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="card-body p-0">' +
    '<div class="justify-content-lg-center" style="display: {{pictureproof}}; position: absolute; color: white; background-color: #93C54B; height: 1.5rem; width: 1.5rem; border-radius: .75rem; z-index: 10; right: .2rem; top: .2rem; box-shadow: 0px 0px .2rem darkslategray;">' +
    '<i class="fa-solid fa-camera d-lg-flex justify-content-center align-items-center align-content-center p-auto m-auto" style="font-size: 15px;"></i>' +
    '</div>' +
    '<div>' +
    '<div data-item="edit-action" data-counter="0" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-template-id="" data-step-id="0" data-type="pencil" onclick="showEditTemplateItemDialog({{cid}})">' +
    '<i class="fa fa-pencil-alt d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</div>' +
    '</div>' +
    '<a class="card-link" href="#{{videoid}}" data-fancybox="{{fancy}}" data-caption="{{name}}" data-options=\'{ "video": {"autoStart": false} }\'>' +
    '<img loading="lazy" class="img-fluid" src="/images/media-loading.gif" data-src="{{picture}}" />' +
    '</a>' +
    '<video src="/images/media-loading.mp4" data-src="{{video}}" controls id="{{videoid}}" style="display:none;" >Your browser doesn\'t support HTML5 video tag.</video >' +
    '<div>' +
    '<p id="view" class="view p-2" >{{name}}</p>' +
    '</div >' +
    '</div>' +
    '</div>' +
    '</li>';

var cardTemplateStepImage =
    '<li class="list-inline-item pb-3" data-cid="{{cid}}" data-type="item" data-name="{{name}}" data-desc="{{desc}}">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="card-body p-0">' +
    '<div>' +
    '<div data-item="edit-action" data-counter="0" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-template-id="" data-step-id="0" data-type="pencil" onclick="showTemplateItemStepDialog({{cid}})">' +
    '<i class="fa fa-pencil-alt d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</div>' +
    '</div>' +
    '<a class="card-link" href="/images/media-loading.gif" data-href="{{picture}}" data-fancybox="{{fancy}}" data-caption="{{name}}">' +
    '<img loading="lazy" class="img-fluid" src="/images/media-loading.gif" data-src="{{picture}}" />' +
    '</a>' +
    '<div>' +
    '<p id="view" class="view p-2" >{{name}}</p>' +
    '</div >' +
    '</div>' +
    '</div>' +
    '</li>';

var cardTemplateStepVideo =
    '<li class="list-inline-item pb-3" data-cid="{{cid}}" data-type="item" data-name="{{name}}" data-desc="{{desc}}">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="card-body p-0">' +
    '<div>' +
    '<div data-item="edit-action" data-counter="0" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-template-id="" data-step-id="0" data-type="pencil" onclick="showTemplateItemStepDialog({{cid}})">' +
    '<i class="fa fa-pencil-alt d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</div>' +
    '</div>' +
    '<a class="card-link" href="#{{videoid}}" data-fancybox="{{fancy}}" data-caption="{{desc}}" data-options=\'{ "video": {"autoStart": false} }\'>' +
    '<img loading="lazy" class="img-fluid" src="/images/media-loading.gif" data-src="{{picture}}" />' +
    '</a>' +
    '<video src="/images/media-loading.mp4" data-src="{{video}}" controls id="{{videoid}}" style="display:none;" >Your browser doesn\'t support HTML5 video tag.</video >' +
    '<div>' +
    '<p id="view" class="view p-2" >{{name}}</p>' +
    '</div >' +
    '</div>' +
    '</div>' +
    '</li>';

// add buttons templates
var newTemplateItem =
    '<li class="list-inline-item pb-3 d-print-none add-button" onclick="showTemplateItemDialog()">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="h-100">' +
    '<div class="addclip h-100">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{listtype}}</div>' +
    '</div>' +
    '</div>' +
    '</div>' +
    '</li>';

var newTemplateItemOrStage =
    '<li class="list-inline-item pb-3 pl-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="addclip-top h-100" onclick="showTemplateItemDialog()">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{listtype}}</div>' +
    '</div>' +
    '<hr style="margin: 0; border-top: 1px solid #93C54B;">' +
    '<div class="addclip-bottom h-100" onclick="ezgolist.addStage()">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addstage}}</div>' +
    '</div>' +
    '</div>' +
    '</li>';

var newTemplateItemStep =
    '<li class="list-inline-item pb-3 d-print-none add-button" onclick="ezgolist.addItemStep({{cid}})">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="h-100">' +
    '<div class="addclip h-100">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addinstruction}}</div>' +
    '</div>' +
    '</div>' +
    '</div>' +
    '</li>';

var workInstructionTemplateChoiceButton =
    '<li class="list-inline-item pb-3 d-print-none add-button" onclick="alert(\'functionality is not yet enabled.\');//ezgolist.addWorkInstruction({{cid}})">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="h-100">' +
    '<div class="addclip h-100">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>Add Work Instruction</div>' +
    '</div>' +
    '</div>' +
    '</div>' +
    '</li>';

var newTemplateItemChooser =
    '<li class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" style = "min-height:209px;">' +
    '<div>' +
    '<div class="addclip" onclick="ezgolist.addItemStep({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addinstruction}}</div>' +
    '</div>' +
    '</div>' +
    '<div class="addclip" onclick="ezgolist.addItemLink({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addlink}}</div>' +
    '</div>' +
    '<div>' +
    '<input class="d-none" type="file" id="uploaderItemAttachment" accept="application/pdf" name="attachment" onchange="ezgolist.previewFile(this)" data-templateid="{{cid}}" data-templatestepid="0" data-type="attachment" required />' +
    '<a href="#" onclick="$(\'#uploaderItemAttachment\').trigger(\'click\'); return false;">' +
    '<div class="addclip">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addpdf}}</div>' +
    '</div>' +
    '</a>' +
    '</div>'
'</div>' +
    '</li >';

var newTemplateItemChooserWI =
    '<li class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" style = "min-height:209px;">' +
    '<div>' +
    '<div class="addclip" onclick="ezgolist.addItemStep({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addinstruction}}</div>' +
    '</div>' +
    '<div class="addclip" onclick="ezgolist.addItemLink({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addlink}}</div>' +
    '</div>' +
    '<div>' +
    '<div class="addclip" onclick="ezgolist.addItemWorkInstruction({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addworkinstruction}}</div>' +
    '</div>' +
    '<div>' +
    '<input class="d-none" type="file" id="uploaderItemAttachment" accept="application/pdf" name="attachment" onchange="ezgolist.previewFile(this)" data-templateid="{{cid}}" data-templatestepid="0" data-type="attachment" required />' +
    '<a href="#" onclick="$(\'#uploaderItemAttachment\').trigger(\'click\'); return false;">' +
    '<div class="addclip">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addpdf}}</div>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div >' +
    '</div >' +
    '</div >' +
    '</li >';

var newTemplateItemChooserWithoutLink =
    '<li class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" style = "min-height:209px;">' +
    '<div>' +
    '<div class="addclip" onclick="ezgolist.addItemStep({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addinstruction}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<input class="d-none" type="file" id="uploaderItemAttachment" accept="application/pdf" name="attachment" onchange="ezgolist.previewFile(this)" data-templateid="{{cid}}" data-templatestepid="0" data-type="attachment" required />' +
    '<a href="#" onclick="$(\'#uploaderItemAttachment\').trigger(\'click\'); return false;">' +
    '<div class="addclip">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addpdf}}</div>' +
    '</div>' +
    '</a>' +
    '</div>'
'</div>' +
    '</li >';

var newTemplateItemChooserWIWithoutLink =
    '<li class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" style = "min-height:209px;">' +
    '<div>' +
    '<div class="addclip" onclick="ezgolist.addItemStep({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addinstruction}}</div>' +
    '</div>' +
    '<div>' +
    '<div class="addclip" onclick="ezgolist.addItemWorkInstruction({{cid}})">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addworkinstruction}}</div>' +
    '</div>' +
    '<div>' +
    '<input class="d-none" type="file" id="uploaderItemAttachment" accept="application/pdf" name="attachment" onchange="ezgolist.previewFile(this)" data-templateid="{{cid}}" data-templatestepid="0" data-type="attachment" required />' +
    '<a href="#" onclick="$(\'#uploaderItemAttachment\').trigger(\'click\'); return false;">' +
    '<div class="addclip">' +
    '<i class="fa fa-plus-circle fa-2x"></i>' +
    '<div>{{addpdf}}</div>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div >' +
    '</div >' +
    '</div >' +
    '</li >';

var newTemplateItemAttachment =
    '<li class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div>' +
    '<a data-selector="deletemedia" data-type="pdf" data-templateid="{{templateid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a data-href="{{url}}" data-fancybox="{{fancy}}" data-type="iframe" data-options=\'{ "type" : "iframe", "iframe" : { "preload" : false, "css" : { "width" : "75%" } } } \'>' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-paperclip fa-3x"></i>' +
    '<div>{{filename}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction PDF</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';

var newTemplateItemAttachmentWorkInstruction =
    '<li class="list-inline-item pb-3" data-type="item" data-name="{{name}}">' +
    '<div class="card h-100 shadow-sm" style="min-height:209px;">' +
    '<div class="card-body p-0">' +
    '<div>' +
    '<div data-item="edit-action" data-counter="0" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil d-print-none" data-template-id="" data-step-id="0" onclick="ezgolist.removeWorkInstructionRelations({{cid}})">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</div>' +
    '</div>' +
    '<a class="card-link" data-href="{{picture}}" data-fancybox="workinstructions" data-caption="{{name}}">' +
    '<img loading="lazy" class="img-fluid" data-src="{{picture}}"  />' +
    '</a>' +
    '<div>' +
    '<p id="view" class="view p-2" >{{name}}</p>' +
    '</div >' +
    '</div>' +
    '</div>' +
    '</li>';

var workInstructionItemAttachmentTemplatePdf =
    '<li id="workInstructionItemAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteWIAttachmentPdf-{{itemid}}" data-type="pdf" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/images/media-loading.gif" data-href="{{url}}" data-fancybox="{{fancy}}" data-type="iframe" data-options=\'{ "type" : "iframe", "iframe" : { "preload" : false, "css" : { "width" : "75%" } } } \'>' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-paperclip fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{filename}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction PDF</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';


var workInstructionItemAttachmentTemplateLink =
    '<li id="workInstructionItemAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteWIAttachmentLink-{{itemid}}" data-type="link" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/externallink/{{encodedUrl}}" target="_blank" >' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-globe fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{url}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction Link</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';



var checklistItemAttachmentTemplatePdf =
    '<li id="checklistItemAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteChecklistItemAttachmentPdf-{{itemid}}" data-type="link" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/images/media-loading.gif" data-href="{{url}}" data-fancybox="{{fancy}}" data-type="iframe" data-options=\'{ "type" : "iframe", "iframe" : { "preload" : false, "css" : { "width" : "75%" } } } \'>' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-paperclip fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{filename}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction PDF</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';

var checklistItemAttachmentTemplateLink =
    '<li id="checklistItemAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteChecklistItemAttachmentLink-{{itemid}}" data-type="link" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/externallink/{{encodedUrl}}" target="_blank" >' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-globe fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{url}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction Link</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';


var auditItemAttachmentTemplateLink =
    '<li id="auditItemAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteAuditItemAttachmentLink-{{itemid}}" data-type="link" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/externallink/{{encodedUrl}}" target="_blank" >' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-globe fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{url}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction Link</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';

var auditItemAttachmentTemplatePdf =
    '<li id="auditItemAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteAuditItemAttachmentPdf-{{itemid}}" data-type="link" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/images/media-loading.gif" data-href="{{url}}" data-fancybox="{{fancy}}" data-type="iframe" data-options=\'{ "type" : "iframe", "iframe" : { "preload" : false, "css" : { "width" : "75%" } } } \'>' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-paperclip fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{filename}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction PDF</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';


var taskTemplateAttachmentTemplateLink =
    '<li id="taskTemplateAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteTaskTemplateAttachmentLink-{{itemid}}" data-type="link" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/externallink/{{encodedUrl}}" target="_blank" >' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-globe fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{url}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction Link</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';

var taskTemplateAttachmentTemplatePdf =
    '<li id="taskTemplateAttachment-{{itemid}}" class="list-inline-item pb-3 d-print-none add-button">' +
    '<div class="card h-100 shadow-sm" >' +
    '<div class="card-body p-0">' +
    '<div style="width: 192px; max-width: 192px">' +
    '<a id="deleteTaskTemplateAttachmentPdf-{{itemid}}" data-type="link" data-itemid="{{itemid}}" class="border rounded-circle float-right d-lg-flex justify-content-lg-center shadow pencil" data-type="pencil">' +
    '<i class="fa fa-trash d-lg-flex justify-content-center align-items-center align-content-center" style="font-size: 20px;"></i>' +
    '</a>' +
    '</div>' +
    '<a href="/images/media-loading.gif" data-href="{{url}}" data-fancybox="{{fancy}}" data-type="iframe" data-options=\'{ "type" : "iframe", "iframe" : { "preload" : false, "css" : { "width" : "75%" } } } \'>' +
    '<div>' +
    '<div class="paperclip">' +
    '<i class="fa fa-paperclip fa-3x"></i>' +
    '<div style="max-width: 192px; text-overflow: ellipsis">{{filename}}</div>' +
    '</div>' +
    '</div>' +
    '<div>' +
    '<p id="view" class="view p-2 text-center text-muted">Instruction PDF</p>' +
    '</div>' +
    '</a>' +
    '</div>' +
    '</div>' +
    '</li >';

var templateObj = {
    Id: 0,
    Name: "",
    Picture: undefined,
    AreaId: undefined,
    PictureType: undefined,
    DescriptionFile: undefined,
    IsDoubleSignatureRequired: false,
    IsSignatureRequired: false,
    ScoreType: undefined,
    MaxScore: 10,
    MinScore: 0,
    Role: 'basic',
    WorkInstructionType: 0,
    CompanyId: 136,
    TaskTemplates: [],
    AreaPath: '',
    RecurrencyType: undefined,
    PlannedTime: undefined,
    StepsCount: 0,
    Tags: [],
    SharedTemplateId: 0
}

var taskTemplateObj = {
    Id: null,
    ChecklistTemplateId: 0,
    Name: '',
    Description: '',
    DescriptionFile: undefined,
    CompanyId: 0,
    Picture: undefined,
    Type: undefined,
    PictureType: undefined,
    VideoType: undefined,
    Recurrency: undefined,
    Weight: 1,
    Video: undefined,
    VideoThumbnail: undefined,
    ActionsCount: 0,
    StepsCount: 0,
    Steps: null,
    isNew: false,
    Tags: [],
    Attachments: []
}

var taskTemplateStepObj = {
    Id: 0,
    Description: '',
    Picture: undefined,
    Video: undefined,
    VideoThumbnail: undefined,
    PictureType: undefined,
    VideoType: undefined,
    TaskTemplateId: 0,
    Index: 0,
    isNew: false,
    Tags: []
}

var stageTemplateObj = {
    id: 0,
    Name: '',
    Description: '',
    BlockNextStagesUntilCompletion: undefined,
    LockStageAfterCompletion: undefined,
    UseShiftNotes: undefined,
    NumberOfSignaturesRequired: 0
}

//Format method (can also be implemented in other JS, move to general structure and include everywhere)
