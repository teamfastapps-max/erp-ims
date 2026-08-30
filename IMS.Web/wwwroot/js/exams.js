/* Exam Module — client-side logic */
(function () {
    "use strict";

    var IMSExamForm = {
        init: function (ajaxUrl) {
            $(document).on("submit", "#examCreateForm, #examEditForm", function (e) {
                e.preventDefault();
                var $form = $(this);
                if (!validateExamForm($form)) return;

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
                            setTimeout(function () { window.location.href = "/Exam/Index"; }, 700);
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

    var IMSMarksEntry = {
        init: function (examId) {
            $("#saveMarksBtn").on("click", function () {
                var $btn = $(this);
                var $form = $("#marksEntryForm");

                $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

                $.ajax({
                    url: "/Exam/SaveMarks",
                    type: "POST",
                    data: $form.serialize(),
                    success: function (r) {
                        if (r.success) {
                            if (window.toastr) toastr.success(r.message || "Marks saved.");
                            setTimeout(function () { window.location.href = "/Exam/Index"; }, 700);
                        } else {
                            if (window.toastr) toastr.error(r.message || "Save failed.");
                            $btn.prop("disabled", false).html('<i class="fa-solid fa-check me-1"></i> Save Marks');
                        }
                    },
                    error: function () {
                        if (window.toastr) toastr.error("Request failed.");
                        $btn.prop("disabled", false).html('<i class="fa-solid fa-check me-1"></i> Save Marks');
                    }
                });
            });
        }
    };

    function validateExamForm($form) {
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

        required("EX_Name", "Exam Name");
        required("EX_Code", "Exam Code");
        required("EX_AcademicYearId", "Academic Year");
        required("EX_CourseId", "Course");
        required("EX_BatchId", "Batch");
        required("EX_ExamTypeId", "Exam Type");
        required("EX_Status", "Status");

        var startDate = $form.find("[name='EX_StartDate']").val();
        var endDate = $form.find("[name='EX_EndDate']").val();
        if (startDate && endDate && new Date(endDate) < new Date(startDate)) {
            $form.find(".field-error[data-for='EX_EndDate']").text("End date cannot be before start date.");
            valid = false;
        }

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
                    url: "/Exam/DeleteExam/" + id,
                    type: "POST",
                    data: { __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                    success: function (r) {
                        if (r.success) {
                            if (window.toastr) toastr.success(r.message || "Deleted.");
                            setTimeout(function () { $("#exam-row-" + id).fadeOut(300, function () { $(this).remove(); }); }, 500);
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

    window.IMSExamForm = IMSExamForm;
    window.IMSMarksEntry = IMSMarksEntry;
    $(function () { wireDelete(); });
})();
