1. Khi action trả dữ liệu về cho view (dưới dạng model)
    return View(dữ_liệu); -> dữ liệu này là 1 object thuộc class nào đó)
thì phải biết kiểu dữ liệu của dữ liệu là gì
2. Nếu view nhận dữ liệu từ action, trong view phải khai báo kiểu dữ liệu nhận được bằng lệnh @model kiểu_dữ_liệu
3. Muốn sử dụng dữ liệu từ Models gửi ra phải sử dung @Model.


Nhược điểm:
- khi tìm kiếm dẫn đến load lại toàn trang, mất thời gian và tài nguyên
- Lưu lại bookmark tìm kiếm khi chuyển trang
    + Khi thực hiện tìm kiếm (Search) thì lưu điều kiện tìm kiếm lại
    + Khi nhập đầu vào tìm kiếm (Index): kiểm tra xem có điều kiện đang lưu không? Nếu có thì dùng, không có thì tạo điều kiện mới
- Dùng session để lưu
- Trong ASP.NET MVC,muốn dùng session thì phải đăng kí
Những đối tượng nhập dữ liệu trong <form> muốn gửi được dữ liệu lên server: 
- phải có thuộc tính name
- Giá trị thuộc tính phải trùng với tên tham số hoặc tên thuộc tính của đối tượng nhận dữ liệu

trong trơngf hợp trong view có sử dụng model, mà các dôi tượng nhập dữ liệu trong <form> có name trùng với tên thuộc tính của model 

