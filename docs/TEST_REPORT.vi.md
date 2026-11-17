# Báo cáo kiểm thử — 17/11/2026

## Kết quả đã thực hiện

| Kiểm tra | Kết quả | Phạm vi chứng minh |
|---|---|---|
| Biên dịch lõi C# bằng Roslyn / .NET 8 | PASS | Cú pháp và kiểu dữ liệu lõi |
| Chạy bộ kiểm thử `RuleChecks` | **145 kiểm tra PASS** | Luật, chi phí, trạng thái, wave, boss, sao và hướng dẫn |
| Biên dịch Core + Runtime với .NET Standard 2.1 và UnityEngine API tham chiếu 2021.3.33 | PASS, không lỗi | Kiểm tra tĩnh các API đang sử dụng; **không phải** Unity 2022 import hoặc test |
| Tính toàn vẹn ảnh/font/WAV và Resources lookup | PASS | File tồn tại, đọc được, transparency/dimensions/audio header hợp lệ |
| Kiểm tra scene binding/GUID, build settings, manifest/asmdef JSON | PASS | Nhất quán văn bản và đường dẫn; chưa xác thực bằng Unity importer |
| Xem ảnh contact sheet của asset | Đã xem | Đánh giá riêng sprite thực vật, cây và pack máy móc; không phải screenshot gameplay |

Log kiểm thử đầy đủ: [CORE_TEST_OUTPUT.txt](CORE_TEST_OUTPUT.txt).

Máy có .NET 8.0.428 được cài vào thư mục công việc. Do CLI first-run không tạo được named mutex trên sandbox, bộ kiểm thử được biên dịch trực tiếp bằng `Roslyn/bincore/csc.dll` với bộ reference .NET 8 của SDK và chạy bằng `dotnet CoreHarness.dll`. Cùng các file Core/RuleChecks/Program được dùng trong project `Tests/CoreHarness/CoreHarness.csproj` và workflow GitHub. Không thay thế logic bằng stub Unity.

Reference assembly cho kiểm tra Runtime lấy từ gói NuGet `UnityEngine.Modules` 2021.3.33, chỉ đặt trong thư mục làm việc, không phân phối kèm game. Kiểm tra này phát hiện lỗi API/cú pháp cơ bản nhưng không chứng minh tính tương thích hoàn toàn với 2022.3.62f1.

## Những hành vi đã có assertion

- Đủ 5 wave/màn, đúng số ô, không đặt ô trên đường, không trùng ô.
- Giá xây/nâng cấp, thiếu tiền, ô bẫy/ô thường/ô ô nhiễm, bán hoàn tiền đúng và một lần.
- Thu nhập theo trạng thái, pause chuẩn bị/di chuyển/cooldown, thưởng chết một lần, phần thưởng wave cuối.
- Giáp, độc xuyên giáp, slow/poison mạnh nhất, hết hiệu ứng, boss kháng khống chế.
- Kỹ năng thiếu điều kiện/vị trí lỗi không thu tiền; Rain dập lửa; ultimate tăng damage nhưng không rút chu kỳ Bloom/Bomb.
- Lửa cảnh báo rồi cháy; cooldown khác 0 dừng giữa wave.
- Titan 70%/30%, vượt ngưỡng trực tiếp, summon, warning/khóa tháp, ngừng gọi quân ở overdrive.
- Titan chết còn quân chưa thắng; Titan chạm cây thua; không có tiền sau kết thúc.
- Hướng dẫn dừng giờ và giữ đủ tiền, chặn mua/nâng/bán sai thứ tự; retry sạch.
- Sao theo HP tối đa, replay giữ tốt nhất, mua nâng cấp không lặp và áp dụng bonus.

## Chưa chạy / chưa xác nhận

- **Unity Editor import, Unity EditMode và PlayMode:** chưa chạy vì máy không có Unity Editor/license. Các test này đã có trong repo.
- **Windows build và chạy exe:** chưa thực hiện. Không cung cấp binary đã xác minh.
- **Chơi thắng cả 5 màn, thời lượng 20–40 phút, cân bằng:** chưa thực hiện. Test bookkeeping dùng API Damage để kết thúc wave không phải chơi thử chiến thuật.
- **Trình bày trong engine, nhấp UI ở mọi tỷ lệ, âm lượng, FPS, GC:** chưa đo/kiểm tra.
- **JSON round-trip bằng Unity JsonUtility:** test đã viết, chưa chạy; kiểm thử SaveData trong bộ lõi không thay cho test serializer.

Chỉ gọi là hoàn tất nghiệm thu sau khi các mục còn lại trong [TEST_PLAN.vi.md](TEST_PLAN.vi.md) đạt và có bản build PC chạy được.
