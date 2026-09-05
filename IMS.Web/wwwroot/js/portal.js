/* ==========================================================================
   ERP-IMS: Student & Guardian Portal Client Scripts (Area: StudentPortal)
   ========================================================================== */

(function ($) {
    'use strict';

    // 1. Multi-Ward / Sibling Switcher
    window.switchActiveStudent = function (studentId) {
        if (!studentId) return;

        var form = document.getElementById('switchWardForm');
        if (form) {
            document.getElementById('switchStudentIdInput').value = studentId;
            form.submit();
        } else {
            var token = $('input[name="__RequestVerificationToken"]').first().val();
            $.ajax({
                url: '/StudentPortal/Auth/SwitchStudent',
                type: 'POST',
                data: {
                    __RequestVerificationToken: token,
                    studentId: studentId
                },
                success: function () {
                    window.location.reload();
                },
                error: function () {
                    if (typeof toastr !== 'undefined') {
                        toastr.error('Failed to switch active student.');
                    } else {
                        alert('Failed to switch student.');
                    }
                }
            });
        }
    };

    // 2. Homework / Home Task Submission Modal Handler
    window.openHomeworkModal = function (taskId, title, maxMarks) {
        $('#hwTaskId').val(taskId);
        $('#hwTaskTitle').text(title);
        $('#hwMaxMarks').text(maxMarks ? 'Max Marks: ' + maxMarks : '');
        $('#hwContent').val('');
        $('#hwAttachmentUrl').val('');
        var modalEl = document.getElementById('homeworkSubmitModal');
        if (modalEl) {
            var modal = new bootstrap.Modal(modalEl);
            modal.show();
        }
    };

    window.submitHomeworkAjax = function () {
        var taskId = $('#hwTaskId').val();
        var content = $('#hwContent').val();
        var attachmentUrl = $('#hwAttachmentUrl').val();

        if (!content && !attachmentUrl) {
            if (typeof toastr !== 'undefined') toastr.warning('Please enter submission details or an attachment link.');
            return;
        }

        var btn = $('#btnSubmitHw');
        btn.prop('disabled', true).text('Submitting...');

        var token = $('input[name="__RequestVerificationToken"]').first().val();

        $.ajax({
            url: '/StudentPortal/Tasks/SubmitHomeTask',
            type: 'POST',
            data: {
                __RequestVerificationToken: token,
                taskId: taskId,
                content: content,
                attachmentUrl: attachmentUrl
            },
            success: function (res) {
                btn.prop('disabled', false).text('Submit Task');
                if (res.success) {
                    if (typeof toastr !== 'undefined') toastr.success(res.message);
                    var modalEl = document.getElementById('homeworkSubmitModal');
                    if (modalEl) {
                        bootstrap.Modal.getInstance(modalEl)?.hide();
                    }
                    setTimeout(function () { location.reload(); }, 600);
                } else {
                    if (typeof toastr !== 'undefined') toastr.error(res.message || 'Submission failed.');
                }
            },
            error: function () {
                btn.prop('disabled', false).text('Submit Task');
                if (typeof toastr !== 'undefined') toastr.error('Submission failed. Please check connection.');
            }
        });
    };

    // 3. Print Helper
    window.printDocument = function () {
        window.print();
    };

})(jQuery);
