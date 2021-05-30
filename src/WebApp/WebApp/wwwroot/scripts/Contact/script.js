$(function () {

    function setClass(element, className, shouldBeSet) {
        if (shouldBeSet) {
            element.addClass(className);
        } else {
            element.removeClass(className);
        }
    }

    function getMail() { return $.trim($("#formMail").val()); }
    function setMail(text) { return $("#formMail").val($.trim(text)); }
    function setMailHasError(hasError) { setClass($("#formMail").closest(".form-group"), "has-danger", hasError); }
    function getContent() { return $.trim($("#formContent").val()); }
    function setContent(text) { return $("#formContent").val($.trim(text)); }
    function setContentHasError(hasError) { setClass($("#formContent").closest(".form-group"), "has-danger", hasError); }
    function getApproval() { return $("#checkApproval").is(":checked") }
    function setApproval(bool) { $("#checkApproval").prop("checked", bool); }
    function setApprovalHasError(hasError) { setClass($("#checkApproval").closest(".form-group"), "has-danger", hasError); }

    function setSendButtonSending(sending) {
        if (sending) {
            $("#messageSubmit").hide();
            $("#messageSubmitSending").show();
            $("#formMail").prop("disabled", true);
            $("#formContent").prop("disabled", true);
            $("#checkApproval").prop("disabled", true);
        } else {
            $("#messageSubmit").show();
            $("#messageSubmitSending").hide();
            $("#formMail").prop("disabled", false);
            $("#formContent").prop("disabled", false);
            $("#checkApproval").prop("disabled", false);
        }
    }

    function hideAllModes() {
        $("#input").hide();
        $("#sendInProgress").hide();
        $("#sendComplete").hide();
        $("#sendError").hide();
    }
    
    function switchModeToCompleteSuccess() {
        hideAllModes();
        $("#sendComplete").show();
    }

    function switchModeToCompleteError() {
        hideAllModes();
        $("#sendError").show();
    }

    function switchModeToInput() {
        hideAllModes();
        setMailHasError(false);
        setContentHasError(false);
        setApprovalHasError(false);
        setSendButtonSending(false);
        $("#input").show();
    }
    
    function validate() {

        var failed = false;

        if (getMail() === "") {
            setMailHasError(true);
            failed = true;
        } else {
            setMailHasError(false);
        }

        if (getContent() === "") {
            setContentHasError(true);
            failed = true;
        } else {
            setContentHasError(false);
        }

        if (getApproval() === false) {
            setApprovalHasError(true);
            failed = true;
        } else {
            setApprovalHasError(false);
        }

        return !failed;
    }
    
    $(document).ready($("#contactForm").submit(function(submitEvent) {
        submitEvent.preventDefault();

        if (!validate()) {
            return false;
        }

        setSendButtonSending(true);

        $.post("/contact?handler=SendRequest",
            {
                Mail: getMail(),
                Content: getContent(),
                Approval: getApproval(),
                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
            })
            .done(function(e) {
                if (e.success === true) {
                    switchModeToCompleteSuccess();
                } else {
                    switchModeToCompleteError();
                }
            })
            .fail(function() {
                switchModeToCompleteError();
            });
        
        return false;
    }));

    $("#confirmError").click(function() {
        switchModeToInput();
    });

    $("#confirmSuccess").click(function () {
        switchModeToInput();
        setMail("");
        setContent("");
        setApproval(false);
        return false;
    });

    $("#formMail").on("change keyup paste", function() {
        setMailHasError(false);
    });

    $("#formContent").on("change keyup paste", function () {
        setContentHasError(false);
    });

    $("#checkApproval").on("change keyup", function () {
        setApprovalHasError(false);
    });
});