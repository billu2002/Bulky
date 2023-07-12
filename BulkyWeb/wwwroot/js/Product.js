var dataTable;
$(document).ready(function () {
    loadDataTable();
});

function loadDataTable() {
    dataTable = $('#tblData').DataTable({
        ajax: { url: '/admin/product/getall' },
  
        "columns": [
            {data: "title", width: "20%"},
            { data: 'isbn', "width": "10%" },
            { data: 'listPrice', "width": "7.5%" },
            { data: 'author', "width": "12.5%" },
            { data: 'category.name', "width": "10%" },
            {
                data: "date", "width": "15%", render: function (data) {
                    var dateObj = new Date(data);
                    var day = dateObj.getDate().toString().padStart(2, '0');
                    var month = (dateObj.getMonth() + 1).toString().padStart(2, '0');
                    var year = dateObj.getFullYear().toString();
                    return day + '-' + month + '-' + year;
                }
},
            {
                data: 'id',
                "render": function (data) {
                    return `<div class="w-75 btn-group" role="group">
                        <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i>Edit</a>
                        <a onClick=Delete('/admin/product/delete/${data}') class="btn btn-danger mx-2"><i class="bi bi-trash-fill"></i>Delete</a>
                    </div>`
                },
                "width:": "10%"
            }
        ]
    });

    $('#tblData').DataTable({
        columnDefs: [{
            targets: 4,
            render: DataTable.render.moment('YYYY/MM/DD', 'Do MMM YY', 'fr')
        }]
    });
}

function Delete(url) {
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                url: url,
                type: "DELETE",
                success: function (data) {
                    dataTable.ajax.reload();
                    toastr.success(data.message);
                }
            })
        }

    })
}