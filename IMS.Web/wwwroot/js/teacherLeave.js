/* Teacher Leave — client-side logic. Same toastr/fallback-toast pattern as students.js/teachers.js */
(function () {
    "use strict";

    if (window.toastr) {
        toastr.options = { closeButton: true, progressBar: true, positionClass: "toast-top-right", timeOut: 3500, preventDuplicates: true };
    }

    function ensureFallbackToastContainer() {
        if ($("#imsToastContainer").length) return;
        $("body").append('<div id="imsToastContainer" class="ims-toast-container"></div>');
    }
    function showFallbackToast(type, message) {
        ensureFallbackToastContainer();
        const $toast = $(`<div class="ims-toast ims-toast-${type}">${message}</div>`);
        $("#imsToastContainer").append($toast);
        requestAnimationFrame(() => $toast.addClass("show"));
        setTimeout(() => { $toast.removeClass("show"); setTimeout(() => $toast.remove(), 250); }, 3500);
    }
    function showSuccess(msg) { window.toastr ? toastr.success(msg) : showFallbackToast("success", msg); }
    function showError(msg) { window.toastr ? toastr.error(msg) : showFallbackToast("error", msg); }

    function clearFieldErrors($form) {
        $form.find(".is-invalid").removeClass("is-invalid");
        $form.find(".field-error").text("");
    }
    function setFieldError($field, message) {
        $field.addClass("is-invalid");
        $field.closest(".col-md-6, .col-md-4, .col-12").find(".field-error[data-for='" + $field.attr("name") + "']").text(message);
    }

    function validateApplyForm($form) {
        clearFieldErrors($form);
        let isValid = true;
        const errors = [];

        const $leaveType = $form.find("[name='LeaveType']");
        if (!$leaveType.val()) { setFieldError($leaveType, "Leave type is required."); errors.push("Leave type is required."); isValid = false; }

        const $from = $form.find("[name='FromDate']");
        const $to = $form.find("[name='ToDate']");
        const fromVal = $from.val();
        const toVal = $to.val();

        if (!fromVal) { setFieldError($from, "From date is required."); errors.push("From date is required."); isValid = false; }
        if (!toVal) { setFieldError($to, "To date is required."); errors.push("To date is required."); isValid = false; }

        if (fromVal && toVal && new Date(toVal) < new Date(fromVal)) {
            setFieldError($to, "To date cannot be before From date.");
            errors.push("To date cannot be before From date.");
            isValid = false;
        }

        if (!isValid) showError(errors[0]);
        return isValid;
    }

    function wireApplyForm() {
        $(document).on("submit", "#applyLeaveForm", function (e) {
            e.preventDefault();
            const $form = $(this);
            if (!validateApplyForm($form)) return;

            const $btn = $form.find("button[type='submit']");
            const original = $btn.html();
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Submitting...');

            $.ajax({
                url: "/TeacherLeave/ApplyTeacherLeave",
                type: "POST",
                data: $form.serialize(),
                success: function (res) {
                    if (res.success) {
                        showSuccess(res.message || "Leave request submitted.");
                        setTimeout(() => window.location.reload(), 700);
                    } else {
                        showError(res.message || "Something went wrong.");
                        $btn.prop("disabled", false).html(original);
                    }
                },
                error: function (xhr) {
                    showError("Request failed: " + (xhr.responseText || xhr.statusText));
                    $btn.prop("disabled", false).html(original);
                }
            });
        });
    }

    function wireApproveReject() {
        $(document).on("click", ".approve-leave-btn", function () {
            const id = $(this).data("id");
            const $btn = $(this);
            $btn.prop("disabled", true);

            $.ajax({
                url: "/TeacherLeave/ApproveTeacherLeave",
                type: "POST",
                data: { id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                success: function (res) {
                    if (res.success) { showSuccess(res.message); setTimeout(() => window.location.reload(), 600); }
                    else { showError(res.message); $btn.prop("disabled", false); }
                },
                error: function () { showError("Request failed."); $btn.prop("disabled", false); }
            });
        });

        $(document).on("click", ".do-reject-btn", function () {
            const $modal = $(this).closest(".modal");
            const id = $modal.data("leave-id");
            const reason = $modal.find(".reject-reason-input").val();

            if (!reason || !reason.trim()) {
                showError("A rejection reason is required.");
                return;
            }

            const $btn = $(this);
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Rejecting...');

            $.ajax({
                url: "/TeacherLeave/RejectTeacherLeave",
                type: "POST",
                data: { id: id, rejectionReason: reason, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                success: function (res) {
                    $modal.modal("hide");
                    if (res.success) { showSuccess(res.message); setTimeout(() => window.location.reload(), 600); }
                    else { showError(res.message); }
                },
                error: function () { $modal.modal("hide"); showError("Request failed."); },
                complete: function () { $btn.prop("disabled", false).html("Reject"); }
            });
        });
    }

    function wireCancel() {
        $(document).on("click", ".cancel-leave-btn", function () {
            const id = $(this).data("id");
            const $btn = $(this);
            $btn.prop("disabled", true);

            $.ajax({
                url: "/TeacherLeave/CancelTeacherLeave",
                type: "POST",
                data: { id: id, __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                success: function (res) {
                    if (res.success) { showSuccess(res.message); setTimeout(() => window.location.reload(), 600); }
                    else { showError(res.message); $btn.prop("disabled", false); }
                },
                error: function () { showError("Request failed."); $btn.prop("disabled", false); }
            });
        });
    }

    $(function () {
        wireApplyForm();
        wireApproveReject();
        wireCancel();
    });

    $(document).on("click", ".edit-leave-btn", function () {
        const $btn = $(this);
        $("#editLeaveModal .edit-leave-id").val($btn.data("id"));
        $("#editLeaveModal .edit-leave-type").val($btn.data("type"));
        $("#editLeaveModal .edit-leave-from").val($btn.data("from"));
        $("#editLeaveModal .edit-leave-to").val($btn.data("to"));
        $("#editLeaveModal .edit-leave-reason").val($btn.data("reason"));
    });

    $(document).on("click", ".do-edit-btn", function () {
        const $modal = $("#editLeaveModal");
        const id = $modal.find(".edit-leave-id").val();
        const from = $modal.find(".edit-leave-from").val();
        const to = $modal.find(".edit-leave-to").val();

        if (!from || !to) { showError("From and To dates are required."); return; }
        if (new Date(to) < new Date(from)) { showError("To date cannot be before From date."); return; }

        const $btn = $(this);
        $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

        $.ajax({
            url: "/TeacherLeave/EditTeacherLeave",
            type: "POST",
            data: {
                id: id,
                LeaveType: $modal.find(".edit-leave-type").val(),
                FromDate: from,
                ToDate: to,
                Reason: $modal.find(".edit-leave-reason").val(),
                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
            },
            success: function (res) {
                $modal.modal("hide");
                if (res.success) { showSuccess(res.message); setTimeout(() => window.location.reload(), 600); }
                else { showError(res.message); }
            },
            error: function () { $modal.modal("hide"); showError("Request failed."); },
            complete: function () { $btn.prop("disabled", false).html("Save Changes"); }
        });
    });
})();
