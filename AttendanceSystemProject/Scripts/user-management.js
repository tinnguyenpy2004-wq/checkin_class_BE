// User Management JavaScript
// This file contains all JavaScript for User Management page

function showToast(message, type) {
    var bg = type === 'success' ? 'bg-success' : (type === 'error' ? 'bg-danger' : 'bg-secondary');
    var id = 't' + Date.now();
    var html = '<div class="toast '+bg+' text-white" id="'+id+'" role="alert" aria-live="assertive" aria-atomic="true" data-delay="2000" style="min-width:240px;">' +
               '  <div class="toast-body">'+ message +'</div>' +
               '</div>';
    $('#toast-container').append(html);
    $('#'+id).toast('show').on('hidden.bs.toast', function(){ $(this).remove(); });
}

$(document).ready(function () {
    var token = $('#__AjaxAntiForgeryForm input[name="__RequestVerificationToken"]').val();
    var userTableBody = $('#user-table-body');
    var userIdToDelete;

    // Create User Button
    $('#createUserBtn').on('click', function() {
        $.get('/Users/_Create', function(data) {
            $('#userModalLabel').text('Create New User');
            $('#userModalBody').html(data);
            $('#userModal').modal('show');
        }).fail(function(xhr, status, error) {
            showToast('Failed to load create form', 'error');
        });
    });

    // Edit User Button
    userTableBody.on('click', '.js-edit-user', function() {
        var userId = $(this).data('id');
        $.get('/Users/_Edit', { id: userId }, function(data) {
            $('#userModalLabel').text('Edit User');
            $('#userModalBody').html(data);
            $('#userModal').modal('show');
        }).fail(function(xhr, status, error) {
            showToast('Failed to load edit form', 'error');
        });
    });

    // View Details Button
    userTableBody.on('click', '.js-view-details', function() {
        var userId = $(this).data('id');
        $('#modal-loader').show();
        $('#modal-content-details').hide();
        
        $.get('/Users/GetUserDetailsJson', { id: userId }, function(data) {
            $('#modalUsername').text(data.username);
            $('#modalFullName').text(data.fullName);
            $('#modalEmail').text(data.email);
            $('#modalPhoneNumber').text(data.phoneNumber || 'N/A');
            $('#modalStudentId').text(data.studentId || 'N/A');
            $('#modalDepartment').text(data.departmentName || 'N/A');
            $('#modalRole').text(data.role);
            $('#modalStatus').text(data.isActive ? 'Active' : 'Locked');
            $('#modalEmailConfirmed').text(data.emailConfirmed ? 'Yes' : 'No');
            $('#modalCreatedDate').text(data.createdDate);
            $('#modalLastLogin').text(data.lastLoginDate || 'Never');
            
            $('#modal-loader').hide();
            $('#modal-content-details').show();
        }).fail(function(xhr, status, error) {
            $('#modal-loader').html('<p class="text-danger">Error loading user details</p>');
        });
    });

    // Delete User Button
    userTableBody.on('click', '.js-delete-user', function() {
        userIdToDelete = $(this).data('id');
        var userName = $(this).data('name');
        $('#deleteUserName').text(userName);
        $('#deleteModal').modal('show');
    });

    // Confirm Delete Button
    $('#confirmDeleteBtn').on('click', function() {
        $.ajax({
            url: '/Users/Delete/' + userIdToDelete,
            type: 'POST',
            data: { __RequestVerificationToken: token },
            success: function (response) {
                if (response.success) {
                    $('#user-row-' + userIdToDelete).fadeOut(300, function () { $(this).remove(); });
                    $('#deleteModal').modal('hide');
                    showToast('User deleted successfully', 'success');
                } else {
                    showToast('Delete failed', 'error');
                }
            },
            error: function(xhr, status, error) {
                showToast('Delete failed', 'error');
            }
        });
    });

    // Lock/Unlock User Button
    userTableBody.on('click', '.js-toggle-lock', function () {
        var button = $(this);
        var userId = button.data('id');
        var currentStatus = button.data('status') === 'true';
        var actionUrl = currentStatus ? '/Users/Lock' : '/Users/Unlock';
        
        if (!confirm(currentStatus ? 'Are you sure you want to lock this user?' : 'Are you sure you want to unlock this user?')) { 
            return; 
        }
        
        $.post(actionUrl, { id: userId, __RequestVerificationToken: token }, function (response) {
            if (response.success) {
                var statusText = $('#status-text-' + userId);
                var lockButtonText = $('#lock-text-' + userId);
                if (currentStatus) {
                    statusText.text('Locked').removeClass('badge-success').addClass('badge-secondary');
                    lockButtonText.text('Unlock');
                    button.removeClass('btn-secondary').addClass('btn-primary').data('status', 'false');
                    showToast('User locked successfully', 'success');
                } else {
                    statusText.text('Active').removeClass('badge-secondary').addClass('badge-success');
                    lockButtonText.text('Lock');
                    button.removeClass('btn-primary').addClass('btn-secondary').data('status', 'true');
                    showToast('User unlocked successfully', 'success');
                }
            } else {
                showToast('Operation failed', 'error');
            }
        }).fail(function(xhr, status, error) {
            showToast('Operation failed', 'error');
        });
    });

    // Handle form submission in modal
    $(document).on('submit', '#userForm', function(e) {
        e.preventDefault();
        var form = $(this);
        var formData = form.serialize();
        var isEdit = form.find('input[name="UserId"]').val() !== '';
        var actionUrl = isEdit ? '/Users/Edit' : '/Users/Create';
        
        $.post(actionUrl, formData, function(response) {
            if (typeof response === 'string') {
                // Server returned HTML (validation errors)
                $('#userModalBody').html(response);
            } else if (response.success) {
                $('#userModal').modal('hide');
                if (isEdit && response.view) {
                    // Update the row if editing
                    $('#user-row-' + response.userId).replaceWith(response.view);
                } else {
                    // Reload page for create
                    location.reload();
                }
                showToast('User saved successfully', 'success');
            } else {
                showToast('Save failed', 'error');
            }
        }).fail(function(xhr, status, error) {
            showToast('Save failed', 'error');
        });
    });

    // Handle Cancel button click
    $(document).on('click', '[data-dismiss="modal"]', function() {
        $('#userModal').modal('hide');
    });

    // Handle modal close button (X)
    $(document).on('click', '.modal .close', function() {
        $(this).closest('.modal').modal('hide');
    });

    // Handle ESC key to close modal
    $(document).on('keydown', function(e) {
        if (e.keyCode === 27) { // ESC key
            $('.modal').modal('hide');
        }
    });

    // Clear form when modal is closed
    $('#userModal').on('hidden.bs.modal', function() {
        $('#userModalBody').empty();
    });
});
