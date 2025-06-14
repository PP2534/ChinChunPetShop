/*************************************************************************************/
// -->Template Name: Bootstrap Press Admin
// -->Author: Themedesigner
// -->Email: niravjoshi87@gmail.com
// -->File: datatable_basic_init
/*************************************************************************************/

/****************************************
 *       Basic Table                   *
 ****************************************/
$('#zero_config').DataTable();

/****************************************
 *       Default Order Table           *
 ****************************************/
$('#default_order').DataTable({
    "order": [
        [3, "desc"]
    ]
});

$('#table_database').DataTable({
    "language": {
        "decimal": "",
        "emptyTable": "Không có mục nào",
        "info": "Hiển thị _START_ đến _END_ của _TOTAL_ mục",
        "infoEmpty": "Hiển thị 0 đến 0 của 0 mục",
        "infoFiltered": "(đã lọc từ _MAX_ tổng số mục)",
        "infoPostFix": "",
        "thousands": ",",
        "lengthMenu": "Hiển thị _MENU_ mục",
        "loadingRecords": "Đang tải...",
        "processing": "",
        "search": "Tìm kiếm nhanh (bất kỳ điều gì):",
        "zeroRecords": "Không có bản ghi nào khớp được tìm thấy",
        "paginate": {
            "first": "Trước",
            "last": "Cuối cùng",
            "next": "Sau",
            "previous": "Đầu tiên"
        },
        "aria": {
            "orderable": "Sắp xếp theo cột này",
            "orderableReverse": "Đảo ngược cột này"
        }
    },
    "order": [
        [0, "asc"]
    ]
});

/****************************************
 *       Multi-column Order Table      *
 ****************************************/
$('#multi_col_order').DataTable({
    columnDefs: [{
        targets: [0],
        orderData: [0, 1]
    }, {
        targets: [1],
        orderData: [1, 0]
    }, {
        targets: [4],
        orderData: [4, 0]
    }]
});