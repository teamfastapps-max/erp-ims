
(function () {
    "use strict";

    if (window.__imsProfileJsLoaded) return;
    window.__imsProfileJsLoaded = true;

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
    // Validation (submit-time only — no live on-change/blur validation)
    // ------------------------------------------------------------------

    const PHONE_REGEX = /^[0-9+\-\s()]{7,20}$/;

    function clearFieldErrors($form) {
        $form.find(".is-invalid").removeClass("is-invalid");
        $form.find(".field-error").text("");
    }

    function setFieldError($field, message) {
        $field.addClass("is-invalid");
        const $errorEl = $field.closest(".col-md-3, .col-md-4, .col-md-6, .col-md-8, .col-md-12, .col-12")
            .find(".field-error[data-for='" + $field.attr("name") + "']");
        if ($errorEl.length) $errorEl.text(message);
    }

    function validateProfileForm($form) {
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
    // Photo select + local-only preview — hero avatar is the only control
    // ------------------------------------------------------------------

    const ALLOWED_IMAGE_TYPES = ["image/jpeg", "image/png", "image/gif", "image/webp"];
    const MAX_PHOTO_BYTES = 2 * 1024 * 1024; // 2MB — must match ProfileController.MaxFileSizeBytes

    let selectedProfilePicFile = null; // holds the File object between select and submit — cleared once actually uploaded

    function heroAvatarInnerHtml(imgSrc) {
        return `<img id="heroAvatarImg" src="${imgSrc}" alt="Profile photo" style="width:100%;height:100%;object-fit:cover;" />
                <div class="position-absolute top-0 start-0 w-100 h-100 d-flex align-items-center justify-content-center"
                     style="background:rgba(0,0,0,0);transition:background .15s ease;" id="heroAvatarHoverOverlay">
                    <i class="bi bi-camera-fill text-white" style="font-size:1.1rem;opacity:0;transition:opacity .15s ease;"></i>
                </div>`;
    }

    $(document).on("click", "#heroAvatarWrap", function () {
        $("#profilePicInput").trigger("click");
    });

    $(document).on("change", "#profilePicInput", function () {
        const $error = $("#profilePicError");
        const file = this.files && this.files[0];
        $error.text("");
        selectedProfilePicFile = null;

        if (!file) return;

        if (ALLOWED_IMAGE_TYPES.indexOf(file.type) < 0) {
            $error.text("Only JPG, PNG, GIF or WEBP images are allowed.");
            $(this).val("");
            return;
        }
        if (file.size > MAX_PHOTO_BYTES) {
            $error.text("Photo must be 2MB or smaller.");
            $(this).val("");
            return;
        }

        selectedProfilePicFile = file;

        const reader = new FileReader();
        reader.onload = function (e) {
            $("#heroAvatarWrap").html(heroAvatarInnerHtml(e.target.result));
        };
        reader.readAsDataURL(file);
    });

    // ------------------------------------------------------------------
    // Submit: upload photo first (if one was picked), then save the profile
    // ------------------------------------------------------------------

    function SaveTenantUserProfile($form, onDone) {
        $.ajax({
            url: "/TenantUserProfile/Update",
            type: "POST",
            data: $form.serialize(),
            success: function (response) {
                if (response.success) {
                    showSuccess(response.message || "Profile updated successfully.");
                    const fullName = ($form.find("[name='FirstName']").val() + " " + $form.find("[name='LastName']").val()).trim();
                    $("#profileDisplayName").text(fullName);
                } else {
                    showError(response.message || "Something went wrong.");
                }
            },
            error: function (xhr) {
                showError("Request failed: " + (xhr.responseText || xhr.statusText));
            },
            complete: onDone
        });
    }

    $(document).on("submit", "#profileForm", function (e) {
        e.preventDefault();
        const $form = $(this);

        if (!validateProfileForm($form)) return;

        const $submitBtn = $form.find("button[type='submit']");
        const originalText = $submitBtn.html();
        $submitBtn.prop("disabled", true).html('<span class="spinner-border spinner-border-sm me-1"></span> Saving...');

        function finish() {
            $submitBtn.prop("disabled", false).html(originalText);
        }

        if (selectedProfilePicFile) {
            const formData = new FormData();
            formData.append("file", selectedProfilePicFile);
            formData.append("__RequestVerificationToken", $form.find("input[name='__RequestVerificationToken']").val());

            $.ajax({
                url: "/TenantUserProfile/UploadPhoto",
                type: "POST",
                data: formData,
                processData: false,
                contentType: false,
                success: function (response) {
                    if (response.success) {
                        $("#ProfilePic").val(response.url);
                        selectedProfilePicFile = null; 
                        SaveTenantUserProfile($form, finish);
                    } else {
                        showError(response.message || "Could not upload the photo.");
                        finish();
                    }
                },
                error: function (xhr) {
                    showError("Photo upload failed: " + (xhr.responseText || xhr.statusText));
                    finish();
                }
            });
        } else {
            SaveTenantUserProfile($form, finish);
        }
    });
})();