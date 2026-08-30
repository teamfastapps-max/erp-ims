/* Fees & Payments Module — client-side logic */
(function () {
    "use strict";

    function wireDelete() {
        $(document).on("click", ".confirm-delete-btn", function () {
            var id = $(this).data("id");
            var $modal = $("#deleteModal-" + id);
            $modal.find(".do-delete-btn").off("click").on("click", function () {
                var $btn = $(this);
                $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Deleting...');
                var deleteUrl = "/Fees/DeleteFeeStructure/" + id;
                if (window.location.pathname.indexOf("/Invoices") > -1) deleteUrl = "/Fees/DeleteInvoice/" + id;
                else if (window.location.pathname.indexOf("/Payments") > -1) deleteUrl = "/Fees/DeletePayment/" + id;
                $.ajax({
                    url: deleteUrl, type: "POST",
                    data: { __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                    success: function (r) {
                        if (r.success) { if (window.toastr) toastr.success(r.message || "Deleted.");
                            setTimeout(function () { $("[id$='row-" + id + "']").fadeOut(300, function () { $(this).remove(); }); }, 500);
                        } else { if (window.toastr) toastr.error(r.message || "Delete failed."); }
                        $btn.prop("disabled", false).html("Delete"); $modal.modal("hide");
                    },
                    error: function () { if (window.toastr) toastr.error("Request failed."); $btn.prop("disabled", false).html("Delete"); $modal.modal("hide"); }
                });
            });
        });
    }

    function wireFeeStructureForm() {
        var $form = $("#feeStructureForm");
        if (!$form.length) return;
        $form.on("submit", function (e) {
            e.preventDefault();
            var $btn = $form.find("button[type='submit']");
            $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');
            $.ajax({ url: $form.attr("action"), type: "POST", data: $form.serialize(),
                success: function (r) { if (r.success) { if (window.toastr) toastr.success(r.message); setTimeout(function () { window.location.href = "/Fees/Index"; }, 700); } else { if (window.toastr) toastr.error(r.message); $btn.prop("disabled", false).html('<i class="fa-solid fa-check me-1"></i> Save'); } },
                error: function () { if (window.toastr) toastr.error("Request failed."); $btn.prop("disabled", false).html('<i class="fa-solid fa-check me-1"></i> Save'); }
            });
        });
    }

    $(function () { wireDelete(); wireFeeStructureForm(); });
})();
