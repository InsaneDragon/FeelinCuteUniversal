function handleStatusChange(purchaseId) {
    var selectedStatus = document.getElementById("orderStatus").value;
    $.ajax({
        url: '/Admin/ChangePurchasePackageStatus',
        type: 'PUT',
        data: {
            status: selectedStatus, // Replace with the selected status
            packageId: purchaseId // Replace with the actual order ID
        },
        success: function (response) {
            console.log('Status change successful:', response);
        },
        error: function (xhr, status, error) {
            console.error('Error changing status:', error);
        }
    });
}