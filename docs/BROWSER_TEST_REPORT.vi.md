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
