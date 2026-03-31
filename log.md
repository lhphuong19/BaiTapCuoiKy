# Tạo Blank Solution có tên SV<MaSV> (Ví dụ: SV22T1020337)

Bổ sung cho solution các projects:
-	<SolutionName>.Admin: project dạng ASP .NET Core MVC
-	<SolutionName>.Shop: project dạng ASP .NET Core MVC
-	<SolutionName>.Models: project dạng Class Library
-	<SolutionName>.DataLayers: project dạng Class Library
-	<SolutionName>.BusinessLayers: project dạng Class Library

# Thiết kế Layout cho app Admin
- Sử dụng Theme AdminLTE4, Bootstrap5
- Copy nội dung của file Layout.html sang file _Layout.cshtml
- Trong _Layout.cshtml:
	+ Thay tiltle = @ViewBag.Title	
	+ Main Content = @RenderBody()

# Các Controller và Action dự kiến (chức năng dự kiến)
## Home
- Home/Index
	+ Trang tổng quan (Dashboard) của hệ thống
	+ Hiển thị các thông tin tổng quát của hệ thống
	+	Điều hướng nhanh đến các chức năng quản lý chính
## Account: Các chức năng liên quan đến tài khoản
- Account/Login
	+ Đăng nhập hệ thống
	+ Xác thực tài khoản và phân quyền người dùng
- Account/Logout
	+ Đăng xuất khỏi hệ thống
- Account/ChangePassword
	+ Thay đổi mật khẩu cho tài khoản đang đăng nhập
	+ Kiểm tra mật khẩu cũ và xác nhận mật khẩu mới
## Supplier
- Supplier/Index
	+ Hiển thị danh sách nhà cung cấp
	+ Tìm kiếm nhà cung cấp theo tên
	+ Điều hướng đến các chức năng Create, Edit, Delete
- Supplier/Create
	+ Tạo mới nhà cung cấp
- Supplier/Edit/{id}
	+ Chinh sửa thông tin nhà cung cấp theo id
- Supplier/Delete/{id}
	+ Xóa nhà cung cấp theo id
## Customer
- Customer/Index
	+ Hiển thị danh sách khách hàng dưới dạng phân trang (pagination)
	+ Tìm kiếm khách hàng theo tên
	+ Điều hướng đến các chức năng Create, Edit, Delete
- Customer/Create
	+ Tạo mới khách hàng
- Customer/Edit/{id}
	+ Chinh sửa thông tin khách hàng theo id
- Customer/Delete/{id}
	+ Xóa khách hàng theo id
- Customer/ChangePassword/{id}
	+ Thay đổi mật khẩu khách hàng theo id
## Shipper
- Shipper/Index
	+ Hiển thị danh sách shipper
	+ Tìm kiếm shipper theo tên hoặc số điện thoại
	+ Điều hướng đến các chức năng Create, Edit, Delete
- Shipper/Create
	+ Tạo mới shipper
- Shipper/Edit/{id}
	+ Chinh sửa thông tin shipper theo id
- Shipper/Delete/{id}
	+ Xóa shipper theo id
## Employee
- Employee/Index
	+ Hiển thị danh sách nhân viên dưới dạng phân trang (pagination)
	+ Tìm kiếm nhân viên theo tên
	+ Điều hướng đến các chức năng Create, Edit, Delete
- Employee/Create
	+ Tạo mới nhân viên
- Employee/Edit/{id}
	+ Chinh sửa thông tin nhân viên theo id
- Employee/Delete/{id}
	+ Xóa nhân viên theo id
- Employee/ChangePassword/{id}
	+ Thay đổi mật khẩu nhân viên theo id
- Employee/ChangeRole/{id}
	+ Thay đổi vai trò (role) của nhân viên theo id
## Category
- Category/Index
	+ Hiển thị danh sách danh mục sản phẩm
	+ Tìm kiếm danh mục theo tên
	+ Điều hướng đến các chức năng Create, Edit, Delete
- Category/Create
	+ Tạo mới danh mục sản phẩm
- Category/Edit/{id}
	+ Chinh sửa thông tin danh mục sản phẩm theo id
- Category/Delete/{id}
	+ Xóa danh mục sản phẩm theo id
## Product
- Product/Index
	+ Hiển thị danh sách sản phẩm dưới dạng phân trang (pagination)
	+ Tìm kiếm sản phẩm theo tên
	+ Điều hướng đến các chức năng Create, Edit, Delete
- Product/Detail/{id}
	+ Hiển thị chi tiết sản phẩm theo id
- Product/Create
	+ Tạo mới sản phẩm
- Product/Edit/{id}
	+ Chinh sửa thông tin sản phẩm theo id
- Product/Delete/{id}
	+ Xóa sản phẩm theo id
- Product/ListAttributes/{id}
	+ Hiển thị danh sách thuộc tính của sản phẩm theo id
- Product/b/{id}
	+ Tạo mới thuộc tính cho sản phẩm theo id
- Product/EditAttribute/{id}?attributeId={attributeId}
	+ chinh sửa thuộc tính của sản phẩm theo id và attributeId
- Product/DeleteAttribute/{id}?attributeId={attributeId}
	+ Xóa thuộc tính của sản phẩm theo id và attributeId
- Product/ListPhotos/{id}
	+ Hiển thị danh sách hình ảnh của sản phẩm theo id
- Product/CreatePhoto/{id}
	+ Tạo mới hình ảnh cho sản phẩm theo id
- Product/DeletePhoto/{id}?photoId={photoId}
	+ Xóa hình ảnh của sản phẩm theo id và photoId
- Product/EditPhoto/{id}?photoId={photoId}
	+ Chinh sửa hình ảnh của sản phẩm theo id và photoId
## Order
- Order/Index
	+ Hiển thị danh sách đơn hàng dưới dạng phân trang (pagination)
	+ Tìm kiếm đơn hàng theo mã đơn hàng hoặc tên khách hàng
	+ Điều hướng đến các chức năng Detail, Create, Edit, Delete
- Order/Create
	+ Tạo mới đơn hàng


## KHÔNG ĐƯỢC ĐỂ NGƯỜI SỬ DỤNG ĐI VÀO NGÕ CỤT
## Lưu ý:
- Đầu vào tìm kiếm/ lọc mặt hàng: Loại hàng (select), Nhà cung cấp (select), khoảng giá, tên hàng
- Đầu vào tìm kiếm/ lọc đơn hàng: Trạng thái (select), Thời gian (từ ngày - đến ngày), tên khách hàng

