const btns = document.querySelectorAll('.circlebtn'); //todo make selecters less generic
const popUpScore = document.querySelector('.popUpScore');
const popUpThumbs = document.querySelector('.popUpThumbs');
const items = document.querySelectorAll('.item');

var activeBtn = null;

const setValue = (e) => {
    activeBtn.innerHTML = e.target.innerHTML;
    activeBtn.className = 'btn circlebtn';
    //activeBtn.classList.add('itemSelected');
    activeBtn.classList.add(e.target.classList[1]);
    activeBtn.classList.add(e.target.classList[2]);
    activeBtn.style.backgoundColor = e.target.style.backgroundColor;
    activeBtn.setAttribute('data-value', e.target.getAttribute('data-actionvalue'));
    activeBtn = null;
}

const togglePopUp = (e) =>{
    var popup;
    var x;
    var y;
    
    e.stopPropagation();    
    
    if (e.pageX || e.pageY) { 
      x = e.pageX;
      y = e.pageY;
    }
    else { 
      x = e.clientX + document.body.scrollLeft + document.documentElement.scrollLeft; 
      y = e.clientY + document.body.scrollTop + document.documentElement.scrollTop; 
    } 
    activeBtn = e.target;

    switch(activeBtn.getAttribute('data-popup')){
        case 'score': 
         popup = popUpScore;
        break;
    }

    //if popup will be displayed too close to the edge of the viewport,
    //make the x location of popup respect the popup width (265px) and how far it is with respect to the right side of the viewport
    if (window.innerWidth - 265 < x) {
        x = x - 265 + (window.innerWidth - x);
    }
    if (popup == undefined)
        return;

    popup.classList.toggle('hidePopUp')
    popup.style.left = x-40+'px';
    popup.style.top = (y+40)+'px';
}

items.forEach((item)=>{
    item.addEventListener('click',setValue);
})

btns.forEach((btn) => {
    if (btn.getAttribute('data-popup') != 'grade') {
        btn.addEventListener('click', togglePopUp)
    }
});


document.addEventListener('click',(e)=>{
    e.stopPropagation();
    if (popUpScore != undefined && popUpScore != null) {
        popUpScore.classList.add('hidePopUp');
    }
    if (popUpThumbs != undefined && popUpThumbs != null) {
        popUpThumbs.classList.add('hidePopUp')
    }
})

