/* Expense Module — client-side logic */
(function () {
    "use strict";

    var IMSExpenseForm = {
        init: function (ajaxUrl) {
            $(document).on("submit", "#expenseCreateForm, #expenseEditForm", function (e) {
                e.preventDefault();
                var $form = $(this);
                if (!validateExpenseForm($form)) return;

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
                            setTimeout(function () { window.location.href = "/Expense/Index"; }, 700);
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

    function validateExpenseForm($form) {
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

        required("EXP_BranchId", "Branch");
        required("EXP_ExpenseCategoryId", "Category");
        required("EXP_ExpenseDate", "Expense Date");
        required("EXP_Amount", "Amount");

        var amount = parseFloat($form.find("[name='EXP_Amount']").val());
        if (isNaN(amount) || amount <= 0) {
            $form.find("[name='EXP_Amount']").addClass("is-invalid");
            $form.find(".field-error[data-for='EXP_Amount']").text("Amount must be greater than 0.");
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
                    url: "/Expense/DeleteExpense/" + id,
                    type: "POST",
                    data: { __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                    success: function (r) {
                        if (r.success) {
                            if (window.toastr) toastr.success(r.message || "Deleted.");
                            setTimeout(function () { $("#expense-row-" + id).fadeOut(300, function () { $(this).remove(); }); }, 500);
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

    // Expose
    window.IMSExpenseForm = IMSExpenseForm;
    $(function () { wireDelete(); });
})();
