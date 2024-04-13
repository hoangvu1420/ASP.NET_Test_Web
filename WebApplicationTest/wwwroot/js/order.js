let dataTable;
$(document).ready(function () {
    const url = window.location.search;
    if (url.includes("inprocess")) {
        loadDataTable("inprocess");
    }
    else if (url.includes("pending")) {
        loadDataTable("pending");
    }
    else if (url.includes("completed")) {
        loadDataTable("completed");
    }
    else if (url.includes("approved")) {
        loadDataTable("approved");
    }
    else {
        loadDataTable("all");
    }
});

function loadDataTable(status) {
    dataTable = $('#tblData').DataTable({
        "ajax": { url: '/admin/order/getall?status=' + status },
        "columns": [
            { data: 'id', "width": "5%", "class": "text-light" },
            { data: 'fullName', "width": "20%", "class": "text-light" },
            { data: 'phoneNumber', "width": "15%", "class": "text-light" },
            { data: 'applicationUser.email', "width": "25%", "class": "text-light" },
            { data: 'orderStatus', "width": "15%", "class": "text-light" },
            {
                data: 'orderTotal',
                "width": "10%",
                "class": "text-light",
                "render": function (data) {
                    return '$' + parseFloat(data).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,');
                }
            },
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="text-center">
                                <style>
							        .btn-custom {
								        padding: 0 4px;
							        }
						        </style>
                                <a href="/Admin/Order/Details?id=${data}" class="btn btn-outline-primary btn-custom" >
                                    <i class="bi bi-file-text"></i>
                                </a>
                            </div>`;
                },
                "width": "10%"
            }
        ]
    });
}