//TODO optimize code;
var matrix = {
    currentMatrixId: 0,
    currentUserId: 0,
    currentGroupId: 0,
    currentSkillId: 0,
    currentUserSkills: {},
    currentUserSkillValues: {},
    currentCertifiedAt: '',
    currentUserSkillName: '',
    currentExpiryInDays: 0,
    currentNotificationWindowInDays: 0,
    skillEdited: false,
    groupEdited: false,
    relationChanged: false,
    legendConfiguration: null,
    language: {
        userSkills: "User skills - ",
        expiryInDays: "Expiry in days: ",
        expiryInDaysNotSet: "Expiry in days: not set",
        expiryWarningInDays: "expiry warning in days: ",
        expiryWarningInDaysNotSet: "expiry warning in days: not set",
        certifiedAt: "Certified at",
        confirm: "Confirm",
        keepOldScoresDialogueBody: 'The assessment template that is connected to this skill has been changed.<br />The scores from the new assessment will be used for everybody who has completed it already.<br /> Do you want to keep the old scores for users who have not completed the new assessment yet?',
        cancel: "Cancel",
        removeOldScores: "Remove old scores",
        keepOldScores: "Keep old scores"
    },
    init: function (matrixId) {
        matrix.initDisplay();
        matrix.initHandlers();
        if (matrixId != null) {
            matrix.currentMatrixId = parseInt(matrixId);
        }
        matrix.calculateMatrix();
        matrix.loadAndApplyLegendConfiguration();
    },
    initDisplay: function () {
        $('[data-containertype="dialog_skilldetails"]').hide();
    },
    initHandlers: function () {
        matrix.initSkillHandlers();
        matrix.initGroupHandlers();

        $('#btnViewModalLegend').on('click', function () {
            matrix.loadAndApplyLegendConfiguration();
            $('#MatrixLegendModal').modal('show');
        });

        $('#UserSkillValuesModalContent').on('click', '.editSkillValue', function (event) {
            let matrixid = +$(event.currentTarget).data('matrixid');
            let userskillid = +$(event.currentTarget).data('userskillid');
            let userid = +$(event.currentTarget).data('userid');
            let expiryindays = +$(event.currentTarget).data('expiryindays');
            let notificationwindowindays = +$(event.currentTarget).data('notificationwindowindays');

            matrix.currentMatrixId = matrixid;
            matrix.currentSkillId = userskillid;
            matrix.currentUserId = userid;
            matrix.currentExpiryInDays = expiryindays;
            matrix.currentNotificationWindowInDays = notificationwindowindays;

            matrix.showUserSkillPickerAndFixPosition(true, $(`#valuedate-${matrix.currentMatrixId}-${matrix.currentSkillId}`)[0], event.currentTarget, $('#UserSkillValuesModal'));

            event.preventDefault();
            event.stopImmediatePropagation();
        });

        $('#UserSkillValuesModalContent').on('click', '.editCustomTarget', function (event) {
            let matrixid = +$(event.currentTarget).data('matrixid');
            let userskillid = +$(event.currentTarget).data('userskillid');
            let userid = +$(event.currentTarget).data('userid');

            matrix.currentMatrixId = matrixid;
            matrix.currentSkillId = userskillid;
            matrix.currentUserId = userid;

            if ($(event.currentTarget).data('userskillname')) {
                $('#customTargetOperationalSkillName').html($(event.currentTarget).data('userskillname'));
            }

            matrix.resetCustomDefaultTargetValue();

            if ($(event.currentTarget).data('userskilltarget')) {

                let userSkillTarget = +$(event.currentTarget).data('userskilltarget');
                if (userSkillTarget == 1 && !$(event.currentTarget).data('isapplicable')) {
                    userSkillTarget = -1;
                }
                matrix.setCustomDefaultTargetValue(userSkillTarget, $(`[id^="customDefaultTarget-${userSkillTarget}"]`));
            }

            $('#CustomTargetModal').modal('show');
        });

        $('#UserSkillValuesModalContent').on('click', '.daterangepicker .applyBtn', function () {
            const picker = $(`#valuedate-${matrix.currentMatrixId}-${matrix.currentSkillId}`).data('daterangepicker');
            if (!picker) return;

            matrix.saveUserSkillValueNew(matrix.currentMatrixId, matrix.currentSkillId, matrix.currentUserId);
        });

        $('#UserSkillValuesModalContent').on('click', '.daterangepicker .deleteUserSkillValueButton', function () {
            let userId = matrix.currentUserId;
            let userSkillId = matrix.currentSkillId;

            let userSkillAndUserMetadata = {
                UserId: userId,
                UserSkillId: userSkillId,
            }

            $.ajax({
                type: "POST",
                url: '/skillsmatrix/skillvalue/remove/' + matrix.currentMatrixId,
                data: JSON.stringify(userSkillAndUserMetadata),
                success: function (data) {
                    toastr.success('Value removed.');

                    let removePicker = $(`#valuedate-${matrix.currentMatrixId}-${userSkillId}`).data('daterangepicker')
                    if (removePicker) {
                        removePicker.hide();
                    }

                    $(`#mandatorySkillDetails-${userSkillId}-${userId}`).html(data);
                    matrix.initDateRangePicker($(`#valuedate-${matrix.currentMatrixId}-${userSkillId}`));

                    let buttons = $('[data-userid="' + userId + '"][data-skillid="' + userSkillId + '"]');
                    //update matrix UI;
                    buttons.each(function () {
                        buttons.attr('class', 'btn circlebtn');
                        buttons.attr('data-value', 0);
                    });

                    matrix.calculateMatrix();

                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });
        });

        $('#UserSkillValuesModalContent').on('click', '.daterangepicker .notRequiredForThisPersonButton', function () {
            console.log("not required for this person clicked");

            let userId = matrix.currentUserId;
            let userSkillId = matrix.currentSkillId;

            let customTargetInfo = {
                CustomTarget: null,
                UserId: userId,
                UserSkillId: userSkillId,
                IsApplicable: false
            }

            $.ajax({
                type: "POST",
                url: '/skillsmatrix/applicability/set/mandatory/' + matrix.currentMatrixId,
                data: JSON.stringify(customTargetInfo),
                success: function (data) {
                    toastr.success('User skill marked as not required for this person!');

                    let removePicker = $(`#valuedate-${matrix.currentMatrixId}-${userSkillId}`).data('daterangepicker')
                    if (removePicker) {
                        removePicker.hide();
                    }

                    $(`#mandatorySkillDetails-${userSkillId}-${userId}`).html(data);
                    matrix.initDateRangePicker($(`#valuedate-${matrix.currentMatrixId}-${userSkillId}`));

                    matrix.calculateMatrix();

                    //todo update matrix
                    //let buttons = $('[data-userid="' + userId + '"][data-skillid="' + userSkillId + '"]');
                    ////update matrix UI;
                    //buttons.each(function () {
                    //    buttons.attr('class', 'btn circlebtn');
                    //    buttons.attr('data-value', 0);
                    //});

                    //ezgomediafetcher.preloadImagesAndVideos();
                    //update/load operational skill row?
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });
        });

        $('#UserSkillValuesModalContent').on('click', '.daterangepicker .requiredForThisPersonButton', function () {
            console.log("not required for this person clicked");

            let userId = matrix.currentUserId;
            let userSkillId = matrix.currentSkillId;

            let userSkillAndUserMetadata = {
                UserId: userId,
                UserSkillId: userSkillId,
            }
            $.ajax({
                type: "POST",
                url: '/skillsmatrix/applicability/remove/mandatory/' + matrix.currentMatrixId,
                data: JSON.stringify(userSkillAndUserMetadata),
                success: function (data) {
                    toastr.success('User skill marked as required for this person!');

                    let removePicker = $(`#valuedate-${matrix.currentMatrixId}-${userSkillId}`).data('daterangepicker')
                    if (removePicker) {
                        removePicker.hide();
                    }

                    $(`#mandatorySkillDetails-${userSkillId}-${userId}`).html(data);
                    matrix.initDateRangePicker($(`#valuedate-${matrix.currentMatrixId}-${userSkillId}`));

                    matrix.calculateMatrix();

                    //todo update matrix
                    //let buttons = $('[data-userid="' + userId + '"][data-skillid="' + userSkillId + '"]');
                    ////update matrix UI;
                    //buttons.each(function () {
                    //    buttons.attr('class', 'btn circlebtn');
                    //    buttons.attr('data-value', 0);
                    //});

                    //matrix.calculateMatrix();
                    //ezgomediafetcher.preloadImagesAndVideos();
                    //update/load operational skill row?
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });
        });

        $('.resetToDefaultTargetButton').on('click', function () {
            console.log("reset to default target button clicked");

            let userId = matrix.currentUserId;
            let userSkillId = matrix.currentSkillId;

            let userSkillAndUserMetadata = {
                UserId: userId,
                UserSkillId: userSkillId,
            }

            $.ajax({
                type: "POST",
                url: '/skillsmatrix/applicability/remove/operational/' + matrix.currentMatrixId,
                data: JSON.stringify(userSkillAndUserMetadata),
                success: function (data) {
                    toastr.success('Custom target removed!');

                    $('#CustomTargetModal').modal('hide');

                    $(`#operationalSkillDetails-${userSkillId}-${userId}`).html(data);

                    matrix.calculateMatrix();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });
        });

        $('.configureCustomTargetButton').on('click', function () {
            console.log("configure custom target button clicked");
            let customTarget = matrix.getCustomDefaultTargetValue();
            let userId = matrix.currentUserId;
            let userSkillId = matrix.currentSkillId;
            let applicable = customTarget != -1;

            let customTargetInfo = {
                CustomTarget: customTarget,
                UserId: userId,
                UserSkillId: userSkillId,
                IsApplicable: applicable
            }

            $.ajax({
                type: "POST",
                url: '/skillsmatrix/applicability/set/operational/' + matrix.currentMatrixId,
                data: JSON.stringify(customTargetInfo),
                success: function (data) {
                    toastr.success('Custom target set ' + (applicable ? 'applicable' : 'not applicable') + '!');
                    $('#CustomTargetModal').modal('hide');

                    $(`#operationalSkillDetails-${userSkillId}-${userId}`).html(data);

                    ezgomediafetcher.preloadImagesAndVideos();

                    matrix.calculateMatrix();

                    //TODO: update matrix score display with new custom target (goal might not be met anymore)
                    //also calculateMatrix (still has to be edited)
                    /*
                    
                    let buttons = $('[data-userid="' + userId + '"][data-skillid="' + userSkillId + '"]');
                    //update matrix UI;
                    buttons.each(function () {
                        buttons.attr('class', 'btn circlebtn');
                        buttons.attr('data-value', 0);
                    });

                    matrix.calculateMatrix();

                    */
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });
        });

        $('#CustomTargetModal').on('hidden.bs.modal', function () {
            if ($('.modal.show').length) {
                $('body').addClass('modal-open');
            }
        });

        $("#matrixBody").scroll(function () {
            if ($('.user-title').first().offset().top <= $('#matrixBody').first().offset().top) {
                //user groups overlay
                $('.user-title-content').each(function (index, elem) {
                    if ($(elem).width() != $($(".user-title-overlay-content")[index]).width()) {
                        $($(".user-title-overlay-content")[index]).width($(elem).width());
                    }
                });

                $('#matrixUsersOverlay').show();
                $('#matrixUsersOverlay').css('left', $('[class^="buttoncell"]').first().position().left + $('#matrixBody').scrollLeft());
                $('#matrixUsersOverlay').css('top', $('#matrixBody').scrollTop());
            }
            else {
                $('#matrixUsersOverlay').hide();
            }

            if ($('.skillcell').first().offset().left <= $('#matrixBody').first().offset().left) {
                //user skills overlay
                $('.matrixmandatoryskill').each(function (index, elem) {
                    if ($(elem).height() != $($('.matrixmandatoryskilloverlay')[index]).height()) {
                        $($('.matrixmandatoryskilloverlay')[index]).height($(elem).height());
                    }
                });
                $('.matrixoperationalskill').each(function (index, elem) {
                    if ($(elem).height() != $($('.matrixoperationalskilloverlay')[index]).height()) {
                        $($('.matrixoperationalskilloverlay')[index]).height($(elem).height());
                    }
                });
                $('.matrixoperationalbehaviour').each(function (index, elem) {
                    if ($(elem).height() != $($('.matrixoperationalbehaviouroverlay')[index]).height()) {
                        $($('.matrixoperationalbehaviouroverlay')[index]).height($(elem).height());
                    }
                });

                $('#greySpacer').height(500);
                $('#matrixSkillsOverlay').show();
                $('#matrixSkillsOverlay').css('top', $('.buttoncell').first().position().top + $('#matrixBody').scrollTop() - 10 - $('#greySpacer').height());
                $('#matrixSkillsOverlay').css('left', $('#matrixBody').scrollLeft());
            }
            else {
                $('#matrixSkillsOverlay').hide();
            }
        });

        $('#btnChangeSkillOrder').on('click', function () {
            $('#ChangeSkillOrderModal').modal('show');
        });

        $('#save_indices').on('click', function () {
            matrix.saveMatrixUserSkillRelations();
        });

        $('#ChangeSkillOrderMandatorySkills').sortable();
        $('#ChangeSkillOrderOperationalSkills').sortable();

        $('#ChangeSkillOrderMandatorySkills').on('sortupdate', function (event, ui) { matrix.orderVisibleMandatorySkills(); });
        $('#ChangeSkillOrderOperationalSkills').on('sortupdate', function (event, ui) { matrix.orderVisibleOperationalSkills(); });

        $('[data-action="update"]').on('click', function () {
            var userGroupModalIsShown = $('#AddChangeUserGroupModal').hasClass('in') || $('#AddChangeUserGroupModal').hasClass('show');
            if (userGroupModalIsShown) {
                matrix.saveGroup();
            }

            var userSkillModalIsShown = $('#AddChangeSkillBehaviourModal').hasClass('in') || $('#AddChangeSkillBehaviourModal').hasClass('show');
            if (userSkillModalIsShown) {
                matrix.saveSkill();
            }

            //matrix.updateMatrix();
        });

        $('[data-actionvalue]').on('click', function () {
            matrix.calculateMatrix();
            matrix.saveSkillValue();
        });

        $('[data-actiontype="user_value_choice"]').on('click', function () {
            matrix.currentUserId = $(this).attr('data-userid');
            matrix.currentGroupId = $(this).attr('data-usergroupid');
            matrix.currentSkillId = $(this).attr('data-skillid');
        });

        $('#UserSkillValuesModal').on('change', '[data-valuetype="userDateTimeChoice"]', function () {
            //use data attr values to construct id of button
            if ($(this).val() != '' && this.checkValidity()) {
                var matrixid = $(this).data('matrixid');
                var userskillid = $(this).data('userskillid');
                //$('#confirmUserSkillValue-' + matrixid + '-' + userskillid).prop('disabled', false);
                $('#confirmUserSkillValue-' + matrixid + '-' + userskillid).removeAttr("disabled");
            }
            else {
                var matrixid = $(this).data('matrixid');
                var userskillid = $(this).data('userskillid');
                $('#confirmUserSkillValue-' + matrixid + '-' + userskillid).prop('disabled', true);
            }
        });

        $('#btnDeleteUserGroup').on('click', function (e) {
            var id = $(this).data('groupid');
            $.fn.dialogue({
                content: $("<p />").text('Are you sure you want to delete this User Group?'),
                closeIcon: true,
                buttons: [
                    {
                        text: "Yes", id: $.utils.createUUID(), click: function ($modal) {
                            $('body').toggleClass('loaded');
                            //reload page to prevent strange interactions like changing plan A, going back, editing plan B or reating a new plan, saving that one, plan A changes are now also saved.
                            $.ajax({
                                type: "POST",
                                url: '/usergroup/setactive/' + id,
                                data: false,
                                success: function (data) {
                                    $('body').toggleClass('loaded');
                                    toastr.success('User Group deleted.');
                                    $modal.dismiss();
                                    window.location.reload();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    $('body').toggleClass('loaded');
                                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                                    $modal.dismiss();
                                },
                                contentType: "application/json; charset=utf-8"
                            });

                        }
                    },
                    { text: "No", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
                ]
            });
        });

        $('#btnDeleteUserSkill').on('click', function (e) {
            var id = $(this).data('skillid');
            $.fn.dialogue({
                content: $("<p />").text('Are you sure you want to delete this User Skill?'),
                closeIcon: true,
                buttons: [
                    {
                        text: "Yes", id: $.utils.createUUID(), click: function ($modal) {
                            $('body').toggleClass('loaded');
                            //reload page to prevent strange interactions like changing plan A, going back, editing plan B or reating a new plan, saving that one, plan A changes are now also saved.
                            $.ajax({
                                type: "POST",
                                url: '/userskill/setactive/' + id,
                                data: false,
                                success: function (data) {
                                    $('body').toggleClass('loaded');
                                    toastr.success('User Skill deleted.');
                                    $modal.dismiss();
                                    window.location.reload();
                                },
                                error: function (jqXHR, textStatus, errorThrown) {
                                    $('body').toggleClass('loaded');
                                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                                    $modal.dismiss();
                                },
                                contentType: "application/json; charset=utf-8"
                            });

                        }
                    },
                    { text: "No", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
                ]
            });
        });
    },
    initSkillHandlers: function () {
        //skill
        $('#btnAddChangeBehaviourSkills').on('click', function () {
            matrix.initSkillModal();
            $('#btn_new_skill').click();
            //$('#dialog_type_2').click();
        });
        $('#btn_new_skill').on('click', function () {
            $('#dialog_skillselection').val('');
            $('#skill_in_matrix_container').hide(); //new one
            matrix.showSkillDetails();
            matrix.showEditSkill();
        });
        $('#btn_edit_skill').on('click', function () {
            matrix.showSkillDetails();
            matrix.loadSkillDetails();
        });
        $('#dialog_skillselection').on('change', function (e) {
            if ($(e.currentTarget).val() == '') {
                $('#skill_in_matrix_container').hide();

                if ($('#dialog_type_1').length) {
                    $('[id^="dialog_type_"]').each(function (index, elem) {
                        $(elem).parent('label').removeClass('active');
                        $(elem).prop('checked', false);
                    });
                    $('#dialog_type_1').parent('label').addClass('active');
                    $('#dialog_type_1').prop('checked', true);
                }

                $('#btnDeleteUserSkill').attr('disabled', 'disabled');
            }
            matrix.showMandatorySkillFields();
            matrix.showEditSkill();
            $('#btn_edit_skill').click();
            if ($('#dialog_skillname').val() != '') {
                $('#skill_update_matrix').removeAttr('disabled');
            }
            else {
                $('#skill_update_matrix').attr('disabled', 'disabled');
            }
        });
        $('#btn_save_skill').on('click', function () {
            matrix.saveSkill();
        });
        $('#btn_cancel_skill').on('click', function () {
            matrix.skillEdited = false;
            //$('[data-containertype="change_skill_buttons"]').hide();
        });
        //user skill form validation
        $('#dialog_skillname').on('keyup', function () {
            if ($('#dialog_skillname').val() != '') {
                $('#skill_update_matrix').removeAttr('disabled');
            }
            else {
                $('#skill_update_matrix').attr('disabled', 'disabled');
            }
        });

        $('#dialog_skillname, #dialog_skilldescription, #dialog_expiryindays, #modal_assessment_choice, #dialog_goal, #dialog_notification_window_days').on('change', function () {
            matrix.skillEdited = true;
            //$('[data-containertype="change_skill_buttons"]').show();

        });

        $('#UserSkillValuesModalBody').on('click', '.daterangepicker td span', function (e) {
            const td = this.closest('td');
            if (td) {
                // Create a native MouseEvent and dispatch it
                const event = new MouseEvent('mousedown', {
                    bubbles: true,
                    cancelable: true,
                    view: window
                });
                td.dispatchEvent(event);
            }
        });

        $('#dialog_type_1, #dialog_type_2').on('click', function (e) {
            matrix.skillEdited = true;
            //change ui
            if ($(e.currentTarget).attr('id') == 'dialog_type_1') {
                matrix.showMandatorySkillFields();
            }
            else if ($(e.currentTarget).attr('id') == 'dialog_type_2') {
                matrix.showOperationalSkillFields();
            }
        });

        $('#dialog_goal').on('change', function () {
            if ($('#dialog_goal').val() < 0) {
                $('#dialog_goal').val(0);
            }
        });
        $('#dialog_notification_window_days').on('change', function () {
            if ($('#dialog_notification_window_days').val() < 0) {
                $('#dialog_notification_window_days').val(0);
            }
        });
        $('#dialog_expiryindays').on('change', function () {
            if ($('#dialog_expiryindays').val() < 0) {
                $('#dialog_expiryindays').val(0);
            }
        });

        $('.item-target').on('click', function (e) {
            matrix.skillEdited = true;

            let currentButtonSelected =
                $(e.currentTarget).hasClass('btn-red-selected') ||
                $(e.currentTarget).hasClass('btn-orangered-selected') ||
                $(e.currentTarget).hasClass('btn-orange-selected') ||
                $(e.currentTarget).hasClass('btn-orangegreen-selected') ||
                $(e.currentTarget).hasClass('btn-green-selected');

            let currentValue = $(e.currentTarget).data('value');

            matrix.resetDefaultTargetValue();

            //only select current item if it wasn't already selected, in which case it would need to be deselected (toggle)
            if (!currentButtonSelected) {
                matrix.setDefaultTargetValue(currentValue, e.currentTarget);
            }
            else {
                //deselecting done by default
            }
        });

        $('.item-custom-target').on('click', function (e) {
            let currentButtonSelected =
                $(e.currentTarget).hasClass('btn-red-selected') ||
                $(e.currentTarget).hasClass('btn-orangered-selected') ||
                $(e.currentTarget).hasClass('btn-orange-selected') ||
                $(e.currentTarget).hasClass('btn-orangegreen-selected') ||
                $(e.currentTarget).hasClass('btn-green-selected');

            let currentValue = $(e.currentTarget).data('value');

            matrix.resetCustomDefaultTargetValue();

            //only select current item if it wasn't already selected, in which case it would need to be deselected (toggle)
            if (!currentButtonSelected) {
                matrix.setCustomDefaultTargetValue(currentValue, e.currentTarget);
            }
            else {
                //deselecting done by default
            }
        })

    },
    showMandatorySkillFields: function () {

        $('#default_target_container').hide();

        $('#notification_window_container').removeClass('col-6');
        $('#notification_window_container').addClass('col-4');

        $('#expiry_in_days_container').removeClass('col-6');
        $('#expiry_in_days_container').addClass('col-4');

        $('#required_goal_container').removeClass('col-6');
        $('#required_goal_container').addClass('col-4');
        $('#required_goal_container').css({
            'align-items': '',
            'display': 'block',
            'gap': ''
        });

        $('#modal_assessment_container').hide();
    },
    showOperationalSkillFields: function () {

        $('#default_target_container').show();

        $('#notification_window_container').removeClass('col-4');
        $('#notification_window_container').addClass('col-6');

        $('#expiry_in_days_container').removeClass('col-4');
        $('#expiry_in_days_container').addClass('col-6');

        $('#required_goal_container').removeClass('col-4');
        $('#required_goal_container').addClass('col-6');

        $('#required_goal_container').css({
            'align-items': 'center',
            'display': 'flex',
            'gap': '20px'
        });

        $('#modal_assessment_container').show();
    },
    resetDefaultTargetValue: function () {
        $('#defaultTarget-1').addClass('btn-red');
        $('#defaultTarget-2').addClass('btn-orangered');
        $('#defaultTarget-3').addClass('btn-orange');
        $('#defaultTarget-4').addClass('btn-orangegreen');
        $('#defaultTarget-5').addClass('btn-green');

        $('#defaultTarget-1').removeClass('btn-red-selected');
        $('#defaultTarget-2').removeClass('btn-orangered-selected');
        $('#defaultTarget-3').removeClass('btn-orange-selected');
        $('#defaultTarget-4').removeClass('btn-orangegreen-selected');
        $('#defaultTarget-5').removeClass('btn-green-selected');
    },
    setDefaultTargetValue: function (targetValue, elem) {
        if (targetValue == 1) {
            $(elem).addClass('btn-red-selected');
            $(elem).removeClass('btn-red');
        }
        else if (targetValue == 2) {
            $(elem).addClass('btn-orangered-selected');
            $(elem).removeClass('btn-orangered');
        }
        else if (targetValue == 3) {
            $(elem).addClass('btn-orange-selected');
            $(elem).removeClass('btn-orange');
        }
        else if (targetValue == 4) {
            $(elem).addClass('btn-orangegreen-selected');
            $(elem).removeClass('btn-orangegreen');
        }
        else if (targetValue == 5) {
            $(elem).addClass('btn-green-selected');
            $(elem).removeClass('btn-green');
        }
    },
    getDefaultTargetValue: function () {
        let selectedElement = $('div[id^="defaultTarget-"][class$="-selected"]');

        if (selectedElement) {
            let selectedValue = $(selectedElement).data('value');
            return selectedValue;
        }

        return null;
    },
    resetCustomDefaultTargetValue: function () {
        $('#customDefaultTarget-1').addClass('btn-red');
        $('#customDefaultTarget-2').addClass('btn-orangered');
        $('#customDefaultTarget-3').addClass('btn-orange');
        $('#customDefaultTarget-4').addClass('btn-orangegreen');
        $('#customDefaultTarget-5').addClass('btn-green');
        $('#customDefaultTarget--1').addClass('btn-white');

        $('#customDefaultTarget-1').removeClass('btn-red-selected');
        $('#customDefaultTarget-2').removeClass('btn-orangered-selected');
        $('#customDefaultTarget-3').removeClass('btn-orange-selected');
        $('#customDefaultTarget-4').removeClass('btn-orangegreen-selected');
        $('#customDefaultTarget-5').removeClass('btn-green-selected');
        $('#customDefaultTarget--1').removeClass('btn-white-selected');
    },
    setCustomDefaultTargetValue: function (targetValue, elem) {
        if (targetValue == 1) {
            $(elem).addClass('btn-red-selected');
            $(elem).removeClass('btn-red');
        }
        else if (targetValue == 2) {
            $(elem).addClass('btn-orangered-selected');
            $(elem).removeClass('btn-orangered');
        }
        else if (targetValue == 3) {
            $(elem).addClass('btn-orange-selected');
            $(elem).removeClass('btn-orange');
        }
        else if (targetValue == 4) {
            $(elem).addClass('btn-orangegreen-selected');
            $(elem).removeClass('btn-orangegreen');
        }
        else if (targetValue == 5) {
            $(elem).addClass('btn-green-selected');
            $(elem).removeClass('btn-green');
        }
        else if (targetValue == -1) {
            $(elem).addClass('btn-white-selected');
            $(elem).removeClass('btn-white');
        }
    },
    getCustomDefaultTargetValue: function () {
        let selectedElement = $('div[id^="customDefaultTarget-"][class$="-selected"]');

        if (selectedElement) {
            let selectedValue = $(selectedElement).data('value');
            return selectedValue;
        }

        return null;
    },
    validateUserSkill: function () {

        let validationCorrect = true;
        //#dialog_skillname, #dialog_skilldescription, #dialog_expiryindays, #modal_assessment_choice, #dialog_goal, #dialog_notification_window_days
        if ($('#dialog_skillname').val() == '') {
            validationCorrect = false;
            toastr.error('User skill name is required.');
        }
        //else if ($('#dialog_skilldescription').val() == '') {
        //    validationCorrect = false;
        //    toastr.error('User skill description is required.');
        //}
        //else if ($('#dialog_type_2').is(':checked') && ($('#modal_assessment_choice').val() == '' || $('#modal_assessment_choice').val() <= 0)) {
        //    validationCorrect = false;
        //    toastr.error('User skill is type operational but no assessment template selected.');
        //}
        else if ($('#dialog_type_2').is(':checked') && (!parseInt(matrix.getDefaultTargetValue()))) {
            validationCorrect = false;
            toastr.error('User skill is type operational but no default target configured.')
        }
        return validationCorrect;
        //else if ($('#dialog_expiryindays').val() == '' || $('#dialog_expiryindays').val() <= 0) {
        //    toastr.error('User skill expiry in days must be larger than 0.');
        //}
        //else if ($('#dialog_goal').val() == '' || $('#dialog_goal').val() <= 0) {
        //    toastr.error('User skill goal must be larger than 0.');
        //}
        //else if ($('#dialog_notification_window_days').val() == '' || $('#dialog_notification_window_days').val() <= 0) {
        //    toastr.error('Notification window in days msut be larger than 0.');
        //}

    },
    initSkillModal: function () {
        $('#dialog_skillselection').val('');
        $('#AddChangeSkillBehaviourModal').modal('show');
        matrix.hideSkillDetails();

        if ($('#dialog_skillname').val() != '') {
            $('#skill_update_matrix').removeAttr('disabled');
        }
        else {
            $('#skill_update_matrix').attr('disabled', 'disabled');
        }
    },
    showSkillDetails: function () {
        matrix.cleanSkillDetails();
        $('[data-containertype="dialog_skilldetails"]').show();
    },
    hideSkillDetails: function () {
        matrix.cleanSkillDetails();
        matrix.showEditSkill();
        $('[data-containertype="dialog_skilldetails"]').hide();
    },
    showEditSkill: function () {
        let selectedSkill = $('#dialog_skillselection').val();
        $('#btn_edit_skill').hide();
        if (selectedSkill != null && selectedSkill !== undefined) {
            if (selectedSkill.toString() !== '') {
                //$('#btn_edit_skill').show();
            }
        }
    },
    orderVisibleMandatorySkills: function () {
        let counter = 0;
        $('[data-container="mandatorySkill"]:visible').each(function () {
            let currentIndex = $(this).attr('data-index');
            if (counter != currentIndex) {
                $(this).find('[data-container="index_display_mandatory"]').text("Index nr: " + counter);
                $(this).attr('data-index', counter);
                $(this).attr('data-ischanged', true);
            }

            counter = counter + 1;
        });
    },
    orderVisibleOperationalSkills: function () {
        let counter = 0;
        $('[data-container="operationalSkill"]:visible').each(function () {
            let currentIndex = $(this).attr('data-index');
            if (counter != currentIndex) {
                $(this).find('[data-container="index_display_operational"]').text("Index nr: " + counter);
                $(this).attr('data-index', counter);
                $(this).attr('data-ischanged', true);
            }

            counter = counter + 1;
        });
    },
    saveMatrixUserSkillRelations: function () {
        var items = [];
        $('[data-ischanged="true"]').each(function () {
            var item = {};
            item.Id = parseInt($(this).attr('data-relationid'));
            item.Index = parseInt($(this).attr('data-index'));
            item.MatrixId = parseInt($(this).attr('data-matrixid'));
            item.UserSkillId = parseInt($(this).attr('data-userskillid'));
            items.push(item);
        });

        toastr.remove();
        var itemsToUpdate = items.length;
        var itemsUpdated = 0;
        $(items).each(function (index, item) {
            $.ajax({
                type: "POST",
                url: '/skillsmatrix/' + item.MatrixId + '/skills/changerelation',
                data: JSON.stringify(item),
                success: function (data) {
                    itemsUpdated++;
                    if (itemsUpdated == itemsToUpdate) {
                        $('#ChangeSkillOrderModal').modal('hide');
                        $('body').toggleClass('loaded');
                        window.location.reload();
                    }
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            })
        });
    },
    cleanSkillDetails: function () {
        matrix.skillEdited = false;
        //$('[data-containertype="change_skill_buttons"]').hide();
        $('#modal_assessment_choice, #dialog_skillname, #dialog_skilldescription, #dialog_goal, #dialog_notification_window_days, #dialog_expiryindays, #dialog_validfrom, #dialog_validto').val('');

        $('[id^="dialog_type_"]').each(function (index, elem) {
            $(elem).parent('label').removeClass('active');
            $(elem).prop('checked', false);
        });

        $('#dialog_type_1').parent('label').addClass('active');
        $('#dialog_type_1').prop('checked', true);

        $('#skill_in_matrix_true').prop('checked', false);

        $('[id^="skill_in_matrix_"]').each(function (index, elem) {
            $(elem).parent('label').removeClass('active');
            $(elem).prop('checked', false);
        });

        $('#skill_in_matrix_false').parent('label').addClass('active');
        $('#skill_in_matrix_false').prop('checked', true);

        $('#modal_assessment_container').hide();

        matrix.resetDefaultTargetValue();

        matrix.showMandatorySkillFields();

        matrix.showEditSkill();
    },
    loadSkillDetails: function () {
        //todo cleanup code
        let data = $('#dialog_skillselection option:selected').attr('data-store');
        let inMatrix = ($('#dialog_skillselection option:selected').attr('data-inmatrix') === 'true');

        //always reset ui first
        matrix.resetDefaultTargetValue();

        if (data != null) {
            let skillItem = JSON.parse(data);
            if (skillItem != null && skillItem.Id > 0) {
                $('#btnDeleteUserSkill').data('skillid', skillItem.Id);
                if (skillItem.InUseInMatrix != undefined && !skillItem.InUseInMatrix) {
                    $('#btnDeleteUserSkill').removeAttr('disabled');
                    $('#btnDeleteUserSkill').removeAttr('title');
                }
                else {
                    $('#btnDeleteUserSkill').attr('disabled', true);
                    $('#btnDeleteUserSkill').attr('title', 'This user skill cannot be deleted because it is in use in one or more Skills Matrices.');
                }
                $('#dialog_skillname').val(skillItem.Name);
                $('#dialog_skilldescription').val(skillItem.Description);
                $('#dialog_goal').val(skillItem.Goal);
                $('#dialog_notification_window_days').val(skillItem.NotificationWindowInDays);
                $('#dialog_expiryindays').val(skillItem.ExpiryInDays);

                if (skillItem.DefaultTarget && skillItem.DefaultTarget != 0 && $('#defaultTarget-' + skillItem.DefaultTarget).length) {
                    matrix.setDefaultTargetValue(skillItem.DefaultTarget, $('#defaultTarget-' + skillItem.DefaultTarget)[0])
                }

                if (skillItem.SkillType == 0) {
                    if ($('#dialog_type_1').length) {
                        $('[id^="dialog_type_"]').each(function (index, elem) {
                            $(elem).parent('label').removeClass('active');
                            $(elem).prop('checked', false);
                        });
                        $('#dialog_type_1').parent('label').addClass('active');
                        $('#dialog_type_1').prop('checked', true);
                    }

                    matrix.showMandatorySkillFields();

                    $('#modal_assessment_container').hide();
                }
                if (skillItem.SkillType == 1) {
                    if ($('#dialog_type_2').length) {
                        $('[id^="dialog_type_"]').each(function (index, elem) {
                            $(elem).parent('label').removeClass('active');
                            $(elem).prop('checked', false);
                        });
                        $('#dialog_type_2').parent('label').addClass('active');
                        $('#dialog_type_2').prop('checked', true);
                    }

                    matrix.showOperationalSkillFields();

                    $('#modal_assessment_container').show();
                }
                $('#modal_assessment_choice').val(skillItem.SkillAssessmentId);
                $('#dialog_validfrom').val(''); //TODO fix
                $('#dialog_validto').val(''); //TODO fix

                $('#skill_in_matrix').prop('checked', false);
                if (inMatrix) {

                    if ($('#skill_in_matrix_true').length) {
                        $('[id^="skill_in_matrix_"]').each(function (index, elem) {
                            $(elem).parent('label').removeClass('active');
                            $(elem).prop('checked', false);
                        });
                        $('#skill_in_matrix_true').parent('label').addClass('active');
                        $('#skill_in_matrix_true').prop('checked', true);
                    }
                }
                else {
                    if ($('#skill_in_matrix_false').length) {
                        $('[id^="skill_in_matrix_"]').each(function (index, elem) {
                            $(elem).parent('label').removeClass('active');
                            $(elem).prop('checked', false);
                        });
                        $('#skill_in_matrix_false').parent('label').addClass('active');
                        $('#skill_in_matrix_false').prop('checked', true);
                    }
                }
                $('#skill_in_matrix_true').attr('data-userskillid', skillItem.Id);
                $('#skill_in_matrix_true').attr('data-id', skillItem.Id);
                $('#skill_in_matrix_container').show();
            }
        }
    },
    saveSkill: function () {
        //warning if operational skill
        if ($('#dialog_type_2').is(':checked') && ($('#modal_assessment_choice').val() == '' || $('#modal_assessment_choice').val() <= 0)) {

            $.fn.dialogue({
                //title: "Alert",
                content: $("<p />").text('Are you sure you do not want to connect an Assessment to this operational skill?'),
                closeIcon: true,
                buttons: [
                    {
                        text: "Yes", id: $.utils.createUUID(), click: function ($modal) {
                            $modal.dismiss();
                            matrix.verifyDeleteOld();
                        }
                    },
                    { text: "No", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
                ]
            });
        }
        else {
            matrix.verifyDeleteOld();
        }

    },
    verifyDeleteOld: function(){
        let Skill;
        let skillData = $('#dialog_skillselection option:selected').attr('data-store');
        if (skillData) {
            Skill = JSON.parse(skillData);
        }

        //warning if assessment was changed, right hand side of && is to prevent warning when assessment id is not and was not chosen (would be undefined and '')
        if (Skill && $('#dialog_type_2').is(':checked') && ($('#modal_assessment_choice').val() != Skill.SkillAssessmentId) && !(!$('#modal_assessment_choice').val() && !Skill.SkillAssessmentId)) {

            $.fn.dialogue({
                //title: "Keep old scores?",
                content: $("<p />").html(matrix.language.keepOldScoresDialogueBody),
                closeIcon: true,
                buttons: [
                    {
                        text: matrix.language.removeOldScores, id: $.utils.createUUID(), click: function ($modal) {
                            $modal.dismiss();
                            matrix.updateSkill(true); //remove old values
                        }
                    },
                    {
                        text: matrix.language.keepOldScores, id: $.utils.createUUID(), click: function ($modal) {
                            $modal.dismiss();
                            matrix.updateSkill(false); //do not remove old values
                        }
                    },
                    {
                        text: matrix.language.cancel, id: $.utils.createUUID(), click: function ($modal) {
                            $modal.dismiss();
                        }
                    }
                ]
            });
        } else {
            matrix.updateSkill(false);
        }
    },
    updateSkill: function (deleteoldvalues = false) {
        let OldInMatrix = ($('#dialog_skillselection option:selected').attr('data-inmatrix') === 'true');
        let NewInMatrix = $('#skill_in_matrix_true').is(':checked');

        if (!matrix.skillEdited && OldInMatrix == NewInMatrix) {
            $('body').toggleClass('loaded');
            window.location.reload();
            return;
        }

        if (!matrix.validateUserSkill()) {
            return;
        }

        $('body').toggleClass('loaded');

        let newSkill = {};

        newSkill.Id = $('#dialog_skillselection').val() !== '' ? parseInt($('#dialog_skillselection').val()) : 0;

        newSkill.Name = $('#dialog_skillname').val();
        newSkill.Description = $('#dialog_skilldescription').val();

        newSkill.IsInMatrix = NewInMatrix;

        newSkill.ExpiryInDays = $('#dialog_expiryindays').val() == '' ? 0 : parseInt($('#dialog_expiryindays').val());
        newSkill.Goal = $('#dialog_goal').val() == '' ? 0 : parseInt($('#dialog_goal').val());
        newSkill.NotificationWindowInDays = $('#dialog_notification_window_days').val() == '' ? 0 : parseInt($('#dialog_notification_window_days').val());

        newSkill.SkillType = $('#dialog_type_2').is(':checked') ? 1 : 0;

        if (newSkill.SkillType == 1) {
            newSkill.SkillAssessmentId = parseInt($('#modal_assessment_choice').val());

            if ($('div[id^="defaultTarget-"][class$="-selected"]').length) {
                newSkill.DefaultTarget = parseInt(matrix.getDefaultTargetValue());
            }

        }

        if (newSkill.Id > 0) {
            $('#dialog_skillselection option[value="' + newSkill.Id + '"]').remove();
        }

        $.ajax({
            type: "POST",
            url: '/skillsmatrix/' + matrix.currentMatrixId + '/skills/addchange?deleteoldvalues=' + deleteoldvalues,
            data: JSON.stringify(newSkill),
            async: false,
            success: function (data) {
                $('body').toggleClass('loaded');
                var returnedSkill = JSON.parse(data);
                if (newSkill.Id == 0 || newSkill.IsInMatrix) {
                    newSkill.Id = returnedSkill.Id;
                    $('#opt_skill_in_matrix').append('<option value="' + returnedSkill.Id + '" data-inmatrix="true">' + (newSkill.SkillType == 0 ? 'Mandatory Skill' : 'Operational Skill') + ' - ' + newSkill.Name + '</option>');
                } else {
                    $('#opt_skill_not_in_matrix').append('<option value="' + newSkill.Id + '" data-inmatrix="false">' + (newSkill.SkillType == 0 ? 'Mandatory Skill' : 'Operational Skill') + ' - ' + newSkill.Name + '</option>');
                }

                $('#dialog_skillselection option[value="' + newSkill.Id + '"]').attr('data-store', JSON.stringify(newSkill));
                $('#dialog_skillselection').val(newSkill.Id);

                matrix.updateSkillMatrix(OldInMatrix, NewInMatrix);
                matrix.hideSkillDetails();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('body').toggleClass('loaded');
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            },
            contentType: "application/json; charset=utf-8"
        });
    },
    updateSkillMatrix: function (oldInMatrix, newInMatrix) {
        if (!matrix.validateUserSkill()) {
            return;
        }
        if ($('#dialog_skillselection').val() !== '') {

            let updateSkillMatrixUri = '';
            let relation = {};

            //relation.Id = ($('#skill_in_matrix_true').attr('data-userskillid') == undefined || $('#skill_in_matrix_true').attr('data-userskillid') == '' ? 0 : parseInt($('#skill_in_matrix_true').attr('data-userskillid')));
            relation.UserSkillId = parseInt($('#skill_in_matrix_true').attr('data-userskillid'));

            relation.MatrixId = matrix.currentMatrixId;

            if (!oldInMatrix && newInMatrix) {
                updateSkillMatrixUri = '/skillsmatrix/' + matrix.currentMatrixId + '/skills/addrelation';
            } else if (oldInMatrix && !newInMatrix) {
                updateSkillMatrixUri = '/skillsmatrix/' + matrix.currentMatrixId + '/skills/removerelation';
            }
            else {
                $('body').toggleClass('loaded');
                document.location.reload();
                return;
            }

            $('body').toggleClass('loaded');

            $.ajax({
                type: "POST",
                url: updateSkillMatrixUri,
                data: JSON.stringify(relation),
                success: function (data) {
                    $('body').toggleClass('loaded');
                    toastr.success('Matrix skill change saved.');

                    window.location.reload();
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $('body').toggleClass('loaded');
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });

            let option = $('#dialog_skillselection option[value="' + relation.UserSkillId + '"]');
            $('#dialog_skillselection option[value="' + relation.UserSkillId + '"]').remove();
            option.attr('data-inmatrix', newInMatrix);
            $(newInMatrix ? '#opt_skill_in_matrix' : '#opt_skill_not_in_matrix').append(option);
            $('#dialog_skillselection').val(relation.UserSkillId);
        }
    },

    initGroupHandlers: function () {
        //group
        $('[data-action="search_person"]').on('keyup', function () {
            matrix.searchModal($(this).val());
        });
        $('#btnChangeUserGroups').on('click', function () {
            matrix.initGroupModal();
            $('#btn_add_group').click();
        });
        $('#group_choice').on('change', function () {
            matrix.displayGroupUserDetails();
        });
        $('[data-actiontype="add_user_to_group"]').on('click', function () {
            matrix.addUserToGroup(parseInt($(this).attr('data-id')));
        });
        $('[data-actiontype="remove_user_from_group"]').on('click', function () {
            matrix.removeUserFromGroup(parseInt($(this).attr('data-id')));
        });
        $('#clear_search_btn').on('click', function () {
            matrix.refreshUserList();
        });
        $('#btn_cancel_group').on('click', function () { matrix.displayGroupUserDetails(); });
        $('#btn_save_group').on('click', function () {
            matrix.saveGroup();
        });
        //$('[data-actiontype="close_modal"]').on('click', function () {
        //matrix.updateMatrix();
        //});
        $('#group_name, #group_description').on('keyup', function () {
            matrix.groupEdited = true;
            $('[data-containertype="change_group_buttons"]').show();
        });
        $('#group_in_matrix').on('click', function () {
            matrix.updateGroupMatrix();
        });
        $('#btn_add_group').on('click', function () {
            $('#group_choice').val(''); //reset dropdown
            //matrix.displayGroupDetails();

            matrix.displayGroupUserDetails();
        });
        $('#btn_change_group').on('click', function () {
            //matrix.displayGroupDetails();

            matrix.displayGroupUserDetails();
        });
    },
    searchModal: function (searchValue) {
        searchValue = searchValue.replaceAll(',', '');
        searchValue = searchValue.replaceAll(';', '');
        searchValue = searchValue.trim();
        var searchTerms = searchValue.split(' ');
        var selectedUsers = [];
        $('[data-containertype="group_users"] li[data-containertype="user_chosen"]:visible').each(function () {
            selectedUsers.push($(this).data('id'));
        });
        if (searchValue != null && searchValue != '') {
            $('[data-containertype="user_choice"]').hide();
            $('[data-containertype="user_choice"]').each(function () {
                searchTerms.forEach(s => {
                    if ((!selectedUsers.includes(+$(this).attr('data-id'))) && ($(this).attr('data-lastname').toLowerCase().includes(s.toLowerCase()) || $(this).attr('data-firstname').toLowerCase().includes(s.toLowerCase()))) {
                        $(this).show();
                    }
                });
            });
        } else {
            $('[data-containertype="user_choice"]').each(function () {
                if ((!selectedUsers.includes(+$(this).attr('data-id')))) {
                    $(this).show();
                }
            });
        }

    },
    initGroupModal: function () {
        $('#group_choice').val('');
        $('#AddChangeUserGroupModal').modal('show');
        matrix.hideGroupDetails();
        matrix.displayGroupUserDetails();
        //matrix.displayUsers();
        //matrix.displayGroupDetails();
    },
    cleanGroupDetails: function () {
        matrix.groupEdited = false;
        $('[data-containertype="change_group_buttons"]').hide();
        $('#group_name').val('');
        $('#group_description').val('');
        $('#group_in_matrix').prop('checked', false);
        $('#group_in_matrix').removeProp('checked');
    },
    hideGroupDetails: function () {
        matrix.cleanGroupDetails();
        if ($('#group_choice').val() == '') { $('#btn_change_group').hide(); }
        $('[data-containertype="group_details"]').hide();
        $('[data-containertype="group_user_details"]').hide();
    },
    displayGroupDetails: function () {
        //matrix.hideGroupDetails();
        $('#group_name').val('');
        $('#group_description').val('');
        if ($('#group_choice').val() != '') {
            let details = JSON.parse($('#group_choice option:selected').attr('data-store'));
            $('#group_name').val(details.Name);
            $('#group_description').val(details.Description);
            $('#btnDeleteUserGroup').data('groupid', details.Id);
            if (details.InUseInMatrix != undefined && !details.InUseInMatrix) {
                $('#btnDeleteUserGroup').removeAttr('disabled');
                $('#btnDeleteUserGroup').removeAttr('title');
            }
            else {
                $('#btnDeleteUserGroup').attr('disabled', true);
                $('#btnDeleteUserGroup').attr('title', 'This user group cannot be deleted because it is in use in one or more Skills Matrices.');
            }
            if ($('#group_choice option:selected').attr('data-inmatrix').toString() === 'true') {
                $('#group_in_matrix').prop('checked', true);
            } else {
                $('#group_in_matrix').removeProp('checked');
                $('#group_in_matrix').prop('checked', false);
            }
            $('[data-containertype="in_matrix_container"]').show();
            $('#group_in_matrix').attr('data-usergroupid', details.Id);

        } else {
            $('[data-containertype="in_matrix_container"]').hide();
            $('#group_in_matrix').prop('checked', true);
        }

        $('[data-containertype="group_details"]').show();
    },
    displayGroupUserDetails: function () {
        matrix.hideGroupDetails();
        let selectedGroup = $('#group_choice').val();
        if (selectedGroup != null && selectedGroup !== undefined) {
            if (selectedGroup.toString() !== '') {

                $('[data-containertype="group_user_details"]').show();
                $('[data-containertype="group_users"] li').hide();
                $('#btn_change_group').show();
                matrix.displayUsers();
            }
        }
        matrix.displayGroupDetails();
    },
    addUserToGroup: function (userId) {
        if (userId != null && $('#group_choice').val() !== '') {
            var currentGroup = parseInt($('#group_choice').val());
            var currentUserElement = $('[data-containertype="group_users"] li[data-containertype="user_chosen"][data-id="' + userId.toString() + '"]');
            if (currentUserElement != null) {
                let possibleGroups = currentUserElement.attr('data-groupid');
                if (possibleGroups != null && possibleGroups.toString() !== '') {
                    var groups = JSON.parse(possibleGroups);
                    if (groups.indexOf(currentGroup) === -1) {
                        groups.push(currentGroup);
                        currentUserElement.attr('data-groupid', '[' + groups.toString() + ']');
                    }
                } else {
                    currentUserElement.attr('data-groupid', '[' + currentGroup + ']');
                }
                currentUserElement.show();
            }
            matrix.addUserWithGroup(userId);
        }
        matrix.displayUsers();
        $('[data-action="search_person"]').trigger('keyup');
    },
    removeUserFromGroup: function (userId) {

        $.fn.dialogue({
            //title: "Alert",
            content: $("<p />").text("Are you sure you want to remove this user from the user group?"),
            closeIcon: true,
            buttons: [
                {
                    text: "Yes", id: $.utils.createUUID(), click: function ($modal) {
                        $modal.dismiss();
                        if (userId != null && $('#group_choice').val() !== '') {
                            let currentGroup = parseInt($('#group_choice').val());
                            let currentUserElement = $('[data-containertype="group_users"] li[data-containertype="user_chosen"][data-id="' + userId.toString() + '"]');
                            if (currentUserElement != null) {
                                let possibleGroups = currentUserElement.attr('data-groupid');
                                if (possibleGroups != null && possibleGroups.toString() !== '') {
                                    let groups = JSON.parse(possibleGroups);
                                    groups = groups.filter(e => e !== currentGroup);
                                    currentUserElement.attr('data-groupid', '[' + groups.toString() + ']');
                                }
                                currentUserElement.hide();
                            }
                            matrix.removeUserWithGroup(userId);
                        }
                        matrix.displayUsers();
                        $('[data-action="search_person"]').trigger('keyup');
                    }
                },
                { text: "No", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
            ]
        });
    },
    showUserSkillValuesModal: function (matrixid, userid, userName) {
        $.ajax({
            type: "GET",
            url: `/skillsmatrix/${matrixid}/skillvalues/${userid}/modalcontent`,
            success: function (modalContent) {
                $('#UserSkillValuesModalContent').html(modalContent);
                $('#UserSkillValuesModal').modal('show');
                matrix.currentMatrixId = matrixid;
                matrix.currentUserId = userid;
                $('[id^="valuedate-"]').each(function (index, elem) {
                    matrix.initDateRangePicker(elem);
                });

                ezgomediafetcher.preloadImagesAndVideos();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            }
        });
    },
    initDateRangePicker: function (elem) {

        let startDate = $(elem).data('iso8601certifiedat');
        if (!startDate) {
            startDate = moment().format('YYYY-MM-DD');
        }
        let picker = $(elem).data('daterangepicker');

        if (picker && picker.container) {
            return;
        }

        $(elem).daterangepicker({
            parentEl: '#UserSkillValuesModalBody',
            singleDatePicker: true,
            showDropdowns: true,
            autoApply: false,
            applyButtonClasses: 'btn-ezgo customConfirmButton',
            cancelButtonClasses: 'btn-secondary customCloseButton',
            startDate: moment(startDate, 'YYYY-MM-DD', true),
            locale: {
                format: 'YYYY-MM-DD',
                firstDay: 1,
                applyLabel: 'Confirm',
                autoUpdateInput: false,
                cancelLabel: 'Close',
                daysOfWeek: ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat']
            }
        });

        picker = $(elem).data('daterangepicker');

        let container = $(picker.container);

        matrix.currentUserSkillName = $(elem).data('userskillname');

        matrix.currentExpiryInDays = $(elem).data('expiryindays');

        matrix.currentNotificationWindowInDays = $(elem).data('notificationwindowindays');

        const $buttons = container.find('.drp-buttons');
        $buttons.css({
            display: 'flex',
            justifyContent: 'center',
            padding: '10px 20px'
        });

        let preventHide = false;

        const originalSetStartDate = picker.setStartDate;
        picker.setStartDate = function (date) {
            preventHide = true;
            originalSetStartDate.call(this, date);
            $('#valuedate-' + matrix.currentMatrixId + '-' + matrix.currentSkillId).val(date.format('YYYY-MM-DD'));
            requestAnimationFrame(() => {
                this.move();
            });
        };

        const originalShow = picker.show;

        picker.show = function () {

            originalShow.call(this);
            picker.container.addClass('showPicker');
        };

        const originalHide = picker.hide;
        picker.hide = function () {
            if (preventHide) {
                preventHide = false;
                return;
            }
            picker.container.removeClass('showPicker');

            $('#datepicker-backdrop').removeClass('show');

            picker.container.one('transitionend', () => {
                originalHide.call(this);
            });
        };

        const originalMove = picker.move;
        picker.move = function () {
            originalMove.call(this);

            requestAnimationFrame(() => {
                matrix.showUserSkillPickerAndFixPosition(false,
                    $(`#valuedate-${matrix.currentMatrixId}-${matrix.currentSkillId}`),
                    $(`#editSkillValuePositioningElement-${matrix.currentMatrixId}-${matrix.currentSkillId}`)[0], $('#UserSkillValuesModal'));
            });
        };

        var originalRenderCalendar = picker.renderCalendar;

        picker.renderCalendar = function (side) {
            originalRenderCalendar.call(this, side);

            const displayedMonth = this.leftCalendar.month.clone();
            const month = displayedMonth.month();
            const year = displayedMonth.year();

            const specialMoment = moment(matrix.currentCertifiedAt, 'YYYY-MM-DD');

            this.container.find('td.available').each(function () {
                const $td = $(this);
                if ($td.hasClass('off')) return;

                const text = $td.text().trim();

                if (!$td.find('span').length) {
                    $td.html('<span>' + text + '</span>');
                }

                const day = parseInt(text, 10);
                if (isNaN(day)) return;

                const cellMoment = moment({ year, month, day });

                if (cellMoment.isSame(specialMoment, 'day')) {
                    $td.addClass('currentUserSkillValueDatePicker');
                } else {
                    $td.removeClass('currentUserSkillValueDatePicker');
                }
            });

            const $row = this.container.find('.drp-calendar.' + side + ' thead tr:first');

            const $prev = $row.find('th.prev');
            const $next = $row.find('th.next');
            const $month = $row.find('th.month');

            $row.append($month);
            $row.append($prev);
            $row.append($next);
        };

        if (!container.find('.drp-custom-header').length) {
            container.prepend(`
                            <div class="modal-header datepicker-header">
                                <div class="row" style="width: 100%;">
                                    <div class="col">
                                        <div class="row">
                                            <div class="col-12">
                                                <h4 style="display: inline-block">Certified at</h4>
                                                &nbsp;&nbsp;
                                                <h6 style="display: inline-block" id="certifiedAtValue-${matrix.currentMatrixId}-${$(elem).data('userskillid')}">${$(elem).data('certifiedat')}</h6>
                                            </div>
                                            <div class="col-12">
                                                <h6 id="datePickerSkillName">${matrix.currentUserSkillName}</h6>
                                            </div>
                                        </div>
                                    </div>
                                    <div class="col-auto">
                                        <button type="button" class="close drp-x-btn" aria-label="Close">
                                            <span aria-hidden="true">&times;</span>
                                        </button>
                                    </div>
                                </div>
                            </div>
                        `);
        }

        container.on('click', '.drp-x-btn', function () {
            const originalFlag = preventHide;
            preventHide = false;  // allow hide
            picker.hide();
            preventHide = originalFlag;
        });

        $('#UserSkillValuesModalBody').on('mousedown', function (e) {
            if (!picker.isShowing) return;

            if (!container.is(e.target) && container.has(e.target).length === 0) {
                preventHide = true;
            }
        });

        let deleteButton = $('<button type="button" class="btn btn-sm btn-danger deleteUserSkillValueButton">Delete</button>');
        container.find('.drp-buttons').prepend(deleteButton);

        //todo make toggle
        let isRequired = $(elem).data('isrequired');
        console.log(isRequired);
        let notRequiredButton = '';
        if (!isRequired) {
            notRequiredButton = $('<button type="button" class="btn btn-sm btn-ezgo requiredForThisPersonButton">Required for this person</button>');
        }
        else {
            notRequiredButton = $('<button type="button" class="btn btn-sm btn-ezgo notRequiredForThisPersonButton">Not required for this person</button>');

        }
        container.find('.drp-buttons').prepend(notRequiredButton);
    },
    showUserSkillPickerAndFixPosition: function (show, dateinput, element, modal) {
        let picker = $(dateinput).data('daterangepicker');
        if (!picker) {
            matrix.initDateRangePicker(dateinput);
            picker = $(dateinput).data('daterangepicker');
        }
        matrix.currentCertifiedAt = $(element).data('iso8601certifiedat');

        if (show) {
            picker.show();
            picker.move();

            $('#datepicker-backdrop').addClass('show');
        }

        if (picker && picker.container) {
            picker.container.find('.drp-buttons').show();

            picker.container.css({
                position: 'fixed',
                top: '50%',
                left: '50%',
                width: '30vw',
                maxWidth: '600px',
                minWidth: '250px',
                transform: 'translate(-50%, -50%)',
                borderRadius: '12px',
                overflow: 'hidden',
                zIndex: 1050  // higher than backdrop
            });
        }
    },
    displayUsers: function () {
        $('[data-containertype="group_users"] li[data-containertype="user_chosen"]').hide();
        $('[data-containertype="group_all_users"] li[data-containertype="user_choice"]').show();
        let selectedGroup = parseInt($('#group_choice').val());
        $('[data-containertype="group_users"] li[data-containertype="user_chosen"]').each(function () {
            let currentUserElement = $(this);
            let possibleGroups = currentUserElement.attr('data-groupid');
            if (possibleGroups != null && possibleGroups.toString() !== '') {
                let groups = JSON.parse(possibleGroups);
                groups = groups.filter(e => e == selectedGroup);
                if (groups !== undefined && groups.length > 0) {
                    currentUserElement.show();
                    $('li[data-containertype="user_choice"][data-id="' + currentUserElement.attr('data-id') + '"]').hide();
                }
            }
        });

    },
    refreshUserList: function () {
        $('#searchpersonexecute').val('');
        var selectedUsers = [];
        $('[data-containertype="group_users"] li[data-containertype="user_chosen"]:visible').each(function () {
            selectedUsers.push($(this).data('id'));
        });

        $('[data-containertype="user_choice"]').hide();
        $('[data-containertype="user_choice"]').each(function () {
            if ((!selectedUsers.includes(+$(this).attr('data-id')))) {
                $(this).show();
            }
        });
    },
    saveGroup: function () {
        if (!matrix.groupEdited && !matrix.relationChanged) {
            return;
        }
        else if (matrix.relationChanged) {
            $('body').toggleClass('loaded');
            document.location.reload();
            return;
        }

        $('body').toggleClass('loaded');

        let newGroup = {};
        newGroup.Id = $('#group_choice').val() !== '' ? parseInt($('#group_choice').val()) : 0;
        newGroup.Name = $('#group_name').val();
        newGroup.Description = $('#group_description').val();
        newGroup.IsInMatrix = $('#group_in_matrix').is(':checked');
        

        if (newGroup.Id > 0) {
            $('#group_choice option[value="' + newGroup.Id + '"]').remove();
        }

        $.ajax({
            type: "POST",
            url: '/skillsmatrix/' + matrix.currentMatrixId + '/groups/addchange',
            data: JSON.stringify(newGroup),
            async: false,
            success: function (data) {
                $('body').toggleClass('loaded');
                var returnedGroup = JSON.parse(data);
                toastr.success('Group saved.');

                if (newGroup.Id == 0 || newGroup.IsInMatrix) {
                    //new group, automatically should be added to matrix, or already a existing group in matrix.
                    newGroup.Id = returnedGroup.Id;
                    $('#opt_group_in_matrix').append('<option value="' + returnedGroup.Id + '" data-inmatrix="true">' + newGroup.Name + '</option>');
                } else {
                    $('#opt_group_not_in_matrix').append('<option value="' + newGroup.Id + '" data-inmatrix="false">' + newGroup.Name + '</option>');

                }
                $('#group_choice option[value="' + newGroup.Id + '"]').attr('data-store', JSON.stringify(newGroup));
                $('#group_choice').val(newGroup.Id);

                matrix.displayGroupUserDetails();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('body').toggleClass('loaded');
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            },
            contentType: "application/json; charset=utf-8"
        });

    },
    addUserWithGroup: function (userid) {
        let groupId = parseInt($('#group_choice').val());
        
        matrix.saveUserWithGroup('add', userid, groupId)
        //document.location.reload();
    },
    removeUserWithGroup: function (userid) {
        let groupId = parseInt($('#group_choice').val());

        matrix.saveUserWithGroup('remove', userid, groupId)
        //document.location.reload();
    },
    saveUserWithGroup: function (uriType, userId, groupId) {
        //uritype -> add or remove
        $('body').toggleClass('loaded');

        $.ajax({
            type: "POST",
            url: '/skillsmatrix/' + matrix.currentMatrixId + '/group/' + groupId + '/users/' + uriType + '/' + userId,
            data: JSON.stringify(''),
            success: function (data) {
                $('body').toggleClass('loaded');
                toastr.success('User saved.');
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('body').toggleClass('loaded');
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            },
            contentType: "application/json; charset=utf-8"
        });
        //document.location.reload();
    },
    updateGroupMatrix: function () {
        if ($('#group_choice').val() !== '') {
            $('body').toggleClass('loaded');
            matrix.relationChanged = true;

            let inMatrix = $('#group_in_matrix').is(':checked');
            let updateGroupMatrixUri = '';
            let relation = {};
            relation.Id = ($('#group_in_matrix').attr('data-id') == undefined || $('#group_in_matrix').attr('data-id') == '' ? 0 : parseInt($('#group_in_matrix').attr('data-id')));
            relation.UserGroupId = parseInt($('#group_in_matrix').attr('data-usergroupid'));
            relation.MatrixId = matrix.currentMatrixId;

            if (inMatrix) {
                updateGroupMatrixUri = '/skillsmatrix/' + matrix.currentMatrixId + '/groups/addrelation';
            } else {
                updateGroupMatrixUri = '/skillsmatrix/' + matrix.currentMatrixId + '/groups/removerelation';
            }

            $.ajax({
                type: "POST",
                url: updateGroupMatrixUri,
                data: JSON.stringify(relation),
                success: function (data) {
                    $('body').toggleClass('loaded');
                    toastr.success('Matrix group change saved.');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $('body').toggleClass('loaded');
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });

            let option = $('#group_choice option[value="' + relation.UserGroupId + '"]');
            $('#group_choice option[value="' + relation.UserGroupId + '"]').remove();
            option.attr('data-inmatrix', inMatrix);
            $(inMatrix ? '#opt_group_in_matrix' : '#opt_group_not_in_matrix').append(option);
            $('#group_choice').val(relation.UserGroupId);
            matrix.displayGroupUserDetails();
        }
       
    },
    // other
    calculateMatrix: function () {
        matrix.calculateMandatorySkills();
        matrix.calculateOperationalSkills();
    },
    calculateMandatorySkills : function () {
       
        $('[data-containertype="mandatory_skill_row"]').each(function () {
            
            let goal = parseInt($(this).find('[data-containertype="goal_value"]').text());
            let goal_difference = 0; // $(this).find('[data-containertype="difference_value"]').text();
            let goal_result = 0; // $(this).find('[data-containertype="result_value"]').text();
            let items = $.merge($(this).find('[data-containertype="value"] button[data-value="2"]'), $(this).find('[data-containertype="value"] button[data-value="5"]'));

            goal_result = 0;
            let userCol = [];
            items.each(function () {
                userCol.push($(this).attr('data-userid'));
            });
            let unique = userCol.filter((v, i, a) => a.indexOf(v) === i && v != undefined);
            if (unique == undefined || unique[0] == undefined) {
                goal_result = 0;
            } else {
                goal_result = unique.length;
            }
            goal_difference = goal_result - goal;
            $(this).find('[data-containertype="difference_value"]').text(goal_difference);
            if (goal_difference < 0) {
                $(this).find('[data-containertype="difference_value"]').parent('div.matrix-tooltip').addClass('gap-too-low');
            }
            else {
                $(this).find('[data-containertype="difference_value"]').parent('div.matrix-tooltip').addClass('gap-high-enough');
            }
            $(this).find('[data-containertype="result_value"]').text(goal_result);
        });
    },
    calculateOperationalSkills: function () {
        let result = 0;
        $('[data-containertype="operational_skill_row"]').each(function () {
            let goal = parseInt($(this).find('[data-containertype="goal_value"]').text());
            let defaultTarget = parseInt($(this).find('[data-containertype="default_target_value"]').text());
            if (!defaultTarget) {
                defaultTarget = 0;
            }
            let goal_difference = 0; // $(this).find('[data-containertype="difference_value"]').text();
            let goal_result = 0; // $(this).find('[data-containertype="result_value"]').text();
            let items = $(this).find('[data-containertype="value"] button[data-value]');
            goal_result = 0;
            let userCol = [];
            items.each(function () {
                if ($(this).attr('data-isapplicable')) {
                    defaultTarget = parseInt($(this).attr('data-customtarget'));
                }
                if ($(this).attr('data-value') != 0 && (!defaultTarget || $(this).attr('data-value') >= defaultTarget)) {
                    console.log(true);
                    userCol.push($(this).attr('data-userid'));
                }
            });
            let unique = userCol.filter((v, i, a) => a.indexOf(v) === i && v != undefined);
            if (unique == undefined || unique[0] == undefined) {
                goal_result = 0;
            } else {
                goal_result = unique.length;
            }
            goal_difference = goal_result - goal;
            $(this).find('[data-containertype="difference_value"]').text(goal_difference);
            if (goal_difference < 0) {
                $(this).find('[data-containertype="difference_value"]').addClass('gap-too-low');
            }
            else {
                $(this).find('[data-containertype="difference_value"]').addClass('gap-high-enough');
            }
            $(this).find('[data-containertype="result_value"]').text(goal_result);
        });
    },
    saveSkillValue: function () {
        let btn = $('[data-userid="' + matrix.currentUserId + '"][data-usergroupid="' + matrix.currentGroupId + '"][data-skillid="' + matrix.currentSkillId + '"]');
        let value = btn.attr('data-value') != undefined && btn.attr('data-value') != '' ? parseInt(btn.attr('data-value')) : -1;
        if (value != -1) {
            let skillValue = {};
            skillValue.Score = parseInt(value);
            skillValue.UserId = parseInt(matrix.currentUserId);
            skillValue.UserSkillId = parseInt(matrix.currentSkillId);
            skillValue.ValueDate = new Date().toISOString();

            $.ajax({
                type: "POST",
                url: '/skillsmatrix/' + matrix.currentMatrixId + '/skillvalue/save',
                data: JSON.stringify(skillValue),
                success: function (data) {
                    toastr.success('Value saved.');
                },
                error: function (jqXHR, textStatus, errorThrown) {

                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });

            let otherBtn = $('[data-userid="' + matrix.currentUserId + '"][data-skillid="' + matrix.currentSkillId + '"]');
            //update UI;
            otherBtn.each(function () {
                otherBtn.attr('class', btn.attr('class'));
                otherBtn.attr('data-value', value);
                otherBtn.text(btn.text());
            });

        }
        matrix.calculateMatrix();
    },
    saveUserSkillValue: function (skillsmatrixid, userskillid, userid, remove) {
        let btn = $('[data-userid="' + userid + '"][data-skillid="' + userskillid + '"]');
        let skillValue = {};
        var skillValueScore = 2;
        var skillClass = 'btn circlebtn btn-green thumbsup';
        if (matrix.currentUserSkills != undefined && matrix.currentUserSkills != null) {
            matrix.currentUserSkills.forEach(userSkill => {
                if (userSkill.UserSkillId == userskillid) {
                    var referenceDate = new Date($('#valuedate-' + skillsmatrixid + '-' + userskillid).val());
                    var now = Date.now();
                    if (referenceDate > now) {
                        skillValue.Score = 0;
                        skillClass = 'btn circlebtn';
                    }
                    else if (userSkill.ExpiryInDays != undefined && userSkill.ExpiryInDays != null) {
                        var expiryDate = new Date(referenceDate.valueOf());
                        expiryDate.setDate(referenceDate.getDate() + userSkill.ExpiryInDays);
                        if (now >= expiryDate) {
                            skillValueScore = 1;
                            skillClass = 'btn circlebtn btn-red thumbsdown';
                        }
                        else if (userSkill.NotificationWindowInDays != undefined && userSkill.NotificationWindowInDays != null) {
                            //2 when now is smaller than expiry date and now is smaller than expiry date - notification window in days
                            //1 when now is bigger than expiry date
                            //5 when now is smaller than expiry date
                            var notificationDate = new Date(expiryDate.valueOf());
                            notificationDate.setDate(expiryDate.getDate() - userSkill.NotificationWindowInDays);
                            if (now >= notificationDate) {
                                skillValueScore = 5;
                                skillClass = 'btn circlebtn btn-orange warning';
                            }
                        }
                    }
                }
            })
        }

        if (remove) {
            skillValue.Score = 0;
            skillValue.UserId = parseInt(userid);
            skillValue.UserSkillId = parseInt(userskillid);
            skillValue.ValueDate = '0001-01-01T00:00:00Z';

            skillClass = 'btn circlebtn';
            skillValueScore = 0;

            $('#valuedate-' + skillsmatrixid + '-' + userskillid).val('');
            $('#confirmUserSkillValue-' + skillsmatrixid + '-' + userskillid).prop('disabled', true);
        }
        else {
            skillValue.Score = skillValueScore;
            skillValue.UserId = parseInt(userid);
            skillValue.UserSkillId = parseInt(userskillid);
            skillValue.ValueDate = new Date($('#valuedate-' + skillsmatrixid + '-' + userskillid).val()).toISOString();

            $('#confirmUserSkillValue-' + skillsmatrixid + '-' + userskillid).prop('disabled', true);
        }

        $.ajax({
            type: "POST",
            url: '/skillsmatrix/' + skillsmatrixid + '/skillvalue/save',
            data: JSON.stringify(skillValue),
            success: function (data) {
                toastr.success('Value saved.');
            },
            error: function (jqXHR, textStatus, errorThrown) {

                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            },
            contentType: "application/json; charset=utf-8"
        });
        let buttons = $('[data-userid="' + userid + '"][data-skillid="' + userskillid + '"]');
        //update UI;
        buttons.each(function () {
            buttons.attr('class', skillClass);
            buttons.attr('data-value', skillValueScore);
        });

        matrix.calculateMatrix();
    },
    saveUserSkillValueNew: function (skillsmatrixid, userskillid, userid) {
        let skillValue = {};
        var skillValueScore = 2;
        var skillClass = 'btn circlebtn btn-green thumbsup';

        var referenceDate = new Date($('#valuedate-' + skillsmatrixid + '-' + userskillid).val());
        var now = Date.now();
        if (referenceDate > now) {
            skillValueScore = 0;
            skillClass = 'btn circlebtn';
        }
        else if (matrix.currentExpiryInDays != undefined && matrix.currentExpiryInDays != null) {
            var expiryDate = new Date(referenceDate.valueOf());
            expiryDate.setDate(referenceDate.getDate() + matrix.currentExpiryInDays);
            if (now >= expiryDate) {
                skillValueScore = 1;
                skillClass = 'btn circlebtn btn-red thumbsdown';
            }
            else if (matrix.currentNotificationWindowInDays != undefined && matrix.currentNotificationWindowInDays != null) {
                //2 when now is smaller than expiry date and now is smaller than expiry date - notification window in days
                //1 when now is bigger than expiry date
                //5 when now is smaller than expiry date
                var notificationDate = new Date(expiryDate.valueOf());
                notificationDate.setDate(expiryDate.getDate() - matrix.currentNotificationWindowInDays);
                if (now >= notificationDate) {
                    skillValueScore = 5;
                    skillClass = 'btn circlebtn btn-orange warning';
                }
            }
        }

        skillValue.Score = skillValueScore;
        skillValue.UserId = parseInt(userid);
        skillValue.UserSkillId = parseInt(userskillid);
        skillValue.ValueDate = new Date($('#valuedate-' + skillsmatrixid + '-' + userskillid).val()).toISOString();

        $.ajax({
            type: "POST",
            url: '/skillsmatrix/' + skillsmatrixid + '/skillvalue/save',
            data: JSON.stringify(skillValue),
            success: function (data) {
                toastr.success('Value saved.');
                $(`#mandatorySkillDetails-${userskillid}-${userid}`).html(data);
                matrix.initDateRangePicker($(`#valuedate-${skillsmatrixid}-${userskillid}`));

                let buttons = $('[data-userid="' + userid + '"][data-skillid="' + userskillid + '"]');
                //update UI;
                buttons.each(function () {
                    buttons.attr('class', skillClass);
                    buttons.attr('data-value', skillValueScore);
                });

                matrix.calculateMatrix();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            },
            contentType: "application/json; charset=utf-8"
        });
    },
    loadAndApplyLegendConfiguration: function () {
        // If already loaded, just apply it
        if (matrix.legendConfiguration) {
            matrix.applyLegendConfiguration(matrix.legendConfiguration);
            return;
        }

        // Load legend configuration from API
        $.ajax({
            url: '/companysettings/legend',
            type: 'GET',
            success: function (response) {
                matrix.legendConfiguration = response;
                matrix.applyLegendConfiguration(response);
            },
            error: function (xhr, status, error) {
                console.error('Failed to load legend configuration:', error);
            }
        });
    },
    applyLegendConfiguration: function (config) {
        if (!config) return;

        // Icon class mapping from config iconClass to Font Awesome class
        var iconClassMapping = {
            'thumbsup': 'fa-thumbs-up',
            'thumbsdown': 'fa-thumbs-down',
            'warning': 'fa-exclamation-triangle'
        };

        // Apply mandatory skills configuration to legend modal
        if (config.mandatorySkills) {
            config.mandatorySkills.forEach(function (item) {
                var btn = $('.legend-btn-mandatory[data-skill-level-id="' + item.skillLevelId + '"]');
                var label = $('.legend-label-mandatory[data-skill-level-id="' + item.skillLevelId + '"]');
                if (btn.length) {
                    btn.css({
                        'background-color': item.backgroundColor,
                        'border-color': item.iconColor,
                        'color': item.iconColor
                    });
                    // Update icon if iconClass is specified
                    if (item.iconClass) {
                        var faClass = iconClassMapping[item.iconClass] || 'fa-' + item.iconClass;
                        var icon = btn.find('i');
                        if (icon.length) {
                            icon.removeClass('fa-thumbs-up fa-thumbs-down fa-exclamation-triangle');
                            icon.addClass(faClass);
                        }
                    }
                }
                if (label.length && item.label) {
                    label.text(item.label);
                }
            });
        }

        // Apply operational skills configuration to legend modal
        if (config.operationalSkills) {
            config.operationalSkills.forEach(function (item) {
                var btn = $('.legend-btn-operational[data-skill-level-id="' + item.skillLevelId + '"]');
                var label = $('.legend-label-operational[data-skill-level-id="' + item.skillLevelId + '"]');
                if (btn.length) {
                    btn.css({
                        'background-color': item.backgroundColor,
                        'border-color': item.iconColor,
                        'color': item.iconColor
                    });
                }
                if (label.length && item.label) {
                    label.text(item.label);
                }
            });
        }

        // Apply colors to matrix cells
        matrix.applyLegendToMatrixCells(config);
    },
    applyLegendToMatrixCells: function (config) {
        if (!config) return;

        // Icon class mapping from config iconClass to Font Awesome class
        var iconClassMapping = {
            'thumbsup': 'fa-thumbs-up',
            'thumbsdown': 'fa-thumbs-down',
            'warning': 'fa-exclamation-triangle'
        };

        // Apply mandatory skills colors and icons to matrix cells
        // Mapping: data-value 1 = expired (skillLevelId 3), data-value 2 = masters (skillLevelId 1), data-value 5 = almost expired (skillLevelId 2)
        if (config.mandatorySkills) {
            var mandatoryMapping = { '1': 3, '2': 1, '5': 2 };
            $('[data-popup="thumbs"][data-value]').each(function () {
                var value = $(this).attr('data-value');
                var skillLevelId = mandatoryMapping[value];
                if (skillLevelId) {
                    var item = config.mandatorySkills.find(function (i) { return i.skillLevelId === skillLevelId; });
                    if (item) {
                        $(this).css({
                            'background-color': item.backgroundColor,
                            'border-color': item.iconColor,
                            'color': item.iconColor
                        });
                        // Update icon if iconClass is specified
                        if (item.iconClass) {
                            var faClass = iconClassMapping[item.iconClass] || 'fa-' + item.iconClass;
                            var icon = $(this).find('i');
                            if (icon.length) {
                                icon.removeClass('fa-thumbs-up fa-thumbs-down fa-exclamation-triangle');
                                icon.addClass(faClass);
                            }
                        }
                    }
                }
            });
        }

        // Apply operational skills colors to matrix cells
        // Mapping: data-value 1-5 maps directly to skillLevelId 1-5
        if (config.operationalSkills) {
            $('[data-popup="score"][data-value]').each(function () {
                var value = parseInt($(this).attr('data-value'));
                if (value >= 1 && value <= 5) {
                    var item = config.operationalSkills.find(function (i) { return i.skillLevelId === value; });
                    if (item) {
                        $(this).css({
                            'background-color': item.backgroundColor,
                            'border-color': item.iconColor,
                            'color': item.iconColor
                        });
                    }
                }
            });
        }

        // Apply mandatory skills colors to operational skill expiry indicators
        // These are small icons that appear beside operational skills when they expire
        if (config.mandatorySkills) {
            // Expired indicators (skillLevelId 3)
            $('[title="Operational skill expired"]').each(function () {
                var item = config.mandatorySkills.find(function (i) { return i.skillLevelId === 3; });
                if (item) {
                    $(this).css({
                        'border-color': item.iconColor,
                        'background-color': item.backgroundColor
                    });
                }
            });
            // Almost expired indicators (skillLevelId 2)
            $('[title="Operational almost expired"]').each(function () {
                var item = config.mandatorySkills.find(function (i) { return i.skillLevelId === 2; });
                if (item) {
                    $(this).css({
                        'border-color': item.iconColor,
                        'background-color': item.backgroundColor
                    });
                }
            });
        }
    }
} 

