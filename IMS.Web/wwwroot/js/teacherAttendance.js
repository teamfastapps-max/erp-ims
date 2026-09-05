/* Teacher Attendance — client-side logic */
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

    function wireDateChange() {
        $(document).on("change", "#attendanceDatePicker", function () {
            window.location.href = "/TeacherAttendance/Index?date=" + $(this).val();
        });
    }

    function wireMarkRow() {
        $(document).on("click", ".mark-attendance-btn", function () {
            const $row = $(this).closest("tr");
            const teacherId = $row.data("teacher-id");
            const date = $("#attendanceDatePicker").val();
            const status = $row.find(".attendance-status-select").val();
            const remarks = $row.find(".attendance-remarks-input").val();

            if (!status) {
                showError("Select a status before saving.");
                return;
            }

            const $btn = $(this);
            const original = $btn.html();
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm"></span>');

            $.ajax({
                url: "/TeacherAttendance/MarkTeacherAttendance",
                type: "POST",
                data: {
                    teacherId: teacherId,
                    date: date,
                    status: status,
                    remarks: remarks,
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                },
                success: function (res) {
                    if (res.success) {
                        showSuccess(res.message || "Saved.");
                        $row.find(".row-saved-badge").remove();
                        $row.find("td:first").append('<span class="badge bg-success ms-2 row-saved-badge">Saved</span>');
                    } else {
                        showError(res.message || "Unable to save.");
                    }
                },
                error: function () { showError("Request failed. Please try again."); },
                complete: function () { $btn.prop("disabled", false).html(original); }
            });
        });
    }

    // Self-service: teacher marks their own attendance for today only.
    // Date is never sent from the client - the server always uses
    // DateTime.Today for this endpoint, so there's nothing to tamper with here.
    function wireMarkOwn() {
        $(document).on("click", "#markOwnBtn", function () {
            const status = $("#markOwnStatus").val();
            const remarks = $("#markOwnRemarks").val();

            if (!status) {
                showError("Select a status first.");
                return;
            }

            const $btn = $(this);
            const original = $btn.html();
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

            $.ajax({
                url: "/TeacherAttendance/MarkTeacherSelfAttendance",
                type: "POST",
                data: {
                    status: status,
                    remarks: remarks,
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                },
                success: function (res) {
                    if (res.success) {
                        showSuccess(res.message || "Attendance marked.");
                        setTimeout(() => window.location.reload(), 700);
                    } else {
                        showError(res.message || "Unable to save.");
                        $btn.prop("disabled", false).html(original);
                    }
                },
                error: function () {
                    showError("Request failed. Please try again.");
                    $btn.prop("disabled", false).html(original);
                }
            });
        });
    }

    $(function () {
        wireDateChange();
        wireMarkRow();
        wireMarkOwn();
    });
})();
