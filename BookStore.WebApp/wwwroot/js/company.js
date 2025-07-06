var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable(
    {
        "ajax": { url: '/admin/company/getall' },
        "columns":
        [
            { data: 'name', "width": "15%", "class": "text-light" },
            { data: 'streetAddress', "width": "25%", "class": "text-light" },
            { data: 'city', "width": "15%", "class": "text-light" },
            { data: 'state', "width": "15%", "class": "text-light" },
            { data: 'postalCode', "width": "10%", "class": "text-light" },
            { data: 'phone', "width": "10%", "class": "text-light" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="text-center">
                                <style>
							        .btn-custom {
								        padding: 0px 4px;
							        }
						        </style>
                                <a href="/Admin/Company/Upsert/${data}" class="btn btn-outline-primary btn-custom" >
                                    <i class="bi bi-pencil-square"></i>
                                </a>
                                <a onClick=Delete('/Admin/Company/Delete/${data}') class="btn btn-outline-danger btn-custom" >
                                    <i class="bi bi-trash"></i>
                                </a>
                            </div>`;
                },
                "width": "10%"
            }
        ]
    });
}
//
function Delete(url) {
    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Delete"
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: 'DELETE',
                success: function (data) {
                    if (data.success) {
                        dataTable.ajax.reload();
                        toastr.success(data.message);
                    }
                }
            })
        }
    });
}

