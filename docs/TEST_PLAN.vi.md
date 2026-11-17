# Kế hoạch kiểm thử

## Tự động

1. `.NET 8`: chạy `dotnet run --project Tests/CoreHarness/CoreHarness.csproj --configuration Release`.
2. Unity 2022.3.62f1: import project, Console không lỗi, chạy **Eco Guardian → Validate Asset Imports**.
3. Test Runner EditMode: luật chiến dịch, import 6 ảnh/4 audio/font và JSON round-trip.
4. Test Runner PlayMode: load scene thật, tạo camera/audio và khởi tạo hướng dẫn màn 1.
5. GitHub `Core rules` chạy trên push/PR. Unity test workflow chạy thủ công sau khi cấu hình license. Không đánh dấu test Unity đạt chỉ vì test C# đạt.

## Checklist thủ công trong Unity và bản PC

| Nhóm | Thao tác | Kết quả cần thấy |
|---|---|---|
| Khởi động | Clone sạch, mở Hub, đợi import, Play MainMenu | Không missing script/asset; tiếng Việt rõ; có menu và nhạc |
| Hướng dẫn | Mở màn 1, chờ, trồng Gai, bắt đầu, nâng cấp | Dừng giờ đúng bước; luôn đủ tiền nâng cấp; không bán/mua thêm gây softlock |
| Xây | Nhấp đường, cây, ngoài ô, ô tím, ô đã có tháp | Báo lỗi hoặc bỏ qua; tiền không đổi |
| Bẫy | Trồng Bomb lên ô xanh và ô vàng | Chỉ ô vàng được chấp nhận |
| UI | Bấm panel tháp/skill/menu, ở cửa sổ nhỏ và toàn màn hình | Không đặt nhầm tháp bên dưới; preview khớp con trỏ |
| Nâng cấp/bán | Đủ/thiếu tiền; cấp 3; bán sau hai lần nâng | Chỉ số đúng; bán floor(60% tổng chi phí); không bán hai lần |
| Wave | Bắt đầu sớm nhiều lần; giết hết quân đang có khi vẫn còn hàng đợi | +10 một lần ở wave tiếp theo; chưa kết thúc nếu còn quân chờ |
| Pause | Pause khi spawn, đầu độc, cháy, Shockwave warning, cooldown | Mọi timer/di chuyển/thu nhập dừng; tiếp tục không nhảy thời gian |
| Hiệu ứng | Hai Frost/Poison đánh chung; bắn Bulldozer; đánh boss | Không stack; độc xuyên giáp; boss chịu nửa slow/root/freeze |
| Thu nhập | Đợi trong prepare, wave, pause, kết quả; dùng Bloom | Chỉ có passive/Bloom trong wave |
| Sửa cây | Sửa khi prepare, sửa lần hai, sửa gần max, sửa khi combat | 60 tiền / +15 HP tối đa; một lần/prepare |
| Màn 2 | Hai cổng và ba ô tím | Hai hàng đợi hoạt động; vẫn có 10 ô dùng được |
| Màn 3 | Wave 2 chờ ≥20 giây, quan sát warning và lan | Warning 3s; lan sau 8s; tối đa 3 ô/cụm; tự tắt; tháp cháy bị khóa |
| Kỹ năng | Chọn vị trí ngoài bản đồ/thiếu tiền/đang cooldown; dùng Rain/Roots/Ancient | Không thu tiền thao tác lỗi; Rain dập; Roots trói; ultimate không tăng thu nhập/rút hồi Bomb |
| Màn 4 | Jammer đi gần nhiều tháp | Chỉ giảm tốc bắn 30%, không stack; hết vùng khôi phục |
| Titan | Qua 70%, 30%; burst vượt cả hai mốc | Đúng pha một lần; pha 2 dừng3s; warning2s/khóa3s; pha 3 ngừng quân và Shockwave |
| Kết thúc | Titan chết còn support; Titan chạm cây | Chưa thắng khi còn support; Titan chạm gây100 trước khi kiểm tra thắng |
| Sao/lưu | Thắng với các mức HP; replay điểm thấp; khởi động lại | 1/2/3 sao đúng ngưỡng; chỉ giữ tốt nhất; mở màn sau; nâng cấp không trừ lặp |
| Thử lại | Thua, retry, về menu rồi mở lại | Xóa tiền/tháp/địch/cooldown cũ, giữ tiến trình vĩnh viễn |
| Build | Build Windows; chạy trên máy không cài Editor | Không lỗi khởi động, menu và lưu hoạt động; thoát được |

## Cân bằng và hình ảnh

Chơi ít nhất ba lượt chiến dịch: Gai/Frost, Poison/Bomb và nền kinh tế Bloom. Ghi mỗi wave: tiền đầu/cuối, số tháp/cấp, HP mất, thời gian, lần dùng kỹ năng, lý do thua. Không dùng Debug API để gọi một lượt là chơi thắng hợp lệ. Mục tiêu 20–40 phút cần đo thực tế.

Kiểm tra 1280×720, 1600×1000, 1920×1080 và 2560×1440; đối chiếu con trỏ/vòng tầm bắn, chữ Việt, panel pause/kết quả và thanh Titan. Ghi FPS/GC sau 5 phút và trong cuối màn 5. Chụp ảnh hoặc video trực tiếp từ Unity/build; không dùng mockup làm bằng chứng test.
