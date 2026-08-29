/* ==========================================================================
   Student Module — client-side logic
   - Field & form validation (NO server-side DataAnnotations by design)
   - Dynamic add/remove guardian rows
   - Guardian search/autocomplete (link an existing guardian)
   - AJAX create/edit/delete with toastr notifications
   Requires: jQuery, Bootstrap 5 JS, toastr.js (all referenced in _Layout.cshtml)
   ========================================================================== */

(function () {
    "use strict";

    // Toastr default config if the library is present (see _Layout.cshtml)
    if (window.toastr) {
        toastr.options = {
            closeButton: true,
            progressBar: true,
            positionClass: "toast-top-right",
            timeOut: 3500,
            preventDuplicates: true
        };
    }

    /**
     * Lightweight built-in toast used ONLY if toastr.js isn't loaded, so we
     * never fall back to the browser's native alert(). Same top-right
     * placement/behavior as toastr for a consistent look either way.
     */
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
    // Validation helpers
    // ------------------------------------------------------------------

    const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    const PHONE_REGEX = /^[0-9+\-\s()]{7,20}$/;

    function clearFieldErrors($form) {
        $form.find(".is-invalid").removeClass("is-invalid");
        $form.find(".field-error").text("");
    }

    function setFieldError($field, message) {
        $field.addClass("is-invalid");
        const $errorEl = $field.closest(".form-group, .col-md-4, .col-md-6, .col-md-12, .col-12")
            .find(".field-error[data-for='" + $field.attr("name") + "']");
        if ($errorEl.length) $errorEl.text(message);
    }

    /**
     * Validates the student form. Returns true if valid, otherwise shows
     * inline field errors + a toastr summary and returns false.
     */
    function validateStudentForm($form) {
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

        required($form.find("[name='S_BranchId']"), "Branch");
        // S_AdmissionNumber is intentionally NOT required - left blank, the
        // server auto-generates a sequential ADM-{year}-{####} number.
        required($form.find("[name='S_FirstName']"), "First Name");
        required($form.find("[name='S_LastName']"), "Last Name");
        required($form.find("[name='S_Status']"), "Status");

        const $email = $form.find("[name='S_Email']");
        const emailVal = ($email.val() || "").toString().trim();
        if (emailVal && !EMAIL_REGEX.test(emailVal)) {
            setFieldError($email, "Enter a valid email address.");
            errors.push("Enter a valid email address.");
            isValid = false;
        }

        const $phone = $form.find("[name='S_Phone']");
        const phoneVal = ($phone.val() || "").toString().trim();
        if (phoneVal && !PHONE_REGEX.test(phoneVal)) {
            setFieldError($phone, "Enter a valid phone number.");
            errors.push("Enter a valid phone number.");
            isValid = false;
        }

        const $dob = $form.find("[name='S_DateOfBirth']");
        const dobVal = $dob.val();
        if (dobVal && new Date(dobVal) > new Date()) {
            setFieldError($dob, "Date of birth cannot be in the future.");
            errors.push("Date of birth cannot be in the future.");
            isValid = false;
        }

        // Guardian rows: if a row has ANY data entered, First Name + Relation become required
        $form.find(".guardian-row").each(function () {
            const $row = $(this);
            const $first = $row.find("[name$='.FirstName']");
            const $relation = $row.find("[name$='.Relation']");
            const $existingId = $row.find("[name$='.ExistingGuardianId']");

            const hasExisting = !!$existingId.val();
            const firstVal = ($first.val() || "").toString().trim();
            const relationVal = ($relation.val() || "").toString().trim();
            const rowHasAnyInput = hasExisting || firstVal ||
                ($row.find("[name$='.Phone']").val() || "").trim() ||
                ($row.find("[name$='.Email']").val() || "").trim();

            if (rowHasAnyInput) {
                if (!hasExisting && !firstVal) {
                    setFieldError($first, "Guardian first name is required.");
                    errors.push("Guardian first name is required.");
                    isValid = false;
                }
                // Guardians_G requires LastName and Phone (NOT NULL in the DB) -
                // only enforced for brand-new guardians; linked existing ones already satisfy it.
                if (!hasExisting) {
                    const lastVal = ($row.find("[name$='.LastName']").val() || "").toString().trim();
                    if (!lastVal) {
                        setFieldError($row.find("[name$='.LastName']"), "Guardian last name is required.");
                        errors.push("Guardian last name is required.");
                        isValid = false;
                    }
                    const guardianPhoneVal = ($row.find("[name$='.Phone']").val() || "").toString().trim();
                    if (!guardianPhoneVal) {
                        setFieldError($row.find("[name$='.Phone']"), "Guardian phone is required.");
                        errors.push("Guardian phone is required.");
                        isValid = false;
                    } else if (!PHONE_REGEX.test(guardianPhoneVal)) {
                        setFieldError($row.find("[name$='.Phone']"), "Enter a valid phone number.");
                        errors.push("Enter a valid guardian phone number.");
                        isValid = false;
                    }
                }
                if (!relationVal) {
                    setFieldError($relation, "Guardian relation is required.");
                    errors.push("Select a relation for this guardian.");
                    isValid = false;
                }

                const gEmail = ($row.find("[name$='.Email']").val() || "").trim();
                if (gEmail && !EMAIL_REGEX.test(gEmail)) {
                    setFieldError($row.find("[name$='.Email']"), "Enter a valid email.");
                    errors.push("Enter a valid guardian email address.");
                    isValid = false;
                }
            }
        });

        if (!isValid) {
            showError(errors[0] || "Please correct the highlighted fields.");
        }

        return isValid;
    }

    // ------------------------------------------------------------------
    // Dynamic guardian rows
    // ------------------------------------------------------------------

    let guardianRowIndex = 0;

    function guardianRowTemplate(index) {
        return `
        <div class="card mb-3 guardian-row" data-index="${index}">
            <div class="card-body">
                <div class="d-flex justify-content-between align-items-start mb-2">
                    <h6 class="mb-0"><i class="bi bi-person-heart me-1"></i>Guardian ${index + 1}</h6>
                    <button type="button" class="btn btn-sm btn-outline-danger remove-guardian-btn">
                        <i class="bi bi-x-lg"></i>
                    </button>
                </div>

                <input type="hidden" name="Guardians[${index}].ExistingGuardianId" class="existing-guardian-id" value="" />

                <div class="row g-2 mb-2">
                    <div class="col-md-8 position-relative">
                        <label class="form-label small">Search Existing Guardian (optional)</label>
                        <input type="text" class="form-control form-control-sm guardian-search-input"
                               placeholder="Type name, phone or email to find an existing guardian..." autocomplete="off" />
                        <div class="list-group guardian-search-results position-absolute w-100 shadow-sm" style="z-index:1050; display:none;"></div>
                    </div>
                    <div class="col-md-4 d-flex align-items-end">
                        <button type="button" class="btn btn-sm btn-outline-secondary clear-guardian-link-btn w-100">
                            <i class="bi bi-x-circle me-1"></i>Clear Link
                        </button>
                    </div>
                </div>

                <div class="row g-2">
                    <div class="col-md-3">
                        <label class="form-label small">First Name</label>
                        <input type="text" name="Guardians[${index}].FirstName" class="form-control form-control-sm" />
                        <div class="text-danger small field-error" data-for="Guardians[${index}].FirstName"></div>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label small">Last Name</label>
                        <input type="text" name="Guardians[${index}].LastName" class="form-control form-control-sm" />
                        <div class="text-danger small field-error" data-for="Guardians[${index}].LastName"></div>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label small">Phone</label>
                        <input type="text" name="Guardians[${index}].Phone" class="form-control form-control-sm" />
                        <div class="text-danger small field-error" data-for="Guardians[${index}].Phone"></div>
                    </div>
                    <div class="col-md-3">
                        <label class="form-label small">Email</label>
                        <input type="email" name="Guardians[${index}].Email" class="form-control form-control-sm" />
                        <div class="text-danger small field-error" data-for="Guardians[${index}].Email"></div>
                    </div>
                    <div class="col-md-4">
                        <label class="form-label small">Occupation</label>
                        <input type="text" name="Guardians[${index}].Occupation" class="form-control form-control-sm" />
                    </div>
                    <div class="col-md-4">
                        <label class="form-label small">Relation</label>
                        <select name="Guardians[${index}].Relation" class="form-select form-select-sm">
                            <option value="">Select Relation</option>
                            ${window.__relationOptions || ""}
                        </select>
                        <div class="text-danger small field-error" data-for="Guardians[${index}].Relation"></div>
                    </div>
                    <div class="col-md-4 d-flex align-items-center">
                        <div class="form-check mt-4">
                            <input type="checkbox" class="form-check-input" name="Guardians[${index}].IsPrimary" value="true" />
                            <label class="form-check-label small">Primary Contact</label>
                        </div>
                    </div>
                </div>
            </div>
        </div>`;
    }

    function addGuardianRow() {
        const $container = $("#guardiansContainer");
        const $row = $(guardianRowTemplate(guardianRowIndex));
        $container.append($row);
        guardianRowIndex++;
    }

    function removeGuardianRow($row) {
        $row.remove();
    }

    // Populate an existing (previously saved) guardian row from server-rendered data-* attrs
    function hydrateExistingGuardianRows() {
        $("#guardiansContainer .guardian-row[data-existing='true']").each(function () {
            // Rows for Edit are rendered server-side with real indices already;
            // just bump guardianRowIndex so newly-added rows don't collide.
            const idx = parseInt($(this).data("index"), 10);
            if (!isNaN(idx) && idx >= guardianRowIndex) guardianRowIndex = idx + 1;
        });
    }

    // ------------------------------------------------------------------
    // Guardian search/autocomplete
    // ------------------------------------------------------------------

    let searchDebounceTimer = null;

    function wireGuardianSearch($row) {
        const $input = $row.find(".guardian-search-input");
        const $results = $row.find(".guardian-search-results");

        $input.on("input", function () {
            const term = $(this).val().trim();
            clearTimeout(searchDebounceTimer);

            if (term.length < 2) {
                $results.hide().empty();
                return;
            }

            searchDebounceTimer = setTimeout(function () {
                $.get("/Students/SearchGuardians", { term: term })
                    .done(function (data) {
                        $results.empty();
                        if (!data || data.length === 0) {
                            $results.append('<div class="list-group-item text-muted small">No matches found</div>');
                        } else {
                            data.forEach(function (g) {
                                const $item = $(`<button type="button" class="list-group-item list-group-item-action small">
                                    <strong>${g.fullName}</strong><br/>
                                    <span class="text-muted">${g.phone || ""} ${g.email ? "· " + g.email : ""}</span>
                                </button>`);
                                $item.on("click", function () {
                                    applyGuardianSelection($row, g);
                                    $results.hide().empty();
                                });
                                $results.append($item);
                            });
                        }
                        $results.show();
                    })
                    .fail(function () {
                        showError("Could not search guardians. Please try again.");
                    });
            }, 300);
        });

        $(document).on("click", function (e) {
            if (!$(e.target).closest($row.find(".col-md-8")).length) {
                $results.hide();
            }
        });

        $row.find(".clear-guardian-link-btn").on("click", function () {
            $row.find(".existing-guardian-id").val("");
            $row.find("[name$='.FirstName']").val("").prop("readonly", false);
            $row.find("[name$='.LastName']").val("").prop("readonly", false);
            $row.find("[name$='.Phone']").val("").prop("readonly", false);
            $row.find("[name$='.Email']").val("").prop("readonly", false);
            $row.find("[name$='.Occupation']").val("").prop("readonly", false);
            $input.val("");
        });
    }

    function applyGuardianSelection($row, guardian) {
        $row.find(".existing-guardian-id").val(guardian.g_Id || guardian.G_Id);

        const nameParts = (guardian.fullName || "").split(" ");
        $row.find("[name$='.FirstName']").val(nameParts[0] || "").prop("readonly", true);
        $row.find("[name$='.LastName']").val(nameParts.slice(1).join(" ") || "").prop("readonly", true);
        $row.find("[name$='.Phone']").val(guardian.phone || "").prop("readonly", true);
        $row.find("[name$='.Email']").val(guardian.email || "").prop("readonly", true);
        $row.find("[name$='.Occupation']").val(guardian.occupation || "").prop("readonly", true);
        $row.find(".guardian-search-input").val(guardian.fullName || "");
    }

    // ------------------------------------------------------------------
    // AJAX form submit (Create / Edit)
    // ------------------------------------------------------------------

    function wireAjaxFormSubmit(formSelector, ajaxUrl) {
        $(document).on("submit", formSelector, function (e) {
            e.preventDefault();
            const $form = $(this);

            if (!validateStudentForm($form)) return;

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
                            window.location.href = "/Students/Index";
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
                    url: "/Students/DeleteStudent/" + id,
                    type: "POST",
                    data: { __RequestVerificationToken: $("input[name='__RequestVerificationToken']").val() },
                    success: function (response) {
                        $modal.modal("hide");
                        if (response.success) {
                            showSuccess(response.message || "Removed successfully.");
                            $("#student-row-" + id).fadeOut(300, function () { $(this).remove(); });
                        } else {
                            showError(response.message || "Unable to remove student.");
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
        // Guardian row add/remove
        $(document).on("click", "#addGuardianBtn", function () {
            addGuardianRow();
            wireGuardianSearch($("#guardiansContainer .guardian-row").last());
        });

        $(document).on("click", ".remove-guardian-btn", function () {
            removeGuardianRow($(this).closest(".guardian-row"));
        });
        $(document).on("change", "#guardiansContainer input[name$='.IsPrimary']", function () {
            if ($(this).is(":checked")) {
                $("#guardiansContainer input[name$='.IsPrimary']").not(this).prop("checked", false);
            }
        });
        hydrateExistingGuardianRows();
        $("#guardiansContainer .guardian-row").each(function () {
            wireGuardianSearch($(this));
        });

        wireAjaxFormSubmit("#studentCreateForm", "/Students/AddStudent");
        wireAjaxFormSubmit("#studentEditForm", "/Students/EditStudent");
        wireAjaxDelete();
    });
})();