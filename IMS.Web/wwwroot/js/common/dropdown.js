/*!
 * ===========================================================
 * Generic Dropdown Framework
 * Project : IMS
 * Author  : IMS
 * ===========================================================
 */

var Dropdown = (function () {

    /**
     * Load Generic Dropdown
     */
    function load(options) {

        var settings = $.extend({

            element: null,

            entityType: null,

            parentId: null,

            search: null,

            activeOnly: true,

            page: 1,

            pageSize: 100,

            selectedValue: null,

            includeDefault: true,

            defaultText: "-- Select --",

            async: true,

            success: null,

            error: null

        }, options);

        if (!settings.element)
            return;

        $.ajax({

            url: "/Dropdown/GetDropdown",

            type: "GET",

            dataType: "json",

            async: settings.async,

            data: {

                entityType: settings.entityType,

                parentId: settings.parentId,

                search: settings.search,

                activeOnly: settings.activeOnly,

                page: settings.page,

                pageSize: settings.pageSize

            },

            success: function (response) {

                var ddl = $(settings.element);

                ddl.empty();

                if (settings.includeDefault) {

                    ddl.append(
                        $("<option>")
                            .val("")
                            .text(settings.defaultText)
                    );

                }

                if (response.Success) {

                    $.each(response.Data, function (i, item) {

                        ddl.append(

                            $("<option>")

                                .val(item.Value)

                                .text(item.Text)

                                .attr("data-code", item.Code)

                        );

                    });

                    if (settings.selectedValue != null) {

                        ddl.val(settings.selectedValue);

                    }

                    if ($.isFunction(settings.success)) {

                        settings.success(response.Data);

                    }

                }

            },

            error: function (xhr) {

                console.log(xhr);

                if ($.isFunction(settings.error)) {

                    settings.error(xhr);

                }

            }

        });

    }

    /**
     * Cascading Dropdown
     */
    function cascade(options) {

        var parent = $(options.parent);

        parent.off("change.dropdown");

        parent.on("change.dropdown", function () {

            load({

                element: options.child,

                entityType: options.entityType,

                parentId: $(this).val(),

                defaultText: options.defaultText || "-- Select --",

                includeDefault: true,

                selectedValue: null

            });

        });

    }

    /**
     * Clear Dropdown
     */
    function clear(element, defaultText) {

        var ddl = $(element);

        ddl.empty();

        ddl.append(

            $("<option>")

                .val("")

                .text(defaultText || "-- Select --")

        );

    }

    /**
     * Reload Dropdown
     */
    function reload(options) {

        load(options);

    }

    /**
     * Select2 Support
     */
    function loadSelect2(options) {

        load($.extend({}, options, {

            success: function () {

                $(options.element).select2({

                    width: "100%",

                    placeholder: options.defaultText || "-- Select --",

                    allowClear: true

                });

            }

        }));

    }

    return {

        load: load,

        reload: reload,

        clear: clear,

        cascade: cascade,

        loadSelect2: loadSelect2

    };

})();