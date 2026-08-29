/* ==========================================================================
   Teacher Module — client-side logic
   Mirrors student.js: no server-side DataAnnotations by design, AJAX
   create/edit/delete with toastr (or the shared fallback toast if toastr
   isn't loaded).
   ========================================================================== */

(function () {
    "use strict";

    if (window.toastr) {
        toastr.options = {
            closeButton: true,
            progressBar: true,
            positionClass: "toast-top-right",
            timeOut: 3500,
            preventDuplicates: true
        };
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
        setTimeout(function () {
            $toast.removeClass("show");
            setTimeout(() => $toast.remove(), 250);
        }, 3500);
    }

    function showSuccess(message) {
        if (window.toastr) toastr.success(message);
        else showFallbackToast("success", message);
    }

    function showError(message) {
        if (window.toastr) toastr.error(message);
        else showFallbackToast("error", message);
    }

    // ------------------------------------------------------------------
    // Validation
    // ------------------------------------------------------------------

    const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const PHONE_REGEX = /^[0-9+\-\s()]{7,20}$/;
    // Mirrors TeacherService.IsStrongPassword on the server: 8+ chars,
    // at least one uppercase, one lowercase, one digit, one special character.
    const STRONG_PASSWORD_REGEX = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).{8,}$/;

    function clearFieldErrors($form) {
        $form.find(".is-invalid").removeClass("is-invalid");
        $form.find(".field-error").text("");
    }

    function setFieldError($field, message) {
        $field.addClass("is-invalid");
        const $errorEl = $field.closest(".form-group, .col-md-3, .col-md-4, .col-md-6, .col-md-12, .col-12")
            .find(".field-error[data-for='" + $field.attr("name") + "']");
        if ($errorEl.length) $errorEl.text(message);
    }

    /**
     * Validates the teacher form. isCreate controls whether Email/Password
     * are required (they're absent from the DOM entirely on Edit, so the
     * jQuery lookups below just no-op there).
     */
    function validateTeacherForm($form, isCreate) {
        clearFieldErrors($form);
        let isValid = true;
        const errors = [];

        function required($field, label) {
            const val = ($field.val() || "").toString().trim();
            if (!val) {
                setFieldError($field, label + " is required.");
                errors.push(label + " is required.");
                isValid = false;
            }
        }

        required($form.find("[name='FirstName']"), "First Name");
        required($form.find("[name='LastName']"), "Last Name");
        required($form.find("[name='CustomRoleId']"), "Role");
        required($form.find("[name='T_BranchId']"), "Branch");
        required($form.find("[name='T_Status']"), "Status");

        const $email = $form.find("[name='Email']");
        const emailVal = ($email.val() || "").toString().trim();
        if (isCreate) required($email, "Email");
        if (emailVal && !EMAIL_REGEX.test(emailVal)) {
            setFieldError($email, "Enter a valid email address.");
            errors.push("Enter a valid email address.");
            isValid = false;
        }

        if (isCreate) {
            const $password = $form.find("[name='Password']");
            const passwordVal = $password.val() || "";
            if (!passwordVal) {
                setFieldError($password, "Password is required.");
                errors.push("Password is required.");
                isValid = false;
            } else if (!STRONG_PASSWORD_REGEX.test(passwordVal)) {
                setFieldError($password, "Min 8 characters, with uppercase, lowercase, a number and a symbol.");
                errors.push("Password does not meet the strength requirements.");
                isValid = false;
            }
        }

        const $phone = $form.find("[name='Phone']");
        const phoneVal = ($phone.val() || "").toString().trim();
        if (phoneVal && !PHONE_REGEX.test(phoneVal)) {
            setFieldError($phone, "Enter a valid phone number.");
            errors.push("Enter a valid phone number.");
            isValid = false;
        }

        if (!isValid) {
            showError(errors[0] || "Please correct the highlighted fields.");
        }

        return isValid;
    }

    // ------------------------------------------------------------------
    // AJAX form submit (Create / Edit)
    // ------------------------------------------------------------------

    function wireAjaxFormSubmit(formSelector, ajaxUrl, isCreate) {
        $(document).on("submit", formSelector, function (e) {
            e.preventDefault();
            const $form = $(this);

            if (!validateTeacherForm($form, isCreate)) return;

            const $submitBtn = $form.find("button[type='submit']");
            const originalText = $submitBtn.html();
            $submitBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

            $.ajax({
                url: ajaxUrl,
                type: "POST",
                data: $form.serialize(),
                success: function (response) {
                    if (response.success) {
                        showSuccess(response.message || "Saved successfully.");
                        setTimeout(function () {
                            window.location.href = "/Teachers/Index";
                        }, 700);
                    } else {
                        showError(response.message || "Something went wrong.");
                        $submitBtn.prop("disabled", false).html(originalText);
                    }
                },
                error: function (xhr) {
                    showError("Request failed: " + (xhr.responseText || xhr.statusText));
                    $submitBtn.prop("disabled", false).html(originalText);
                }
            });
        });
    }

    // ------------------------------------------------------------------
    // AJAX delete (from the Index list)
    // ------------------------------------------------------------------

    function wireAjaxDelete() {
        $(document).on("click", ".confirm-delete-btn", function () {
            const id = $(this).data("id");
            const $modal = $("#deleteModal-" + id);

            $modal.find(".do-delete-btn").off("click").on("click", function () {
                const $btn = $(this);
                $btn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Removing...');

                $.ajax({
                    url: "/Teachers/DeleteTeacher/" + id,
                    type: "POST",
                    data: { __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                    success: function (response) {
                        $modal.modal("hide");
                        if (response.success) {
                            showSuccess(response.message || "Removed successfully.");
                            $("#teacher-row-" + id).fadeOut(300, function () { $(this).remove(); });
                        } else {
                            showError(response.message || "Unable to remove teacher.");
                        }
                    },
                    error: function () {
                        $modal.modal("hide");
                        showError("Request failed. Please try again.");
                    },
                    complete: function () {
                        $btn.prop("disabled", false).html("Remove");
                    }
                });
            });
        });
    }

    // ------------------------------------------------------------------
    // Init
    // ------------------------------------------------------------------

    $(function () {
        wireAjaxFormSubmit("#teacherCreateForm", "/Teachers/AddTeacher", true);
        wireAjaxFormSubmit("#teacherEditForm", "/Teachers/EditTeacher", false);
        wireAjaxDelete();
    });
})();