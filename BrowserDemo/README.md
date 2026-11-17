# Bản chơi thử không cần cài Unity

BrowserDemo dùng **Blazor WebAssembly / .NET 8**, liên kết trực tiếp các file `Assets/EcoGuardian/Core/*.cs`. Không port lại luật bằng JavaScript và không thay thế dự án Unity. Đây **không phải bản Unity WebGL** và không chứng minh Unity import/build thành công.

Giao diện Canvas/HTML dùng cùng bộ hình, âm thanh và font. Có 5 màn/25 wave, tháp/nâng cấp/bán, kỹ năng, pause, hướng dẫn, chọn màn và lưu sao/nâng cấp vào localStorage của trình duyệt. Âm thanh mặc định tắt để phù hợp chính sách autoplay; bấm nút để bật. Nên dùng Chrome hoặc Edge hiện đại, màn hình PC.

## Chơi

Link private Sites cho chủ dự án: https://eco-guardian-da-game.bronzeguppy19.chatgpt.site

Chờ lần tải đầu, bấm **Bắt đầu hành trình**, đóng hội thoại, chọn Cây Gai và nhấp một ô xanh. Bắt đầu wave và nâng cấp theo hướng dẫn. Chuột phải hủy chọn. Esc hủy hoặc pause. Tiến trình lưu riêng với file save của Unity, không đồng bộ giữa trình duyệt/máy hoặc khi xóa dữ liệu trang.

## Build từ mã nguồn

```sh
dotnet workload install wasm-tools
dotnet publish BrowserDemo/BrowserDemo.csproj -c Release -o browser-build
```

Phục vụ thư mục `browser-build/wwwroot` bằng máy chủ HTTP hỗ trợ MIME `.wasm` và đường dẫn `_framework/`; không mở bằng `file://`. Không cần Unity license để chạy bản này. Mã nguồn game Unity vẫn ở thư mục `Assets` của repo.

Giấy phép asset giữ như `docs/ASSETS.md`; font Noto Sans là OFL và bộ Kenney là CC0. Cấu hình và quyền truy cập Sites được giữ ở repo triển khai riêng; không chứa token trong repo GitHub này.
