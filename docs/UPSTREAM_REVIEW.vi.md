# Kiểm tra nguồn GitHub để tái sử dụng

Đã tải và đọc mã nguồn ba dự án, thay vì suy đoán từ mô tả tìm kiếm:

| Repo | Kết quả kiểm tra |
|---|---|
| https://github.com/witaliden/Tower-Defense | Unity TD góc nhìn từ trên xuống, hai màn, waypoint, spawn/reuse enemy, đặt tháp, đạn/pool, thắng/thua. C# có MIT, tác giả Witalij Denysewicz (2024). Được lấy về bằng Git submodule `External/TowerDefenseTemplate`. |
| https://github.com/lorenzo-iannaccaro/GardenDefense | Unity 2019, dự án học theo Glitch Garden, tower defense theo làn như Plants vs Zombies. Có MIT; khác bố cục đường cong trong brief. |
| https://github.com/WilliamHiiTW/TinyDefense | Unity 6, TD/RTS theo làn, có bản Windows và code MIT. README nói rõ asset bên thứ ba không thuộc MIT và phải tải riêng. Khác hệ thống waypoint thủ thành trong brief. |

## Nguồn đã lấy về

`External/TowerDefenseTemplate` là **project Unity riêng**, ghim commit bằng submodule. Nó chưa được dùng làm lõi chạy của bản web và chưa được port sang chiến dịch Eco Guardian. Không gọi việc thêm submodule là đã thay engine hay có giao diện như ảnh mẫu.

```sh
git submodule update --init --recursive
```

Có thể mở thư mục submodule như một project Unity độc lập. Không kéo toàn bộ script thẳng vào `Assets` hiện tại vì tên lớp, API Unity, prefab và các thành phần UI khác nhau. README upstream chỉ xác nhận MIT cho **code**; quyền tái phân phối asset phải kiểm tra riêng trước khi đưa asset đó vào bản phát hành. Submodule giữ URL/commit nguồn; không chép các asset đó vào bản web.

## Khả năng phù hợp với ảnh mẫu

Chưa tìm thấy repo được xác minh có đúng năm map và hình ảnh trong ảnh brief. Cả ba nguồn trên không chứng minh quyền sử dụng hình ảnh tham chiếu đó. Cần chọn một pipeline asset địa hình/tilemap thực tế phù hợp thay vì hứa rằng clone repo sẽ có hình giống mẫu.

## Lỗi không thấy quái

Lõi hiện tại chủ động dừng combat trong bước tutorial 2 cho đến khi Cây Gai được nâng cấp. UI trước đó ẩn nút Bắt đầu vì phase đã là Combat, khiến người chơi không thấy hành động tiếp tục rõ ràng. Bản sửa giữ nút chính hiện ở bước này, ghi rõ “Nâng cấp Cây Gai & cho quái vào”, thu 60 qua API Upgrade thông thường và mở lại combat. Thanh HUD nêu rõ quái đang chờ hướng dẫn. Không bơm tiền hoặc bỏ qua luật chiến đấu.

Kiểm tra: `node tools/test_wave_start.cjs`, cùng 19 assertion bridge C# (bao gồm enemy được sinh sau nâng cấp và Frame).
