/*!
 * ===========================================================
 * Generic Date Picker Framework
 * Project : IMS
 * Author  : IMS
 *
 * Usage:
 *   IMSDatePicker.init({
 *       input: '#field_AY_StartDate',
 *       format: 'dd/MM/yyyy',       // display format
 *       valueFormat: 'yyyy-MM-dd',   // value sent to server
 *       placeholder: 'DD/MM/YYYY',
 *       allowFutureDates: true,
 *       allowPastDates: true,
 *       minDate: null,
 *       maxDate: null,
 *       required: true,
 *       onChange: null
 *   });
 *
 *   IMSDatePicker.getValue('#field_AY_StartDate')  -> '2026-08-29'
 *   IMSDatePicker.setValue('#field_AY_StartDate', '2026-08-29')
 *   IMSDatePicker.clear('#field_AY_StartDate')
 * ===========================================================
 */

var IMSDatePicker = (function () {

    var _instances = {};

    var MONTHS = [
        'January', 'February', 'March', 'April', 'May', 'June',
        'July', 'August', 'September', 'October', 'November', 'December'
    ];

    var DAYS_SHORT = ['Mo', 'Tu', 'We', 'Th', 'Fr', 'Sa', 'Su'];

    // ---- Helpers ----

    function parseDate(str) {
        if (!str) return null;
        if (str instanceof Date) return isNaN(str.getTime()) ? null : str;

        // Try native Date parser first (handles ISO formats like "2026-08-29T00:00:00",
        // "2026-08-29", "2026-08-29T00:00:00Z", etc.)
        if (typeof str === 'string') {
            var native = new Date(str);
            if (!isNaN(native.getTime())) {
                // Return a date-only copy (no time component) to avoid timezone shifts
                return new Date(native.getFullYear(), native.getMonth(), native.getDate());
            }
        }

        // Manual parsing for dd/MM/yyyy format
        var parts = str.split('/');
        if (parts.length === 3) {
            var d = parseInt(parts[0], 10);
            var m = parseInt(parts[1], 10) - 1;
            var y = parseInt(parts[2], 10);
            if (!isNaN(d) && !isNaN(m) && !isNaN(y)) {
                return new Date(y, m, d);
            }
        }

        return null;
    }

    function formatDate(date, fmt) {
        if (!date) return '';
        var dd = String(date.getDate()).padStart(2, '0');
        var mm = String(date.getMonth() + 1).padStart(2, '0');
        var yyyy = date.getFullYear();
        if (fmt === 'yyyy-MM-dd') return yyyy + '-' + mm + '-' + dd;
        return dd + '/' + mm + '/' + yyyy;
    }

    function today() {
        var d = new Date();
        return new Date(d.getFullYear(), d.getMonth(), d.getDate());
    }

    function isSameDay(a, b) {
        return a && b &&
            a.getFullYear() === b.getFullYear() &&
            a.getMonth() === b.getMonth() &&
            a.getDate() === b.getDate();
    }

    function getDaysInMonth(year, month) {
        return new Date(year, month + 1, 0).getDate();
    }

    function getFirstDayOfMonth(year, month) {
        var day = new Date(year, month, 1).getDay();
        return day === 0 ? 6 : day - 1; // Monday=0
    }

    // ---- Calendar Rendering ----

    function renderCalendar(inst) {
        var cal = inst.calendarEl;
        var year = inst.viewYear;
        var month = inst.viewMonth;

        var html = '';

        // Header: month/year + nav
        html += '<div class="ims-dp-header">';
        html += '<button type="button" class="ims-dp-nav ims-dp-prev-month" aria-label="Previous month">&lsaquo;</button>';
        html += '<span class="ims-dp-month-year">';
        html += '<select class="ims-dp-month-select" aria-label="Month">';
        for (var m = 0; m < 12; m++) {
            html += '<option value="' + m + '"' + (m === month ? ' selected' : '') + '>' + MONTHS[m] + '</option>';
        }
        html += '</select> ';
        html += '<select class="ims-dp-year-select" aria-label="Year">';
        var minYear = inst.minDate ? inst.minDate.getFullYear() : year - 100;
        var maxYear = inst.maxDate ? inst.maxDate.getFullYear() : year + 50;
        for (var y = minYear; y <= maxYear; y++) {
            html += '<option value="' + y + '"' + (y === year ? ' selected' : '') + '>' + y + '</option>';
        }
        html += '</select>';
        html += '</span>';
        html += '<button type="button" class="ims-dp-nav ims-dp-next-month" aria-label="Next month">&rsaquo;</button>';
        html += '</div>';

        // Day headers
        html += '<div class="ims-dp-weekdays">';
        for (var d = 0; d < 7; d++) {
            html += '<span class="ims-dp-weekday">' + DAYS_SHORT[d] + '</span>';
        }
        html += '</div>';

        // Days grid
        var daysInMonth = getDaysInMonth(year, month);
        var firstDay = getFirstDayOfMonth(year, month);
        var selectedDate = inst.selectedDate;
        var todayDate = today();

        html += '<div class="ims-dp-days">';

        // Empty cells before first day
        for (var e = 0; e < firstDay; e++) {
            html += '<span class="ims-dp-day ims-dp-empty"></span>';
        }

        for (var day = 1; day <= daysInMonth; day++) {
            var dayDate = new Date(year, month, day);
            var classes = ['ims-dp-day'];

            // Check if day is disabled
            var disabled = false;
            if (!inst.allowFutureDates && dayDate > todayDate) disabled = true;
            if (!inst.allowPastDates && dayDate < todayDate) disabled = true;
            if (inst.minDate && dayDate < inst.minDate) disabled = true;
            if (inst.maxDate && dayDate > inst.maxDate) disabled = true;

            if (disabled) classes.push('ims-dp-disabled');
            if (isSameDay(dayDate, todayDate)) classes.push('ims-dp-today');
            if (isSameDay(dayDate, selectedDate)) classes.push('ims-dp-selected');

            html += '<button type="button" class="' + classes.join(' ') + '"'
                + ' data-day="' + day + '"'
                + (disabled ? ' tabindex="-1" aria-disabled="true"' : '')
                + ' aria-label="' + day + ' ' + MONTHS[month] + ' ' + year + '"'
                + '>' + day + '</button>';
        }

        html += '</div>';

        // Footer: Today + Clear
        html += '<div class="ims-dp-footer">';
        html += '<button type="button" class="ims-dp-btn ims-dp-today-btn">Today</button>';
        html += '<button type="button" class="ims-dp-btn ims-dp-clear-btn">Clear</button>';
        html += '</div>';

        cal.html(html);

        // Bind calendar events
        cal.find('.ims-dp-day:not(.ims-dp-disabled):not(.ims-dp-empty)').on('click', function () {
            var day = parseInt($(this).data('day'));
            selectDate(inst, new Date(year, month, day));
        });

        cal.find('.ims-dp-prev-month').on('click', function () {
            navigateMonth(inst, -1);
        });

        cal.find('.ims-dp-next-month').on('click', function () {
            navigateMonth(inst, 1);
        });

        cal.find('.ims-dp-month-select').on('change', function () {
            inst.viewMonth = parseInt($(this).val());
            renderCalendar(inst);
        });

        cal.find('.ims-dp-year-select').on('change', function () {
            inst.viewYear = parseInt($(this).val());
            renderCalendar(inst);
        });

        cal.find('.ims-dp-today-btn').on('click', function () {
            selectDate(inst, today());
        });

        cal.find('.ims-dp-clear-btn').on('click', function () {
            clearValue(inst);
        });
    }

    function navigateMonth(inst, delta) {
        inst.viewMonth += delta;
        if (inst.viewMonth > 11) {
            inst.viewMonth = 0;
            inst.viewYear++;
        } else if (inst.viewMonth < 0) {
            inst.viewMonth = 11;
            inst.viewYear--;
        }
        renderCalendar(inst);
    }

    function selectDate(inst, date) {
        inst.selectedDate = date;
        inst.inputEl.val(formatDate(date, inst.valueFormat));
        inst.displayEl.text(formatDate(date, inst.format));
        inst.displayEl.removeClass('ims-dp-placeholder');
        hideCalendar(inst);
        inst.inputEl.trigger('change');
        if (typeof inst.onChange === 'function') {
            inst.onChange(formatDate(date, inst.valueFormat), date);
        }
    }

    function clearValue(inst) {
        inst.selectedDate = null;
        inst.inputEl.val('');
        inst.displayEl.text(inst.placeholder);
        inst.displayEl.addClass('ims-dp-placeholder');
        hideCalendar(inst);
        inst.inputEl.trigger('change');
        if (typeof inst.onChange === 'function') {
            inst.onChange('', null);
        }
    }

    function showCalendar(inst) {
        // Close any other open calendars
        Object.keys(_instances).forEach(function (key) {
            if (key !== inst.inputId) hideCalendar(_instances[key]);
        });

        inst.calendarEl.addClass('ims-dp-open');
        inst.isOpen = true;

        // Set initial view to selected date or today
        if (inst.selectedDate) {
            inst.viewYear = inst.selectedDate.getFullYear();
            inst.viewMonth = inst.selectedDate.getMonth();
        } else {
            var t = today();
            inst.viewYear = t.getFullYear();
            inst.viewMonth = t.getMonth();
        }

        renderCalendar(inst);

        // Position calendar below input
        var inputRect = inst.wrapperEl[0].getBoundingClientRect();
        inst.calendarEl.css({
            'top': '100%',
            'left': '0',
            'right': 'auto'
        });
    }

    function hideCalendar(inst) {
        inst.calendarEl.removeClass('ims-dp-open');
        inst.isOpen = false;
    }

    function toggleCalendar(inst) {
        if (inst.isOpen) {
            hideCalendar(inst);
        } else {
            showCalendar(inst);
        }
    }

    // ---- Public API ----

    function init(options) {
        var settings = $.extend({
            input: null,
            format: 'dd/MM/yyyy',
            valueFormat: 'yyyy-MM-dd',
            placeholder: 'DD/MM/YYYY',
            allowFutureDates: true,
            allowPastDates: true,
            minDate: null,
            maxDate: null,
            required: false,
            onChange: null
        }, options);

        var $input = $(settings.input);
        if (!$input.length) return;

        var inputId = $input.attr('id') || $input.data('column') || 'dp_' + Math.random().toString(36).substr(2, 9);

        // Build DOM wrapper
        var $wrapper = $('<div class="ims-datepicker-wrapper"></div>');
        var $display = $('<span class="ims-dp-display"></span>');
        var $icon = $('<button type="button" class="ims-dp-icon" aria-label="Open date picker"><i class="fa fa-calendar"></i></button>');
        var $calendar = $('<div class="ims-dp-calendar" role="dialog" aria-label="Date picker"></div>');
        var $error = $('<div class="ims-dp-error" role="alert"></div>');

        $input.attr('type', 'hidden');
        $input.addClass('master-field');
        $input.before($wrapper);
        $wrapper.append($input).append($display).append($icon).append($calendar).append($error);

        // Parse initial value
        var initialDate = null;
        var initialValue = $input.val();
        if (initialValue) {
            initialDate = parseDate(initialValue);
            if (!initialDate) {
                // Try parsing yyyy-MM-dd format
                initialDate = parseDate(initialValue);
            }
        }

        // Parse min/max dates
        var minDate = settings.minDate ? parseDate(settings.minDate) : null;
        var maxDate = settings.maxDate ? parseDate(settings.maxDate) : null;

        var inst = {
            inputId: inputId,
            inputEl: $input,
            wrapperEl: $wrapper,
            displayEl: $display,
            calendarEl: $calendar,
            errorEl: $error,
            iconEl: $icon,
            format: settings.format,
            valueFormat: settings.valueFormat,
            placeholder: settings.placeholder,
            allowFutureDates: settings.allowFutureDates,
            allowPastDates: settings.allowPastDates,
            minDate: minDate,
            maxDate: maxDate,
            required: settings.required,
            onChange: settings.onChange,
            selectedDate: initialDate,
            viewYear: initialDate ? initialDate.getFullYear() : today().getFullYear(),
            viewMonth: initialDate ? initialDate.getMonth() : today().getMonth(),
            isOpen: false
        };

        _instances[inputId] = inst;

        // Set initial display
        if (initialDate) {
            $display.text(formatDate(initialDate, settings.format));
            $display.removeClass('ims-dp-placeholder');
        } else {
            $display.text(settings.placeholder);
            $display.addClass('ims-dp-placeholder');
        }

        // Events
        $display.on('click', function () {
            toggleCalendar(inst);
        });

        $icon.on('click', function (e) {
            e.stopPropagation();
            toggleCalendar(inst);
        });

        // Keyboard support on display
        $display.on('keydown', function (e) {
            if (e.key === 'Enter' || e.key === ' ') {
                e.preventDefault();
                toggleCalendar(inst);
            } else if (e.key === 'Escape') {
                hideCalendar(inst);
            } else if (e.key === 'ArrowLeft') {
                e.preventDefault();
                if (!inst.isOpen) showCalendar(inst);
                navigateMonth(inst, -1);
            } else if (e.key === 'ArrowRight') {
                e.preventDefault();
                if (!inst.isOpen) showCalendar(inst);
                navigateMonth(inst, 1);
            }
        });

        // Close on outside click
        $(document).on('click.imsdp_' + inputId, function (e) {
            if (inst.isOpen && !$wrapper[0].contains(e.target)) {
                hideCalendar(inst);
            }
        });

        // Prevent calendar clicks from closing
        $calendar.on('click', function (e) {
            e.stopPropagation();
        });

        // Set aria attributes
        $display.attr({
            'role': 'combobox',
            'aria-expanded': 'false',
            'aria-haspopup': 'dialog',
            'tabindex': '0'
        });
    }

    function getValue(inputSelector) {
        var inst = _instances[typeof inputSelector === 'string'
            ? inputSelector.replace('#', '')
            : inputSelector];
        return inst ? inst.inputEl.val() : '';
    }

    function setValue(inputSelector, dateStr) {
        var key = typeof inputSelector === 'string'
            ? inputSelector.replace('#', '')
            : inputSelector;
        var inst = _instances[key];
        if (!inst) return;

        var date = parseDate(dateStr);
        if (date) {
            selectDate(inst, date);
        } else {
            clearValue(inst);
        }
    }

    function clear(inputSelector) {
        var key = typeof inputSelector === 'string'
            ? inputSelector.replace('#', '')
            : inputSelector;
        var inst = _instances[key];
        if (inst) clearValue(inst);
    }

    function showError(inputSelector, message) {
        var key = typeof inputSelector === 'string'
            ? inputSelector.replace('#', '')
            : inputSelector;
        var inst = _instances[key];
        if (inst) {
            inst.errorEl.text(message).show();
            inst.wrapperEl.addClass('ims-dp-has-error');
        }
    }

    function clearError(inputSelector) {
        var key = typeof inputSelector === 'string'
            ? inputSelector.replace('#', '')
            : inputSelector;
        var inst = _instances[key];
        if (inst) {
            inst.errorEl.text('').hide();
            inst.wrapperEl.removeClass('ims-dp-has-error');
        }
    }

    function destroy(inputSelector) {
        var key = typeof inputSelector === 'string'
            ? inputSelector.replace('#', '')
            : inputSelector;
        var inst = _instances[key];
        if (inst) {
            hideCalendar(inst);
            $(document).off('click.imsdp_' + key);
            inst.wrapperEl.find('.ims-dp-display').off();
            inst.wrapperEl.find('.ims-dp-icon').off();
            inst.inputEl.removeClass('master-field').attr('type', 'hidden');
            inst.wrapperEl.find('.ims-dp-display, .ims-dp-icon, .ims-dp-calendar, .ims-dp-error').remove();
            delete _instances[key];
        }
    }

    function initAll() {
        $('.ims-datepicker-input').each(function () {
            var $el = $(this);
            if (_instances[$el.attr('id') || $el.data('column')]) return;

            init({
                input: '#' + ($el.attr('id') || $el.data('column')),
                format: $el.data('format') || 'dd/MM/yyyy',
                valueFormat: $el.data('value-format') || 'yyyy-MM-dd',
                placeholder: $el.data('placeholder') || 'DD/MM/YYYY',
                allowFutureDates: $el.data('allow-future') !== false,
                allowPastDates: $el.data('allow-past') !== false,
                minDate: $el.data('min-date') || null,
                maxDate: $el.data('max-date') || null,
                required: $el.data('required') === true || $el.data('required') === 'true'
            });
        });
    }

    return {
        init: init,
        getValue: getValue,
        setValue: setValue,
        clear: clear,
        showError: showError,
        clearError: clearError,
        destroy: destroy,
        initAll: initAll,
        formatDate: formatDate,
        parseDate: parseDate
    };

})();
