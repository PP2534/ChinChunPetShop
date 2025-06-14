$(function () {
    var $minInput = $('#giamin');
    var $maxInput = $('#giamax');

    function updateMaxConstraints() {
        // Lấy giá trị giamin, nếu rỗng hoặc không phải số thì thành 0
        var minVal = parseFloat($minInput.val());
        if (isNaN(minVal) || minVal < 0) {
            minVal = 0;
        }
        // Cập nhật thuộc tính min cho giamax
        $maxInput.attr('min', minVal);

        // Nếu giamax < minVal, tự động set lại
        var maxVal = parseFloat($maxInput.val());
        if (!isNaN(maxVal) && maxVal < minVal) {
            $maxInput.val(minVal);
        }
    }

    // Khi người dùng nhập vào giamin, cập nhật ngay
    $minInput.on('input', updateMaxConstraints);

    // Khi giamax thay đổi, đảm bảo nó không thấp hơn min
    $maxInput.on('input', updateMaxConstraints);

    // Khởi tạo lần đầu
    updateMaxConstraints();

    $('.xoanhanvien').on('click', function () {
        const maNhanVien = $(this).data('manhanvien');
        const name = $(this).data('name');
        const image = $(this).data('image');

        // Populate modal
        $('.MaNhanVien-input').val(maNhanVien);
        $('.MaNhanVien').text(maNhanVien);
        $('.TenNhanVien').text(name);
        $('.AnhNhanVien').attr('src', image);

        // Show modal
        $('#deleteNhanVienModal').modal('show');
    });
    
});


