/* Transfer Certificate (Admin Panel) — client-side logic */
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

    function wireReviewModal() {
        $(document).on("click", ".open-tc-review-btn", function () {
            const id = $(this).data("id");
            const name = $(this).data("name");
            const appno = $(this).data("appno");
            const lib = $(this).data("lib") === true || $(this).data("lib") === "true";
            const fee = $(this).data("fee") === true || $(this).data("fee") === "true";
            const lab = $(this).data("lab") === true || $(this).data("lab") === "true";
            const status = $(this).data("status") || "Submitted";
            const remarks = $(this).data("remarks") || "";

            $("#modalTcId").val(id);
            $("#modalStudentName").text(name);
            $("#modalAppNumber").text(appno);
            $("#modalLibClearance").prop("checked", lib);
            $("#modalFeeClearance").prop("checked", fee);
            $("#modalLabClearance").prop("checked", lab);
            $("#modalStatus").val(status);
            $("#modalRemarks").val(remarks);

            $("#reviewModal").modal("show");
        });

        $(document).on("click", "#saveTCReviewBtn", function () {
            const id = $("#modalTcId").val();
            const lib = $("#modalLibClearance").is(":checked");
            const fee = $("#modalFeeClearance").is(":checked");
            const lab = $("#modalLabClearance").is(":checked");
            const status = $("#modalStatus").val();
            const remarks = $("#modalRemarks").val();

            const $btn = $(this);
            const original = $btn.html();
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

            $.ajax({
                url: "/TransferCertificate/Review",
                type: "POST",
                data: {
                    tcId: id,
                    libraryClearance: lib,
                    feeClearance: fee,
                    labClearance: lab,
                    status: status,
                    remarks: remarks,
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                },
                success: function (res) {
                    $("#reviewModal").modal("hide");
                    if (res.success) {
                        showSuccess(res.message || "Transfer Certificate updated.");
                        setTimeout(() => window.location.reload(), 600);
                    } else {
                        showError(res.message || "Unable to update Transfer Certificate.");
                    }
                },
                error: function () {
                    $("#reviewModal").modal("hide");
                    showError("Request failed. Please try again.");
                },
                complete: function () {
                    $btn.prop("disabled", false).html(original);
                }
            });
        });
    }

    function wireDelete() {
        $(document).on("click", ".delete-tc-btn", function () {
            $("#deleteTcId").val($(this).data("id"));
            $("#deleteStudentName").text($(this).data("name") || "this student");
            $("#deleteTcModal").modal("show");
        });

        $(document).on("click", "#confirmDeleteTcBtn", function () {
            const tcId = $("#deleteTcId").val();
            if (!tcId) return;

            const $btn = $(this);
            const original = $btn.html();
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Deleting...');

            $.ajax({
                url: "/TransferCertificate/Delete",
                type: "POST",
                data: {
                    tcId: tcId,
                    __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val()
                },
                success: function (res) {
                    $("#deleteTcModal").modal("hide");
                    if (res.success) {
                        showSuccess(res.message || "Transfer Certificate application deleted.");
                        $(`#tc-row-${tcId}`).fadeOut(300, function () { $(this).remove(); });
                    } else {
                        showError(res.message || "Unable to delete Transfer Certificate.");
                    }
                },
                error: function () {
                    $("#deleteTcModal").modal("hide");
                    showError("Request failed. Please try again.");
                },
                complete: function () {
                    $btn.prop("disabled", false).html(original);
                }
            });
        });
    }

    $(function () {
        wireReviewModal();
        wireDelete();
    });
})();
