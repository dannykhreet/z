
var ezgoassessment = {
    language: {
        instructionTitle: '{{assessmenttitle}} - Instructions',
        instructionStepTitle: '{{instructiontitle}} - Instruction steps'
    },
    container: '#contentBody',
    participantContainer: '#participants',
    popUpScoreId: '#popUpScore',
    data: '',
    users: [],
    itemIds: [],
    activeAssessmentId: 0,
    activeItemId: 0,
    firstItemId: 0,
    lastItemId: 0,
    debug: false,
    popUpScore: document.querySelector('.popUpScore'),
    popUpThumbs: document.querySelector('.popUpThumbs'),
    items: document.querySelectorAll('.item'),
    activeBtn: null,
    templateId: 0,
    mediaUrl: '',
    placeholderProfileImageUrl: '',
    placeholderImageUrl: '',
    scoreColors: [
        'btn-clear',
        'btn-red',
        'btn-orangered',
        'btn-orange',
        'btn-orangegreen',
        'btn-green'
    ],

    //TODO: Enable button when drawn on canvas, code snippet to enable button:
    //$('#btnFinish').prop('disabled', false);

    // initialize assessment and set handlers 
    init(dataObject) {
        ezgoassessment.log('ezgoassessment::init');
        $(ezgoassessment.container).on('click', 'div[data-type="instruction"]', ezgoassessment.renderInstructionItems);
        $(ezgoassessment.participantContainer).on('click', 'li[data-type="user"]', ezgoassessment.render);
        $(ezgoassessment.container).on('click', 'button[data-popup="score"]', ezgoassessment.togglePopUp);
        $(ezgoassessment.popUpScoreId).on('click', 'div.item', ezgoassessment.setScore);
        $('#assessmentNavigationContainer').on('click', '#navigateHome', ezgoassessment.renderInstructions);
        $('#assessmentNavigationContainer').on('click', '#navigatePrevious', ezgoassessment.navigatePrevious);
        $('#assessmentNavigationContainer').on('click', '#navigateNext', ezgoassessment.navigateNext);

        ezgoassessment.data = dataObject;
        ezgoassessment.renderParticipants();
        ezgoassessment.render();

        //hide navigation buttons
        $('#navigatePrevious').hide();
        $('#navigateHome').hide();
        $('#navigateNext').hide();

        $(ezgoassessment.participantContainer).on('click', '.removeParticipant', ezgoassessment.removeParticipant)

        ezgoassessment.initCanvasEventHandlers();

        //$('.removeParticipant').on('click', function (e) {
        //    e.preventDefault();
        //    e.stopPropagation();


        //});
    }, //init

    initCanvasEventHandlers() {
        if (signPads) {
            signPads.forEach(s => {
                s.canvas.addEventListener('mouseleave', function (e) {
                    var drawn = true;
                    signPads.forEach(signPad => {
                        if (signPad.isCanvasEmpty()) {
                            drawn = false;
                        }
                    });
                    $('#btnFinish').prop('disabled', !drawn);
                });
            });
        }
    },

    removeParticipant(e) {
        e.preventDefault();
        e.stopPropagation();

        $.fn.dialogue({
            //title: "Alert",
            content: $("<p />").text("Are you sure you want to remove this participant from this ongoing assessment? Current progress will be lost."),
            closeIcon: true,
            buttons: [
                {
                    text: "Yes", id: $.utils.createUUID(), click: function ($modal) {

                        $('body').toggleClass('loaded');
                        $.ajax({
                            type: "POST",
                            url: `/assessment/delete/${$(e.currentTarget).closest('li').data('assessmentid')}`,
                            success: function (data) {
                                $modal.dismiss();
                                toastr.success('Participant removed.. reloading');

                                setTimeout(() => {
                                    window.location.reload();
                                }, 1000)
                            },
                            error: function (jqXHR, textStatus, errorThrown) {
                                $('body').toggleClass('loaded');
                                $modal.dismiss();
                                toastr.error(jqXHR.responseText + ' (' + jqXHR.status + ')');
                                ezgoassessment.log(jqXHR);
                            },
                            contentType: "application/json; charset=utf-8"
                        });
                    }
                },
                { text: "No", id: $.utils.createUUID(), click: function ($modal) { $modal.dismiss(); } }
            ]
        });
    }, //removeParticipant

    render() {
        ezgoassessment.log('ezgoassessment::render');
        ezgoassessment.activeAssessmentId = $(this).data('assessmentid') !== undefined ? $(this).data('assessmentid') : ezgoassessment.activeAssessmentId;
        $(ezgoassessment.participantContainer + ' li').removeClass('active');
        $(ezgoassessment.participantContainer).find('[data-assessmentid="' + ezgoassessment.activeAssessmentId + '"]').addClass('active');

        ezgoassessment.renderInstructions();
        
        ezgomediafetcher.preloadImagesAndVideos();
    }, //render

    renderParticipants() {
        ezgoassessment.log('ezgoassessment::renderParticipants');
        var container = $(participantContainer);
        var firstItem = false
        $(ezgoassessment.data).each(function (index, item) {
            if (!item.IsCompleted) {
                if (!firstItem) {
                    ezgoassessment.activeAssessmentId = item.Id;
                    firstItem = true;
                }
                container.append(
                    participantsTemplate
                        .replaceAll('{{id}}', item.CompletedForId)
                        .replaceAll('{{userid}}', item.CompletedForId)
                        .replaceAll('{{assessmentid}}', item.Id)
                        .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(item.CompletedFor))
                        .replaceAll('{{picture}}', item.CompletedForPicture !== undefined && item.CompletedForPicture !== '' && item.CompletedForPicture !== 'emptyprofile' ? ezgoassessment.mediaUrl + item.CompletedForPicture : ezgoassessment.placeholderProfileImageUrl)
                )
            }
        });
        $(ezgoassessment.participantContainer).append(container);
        $(ezgoassessment.participantContainer).append(`<li class="list-group-item d-flex justify-content-between align-items-center" style="padding: 6px 20px; text-align: center">
            <a href="#" onclick="ezgoassessment.showParticipantsModal()" style="text-align: center; margin-left: 42%; float: left;width: 36px;height: 36px;border: 2px solid #dfdfdf ;border-radius: 100%; margin-right: 7px;font-size: 20px;line-height: 34px;color: #909090;">
            <i class="fa fa-plus fa-lg" style="font-size: 20px;line-height: 34px;color: #93C54B;"></i>
            </a>
            </li>`);
            
        ezgomediafetcher.preloadImagesAndVideos();
    }, //renderParticipants

    showParticipantsModal() {
        $('#executeModal').modal('show');
        $('[data-containertype="user_choice"]').show();
        $(ezgoassessment.data).each(function (index, item) {
            $(`div[data-id="${item.CompletedForId}"]`).hide();
        });
    }, //showParticipantsModal

    renderInstructions() {
        ezgoassessment.log('ezgoassessment::renderInstructions');
        $(ezgoassessment.container).empty();

        //hide navigation buttons
        $('#navigatePrevious').hide();
        $('#navigateHome').hide();
        $('#navigateNext').hide();
        var itmArray = [];
        var template = ezgoassessment.data.filter(obj => {
            return obj.Id === ezgoassessment.activeAssessmentId;
        })[0];

        if (template === undefined) {
            return;
        }

        if (template.IsCompleted === true) {
            return;
        }

        $(template.SkillInstructions).each(function (index, item) {
            itmArray.push(item);
            if (itmArray.length === 3) {
                renderContainer(itmArray, template);
                itmArray.length = 0;
            }
        });
        renderContainer(itmArray, template);
        function renderContainer(items, template) {
            var container = $(itemContainer);
            if (items.length > 0) {
                ezgoassessment.itemIds = [];
            }
            $(items).each(function (index, item) {
                ezgoassessment.log(index, item);

                if (index == 0) {
                    ezgoassessment.firstItemId = item.Id;
                }

                if (index == items.length - 1) {
                    ezgoassessment.lastItemId = item.Id;
                }

                ezgoassessment.itemIds.push(item.Id);
                ezgoassessment.log(ezgoassessment.itemIds);

                container.append(
                    instructionTemplate
                        .replaceAll('{{id}}', item.Id)
                        .replaceAll('{{description}}', GlobalParser.escapeHtmlCharacters(item.Description))
                        .replaceAll('{{picture}}', item.Picture !== undefined && item.Picture !== '' ? ezgoassessment.mediaUrl + item.Picture : ezgoassessment.placeholderImageUrl)
                        .replaceAll('{{classes}}', item.IsCompleted ? 'imgFade' : '')
                        .replaceAll('{{visible}}', item.IsCompleted ? '' : 'd-none')
                        .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(item.Name)));
            });

            $(ezgoassessment.container).append(container);

            ezgoassessment.renderCardHeaders(
                ezgoassessment.language.instructionTitle.replaceAll('{{assessmenttitle}}', template.Name),
                undefined,
                false
            );

        } // renderContainer

        ezgoassessment.renderSignature();
        
        ezgomediafetcher.preloadImagesAndVideos();
    }, //renderInstructions

    renderInstructionItems() {
        ezgoassessment.log('ezgoassessment::renderInstructionItems');
        $(ezgoassessment.container).empty();

        var instructionId = $(this).data('id');
        ezgoassessment.activeItemId = parseInt(instructionId);
        var template = ezgoassessment.data.filter(obj => {
            return obj.Id === ezgoassessment.activeAssessmentId;
        })[0];

        var instruction = template.SkillInstructions.filter(obj => {
            return obj.Id === instructionId;
        })[0];

        ezgoassessment.renderCardHeaders(
            ezgoassessment.language.instructionStepTitle.replaceAll('{{instructiontitle}}', instruction.Name),
            instruction.Description,
            true
        );

        var itmArray = [];
        $(instruction.InstructionItems).each(function (index, item) {
            itmArray.push(item);
            if (itmArray.length === 3) {
                renderContainer(itmArray);
                itmArray.length = 0;
            }
        });
        renderContainer(itmArray);


        function renderContainer(items) {
            var container = $(itemContainer);
            $(items).each(function (index, item) {
                var newDiv = instructionItemTemplatePicture
                    .replaceAll('{{id}}', item.Id)
                    .replaceAll('{{colorclass}}', ezgoassessment.scoreColors[parseInt(item.Score !== undefined ? parseInt(item.Score) : 0)])
                    .replaceAll('{{instructionid}}', instruction.Id)
                    .replaceAll('{{description}}', GlobalParser.escapeHtmlCharacters(item.Description))
                    .replaceAll('{{picture}}', item.Picture !== undefined && item.Picture !== '' ? ezgoassessment.mediaUrl + item.Picture : ezgoassessment.placeholderImageUrl)
                    .replaceAll('{{score}}', item.Score !== undefined ? item.Score : '?')
                    .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(item.Name));
                if (item.Video !== undefined && item.Video !== '') {
                    newDiv = instructionItemTemplateVideo
                        .replaceAll('{{id}}', item.Id)
                        .replaceAll('{{colorclass}}', ezgoassessment.scoreColors[parseInt(item.Score !== undefined ? parseInt(item.Score) : 0)])
                        .replaceAll('{{instructionid}}', instruction.Id)
                        .replaceAll('{{description}}', GlobalParser.escapeHtmlCharacters(item.Description))
                        .replaceAll('{{picture}}', item.VideoThumbnail !== undefined && item.VideoThumbnail !== '' ? ezgoassessment.mediaUrl + item.VideoThumbnail : ezgoassessment.placeholderImageUrl)
                        .replaceAll('{{score}}', item.Score !== undefined ? item.Score : '?')
                        .replaceAll('{{name}}', GlobalParser.escapeHtmlCharacters(item.Name))
                        .replaceAll('{{videoid}}', 'video-'+item.Id)
                        .replaceAll('{{fancy}}', 'items')
                        .replaceAll('{{video}}', item.Video);
                }
                container.append(newDiv);
            })

            $(ezgoassessment.container).append(container);


        }

        ezgoassessment.renderNavigation();
        ezgomediafetcher.preloadImagesAndVideos();
    }, //renderInstructionItems

    renderNavigation() {
        $('#navigateHome').show();

        if (ezgoassessment.activeItemId == ezgoassessment.firstItemId) {
            $('#navigatePrevious').hide();
        }
        else {
            $('#navigatePrevious').show();
        }

        if (ezgoassessment.activeItemId == ezgoassessment.lastItemId) {
            $('#navigateNext').hide();
        }
        else {
            $('#navigateNext').show();
        }

        if (ezgoassessment.firstItemId == ezgoassessment.lastItemId) {
            $('#navigatePrevious').hide();
            $('#navigateNext').hide();
        }
    },

    navigatePrevious() {
        var previousItem = 0;
        for (var i = 0; i < ezgoassessment.itemIds.length; i++) {
            if (ezgoassessment.activeItemId == ezgoassessment.itemIds[i]) {
                ezgoassessment.activeItemId = previousItem;
                break;
            }
            else {
                previousItem = ezgoassessment.itemIds[i];
            }
        }

        ezgoassessment.renderInstructions();
        $('div[data-type="instruction"][data-id="' + ezgoassessment.activeItemId + '"').click();
    },

    navigateNext() {
        var nextItem = 0;
        for (var i = ezgoassessment.itemIds.length; i >= 0; i--) {
            if (ezgoassessment.activeItemId == ezgoassessment.itemIds[i]) {
                ezgoassessment.activeItemId = nextItem;
                break;
            }
            else {
                nextItem = ezgoassessment.itemIds[i];
            }
        }

        ezgoassessment.renderInstructions();
        $('div[data-type="instruction"][data-id="' + ezgoassessment.activeItemId + '"').click();
    },

    renderCardHeaders(name, description, show) {
        if (show) {
            $('#cardHeader').html(name);
            $('#cardDescription').html(description !== undefined ? description : '');
            $('#cardDescription').parent().show();
        }
        else {
            $('#cardHeader').html(name);
            $('#cardDescription').parent().hide();
        }


    },

    renderSignature() {

        var instructionId = $(ezgoassessment.activeBtn).data('instructionid');
        var itemId = $(ezgoassessment.activeBtn).data('id');
        var signBtn = signatureButton.replaceAll('{{title}}', 'Finish').replaceAll('{{description}}', 'Finish this assessment');

        var assessment = ezgoassessment.data.filter(obj => {
            return obj.Id === ezgoassessment.activeAssessmentId;
        })[0];

        var instruction = assessment.SkillInstructions.filter(obj => {
            return obj.Id === instructionId;
        })[0];


        if (assessment.SkillInstructions !== undefined) {
            let instructions = assessment.SkillInstructions.filter(obj => obj['IsCompleted'] === true);
            if (instructions.length === assessment.SkillInstructions.length && assessment.SignatureRequired === false) {
                assessment.IsCompleted = true;
                $(ezgoassessment.container).append(signBtn);
                $('div[data-type="signpad"]').unbind('click');
                $(ezgoassessment.container).on('click', 'div[data-type="signpad"]', ezgoassessment.signAndFinish);
            }
            if (instructions.length === assessment.SkillInstructions.length && assessment.SignatureRequired === true) {
                //assessment.IsCompleted = true;

                if (assessment.SignatureType === 1) {
                    $('#signature1').removeClass('col-6').addClass('col-12');
                    $('#signature2').addClass('d-none');
                    $('#signatures .modal-dialog').removeClass('modal-lg');
                    $('#assessor1').css('marginLeft', '58px');
                    $('#assessor1').html(assessment.Assessor);
                    signBtn = signatureButton.replaceAll('{{title}}', 'Signature').replaceAll('{{description}}', 'Sign this assessment');
                    $('#btnFinish').prop('disabled', false);
                    if (signPads.length === 2) {
                        signPads.pop();
                    }

                }

                if (assessment.SignatureType === 2) {
                    $('#signature1').removeClass('col-12').addClass('col-6');
                    $('#signature2').removeClass('d-none');
                    $('#signatures .modal-dialog').addClass('modal-lg');
                    $('#assessor1').html(assessment.Assessor);
                    $('#assessor2').html(assessment.CompletedFor);
                    signBtn = signatureButton.replaceAll('{{title}}', 'Signature').replaceAll('{{description}}', 'Sign this assessment');

                    //$('#userlist').empty();
                    //$(ezgoassessment.users).each(function (index, user) {
                    //    var user = userlistitem.replaceAll('{{userid}}', user.Id).replaceAll('{{username}}', user.FirstName + ' ' + user.LastName);
                    //    $('#userlist').append(user);
                    //});
                    //$('#userlist').on('click', 'a.dropdown-item', ezgoassessment.selectSecondAssessor);
                }

                $(ezgoassessment.container).append(signBtn);

                $('div[data-type="signpad"]').unbind('click');
                $(ezgoassessment.container).on('click', 'div[data-type="signpad"]', function () {
                    $('#signatures').modal({ backdrop: 'static' });
                    $('#btnFinish').prop('disabled', true);
                });

            }
        }


    },

    selectSecondAssessor() {

        $('#btnSelectUser').html($(this).html());
        $('#btnSelectUser').data('userid', $(this).data('userid'));
        ezgoassessment.log($(this).data('userid'));
        $('#btnFinish').prop('disabled', false);

    },

    log(message) {

        if (ezgoassessment.debug) {
            //console.log(message);
        }

    }, //log

    togglePopUp(e) {
        var popup;
        var x, y;

        e.stopPropagation();

        if (e.pageX || e.pageY) {
            x = e.pageX;
            y = e.pageY;
        }
        else {
            x = e.clientX + document.body.scrollLeft + document.documentElement.scrollLeft;
            y = e.clientY + document.body.scrollTop + document.documentElement.scrollTop;
        }
        ezgoassessment.activeBtn = e.target;

        switch (ezgoassessment.activeBtn.getAttribute('data-popup')) {
            case 'score':
                popup = ezgoassessment.popUpScore;
                break;
            case 'thumbs':
                popup = ezgoassessment.popUpThumbs;
                break;
        }
        //if popup will be displayed too close to the edge of the viewport,
        //make the x location of popup respect the popup width (265px) and how far it is with respect to the right side of the viewport
        if (window.innerWidth - 265 < x) {
            x = x - 265 + (window.innerWidth - x);
        }

        popup.classList.toggle('hidePopUp')
        popup.style.left = x - 40 + 'px';
        popup.style.top = (y + 40) + 'px';
    }, //togglePopup

    setScore(e) {
        e.stopPropagation();
        var activeBtn = ezgoassessment.activeBtn;
        activeBtn.innerHTML = e.target.innerHTML;
        activeBtn.className = 'circlebtn';
        activeBtn.classList.add('itemSelected');
        activeBtn.classList.add(e.target.classList[1]);
        activeBtn.style.backgoundColor = e.target.style.backgroundColor;
        activeBtn = null;
        ezgoassessment.popUpScore.classList.add('hidePopUp')
        ezgoassessment.popUpThumbs.classList.add('hidePopUp')

        ezgoassessment.getInstructionItem(e);
    }, //setScore

    getInstructionItem(e) {
        var timestamp = new Date().toISOString().split('.').shift();
        ezgoassessment.log(timestamp);
        var instructionId = $(ezgoassessment.activeBtn).data('instructionid');
        var itemId = $(ezgoassessment.activeBtn).data('id');

        var assessment = ezgoassessment.data.filter(obj => {
            return obj.Id === ezgoassessment.activeAssessmentId;
        })[0];

        var instruction = assessment.SkillInstructions.filter(obj => {
            return obj.Id === instructionId;
        })[0];

        var instructionItem = instruction.InstructionItems.filter(obj => {
            return obj.Id === itemId;
        })[0];

        instructionItem.Score = parseInt(ezgoassessment.activeBtn.innerHTML);
        instructionItem.IsCompleted = true;
        instructionItem.CompletedAt = timestamp;

        instruction.IsCompleted = false;
        var itemCntr = 0;

        $(instruction.InstructionItems).each(function (index, item) {
            if (item.Score !== undefined && item.Score !== 0) {
                itemCntr += 1;
            }
        });
        instruction.IsCompleted = instruction.InstructionItems.length === itemCntr ? true : false;
        instruction.CompletedAt = timestamp;

        assessment.IsCompleted = false;
        var instrucCntr = 0;
        var mainScore = 0;
        var totalScore = 0;
        $(assessment.SkillInstructions).each(function (index, item) {

            $(item.InstructionItems).each(function (index, instructionitem) {
                if (instructionitem.Score !== undefined && parseInt(instructionitem.Score) !== 0) {
                    totalScore = parseInt(totalScore) + parseInt(instructionitem.Score);
                    ezgoassessment.log(index + ' -> ' + instructionitem.Score);
                }
            });
            ezgoassessment.log('Total for instruction: ' + totalScore);
            item.TotalScore = parseInt(totalScore);
            totalScore = 0;

            if (item.IsCompleted) {
                instrucCntr += 1;
                mainScore = parseInt(mainScore) + parseInt(item.TotalScore);
            }

        });
        assessment.TotalScore = parseInt(mainScore);
        ezgoassessment.updateAssessment();
    },

    async signAndFinish() {
        var instructionId = $(ezgoassessment.activeBtn).data('instructionid');
        var itemId = $(ezgoassessment.activeBtn).data('id');

        var assessment = ezgoassessment.data.filter(obj => {
            return obj.Id === ezgoassessment.activeAssessmentId;
        })[0];

        var instruction = assessment.SkillInstructions.filter(obj => {
            return obj.Id === instructionId;
        })[0];

        if (assessment.SkillInstructions !== undefined) {
            let instructions = assessment.SkillInstructions.filter(obj => obj['IsCompleted'] === true);
            if (instructions.length === assessment.SkillInstructions.length) {


                assessment.IsCompleted = true;
                if (instructions.length === assessment.SkillInstructions.length && assessment.SignatureRequired === true) {

                    const starterPromise = Promise.resolve(null);
                    const log = result => ezgoassessment.log(result);
                    await signPads.reduce(
                        (p, signpad) => p.then(() => ezgoassessment.saveImage(signpad.canvas).then(log)),
                        starterPromise
                    );

                }

                var timestamp = new Date().toISOString().split('.').shift();
                if (assessment.SignatureType === 2) {
                    //assessment.Signatures[1].signedById = parseInt($('#btnSelectUser').data('userid'));
                    //assessment.Signatures[1].signedBy = $('#btnSelectUser').html();
                    assessment.Signatures[1].signedById = assessment.CompletedForId;
                    assessment.Signatures[1].signedBy = assessment.CompletedFor;
                }


                assessment.CompletedAt = timestamp;
                ezgoassessment.updateAssessment();

                $('#signatures').modal('hide');

                $('#participants').empty();

                for (let i = 0; i < signPads.length; i++) {
                    signPads[i].resetCanvas();
                }

                ezgoassessment.activeAssessmentId = null;
                ezgoassessment.activeBtn = null;


                ezgoassessment.renderParticipants();
                ezgoassessment.render();
                //window.location.href = '/assessment/viewer/' + assessment.TemplateId;
            }
        }
    },

    updateAssessment() {

        var assessment = ezgoassessment.data.filter(obj => {
            return obj.Id === ezgoassessment.activeAssessmentId;
        })[0];

        var endpoint = '/assessment/save';

        $.ajax({
            type: "POST",
            url: endpoint,
            data: JSON.stringify(assessment),
            contentType: 'application/json; charset=utf-8',
            success: function (data) {

                let template = JSON.parse(data);
                if (template.IsCompleted === true) {
                    ezgoassessment.render();
                }
            },
            error: function (ex) {
                ezgoassessment.log('error: ' + ex)
            }
        });

    },

    executeAssessment() {

        var participants = $('#participantlist div[data-containertype="user_choice"]');
        ezgoassessment.log(participants);

    },

    async uploadImage(file) {
        const formData = new FormData();
        formData.append('file', file);
        const rawResponse = await fetch(`/assessment/upload`, {
            method: 'POST',
            body: formData
        });
        const response = await rawResponse.json();
        return response
    },

    async saveImage(canvas) {
        const dataUrl = canvas.toDataURL("image/png", 0.5);
        const blob = await ezgoassessment.dataURItoBlob(dataUrl);
        const file = new File([blob], "image.png");
        ezgoassessment.log(file)
        const response = await ezgoassessment.uploadImage(file)
        ezgoassessment.log(response)

        var assessment = ezgoassessment.data.filter(obj => {
            return obj.Id === ezgoassessment.activeAssessmentId;
        })[0];

        assessment.Signatures.push(response);
    },

    async dataURItoBlob(dataURI) {
        let byteString;
        if (dataURI.split(",")[0].indexOf("base64") >= 0)
            byteString = atob(dataURI.split(",")[1]);
        else byteString = unescape(dataURI.split(",")[1]);
        const mimeString = dataURI.split(",")[0].split(":")[1].split(";")[0];
        let ia = new Uint8Array(byteString.length);
        for (let i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }
        return new Blob([ia], { type: mimeString });
    }

}

