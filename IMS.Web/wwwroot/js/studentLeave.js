/* Student Leave (admin review) — client-side logic. Same pattern as teacherLeave.js */
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

    function reviewLeave(leaveId, status, rejectionReason, $triggerBtn) {
        const original = $triggerBtn ? $triggerBtn.html() : null;
        if ($triggerBtn) $triggerBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm"></span>');

        $.ajax({
            url: "/StudentLeave/Review",
            type: "POST",
            data: {
                leaveId: leaveId,
                status: status,
                rejectionReason: rejectionReason || "",
                __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
            },
            success: function (res) {
                if (res.success) {
                    showSuccess(res.message || "Updated successfully.");
                    setTimeout(() => window.location.reload(), 600);
                } else {
                    showError(res.message || "Unable to update leave status.");
                    if ($triggerBtn) $triggerBtn.prop("disabled", false).html(original);
                }
            },
            error: function () {
                showError("Request failed. Please try again.");
                if ($triggerBtn) $triggerBtn.prop("disabled", false).html(original);
            }
        });
    }

    function wireApprove() {
        // No confirmation step - matches the Teacher Leave approve pattern
        // (a single click, same as teacherLeave.js's approve-leave-btn).
        $(document).on("click", ".approve-leave-btn", function () {
            reviewLeave($(this).data("id"), "Approved", null, $(this));
        });
    }

    function wireReject() {
        $(document).on("click", ".open-reject-btn", function () {
            $("#rejectLeaveId").val($(this).data("id"));
            $("#rejectStudentName").text($(this).data("name"));
            $("#rejectionReasonInput").val("");
        });

        $(document).on("click", "#confirmRejectBtn", function () {
            const leaveId = $("#rejectLeaveId").val();
            const reason = $("#rejectionReasonInput").val();

            if (!reason || !reason.trim()) {
                showError("A rejection reason is required.");
                return;
            }

            const $btn = $(this);
            const original = $btn.html();
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Rejecting...');

            $.ajax({
                url: "/StudentLeave/Review",
                type: "POST",
                data: {
                    leaveId: leaveId,
                    status: "Rejected",
                    rejectionReason: reason,
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                },
                success: function (res) {
                    $("#rejectModal").modal("hide");
                    if (res.success) {
                        showSuccess(res.message || "Leave rejected.");
                        setTimeout(() => window.location.reload(), 600);
                    } else {
                        showError(res.message || "Unable to reject leave.");
                    }
                },
                error: function () {
                    $("#rejectModal").modal("hide");
                    showError("Request failed. Please try again.");
                },
                complete: function () {
                    $btn.prop("disabled", false).html(original);
                }
            });
        });
    }

    function wireDelete() {
        $(document).on("click", ".delete-leave-btn", function () {
            $("#deleteLeaveId").val($(this).data("id"));
            $("#deleteStudentName").text($(this).data("name") || "this student");
            $("#deleteLeaveModal").modal("show");
        });

        $(document).on("click", "#confirmDeleteLeaveBtn", function () {
            const leaveId = $("#deleteLeaveId").val();
            if (!leaveId) return;

            const $btn = $(this);
            const original = $btn.html();
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Deleting...');

            $.ajax({
                url: "/StudentLeave/Delete",
                type: "POST",
                data: {
                    leaveId: leaveId,
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                },
                success: function (res) {
                    $("#deleteLeaveModal").modal("hide");
                    if (res.success) {
                        showSuccess(res.message || "Leave deleted successfully.");
                        $(`#leave-row-${leaveId}`).fadeOut(300, function () { $(this).remove(); });
                    } else {
                        showError(res.message || "Unable to delete leave record.");
                    }
                },
                error: function () {
                    $("#deleteLeaveModal").modal("hide");
                    showError("Request failed. Please try again.");
                },
                complete: function () {
                    $btn.prop("disabled", false).html(original);
                }
            });
        });
    }

    $(function () {
        wireApprove();
        wireReject();
        wireDelete();
    });
})();