# Eco Guardian — Lời Thì Thầm Của Đại Ngàn

Dự án **Unity 2D cho PC**, xây dựng theo kịch bản Người Gác Rừng: An dùng thực vật phòng thủ để bảo vệ Cây Cổ Thụ khỏi đoàn máy khai thác.

> **Trạng thái: bản triển khai đầu tiên, chờ nghiệm thu trong Unity.** Lõi C# đã được kiểm thử trên máy phát triển; chưa chạy Unity Editor, chưa có bản `.exe` được xác minh. Không coi việc qua kiểm thử logic là đã hoàn tất chơi thử/cân bằng chiến dịch.

## Mở và chơi

1. Cài **Unity Hub → Unity 2022.3.62f1 LTS**, thêm module Windows Build Support nếu muốn xuất PC.
2. Clone repo, chọn **Add project from disk** trong Hub và trỏ tới thư mục gốc có `Assets`, `Packages`, `ProjectSettings`.
3. Đợi import. Repo đã có năm asset `Resources/Levels/Level01…05.asset` ; Editor tạo lại asset còn thiếu khi cần. Nếu cần, chọn **Eco Guardian → Generate Scenes and Level Assets**. Lệnh này giữ nguyên các LevelData đã tồn tại.
4. Mở `Assets/EcoGuardian/Scenes/MainMenu.unity`, bấm **Play**. Màn 1 có hội thoại và hướng dẫn dừng thời gian.
5. Để build: **Eco Guardian → Build Windows Demo**. Kết quả mong đợi: `Builds/Windows/EcoGuardian.exe` cùng thư mục dữ liệu. Đây là hướng dẫn build, không phải bản build đã kiểm chứng.

Chuột trái chọn tháp/kỹ năng và xác nhận trên bản đồ. Ô xanh dành cho tháp; ô vàng dành riêng cho Quả Nổ. Nhấp tháp để nâng cấp/bán/đổi ưu tiên. Chuột phải hoặc Esc hủy; Esc khi không chọn gì để pause. Giữ HP ≥80% để nhận ba sao.

## Phạm vi đã có trong mã nguồn

- Cấu hình 5 màn / 25 wave đúng thành phần quân trong tài liệu; 1–3 cổng, 8/10/12/14/16 ô hợp lệ.
- Năm tháp, ba cấp, ba ưu tiên; chậm, độc, đóng băng, giáp và nhiễu.
- Thu nhập chỉ trong combat; thưởng wave, bắt đầu sớm, sửa cây, hoàn tiền bán.
- Ba kỹ năng; lửa cảnh báo/lan/tự tắt; Titan ba giai đoạn, quân hỗ trợ và Shockwave.
- Menu, chọn màn, hướng dẫn, HUD, pause, kết quả; lưu JSON, sao tốt nhất và ba nâng cấp vĩnh viễn.
- Hình Kenney CC0, tháp thực vật vẽ riêng, font Noto Sans có tiếng Việt, nhạc nền ambient và bốn cue âm thanh gốc.
- Kiểm thử mở rộng: 100 phiên thao tác ngẫu nhiên kèm replay, 30 lượt bot chiến thuật; hai chiến thuật thắng được từng màn 1–5 trong mô phỏng lõi, không bơm tiền/HP.
- Bộ kiểm thử lõi dùng chung với Unity EditMode; smoke test PlayMode và kiểm tra import asset đã được viết, **chưa chạy** trên máy này.

## Kiểm thử

Không cần Unity để chạy logic:

```sh
dotnet run --project Tests/CoreHarness/CoreHarness.csproj --configuration Release
```

Chưa cài Unity? Xem [hướng dẫn cài và công cụ chạy test](docs/TESTING_SETUP.vi.md). Có script `python tools/run_unity_tests.py --unity /path/to/Unity` tự chạy hai nhóm và kiểm tra báo cáo NUnit.

Trong Unity: **Window → General → Test Runner**, chạy EditMode rồi PlayMode. Chạy thêm **Eco Guardian → Validate Asset Imports** và checklist ở [docs/TEST_PLAN.vi.md](docs/TEST_PLAN.vi.md). CI `Core rules` tự chạy sau push; `Unity acceptance` chạy thủ công và cần Unity license hợp lệ trong GitHub Secrets. Không đưa license/token vào repo.

Xem [kết quả và giới hạn kiểm thử](docs/TEST_REPORT.vi.md), [kế hoạch triển khai](docs/PLAN.vi.md), [nguồn asset](docs/ASSETS.md), [kiến trúc](docs/ARCHITECTURE.md) và [các phần cần hoàn thiện](docs/KNOWN_LIMITATIONS.vi.md).

## Thư mục

```text
Assets/EcoGuardian/Core/        Mô phỏng C# độc lập Unity, dữ liệu và quy tắc
Assets/EcoGuardian/Runtime/     Giao diện, input, audio, lưu JSON
Assets/EcoGuardian/Editor/      Tạo LevelData, scene, validate asset, build
Assets/EcoGuardian/Resources/   Đồ họa, font, audio, LevelData
Assets/EcoGuardian/Scenes/      MainMenu, LevelSelect, Gameplay
Assets/EcoGuardian/Tests/       EditMode và PlayMode
Tests/CoreHarness/             Chạy kiểm thử logic bằng .NET 8
```

Dữ liệu lưu nằm trong `Application.persistentDataPath/eco-guardian-save.json`; bản trước đó được sao lưu thành `.bak`. Xóa file lưu để bắt đầu lại. Thử lại một màn luôn tạo session mới.

Kịch bản gốc: [Google Docs](https://docs.google.com/document/d/1Cbk61pEfqROliLwuMpTX3IvmT5am5hzci9YLLIYV298/edit). Bản chụp văn bản làm việc ở `docs/BRIEF.vi.txt`.
