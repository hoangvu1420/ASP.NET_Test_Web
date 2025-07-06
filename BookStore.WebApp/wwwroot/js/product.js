var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/product/getall' },
        "columns": [
            { data: 'title', "width": "15%", "class": "text-light" },
            { data: 'isbn', "width": "15%", "class": "text-light" },
            { data: 'category.name', "width": "15%", "class": "text-light" },
            { data: 'author', "width": "15%", "class": "text-light" },
            { data: 'listPrice', "width": "9%", "class": "text-light" },
            { data: 'price', "width": "8%", "class": "text-light" },
            { data: 'price50', "width": "8%", "class": "text-light" },
            { data: 'price100', "width": "8%", "class": "text-light" },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="text-center">
                                <style>
							        .btn-custom {
								        padding: 0 4px;
							        }
						        </style>
                                <a href="/Admin/Product/Upsert/${data}" class="btn btn-outline-primary btn-custom" >
                                    <i class="bi bi-pencil-square"></i>
                                </a>
                                <a onClick=Delete('/Admin/Product/Delete/${data}') class="btn btn-outline-danger btn-custom" >
                                    <i class="bi bi-trash"></i>
                                </a>
                            </div>`;
                },
                "width": "7%"
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

