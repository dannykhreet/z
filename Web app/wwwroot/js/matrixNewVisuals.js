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
    skillEdited: false,
    groupEdited: false,
    language: {
        userSkills: "User skills - ",
        expiryInDays: "Expiry in days: ",
        expiryInDaysNotSet: "Expiry in days: not set",
        expiryWarningInDays: "expiry warning in days: ",
        expiryWarningInDaysNotSet: "expiry warning in days: not set",
        certifiedAt: "Certified at",
        confirm: "Confirm"
    },
    init: function (matrixId) {
        matrix.initDisplay();
        matrix.initHandlers();
        if (matrixId != null) {
            matrix.currentMatrixId = parseInt(matrixId);
        }
        matrix.calculateMatrix();
    },
    initDisplay: function () {
        $('[data-containertype="dialog_skilldetails"]').hide();
    },
    initHandlers: function () {
        matrix.initSkillHandlers();
        matrix.initGroupHandlers();

        $('#btnViewModalLegend').on('click', function () {
            $('#MatrixLegendModal').modal('show');
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

            var userSkillModalIsShown = $('#AddChangeSkillBehaviourModal').hasClass('in') || $('#AddChangeSkillBehaviourModal').hasClass('show')
            if (userSkillModalIsShown) {
                matrix.saveSkill();
            }

            matrix.updateMatrix();
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
            $('#dialog_type_2').click();
        });
        $('#btn_new_skill').on('click', function () {
            $('#dialog_skillselection').val('');
            $('[data-containertype="skill_in_matrix_container"]').hide(); //new one
            matrix.showSkillDetails();
            matrix.showEditSkill();
        });
        $('#btn_edit_skill').on('click', function () {
            matrix.showSkillDetails();
            matrix.loadSkillDetails();
        });
        $('#dialog_skillselection').on('change', function () {
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
        $('#skill_in_matrix').on('click', function () {
            matrix.updateSkillMatrix();
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

        $('#dialog_skillname, #dialog_skilldescription, #dialog_goal, #dialog_notification_window_days, #dialog_expiryindays, #modal_assessment_choice, #dialog_validfrom, #dialog_validto').on('change', function () {
            matrix.skillEdited = true;
            //$('[data-containertype="change_skill_buttons"]').show();

        });

        $('#dialog_type_1, #dialog_type_2').on('click', function () {
            matrix.skillEdited = true;
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
        //[{"index":1,"id":1},{"index":2,"id":2}]
        //console.log(items);
        toastr.remove();
        $('body').toggleClass('loaded');
        $(items).each(function (index, item) {
            $.ajax({
                type: "POST",
                url: '/skillsmatrix/' + item.MatrixId + '/skills/changerelation',
                data: JSON.stringify(item),
                success: function (data) {
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    console.log(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            })
        });

        $('#ChangeSkillOrderModal').modal('hide');
        setTimeout(function () {
            window.location.reload();
        }, 2000);

    },
    cleanSkillDetails: function () {
        matrix.skillEdited = false;
        //$('[data-containertype="change_skill_buttons"]').hide();
        $('#dialog_skillname, #dialog_skilldescription, #dialog_goal, #dialog_notification_window_days, #dialog_expiryindays, #dialog_validfrom, #dialog_validto').val('');
        $("#modal_assessment_choice").val($("#modal_assessment_choice option:first").val());
        $('#dialog_type_1, #dialog_type_2').prop('checked', false);
        $('#skill_in_matrix').prop('checked', false);
        $('#modal_assessment_container').hide();
        matrix.showEditSkill();
    },
    loadSkillDetails: function () {
        //todo cleanup code
        let data = $('#dialog_skillselection option:selected').attr('data-store');
        let inMatrix = ($('#dialog_skillselection option:selected').attr('data-inmatrix') === 'true');
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
                $('#dialog_type_1, #dialog_type_2').prop('checked', false);

                if (skillItem.SkillType == 0) {
                    $('#dialog_type_1').prop('checked', true);
                }
                if (skillItem.SkillType == 1) {
                    $('#dialog_type_2').prop('checked', true);
                    $('#modal_assessment_container').show();
                }
                $('#modal_assessment_choice').val(skillItem.SkillAssessmentId);
                $('#dialog_validfrom').val(''); //TODO fix
                $('#dialog_validto').val(''); //TODO fix

                $('#skill_in_matrix').prop('checked', false);
                if (inMatrix) {
                    $('#skill_in_matrix').prop('checked', true);
                }
                $('#skill_in_matrix').attr('data-userskillid', skillItem.Id);
                $('[data-containertype="skill_in_matrix_container"]').show();
            } 
        } 
    },
    saveSkill: function () {
        if (!matrix.skillEdited)
        {
            return;
        }
        $('body').toggleClass('loaded');

        let newSkill = {};
        newSkill.Id = $('#dialog_skillselection').val() !== '' ? parseInt($('#dialog_skillselection').val()) : 0;
        newSkill.Name = $('#dialog_skillname').val();
        newSkill.Description = $('#dialog_skilldescription').val();
        newSkill.IsInMatrix = $('#skill_in_matrix').is(':checked');

        newSkill.ExpiryInDays = $('#dialog_expiryindays').val() == '' ? 0 : parseInt($('#dialog_expiryindays').val());
        newSkill.Goal = $('#dialog_goal').val() == '' ? 0 : parseInt($('#dialog_goal').val());
        newSkill.NotificationWindowInDays = $('#dialog_notification_window_days').val() == '' ? 0 : parseInt($('#dialog_notification_window_days').val());
        if ($('#modal_assessment_choice').val() != '' && $('#modal_assessment_choice').val()!= null) {
            newSkill.SkillAssessmentId = parseInt($('#modal_assessment_choice').val());
        }
        newSkill.SkillType = $('#dialog_type_2').is(':checked') ? 1 : 0;

        if (newSkill.Id > 0) {
            $('#dialog_skillselection option[value="' + newSkill.Id + '"]').remove();
        }

        $.ajax({
            type: "POST",
            url: '/skillsmatrix/' + matrix.currentMatrixId + '/skills/addchange',
            data: JSON.stringify(newSkill),
            async: false,
            success: function (data) {
                $('body').toggleClass('loaded');
                var returnedSkill = JSON.parse(data);
                toastr.success('Skill saved.');

                if (newSkill.Id == 0 || newSkill.IsInMatrix) {
                    //new skill, automatically should be added to matrix, or already a existing skill in matrix.
                    newSkill.Id = returnedSkill.Id;
                    $('#opt_skill_in_matrix').append('<option value="' + returnedSkill.Id + '" data-inmatrix="true">' + (newSkill.SkillType == 0 ? 'Mandatory Skill' : 'Operational Skill') + ' - ' + newSkill.Name + '</option>');
                } else {
                    $('#opt_skill_not_in_matrix').append('<option value="' + newSkill.Id + '" data-inmatrix="false">' + (newSkill.SkillType == 0 ? 'Mandatory Skill' : 'Operational Skill') + ' - ' + newSkill.Name + '</option>');

                }
                $('#dialog_skillselection option[value="' + newSkill.Id + '"]').attr('data-store', JSON.stringify(newSkill));
                $('#dialog_skillselection').val(newSkill.Id);

                matrix.hideSkillDetails();
            },
            error: function (jqXHR, textStatus, errorThrown) {
                $('body').toggleClass('loaded');
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            },
            contentType: "application/json; charset=utf-8"
        });

    },
    updateSkillMatrix: function () {
        if ($('#dialog_skillselection').val() !== '') {
            $('body').toggleClass('loaded');


            let inMatrix = $('#skill_in_matrix').is(':checked');
            let updateSkillMatrixUri = '';
            let relation = {};
            relation.Id = ($('#skill_in_matrix').attr('data-id') == undefined || $('#skill_in_matrix').attr('data-id') == '' ? 0 : parseInt($('#skill_in_matrix').attr('data-id')));
            relation.UserSkillId = parseInt($('#skill_in_matrix').attr('data-userskillid'));
            relation.MatrixId = matrix.currentMatrixId;

            if (inMatrix) {
                updateSkillMatrixUri = '/skillsmatrix/' + matrix.currentMatrixId + '/skills/addrelation';
            } else {
                updateSkillMatrixUri = '/skillsmatrix/' + matrix.currentMatrixId + '/skills/removerelation';
            }

            //TODO enable
            $.ajax({
                type: "POST",
                url: updateSkillMatrixUri,
                data: JSON.stringify(relation),
                success: function (data) {
                    $('body').toggleClass('loaded');
                    toastr.success('Matrix skill change saved.');
                },
                error: function (jqXHR, textStatus, errorThrown) {
                    $('body').toggleClass('loaded');
                    toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                },
                contentType: "application/json; charset=utf-8"
            });

            let option = $('#dialog_skillselection option[value="' + relation.UserSkillId + '"]');
            $('#dialog_skillselection option[value="' + relation.UserSkillId + '"]').remove();
            option.attr('data-inmatrix', inMatrix);
            $(inMatrix ? '#opt_skill_in_matrix' : '#opt_skill_not_in_matrix').append(option);
            $('#dialog_skillselection').val(relation.UserSkillId);

            //temp
            //$('body').toggleClass('loaded');
            //toastr.success('Matrix skill change saved.');
            //matrix.hideSkillDetails(); //saved, so hide inputs.
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
        var searchTerms = searchValue.split(' ');
        if (searchValue != null && searchValue != '') {
            $('[data-containertype="user_choice"]').hide();
            $('[data-containertype="user_choice"]').each(function () {
                searchTerms.forEach(s => {
                    if ($(this).attr('data-lastname').toLowerCase().includes(s.toLowerCase()) || $(this).attr('data-firstname').toLowerCase().includes(s.toLowerCase())) {
                        $(this).show();
                    }
                });
            });
        } else {
            $('[data-containertype="user_choice"]').show();
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
                $('#btnDeleteUserGroup').attr('title', 'This user group cannot be deleted because it is in use in one or more Skills Matrices.');            }
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
                    }
                },
                { text: "No", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
            ]
        });
    },
    showUserSkillValuesModal: function (matrixid, userid, userName) {
        $('#UserSkillValuesModalTitle').html(matrix.language.userSkills + userName);
        $.ajax({
            type: "GET",
            url: '/skillsmatrix/' + matrixid + '/skillvalues/' + userid,
            success: function (uservalues) {
                matrix.currentUserSkillValues = uservalues;
                $.ajax({
                    type: "GET",
                    url: '/skillsmatrix/' + matrixid + '/skills/' + userid,
                    success: function (data) {
                        matrix.currentUserSkills = data;
                        $('#UserSkillValuesModalBodyMandatory').html('');
                        $('#UserSkillValuesModalBodyOperational').html('');

                        $.each(data, function (index, elem) {
                            var expiryInDays = "";
                            var expiryWarningInDays = "";
                            var dateInput = "";
                            var readonly = "";
                            if (elem.ExpiryInDays != undefined && elem.ExpiryInDays != null) {
                                expiryInDays = matrix.language.expiryInDays + elem.ExpiryInDays + ', ';
                            }
                            else {
                                expiryInDays = matrix.language.expiryInDays + ', ';
                            }

                            if (elem.NotificationWindowInDays != undefined && elem.NotificationWindowInDays != null) {
                                expiryWarningInDays = matrix.language.expiryWarningInDays + elem.NotificationWindowInDays;
                            }
                            else {
                                expiryWarningInDays = matrix.language.expiryWarningInDaysNotSet;
                            }
                            var uservalue = uservalues.find(u => u.UserSkillId == elem.UserSkillId);
                            
                            if (elem.SkillType == 0) {
                                readonly = "";
                            }
                            else if (elem.SkillType == 1) {
                                readonly = "readonly";
                            }

                            if (uservalue && !uservalue.ValueDate.startsWith("0001-01-01")) {
                                //date
                                dateInput = '<input type="date" ' + readonly + ' min="1970-01-01" max="2050-01-01" data-matrixid="' + matrixid + '" data-userskillid="' + elem.UserSkillId + '" data-valuetype="userDateTimeChoice" class="form-control" id="valuedate-' + matrixid + '-' + elem.UserSkillId + '" value="' + uservalue.ValueDate.substring(0, 10) + '"> ';
                            }
                            else {
                                //date
                                dateInput = '<input type="date" ' + readonly + ' min="1970-01-01" max="2050-01-01" data-matrixid="' + matrixid + '" data-userskillid="' + elem.UserSkillId + '" class="form-control" id="valuedate-' + matrixid + '-' + elem.UserSkillId + '" data-valuetype="userDateTimeChoice"> ';
                            }

                            //set button disabled by default
                            if (elem.SkillType == 0) {
                                $('#UserSkillValuesModalBodyMandatory').append('<strong>' + elem.Name + '</strong>' +
                                    '<br />' + expiryInDays + expiryWarningInDays + '<br />' +
                                    '<div class="input-group mb-3">' +
                                    '<div class="input-group-prepend">' +
                                    '<span class="input-group-text">' + matrix.language.certifiedAt + '</span>' +
                                    '</div>' +
                                    dateInput +
                                    '<div class="input-group-append">' +
                                    '<button class="btn btn-outline-secondary" disabled id="confirmUserSkillValue-' + matrixid + '-' + elem.UserSkillId + '" onclick="matrix.saveUserSkillValue(' + matrixid + ', ' + elem.UserSkillId + ', ' + userid + ', false)">' + matrix.language.confirm + '</button>' +
                                    '<button class="btn btn-outline-secondary" onclick="matrix.saveUserSkillValue(' + matrixid + ', ' + elem.UserSkillId + ', ' + userid + ', true)"><i class="fas fa-trash"></i></button>' +
                                    '</div>' +
                                    '</div>' +
                                    '<br />');
                            }
                            else if (elem.SkillType == 1) {
                                $('#UserSkillValuesModalBodyOperational').append('<strong>' + elem.Name + '</strong>' +
                                    '<br />' + expiryInDays + expiryWarningInDays + '<br />' +
                                    '<div class="input-group mb-3">' +
                                    '<div class="input-group-prepend">' +
                                    '<span class="input-group-text">' + matrix.language.certifiedAt + '</span>' +
                                    '</div>' +
                                    dateInput +
                                    '<div class="input-group-append">' +
                                    '</div>' +
                                    '</div>' +
                                    '<br />');
                                }

                        });
                        $('#UserSkillValuesModal').modal('show');
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        //console.log('getting user skills failed');
                        toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                    },
                    dataType: 'json',
                    contentType: "application/json; charset=utf-8"
                });
            },
            error: function (jqXHR, textStatus, errorThrown) {
                //console.log('getting user skill values failed');
                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
            },
            dataType: 'json',
            contentType: "application/json; charset=utf-8"
        });
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
        $('[data-containertype="user_choice"]').show();
    },
    updateMatrix: function () {
        document.location.reload();
    },
    saveGroup: function () {
        if (!matrix.groupEdited) {
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

            //console.log(relation);

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
            $(this).find('[data-containertype="result_value"]').text(goal_result);
        });
    },
    calculateOperationalSkills: function () {
        let result = 0;
        $('[data-containertype="operational_skill_row"]').each(function () {
            let goal = parseInt($(this).find('[data-containertype="goal_value"]').text());
            let goal_difference = 0; // $(this).find('[data-containertype="difference_value"]').text();
            let goal_result = 0; // $(this).find('[data-containertype="result_value"]').text();
            let items = $(this).find('[data-containertype="value"] button[data-value]');
            goal_result = 0;
            let userCol = [];
            items.each(function () {
                if ($(this).attr('data-value') != 0) {
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
            //console.log(skillValue);
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
        var skillClass = 'btn circlebtn btn-thumbs-green'; //thumbsup
        var skillIcon = '<i class="fa-solid fa-thumbs-up"></i>';

        if (matrix.currentUserSkills != undefined && matrix.currentUserSkills != null) {
            matrix.currentUserSkills.forEach(userSkill => {
                if (userSkill.UserSkillId == userskillid) {
                    var referenceDate = new Date($('#valuedate-' + skillsmatrixid + '-' + userskillid).val());
                    var now = Date.now();

                    if (userSkill.ExpiryInDays != undefined && userSkill.ExpiryInDays != null) {
                        var expiryDate = new Date(referenceDate.valueOf());
                        expiryDate.setDate(referenceDate.getDate() + userSkill.ExpiryInDays);
                        if (now >= expiryDate) {
                            skillValueScore = 1;
                            skillClass = 'btn circlebtn btn-thumbs-red'; //thumbsdown
                            skillIcon = '<i class="fa-solid fa-thumbs-down"></i>';
                        }
                        else if (userSkill.NotificationWindowInDays != undefined && userSkill.NotificationWindowInDays != null) {
                            //2 when now is smaller than expiry date and now is smaller than expiry date - notification window in days
                            //1 when now is bigger than expiry date
                            //5 when now is smaller than expiry date
                            var notificationDate = new Date(expiryDate.valueOf());
                            notificationDate.setDate(expiryDate.getDate() - userSkill.NotificationWindowInDays);
                            if (now >= notificationDate) {
                                skillValueScore = 5;
                                skillClass = 'btn circlebtn btn-thumbs-orange'; //warning
                                skillIcon = '<i class="fa-solid fa-triangle-exclamation"></i>';
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
            buttons.html(skillIcon);
        });

        matrix.calculateMatrix();
    }
} 