var participantContainer =
    `<ul class="list-group"></ul>`;

var participantsTemplate =
    `<li class="list-group-item d-flex justify-content-between align-items-center" data-type="user" data-userid="{{id}}" data-assessmentid="{{assessmentid}}" style="padding: 6px 20px;"><span>{{name}}</span>
        <div class="image-parent"><a class="removeParticipant" data-userid="{{userid}}" ><i class="fas fa-user-times fa-2x"></i></a><img class="img-fluid" src="/images/media-loading.gif" data-src="{{picture}}"  /></div>
    </li>`;

var itemContainer =
    `<div class='card-columns p-1'></div>`;

var instructionTemplate =
    `<div class="card" data-type="instruction" data-id="{{id}}">
        <div class="pt-4 {{visible}}" style="position:absolute;text-align:center;width:100%;z-index:2;">
            <span style="display:inline-block; margin:10px;">
                <i class="fa fa-check fa-2x" style="color: #93C54B;font-size:60px;"></i>
            </span>
        </div>
        <img class="card-img-top {{classes}}" src="/images/media-loading.gif" data-src="{{picture}}" alt="Card image" >
        <div class="card-header">
            <h5>{{name}}</h5>
            {{description}}
        </div>
    </div>`;

var instructionItemTemplatePicture =
    `<div class="card" data-type="instructionitem">
        <div style="position:absolute;text-align:right;width:100%;">
            <span style="display:inline-block; margin:10px;">
                <button class="circlebtn {{colorclass}}" type="button" data-popup="score" data-instructionid="{{instructionid}}" data-id="{{id}}">{{score}}</button>
            </span>
        </div>
        <img class="card-img-top" src="/images/media-loading.gif" data-src="{{picture}}" alt="Card image" >
        <div class="card-header">
            <h5>{{name}}</h5>
            {{description}}
        </div>
    </div>`;

