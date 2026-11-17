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
- **Chơi thắng cả 5 màn trong Unity, thời lượng 20–40 phút, cân bằng bởi người chơi:** chưa thực hiện. Test bookkeeping dùng API Damage để kết thúc wave không phải chơi thử chiến thuật.
- **Trình bày trong engine, nhấp UI ở mọi tỷ lệ, âm lượng, FPS, GC:** chưa đo/kiểm tra.
- **JSON round-trip bằng Unity JsonUtility:** test đã viết, chưa chạy; kiểm thử SaveData trong bộ lõi không thay cho test serializer.

Chỉ gọi là hoàn tất nghiệm thu sau khi các mục còn lại trong [TEST_PLAN.vi.md](TEST_PLAN.vi.md) đạt và có bản build PC chạy được.

## Bổ sung công cụ và kiểm thử mở rộng

- **100 phiên thao tác ngẫu nhiên trên 5 màn, 83.320 bước**, mỗi phiên được chạy lại cùng seed để so trạng thái. Không phát hiện tiền âm, xây chồng ô, charge thao tác lỗi, enemy/HP/timer bất hợp lệ, thắng sớm hoặc trạng thái kết thúc tiếp tục thay đổi. Số bước trên không cộng lần replay; tổng số bước hai lượt là 166.640. Tất cả bot ngẫu nhiên đều thua; đây là kiểm tra độ bền luật, không phải kiểm tra cân bằng.
- **30 lượt bot chiến thuật, 6 cách chơi × 5 màn**, chỉ dùng Plant/Upgrade/Repair/Cast/StartWave/Tick. Không thêm tiền/HP, không gọi Damage để giết hộ. Hai chiến thuật ưu tiên nâng cấp thắng được từng màn 1–5 ở seed 42; riêng `upgrade-four` là smoke test bắt buộc phải thắng cả năm màn. Màn 5 kết thúc còn 90 HP với bot này. Đây là mô phỏng lõi từng màn độc lập; chưa chạy UI hay toàn bộ chiến dịch trong Unity.
- **6 test Python đạt**, xác nhận công cụ tổng hợp NUnit không nhận XML rỗng, có test fail/skipped, sai định dạng hoặc thiếu số liệu là PASS. Đây là test của công cụ đọc báo cáo, không phải test Unity.
- Runtime sau thay đổi thư mục lưu test vẫn biên dịch tĩnh với Unity API reference được nêu ở trên.
- Đã bổ sung PlayMode kiểm tra scene, stage bị khóa và retry; EditMode kiểm tra đủ năm LevelData thật. **Chưa chạy các test Unity này**.
- Đã thực sự dispatch GameCI EditMode/PlayMode: [run 44025731844](https://github.com/meomeohuhu/da_game/actions/runs/44025731844). Cả hai dừng ở kích hoạt với `Licensing method: <none>` / `License activation strategy could not be determined`, trước khi test thực thi. Không ghi nhận là lỗi gameplay hay test pass. Workflow mới kiểm tra cấu hình kích hoạt trước bước tải Unity.
- Chạy `tools/run_unity_tests.py` trên máy này trả về **BLOCKED / exit 2: Unity Editor not found**, đúng với môi trường. Không tạo XML kết quả giả.

Công cụ và cách cài Editor: [TESTING_SETUP.vi.md](TESTING_SETUP.vi.md). Log C# cập nhật nằm trong `CORE_TEST_OUTPUT.txt`.
