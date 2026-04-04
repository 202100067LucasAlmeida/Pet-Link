$(document).ready(function () {
    // Mark notification as read
    $('.mark-read-btn').on('click', function () {
        var btn = $(this);
        var notificationId = btn.data('id');
        var notificationItem = btn.closest('.notification-item');

        $.ajax({
            url: '@Url.Action("MarkNotificationAsRead", "Profile")',
            type: 'POST',
            data: { notificationId: notificationId },
            success: function (response) {
                // Remove the "New" badge and border highlight
                notificationItem.find('.badge.bg-primary').remove();
                notificationItem.removeClass('border-primary').addClass('border-secondary');
                btn.remove(); // Remove the mark as read button
            },
            error: function (xhr, status, error) {
                console.error('Error marking notification as read:', error);
            }
        });
    });