(function ($) {
    "use strict";
    $(document).ready(function () {
        $('.HinhAnh').on('change', function (event) {
            const file = event.target.files[0]; // Lấy file người dùng chọn
            if (file) {
                const reader = new FileReader(); // Khởi tạo FileReader để đọc file

                // Khi file được load xong, cập nhật src của ảnh
                reader.onload = function (e) {
                    $('#HinhAnh').attr('src', e.target.result);
                };

                reader.readAsDataURL(file); // Đọc file dưới dạng Data URL
            }
        });
        $('.Avatar').on('change', function (event) {
            const file = event.target.files[0]; // Lấy file người dùng chọn
            if (file) {
                const reader = new FileReader(); // Khởi tạo FileReader để đọc file

                // Khi file được load xong, cập nhật src của ảnh
                reader.onload = function (e) {
                    $('#Avatar').attr('src', e.target.result);
                };

                reader.readAsDataURL(file); // Đọc file dưới dạng Data URL
            }
        });
        $('.ipt').each(function () {
            $(this).append(' <span class="text-danger">*</span>');
        });

        //Vadidate
        function validateField($el) {
            // Lấy giá trị đã trim
            var val = $el.val().trim();
            // Xoá các feedback cũ
            $el.removeClass('is-valid is-invalid');
            $el.next('.valid-feedback, .invalid-feedback').remove();

            var feedback_isvalid = $el.attr("feedback-show_is_vaild") ?? false;
            var feedback_is_invalid = $el.attr("feedback-is_invaild") ?? "Nè, cái này không được để trống!";
            var feedback_is_valid = $el.attr("feedback-is_vaild") ?? "Có vẻ ổn!";
            //alert("F" + feedback_is_invalid);
            
            if (feedback_isvalid) {
                if (val === '') {
                    // Thêm class invalid
                    $el.addClass('is-invalid');
                    // Chèn feedback
                    $el.after('<div class="invalid-feedback">'+feedback_is_invalid+'</div>');
                    return false;
                } else {
                    // Thêm class valid
                    $el.addClass('is-valid');
                    // Chèn feedback
                    $el.after('<div class="valid-feedback">'+feedback_is_valid+'</div>');
                    return true;
                }
            }
            if (val === '') {
                // Thêm class invalid
                $el.addClass('is-invalid');
                // Chèn feedback
                $el.after('<div class="invalid-feedback">' + feedback_is_invalid + '</div>');
                return false;
            }
            //else {
            //    // Thêm class valid
            //    $el.addClass('is-valid');
            //    // Chèn feedback
            //    $el.after('<div class="valid-feedback">Có vẻ ổn!</div>');
            //    return true;
            //}
            return true;
        }

        // Bắt sự kiện blur và input (vừa mất focus, vừa gõ)
        $('body').on('blur input', '.not-null', function () {
            validateField($(this));
        });

        // Bắt sự kiện submit
        $('.submit-vadidate').on('click', function (e) {
            var allValid = true;
            $('.not-null').each(function () {
                if (!validateField($(this))) {
                    allValid = false;
                }
            });
            if (!allValid) {
                e.preventDefault();
                // Nếu cần disable button:
                $(this).prop('disabled', true);
                // Bật lại khi có thay đổi
                $('body').on('input', '.not-null', function () {
                    $('.submit-vadidate').prop('disabled', false);
                });
            }
        });

        $('.product-qty').each(function () {
            var $el_product = $(this);
            const min = parseInt($el_product.find('#quantity').attr('min'));
            const max = parseInt($el_product.find('#quantity').attr('max'));
            // Sự kiện khi nhấn nút tăng
            $el_product.on('click', '.quantity-right-plus', function (e) {
                e.preventDefault();
                var quantity = parseInt($el_product.find('#quantity').val(), 10) || 0;
                if (quantity < max) {
                    quantity += 1;
                    $el_product.find('#quantity').val(quantity);
                }
                if (quantity >= min && quantity <= max) {
                    //// Submit form chứa thẻ input
                    //var $row = $(this).closest('tr');
                    //var donGia = parseFloat($row.find('input[name^="CTDH"][name$=".DonGia"]').val()) || 0;
                    //var thanhTien = donGia * quantity;
                    //var formattedThanhTien = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhTien);
                    //$row.find('.tt-thanhtien-mua').text(formattedThanhTien);
                    //var dongiamua = parseFloat($row.find('.giamua').text().replace(/[^\d]/g, '')) || 0;
                    //var thanhtiencoc = dongiamua * quantity;
                    //var formattedThanhTienCoc = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhtiencoc);
                    //$row.find('.tt-tiencoc-thue').text(formattedThanhTienCoc);
                    //formattedThanhTienCoc = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhtiencoc + quantity * 20000);
                    //$row.find('.tt-thanhtien-thue').text(formattedThanhTienCoc);
                    $(this).closest('form').submit();
                } else {
                    alert('Giá trị không hợp lệ!'); // Thông báo nếu giá trị vượt min/max
                }
            });

            // Sự kiện khi nhấn nút giảm
            $el_product.on('click', '.quantity-left-minus', function (e) {
                e.preventDefault();
                var quantity = parseInt($el_product.find('#quantity').val(), 10) || 0;
                if (quantity > min) {
                    quantity -= 1;
                    $el_product.find('#quantity').val(quantity);
                }
                if (quantity >= min && quantity <= max) {
                    //var $row = $(this).closest('tr');
                    //var donGia = parseFloat($row.find('input[name^="CTDH"][name$=".DonGia"]').val()) || 0;
                    //var thanhTien = donGia * quantity;
                    //var formattedThanhTien = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhTien);
                    //$row.find('.tt-thanhtien-mua').text(formattedThanhTien);
                    //var dongiamua = parseFloat($row.find('.giamua').text().replace(/[^\d]/g, '')) || 0;
                    //var thanhtiencoc = dongiamua * quantity;
                    //var formattedThanhTienCoc = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhtiencoc);
                    //$row.find('.tt-tiencoc-thue').text(formattedThanhTienCoc);
                    //formattedThanhTienCoc = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhtiencoc + quantity * 20000);
                    //$row.find('.tt-thanhtien-thue').text(formattedThanhTienCoc);
                    $(this).closest('form').submit();
                } else {
                    alert('Giá trị không hợp lệ!'); // Thông báo nếu giá trị vượt min/max
                }
            });
        });
        $('.quantity-cart-change').on('change', function () {
            // Kiểm tra giá trị mới
            const min = parseInt($(this).attr('min'));
            const max = parseInt($(this).attr('max'));
            const value = parseInt($(this).val());

            if (value >= min && value <= max) {
                //var $row = $(this).closest('tr');
                //var quantity = value;
                //var donGia = parseFloat($row.find('input[name^="CTDH"][name$=".DonGia"]').val()) || 0;
                //var thanhTien = donGia * quantity;
                //var formattedThanhTien = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhTien);
                //$row.find('.tt-thanhtien-mua').text(formattedThanhTien);
                //var dongiamua = parseFloat($row.find('.giamua').text().replace(/[^\d]/g, '')) || 0;
                //var thanhtiencoc = dongiamua * quantity;
                //var formattedThanhTienCoc = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhtiencoc);
                //$row.find('.tt-tiencoc-thue').text(formattedThanhTienCoc);
                //formattedThanhTienCoc = new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(thanhtiencoc + quantity * 20000);
                //$row.find('.tt-thanhtien-thue').text(formattedThanhTienCoc);
                $(this).closest('form').submit();
            } else {
                alert('Giá trị không được vượt quá số lượng trong kho: ' + max + '!'); // Thông báo nếu giá trị vượt min/max
                $(this).val(max);
            }
        });
        $('.product-qty-order').each(function () {
            var $el_product = $(this);
            var min = parseInt($el_product.find('#quantity').attr('min'));
            var max = parseInt($el_product.find('#quantity').attr('max'));
            // Sự kiện khi nhấn nút tăng
            $el_product.on('click', '.quantity-right-plus', function (e) {
                min = parseInt($el_product.find('#quantity').attr('min'));
                max = parseInt($el_product.find('#quantity').attr('max'));
                e.preventDefault();
                var quantity = parseInt($el_product.find('#quantity').val(), 10) || 0;
                if (quantity < max) {
                    quantity += 1;
                    $el_product.find('#quantity').val(quantity);
                }
                if (quantity < min || quantity > max) {
                    alert('Giá trị không hợp lệ!'); // Thông báo nếu giá trị vượt min/max
                }
            });

            // Sự kiện khi nhấn nút giảm
            $el_product.on('click', '.quantity-left-minus', function (e) {
                min = parseInt($el_product.find('#quantity').attr('min'));
                max = parseInt($el_product.find('#quantity').attr('max'));
                e.preventDefault();
                var quantity = parseInt($el_product.find('#quantity').val(), 10) || 0;
                if (quantity > min) {
                    quantity -= 1;
                    $el_product.find('#quantity').val(quantity);
                }
                if (quantity < min || quantity > max){
                    alert('Giá trị không hợp lệ!'); // Thông báo nếu giá trị vượt min/max
                }
            });
        });
        $('.quantity-order-change').on('change', function () {
            // Kiểm tra giá trị mới
            const min = parseInt($(this).attr('min'));
            const max = parseInt($(this).attr('max'));
            const value = parseInt($(this).val());
            if (value < min) {
                alert('Giá trị không được dưới: ' + min + '!'); // Thông báo nếu giá trị vượt min/max
                $(this).val(min);
            }
            if (value > max) {
                alert('Giá trị không được vượt quá số lượng trong kho: ' + max + '!'); // Thông báo nếu giá trị vượt min/max
                $(this).val(max);
            }
        });

        $('.nhapxuat-sanpham')
            .on('click', '.quantity-left-minus', function (e) {
                // 'this' là button.quantity-left-minus vừa bấm
                var $input = $(this).siblings('.quantity-xn-change');
                var quantity = parseInt($input.val()) || 0;
                const min = parseInt($input.attr('min'));
                const max = parseInt($input.attr('max'));
                if (quantity > min) {
                    quantity -= 1;
                    $input.val(quantity);
                }
                if (quantity < min || quantity > max) {
                    alert('Giá trị không hợp lệ!');
                }
                
            })
            .on('click', '.quantity-right-plus', function (e) {
                var $input = $(this).siblings('.quantity-xn-change');
                var quantity = parseInt($input.val()) || 0;
                const min = parseInt($input.attr('min'));
                const max = parseInt($input.attr('max'));
                if (quantity < max) {
                    quantity += 1;
                    $input.val(quantity);
                }
                if (quantity < min || quantity > max) {
                    alert('Giá trị không hợp lệ!');
                }
            })
            .on('input', '.quantity-xn-change', function (e) {
                const min = parseInt($(this).attr('min'));
                const max = parseInt($(this).attr('max'));
                const value = parseInt($(this).val());
                if (value < min) {
                    alert('Giá trị không được dưới: ' + min + '!'); // Thông báo nếu giá trị vượt min/max
                    $(this).val(min);
                }
                if (value > max) {
                    alert('Giá trị không được vượt quá số lượng trong kho: ' + max + '!'); // Thông báo nếu giá trị vượt min/max
                    $(this).val(max);
                }
            })
            .on('input', '.quantity-input, #dongia', function () {
                var $row = $(this).closest('.sanpham-index');
                var qty = parseInt($row.find('.quantity-input').val()) || 0;
                var price = parseFloat($row.find('#dongia').val()) || 0;
                var total = qty * price;
                $row.find('.thanhtien').text(total.toLocaleString() + 'VNĐ');
            });


        $('.add-to-cart').on('click', function () {
            //console.log("ok");
            //alert("ok");
            const MaSP = $(this).data('masp');
            const name = $(this).data('name');
            const max = $(this).data('max');
            const nhanhieu = $(this).data('nhanhieu');
            const image = $(this).data('image');
            const next = $(this).data('next');
            // Populate modal
            $('#MaSP').val(MaSP);
            $('#pnext').val(next);
            $('#productName').text(name);
            $('#productNhanHieu').text(nhanhieu);
            $('#productImage').attr('src', image);
            $('.quantity-order-change').attr('max',max);

            // Show modal
            $('#ThemVaoGioHang').modal('show');
        });

        $('.them-san-pham').on('click', function () {
            $('#them_sanpham').modal('show');
        });

        $('.submit-delete-cart').on('click', function () {
            // Lấy giá trị từ data-mgh
            var maGioHang = $(this).data('mgh');

            // Gán giá trị vào input ẩn trong form
            $('#hiddenMaGioHang').val(maGioHang);

            // Gửi form
            $('#deleteCartForm').submit();
        });
        document.querySelectorAll('.actionlink').forEach(div => {
            div.addEventListener('click', function () {
                const url = div.getAttribute('data-url');
                if (url) {
                    window.location.href = url;
                }
            });
        });

        function reindexAll() {
            $('.sanpham-index').each(function (i, row) {
                var $row = $(row);
                // 1. Hidden input MaSP
                $row.find('.masanpham')
                    .attr('name', 'CTGD[' + i + '].MaSP');
                // 2. Input số lượng
                $row.find('.quantity-input')
                    .attr('name', 'CTGD[' + i + '].SoLuong');
                // 3. Input đơn giá
                $row.find('#dongia')
                    .attr('name', 'CTGD[' + i + '].DonGia');
            });
        }

        // 2. Xử lý click nút "Thêm sản phẩm" (class .them-sp)
        $('.them-sp').on('click', function () {
            // Đếm hiện tại có bao nhiêu block .sanpham-index
            var count = $('.nhapxuat-sanpham .sanpham-index').length;

            // Tạo HTML mới với i = count
            var newHtml =
                '<div class="row cart-item mb-3 sanpham-index">' +
                '<div class="col-md-2">' +
                '<img src="/images/sanpham/petshop_sp.png" alt="Sản phẩm" class="img-fluid rounded hinhanh">' +
                '</div>' +
                '<div class="col-md-4 border border-1">' +
                '<div class="d-flex justify-content-between">' +
                '<h5 class="card-title pt-1 tensanpham">Tên sản phẩm</h5>' +
                '<input type="hidden" class="masanpham" name="CTGD[' + count + '].MaSP" value="" />' +
                '<div type="button" class="them-san-pham"><i class="bi bi-pencil-square"></i></div>' +
                '</div>' +
                '<div class="text-muted">Phân loại: <span class="phanloai">Loại sản phẩm</span></div>' +
                '<div class="text-muted">Nhãn hiệu: <span class="nhanhieu">Nhãn hiệu</span></div>' +
                '</div>' +
                '<div class="col-md-2">' +
                '<div class="input-group product-qty-xn">' +
                '<label for="quantity" class="col-form-label fw-bold ipt w-100 fs-7">Số lượng:</label>' +
                '<button class="btn btn-outline-primary btn-sm quantity-left-minus" type="button" data-type="minus">-</button>' +
                '<input style="max-width:100px" id="quantity" min="1" max="10"' +
                ' name="CTGD[' + count + '].SoLuong" type="text"' +
                ' class="form-control form-control-sm text-center quantity-input quantity-xn-change"' +
                ' value="1">' +
                '<button class="btn btn-outline-primary btn-sm quantity-right-plus" type="button" data-type="plus">+</button>' +
                '</div>' +
                '</div>' +
                '<div class="col-md-2">' +
                '<div class="input-group">' +
                '<label for="dongia" class="col-form-label fw-bold ipt w-100 fs-7">Đơn giá (VNĐ):</label>' +
                '<input type="number" min="0" class="form-control form-control-sm not-null"' +
                ' id="dongia" name="CTGD[' + count + '].DonGia" placeholder="Nhập đơn giá...">' +
                '</div>' +
                '</div>' +
                '<div class="col-md-2 text-end">' +
                '<p class="fw-bold thanhtien">0VNĐ</p>' +
                '<button type="button" class="btn btn-sm btn-outline-danger delete-sanpham">' +
                '<i class="bi bi-trash"></i>' +
                '</button>' +
                '</div>' +
                '</div>' +
                '<hr/>';

            // Chèn trước .button-sanpham-index
            $('.nhapxuat-sanpham .button-sanpham-index').before(newHtml);
        });

        // 3. Xử lý click nút "delete-sanpham": xóa block hiện tại và hr kèm theo, sau đó reindex
        $(document).on('click', '.delete-sanpham', function () {
            var $row = $(this).closest('.sanpham-index');
            // Xóa <hr> ngay sau nó
            $row.next('hr').remove();
            // Xóa chính block
            $row.remove();
            // Đánh lại chỉ số i cho các block còn lại
            reindexAll();
        });

        

    });

})(jQuery);