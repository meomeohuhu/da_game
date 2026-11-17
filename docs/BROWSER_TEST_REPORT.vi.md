# Bản trình duyệt — kiểm tra và giới hạn

Ngày 17/11/2026, bổ sung BrowserDemo để chủ dự án chơi mà không cài Unity.

- Build Release bằng .NET 8 / Blazor WebAssembly thành công, có bộ chạy WASM và assembly chứa cùng Core C# như Unity.
- Kiểm tra cú pháp JavaScript bằng `node --check` đạt.
- Kiểm tra bridge C#/JSON chạy trên .NET: 19 assertion đạt, gồm đọc save hỏng, khóa màn, 5 cấu hình wave, đặt/nâng cấp, spawn qua Frame, pause, retry và nâng cấp vĩnh viễn. Xem `BROWSER_BRIDGE_OUTPUT.txt`. Những test này không thực thi WASM trong trình duyệt.
- Các kiểm thử lõi 145 assertion, 100 scenario/replay và 30 bot chiến thuật vẫn áp dụng vì BrowserDemo liên kết trực tiếp các file Core. Không copy/port lại quy tắc vào JavaScript.
- Sites báo xuất bản thành công tại https://eco-guardian-da-game.bronzeguppy19.chatgpt.site, giữ quyền truy cập riêng cho chủ dự án.
- Preview nội bộ không sẵn sàng: relay port 4173 hết thời gian. Đã dừng preview, không tự thay đổi mạng hoặc dựng đường vòng.
- Trình duyệt kiểm thử đã mở URL đã xuất bản và xác nhận cổng đăng nhập riêng hoạt động. Phiên kiểm thử không đăng nhập ChatGPT, nên **chưa kiểm tra trực tiếp Canvas, thao tác chuột hoặc thực thi WebAssembly trên trang live**. Không tự đổi website thành công khai để vượt cổng này.
- Đây là bản frontend HTML/Canvas + .NET WebAssembly, **không phải Unity WebGL, không thay kết quả Unity PlayMode/import/build**. Thời lượng, cân bằng với người chơi và FPS vẫn cần đánh giá.

Chủ dự án có thể mở link bằng Chrome/Edge, dùng cùng tài khoản ChatGPT đã tạo Sites. Tiến trình dùng localStorage riêng, không chia sẻ với bản Unity hoặc các trình duyệt khác. Nếu gặp lỗi hiển thị hoặc khởi động, lưu thông báo lỗi để đối chiếu; không cần gửi mật khẩu hay token.

## Kiểm thử trình duyệt thực tế sau sửa wave và nền

Run https://github.com/meomeohuhu/da_game/actions/runs/44036778521 **PASS** trên Ubuntu 24.04, Chromium headless qua Playwright. Test phục vụ bản WebAssembly Release từ HTTP cục bộ của runner (không qua cổng đăng nhập Sites), mở giao diện thật, chọn hành trình, đóng hội thoại và nhấn Bắt đầu đúng một lần. Assertion xác nhận có enemy sống, tọa độ enemy tiến vào bản đồ, tutorial hoàn tất, Gai cấp 2, wave 1, pause giữ nguyên thời gian và resume tiếp tục. Không có `pageerror` trong lượt test. Screenshot `wave-running.png` và HTML report được upload vào artifact `browser-e2e-results`.

Bản sửa hiện tự hoàn tất trồng/nâng Gai đầu màn qua các API trả tiền bình thường nếu chưa thực hiện. Tổng chi phí 110 Sinh Khí được ghi ở nút. Không còn cần lần nhấn thứ hai để mở combat. HUD hiển thị số địch sống và tổng còn lại. Renderer có hình thay thế nếu ảnh enemy chưa tải, tránh quái vô hình. Tệp CSS/JS được gắn phiên bản URL để tránh dùng bản cache cũ.

Nền mới `battlefield-painted.png` là ảnh AI tạo riêng: rừng, vách đá, thác và khoảng đất trống giữa map. Đường và ô xây vẫn vẽ theo tọa độ gameplay thực tế. Test trên runner không đồng nghĩa đã đăng nhập và kiểm tra trang Sites live; giữ phân biệt này khi bàn giao. Việc tải artifact về môi trường làm việc bị endpoint lưu trữ từ chối, nên chưa trực tiếp xem screenshot đã upload tại đây.
