/* ==========================================================================
   IMS Student Attendance — Interactive Client-Side Engine
   ========================================================================== */

(function ($) {
    "use strict";

    // Toast helpers
    function showNotification(type, message) {
        if (window.toastr) {
            toastr[type](message);
        } else {
            alert(message);
        }
    }

    // -------------------------------------------------------------------------
    // 1. Session Create / Edit Form Handling
    // -------------------------------------------------------------------------
    window.IMSAttendanceSessionForm = {
        init: function (ajaxUrl) {
            $(document).on("submit", "#attendanceCreateForm, #attendanceEditForm", function (e) {
                e.preventDefault();
                var $form = $(this);
                if (!validateSessionForm($form)) return;

                var $submitBtn = $form.find("button[type='submit']");
                var originalHtml = $submitBtn.html();
                $submitBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

                $.ajax({
                    url: ajaxUrl,
                    type: "POST",
                    data: $form.serialize(),
                    success: function (response) {
                        if (response.success) {
                            showNotification("success", response.message || "Attendance session saved successfully.");
                            setTimeout(function () {
                                if (response.id && window.location.pathname.indexOf("Create") !== -1) {
                                    // Option to go directly to marking sheet
                                    window.location.href = "/Attendance/MarkAttendance?id=" + response.id;
                                } else {
                                    window.location.href = "/Attendance/Index";
                                }
                            }, 600);
                        } else {
                            showNotification("error", response.message || "Something went wrong.");
                            $submitBtn.prop("disabled", false).html(originalHtml);
                        }
                    },
                    error: function (xhr) {
                        showNotification("error", "Request failed: " + (xhr.responseText || xhr.statusText));
                        $submitBtn.prop("disabled", false).html(originalHtml);
                    }
                });
            });
        }
    };

    function validateSessionForm($form) {
        $form.find(".is-invalid").removeClass("is-invalid");
        $form.find(".field-error").text("");
        var valid = true;

        function requireField(name, label) {
            var $f = $form.find("[name='" + name + "']");
            if (!$f.val() || !$f.val().toString().trim()) {
                $f.addClass("is-invalid");
                $form.find(".field-error[data-for='" + name + "']").text(label + " is required.");
                valid = false;
            }
        }

        requireField("AS_BranchId", "Branch");
        requireField("AS_BatchId", "Batch");
        requireField("AS_AttendanceDate", "Date");

        return valid;
    }

    // -------------------------------------------------------------------------
    // 2. Interactive Mark Attendance Sheet
    // -------------------------------------------------------------------------
    window.IMSAttendanceMark = {
        init: function (sessionId) {
            var $roster = $("#studentRosterBody");

            // Recalculate metrics
            function recalculateStats() {
                var total = $roster.find(".student-roster-row").length;
                var present = 0, absent = 0, late = 0, onleave = 0;

                $roster.find(".student-roster-row").each(function () {
                    var status = $(this).attr("data-current-status") || "present";
                    status = status.toLowerCase();

                    if (status === "present" || status === "p") present++;
                    else if (status === "absent" || status === "a") absent++;
                    else if (status === "late" || status === "l") late++;
                    else if (status === "onleave" || status === "excused") onleave++;
                });

                $("#statTotalStudents").text(total);
                $("#statPresentCount").text(present);
                $("#statAbsentCount").text(absent);
                $("#statLateCount").text(late);
                $("#statLeaveCount").text(onleave);

                // Update filter chips count
                $("#chipCountAll").text(total);
                $("#chipCountPresent").text(present);
                $("#chipCountAbsent").text(absent);
                $("#chipCountLate").text(late);
                $("#chipCountLeave").text(onleave);

                var rate = total > 0 ? Math.round((present / total) * 100) : 0;
                $("#statAttendanceRate").text(rate + "%");
                $("#statRateProgressBar").css("width", rate + "%");
            }

            // Status Pill Button Click
            $(document).on("click", ".status-pill-btn", function (e) {
                e.preventDefault();
                var $btn = $(this);
                var $row = $btn.closest(".student-roster-row");

                if ($row.attr("data-is-leave") === "true") {
                    showNotification("info", "This student has an approved leave for today.");
                    return;
                }

                var targetStatus = $btn.attr("data-status");
                var $group = $btn.closest(".status-pill-group");

                $group.find(".status-pill-btn").removeClass("active");
                $btn.addClass("active");

                // Update hidden input
                $row.find(".attendance-hidden-status").val(targetStatus);
                $row.attr("data-current-status", targetStatus);

                // Update row styling
                $row.removeClass("row-absent row-late row-onleave");
                if (targetStatus === "absent") $row.addClass("row-absent");
                else if (targetStatus === "late") $row.addClass("row-late");

                recalculateStats();
            });

            // "Mark All Present" Action
            $("#btnMarkAllPresent").on("click", function () {
                $roster.find(".student-roster-row").each(function () {
                    var $row = $(this);
                    if ($row.attr("data-is-leave") === "true") return; // Keep leave intact

                    $row.find(".status-pill-btn").removeClass("active");
                    $row.find(".status-pill-btn[data-status='present']").addClass("active");
                    $row.find(".attendance-hidden-status").val("present");
                    $row.attr("data-current-status", "present");
                    $row.removeClass("row-absent row-late row-onleave");
                });

                recalculateStats();
                showNotification("success", "Marked all active students as Present.");
            });

            // "Mark All Absent" Action
            $("#btnMarkAllAbsent").on("click", function () {
                $roster.find(".student-roster-row").each(function () {
                    var $row = $(this);
                    if ($row.attr("data-is-leave") === "true") return;

                    $row.find(".status-pill-btn").removeClass("active");
                    $row.find(".status-pill-btn[data-status='absent']").addClass("active");
                    $row.find(".attendance-hidden-status").val("absent");
                    $row.attr("data-current-status", "absent");
                    $row.removeClass("row-late row-onleave").addClass("row-absent");
                });

                recalculateStats();
            });

            // Filter Chips
            $(".filter-chip").on("click", function () {
                var $chip = $(this);
                $(".filter-chip").removeClass("active");
                $chip.addClass("active");

                var filter = $chip.attr("data-filter");
                applyRowFilters();
            });

            // Instant Student Search
            $("#studentSearchInput").on("input", function () {
                applyRowFilters();
            });

            function applyRowFilters() {
                var activeFilter = $(".filter-chip.active").attr("data-filter") || "all";
                var searchQuery = ($("#studentSearchInput").val() || "").toLowerCase().trim();

                $roster.find(".student-roster-row").each(function () {
                    var $row = $(this);
                    var status = ($row.attr("data-current-status") || "present").toLowerCase();
                    var name = ($row.attr("data-student-name") || "").toLowerCase();
                    var code = ($row.attr("data-student-code") || "").toLowerCase();

                    var matchesFilter = true;
                    if (activeFilter !== "all") {
                        if (activeFilter === "onleave") {
                            matchesFilter = (status === "onleave" || status === "excused" || $row.attr("data-is-leave") === "true");
                        } else {
                            matchesFilter = (status === activeFilter);
                        }
                    }

                    var matchesSearch = true;
                    if (searchQuery.length > 0) {
                        matchesSearch = (name.indexOf(searchQuery) !== -1 || code.indexOf(searchQuery) !== -1);
                    }

                    if (matchesFilter && matchesSearch) {
                        $row.show();
                    } else {
                        $row.hide();
                    }
                });
            }

            // Save Attendance AJAX
            $("#saveAttendanceBtn, #saveAttendanceBtnBottom").on("click", function () {
                var $btn = $(this);
                var $form = $("#markAttendanceForm");

                var records = [];
                $form.find(".student-roster-row").each(function () {
                    var $row = $(this);
                    var studentId = $row.attr("data-student-id");
                    var status = $row.find(".attendance-hidden-status").val() || "present";
                    var remarks = $row.find(".attendance-remarks-input").val();

                    records.push({
                        StudentId: studentId,
                        Status: status,
                        Remarks: remarks
                    });
                });

                var payload = {
                    SessionId: sessionId,
                    Records: records,
                    __RequestVerificationToken: $form.find("input[name='__RequestVerificationToken']").val()
                };

                var originalHtml = $btn.html();
                $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

                $.ajax({
                    url: "/Attendance/SaveAttendance",
                    type: "POST",
                    data: payload,
                    success: function (res) {
                        if (res.success) {
                            showNotification("success", res.message || "Attendance saved successfully.");
                            setTimeout(function () {
                                window.location.href = "/Attendance/Index";
                            }, 600);
                        } else {
                            showNotification("error", res.message || "Failed to save attendance.");
                            $btn.prop("disabled", false).html(originalHtml);
                        }
                    },
                    error: function () {
                        showNotification("error", "A network error occurred while saving attendance.");
                        $btn.prop("disabled", false).html(originalHtml);
                    }
                });
            });

            // Initial calculation
            recalculateStats();
        }
    };

    // -------------------------------------------------------------------------
    // 3. Index Page Session Delete Handling (Single Unified IMSConfirm Modal)
    // -------------------------------------------------------------------------
    $(document).on("click", ".delete-session-btn", function (e) {
        e.preventDefault();
        var $btn = $(this);
        var sessionId = $btn.data("id");
        var batchName = $btn.data("batch") || "this batch";
        var dateStr = $btn.data("date") || "";
        var token = $("input[name='__RequestVerificationToken']").val();

        var message = "Are you sure you want to delete the attendance session for <strong>" + batchName + "</strong> on <strong>" + dateStr + "</strong>?<br><small class='text-muted'>All marked records for this session will be permanently deleted.</small>";

        function executeDelete(done) {
            $btn.prop("disabled", true);
            $.ajax({
                url: "/Attendance/DeleteSession",
                type: "POST",
                data: { id: sessionId, __RequestVerificationToken: token },
                success: function (r) {
                    if (r.success) {
                        showNotification("success", r.message || "Attendance session deleted successfully.");
                        $("#session-row-" + sessionId).fadeOut(300, function () { $(this).remove(); });
                    } else {
                        showNotification("error", r.message || "Unable to delete session.");
                        $btn.prop("disabled", false);
                    }
                },
                error: function () {
                    showNotification("error", "Error connecting to server. Please try again.");
                    $btn.prop("disabled", false);
                },
                complete: function () {
                    if (typeof done === "function") done();
                }
            });
        }

        if (window.IMSConfirm && window.IMSConfirm.delete) {
            IMSConfirm.delete({
                title: "Delete Attendance Session",
                message: message,
                confirmText: "Delete Permanently",
                onConfirm: function (done) {
                    executeDelete(done);
                }
            });
        } else {
            if (confirm("Are you sure you want to delete the attendance session for " + batchName + " on " + dateStr + "?")) {
                executeDelete();
            }
        }
    });

})(jQuery);