var instructionItemTemplateVideo =
    `<div class="card" data-type="instructionitem">
        <div style="position:absolute;text-align:right;width:100%;">
            <span style="display:inline-block; margin:10px;">
                <button class="circlebtn {{colorclass}}" type="button" data-popup="score" data-instructionid="{{instructionid}}" data-id="{{id}}">{{score}}</button>
            </span>
        </div>
        
        <a class="card-link" href="#{{videoid}}" data-fancybox="{{fancy}}" data-caption="{{name}}">
            <img class="card-img-top" src="/images/media-loading.gif" data-src="{{picture}}" alt="Card image" >
        </a>
        <video src="/images/media-loading.mp4" data-src="{{video}}" controls id="{{videoid}}" style="display:none;" >Your browser doesn\'t support HTML5 video tag.</video >
        <div class="card-header">
            <h5>{{name}}</h5>
            {{description}}
        </div>
    </div>`;

var signatureButton =
    `<div class="row p-1">
        <div class="card w-100 bg-success" style="margin-left: 9px;margin-right: 9px;" data-type="signpad">
            <div class="card-header">
                <h5>{{title}}</h5>
                {{description}}
            </div>
        </div>
    </div>`;

var userlistitem =
    `<a class="dropdown-item" data-userid="{{userid}}" href="#">{{username}}</a>`