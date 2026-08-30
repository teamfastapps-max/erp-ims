/* Attendance Module — client-side logic */
(function () {
    "use strict";

    var IMSAttendanceSessionForm = {
        init: function (ajaxUrl) {
            $(document).on("submit", "#attendanceCreateForm, #attendanceEditForm", function (e) {
                e.preventDefault();
                var $form = $(this);
                if (!validateSessionForm($form)) return;

                var $submitBtn = $form.find("button[type='submit']");
                var originalText = $submitBtn.html();
                $submitBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

                $.ajax({
                    url: ajaxUrl,
                    type: "POST",
                    data: $form.serialize(),
                    success: function (response) {
                        if (response.success) {
                            if (window.toastr) toastr.success(response.message || "Saved successfully.");
                            setTimeout(function () { window.location.href = "/Attendance/Index"; }, 700);
                        } else {
                            if (window.toastr) toastr.error(response.message || "Something went wrong.");
                            $submitBtn.prop("disabled", false).html(originalText);
                        }
                    },
                    error: function (xhr) {
                        if (window.toastr) toastr.error("Request failed: " + (xhr.responseText || xhr.statusText));
                        $submitBtn.prop("disabled", false).html(originalText);
                    }
                });
            });
        }
    };

    var IMSAttendanceMark = {
        init: function (sessionId) {
            $("#saveAttendanceBtn").on("click", function () {
                var $btn = $(this);
                var $form = $("#markAttendanceForm");

                var records = [];
                $form.find("select[name$='.Status']").each(function () {
                    var name = $(this).attr("name");
                    var idx = name.match(/\[(\d+)\]/)[1];
                    var studentId = $form.find("input[name='Records[" + idx + "].StudentId']").val();
                    var status = $(this).val();
                    var remarks = $form.find("input[name='Records[" + idx + "].Remarks']").val();
                    records.push({ StudentId: studentId, Status: status, Remarks: remarks });
                });

                var payload = {
                    SessionId: sessionId,
                    Records: records,
                    __RequestVerificationToken: $form.find("input[name='__RequestVerificationToken']").val()
                };

                $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

                $.ajax({
                    url: "/Attendance/SaveAttendance",
                    type: "POST",
                    data: payload,
                    success: function (r) {
                        if (r.success) {
                            if (window.toastr) toastr.success(r.message || "Attendance saved.");
                            setTimeout(function () { window.location.href = "/Attendance/Index"; }, 700);
                        } else {
                            if (window.toastr) toastr.error(r.message || "Save failed.");
                            $btn.prop("disabled", false).html('<i class="fa-solid fa-check me-1"></i> Save Attendance');
                        }
                    },
                    error: function () {
                        if (window.toastr) toastr.error("Request failed.");
                        $btn.prop("disabled", false).html('<i class="fa-solid fa-check me-1"></i> Save Attendance');
                    }
                });
            });
        }
    };

    function validateSessionForm($form) {
        $form.find(".is-invalid").removeClass("is-invalid");
        $form.find(".field-error").text("");
        var valid = true;

        function required(name, label) {
            var $f = $form.find("[name='" + name + "']");
            if (!$f.val() || !$f.val().toString().trim()) {
                $f.addClass("is-invalid");
                $form.find(".field-error[data-for='" + name + "']").text(label + " is required.");
                valid = false;
            }
        }

        required("AS_BranchId", "Branch");
        required("AS_BatchId", "Batch");
        required("AS_AttendanceDate", "Date");

        if (!valid && window.toastr) toastr.error("Please correct the highlighted fields.");
        return valid;
    }

    function wireDelete() {
        $(document).on("click", ".confirm-delete-btn", function () {
            var id = $(this).data("id");
            var $modal = $("#deleteModal-" + id);
            $modal.find(".do-delete-btn").off("click").on("click", function () {
                var $btn = $(this);
                $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Deleting...');
                $.ajax({
                    url: "/Attendance/DeleteSession/" + id,
                    type: "POST",
                    data: { __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                    success: function (r) {
                        if (r.success) {
                            if (window.toastr) toastr.success(r.message || "Deleted.");
                            setTimeout(function () { $("#session-row-" + id).fadeOut(300, function () { $(this).remove(); }); }, 500);
                        } else {
                            if (window.toastr) toastr.error(r.message || "Delete failed.");
                        }
                        $btn.prop("disabled", false).html("Delete");
                        $modal.modal("hide");
                    },
                    error: function () {
                        if (window.toastr) toastr.error("Request failed.");
                        $btn.prop("disabled", false).html("Delete");
                        $modal.modal("hide");
                    }
                });
            });
        });
    }

    window.IMSAttendanceSessionForm = IMSAttendanceSessionForm;
    window.IMSAttendanceMark = IMSAttendanceMark;
    $(function () { wireDelete(); });
})();
