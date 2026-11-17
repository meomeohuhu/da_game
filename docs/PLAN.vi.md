# Kế hoạch triển khai và nghiệm thu

## Mục tiêu

Một game Unity 2D PC có chiến dịch hoàn chỉnh theo tài liệu **Eco Guardian – Lời Thì Thầm Của Đại Ngàn**: 5 màn, 25 wave, tháp thực vật chống máy khai thác, kết thúc bằng Titan. Chuột là điều khiển chính. Tập trung khả năng đọc chiến trường, chất lượng phối hình và sự rõ ràng của các cảnh báo. Chỉ xem là bản bàn giao hoàn chỉnh sau khi mở, chơi thử và build bằng Unity thành công.

## Quyết định kỹ thuật

- Unity **2022.3.62f1 LTS**, Built-in, Windows 64-bit. Mục tiêu tham khảo 1600×1000; giao diện co theo kích thước cửa sổ, có vùng viền ở tỷ lệ khác.
- Mô phỏng C# độc lập Unity để kiểm tra luật và tránh dữ liệu cấu hình bị sửa khi chơi. Mỗi lượt tạo một `Simulation` mới.
- Cấu hình màn lưu bằng `LevelData` ScriptableObject đã được đưa vào repo. `Campaign.cs` là dữ liệu khởi tạo; `Rules` là nguồn chỉ số tháp/địch hiện tại.
- Bản hiện tại dựng cảnh bằng hình 2D và IMGUI. Đưa sang Tilemap/prefab/uGUI là giai đoạn biên tập mỹ thuật, không tự nhận bản này đã có pipeline Tilemap trong brief.
- Asset CC0 hoặc giấy phép cho phép phân phối lại trong repo công khai; không dùng asset trả phí/giấy phép mơ hồ. Lưu nguồn và license cùng asset.
- Lưu JSON cục bộ. Sao tốt nhất được giữ riêng khỏi sao đã chi; không lưu giữa wave.

## Các mốc có đầu ra và tiêu chí đạt

| Mốc | Công việc | Đầu ra kiểm tra được | Điều kiện qua |
|---|---|---|---|
| 1. Chốt yêu cầu | Chuyển brief thành luật, tách phần bắt buộc/mở rộng | Brief snapshot, plan, danh sách nghiệm thu | Có đủ 5 màn và luật thắng/thua không mâu thuẫn |
| 2. Lõi một màn | Đường, spawn, Cây Gai, HP, tiền, 5 wave | Session chơi được; kiểm thử logic | Không trừ tiền sai; chết chỉ thưởng một lần; không kết thúc wave sớm |
| 3. Chiến thuật | 5 tháp/3 cấp, 5 địch thường, status, ưu tiên, bán/sửa | Core và UI tương tác | Slow/độc không stack; giáp/đóng băng/nhiễu đúng; pause dừng timer |
| 4. Chiến dịch | 5 map, mở khóa, lửa, kỹ năng, boss/Titan | 25 wave; tiến trình màn | Titan đúng ngưỡng; hết hỗ trợ mới thắng; chạm cây xử lý thua trước |
| 5. Mỹ thuật và UX | Kiểm tra asset, phối cảnh, audio, hội thoại, menu/HUD | Bản Unity chơi trực tiếp | Phân biệt được tháp/địch/cấp; đường và ô xây rõ; chữ Việt không lỗi |
| 6. Kiểm thử và cân bằng | EditMode, PlayMode, chơi thử cả 5 màn, đo thời lượng | Báo cáo test, video/screenshot thật, bảng cân bằng | Không lỗi chặn chơi; 20–40 phút là số đo thực tế, không phải giả định |
| 7. Đóng gói | Build Windows; thử trên máy sạch | ZIP exe + data; mã nguồn GitHub; hướng dẫn | Mở không cần Editor; tải/lưu/chơi lại ổn; ghi rõ mọi hạn chế còn lại |

## Trạng thái tại commit đầu

- Đã viết các mốc 1–4 ở mức mã nguồn và dữ liệu; đã kiểm thử lõi tự động.
- Mốc 5 có asset nhập repo, UI, âm thanh và hội thoại; **chưa đánh giá hình ảnh trực tiếp trong Unity**.
- Mốc 6 đã chạy kiểm thử C#; các kiểm thử Unity được viết nhưng chưa chạy. Chơi thử cân bằng chưa thực hiện.
- Mốc 7 có công cụ build và hướng dẫn; **chưa có bản Windows được xác minh**.

## Lịch tham khảo khi tiếp tục bằng Unity

Đây là thứ tự ưu tiên cho một người phát triển, không phải cam kết thời hạn:

1. **Ngày 1–2:** import Unity, sửa mọi lỗi Console, chạy EditMode/PlayMode, kiểm tra chuột/UI/tiếng Việt; build thử sớm.
2. **Ngày 3–5:** chuyển các map sang Tilemap và tạo prefab nếu nhóm cần chỉnh kéo thả; hoàn thiện sông/nước, vệt ô nhiễm, silhouette robot, animation ba cấp.
3. **Ngày 6–8:** chơi chiến dịch với ít nhất ba cách phối tháp, ghi HP/thời gian/thu nhập từng wave, điều chỉnh đường đi và nhịp spawn trước khi đổi chỉ số trong brief.
4. **Ngày 9–10:** cinematic mở/kết, âm nhạc riêng từng màn, âm thanh kỹ năng, tối ưu draw/allocation và kiểm tra các tỷ lệ màn hình.
5. **Ngày 11–12:** regression, build Windows, chạy trên máy sạch, chụp/video thực tế, đóng gói và gắn release sau khi tất cả mục bắt buộc đạt.

Nếu có lỗi chặn chơi ở mốc trước, sửa trước khi tăng độ bóng hình ảnh. Không thêm multiplayer, nhiệm vụ hoặc nội dung ngoài brief.

## Cổng chất lượng trước khi gọi là hoàn tất

- Unity import và biên dịch không lỗi; asset không bị missing reference.
- Tất cả EditMode/PlayMode test pass; hoàn thành checklist thủ công.
- Chơi thắng hợp lệ cả 5 màn không sửa tiền/HP; xác nhận boss cuối tiêu diệt được và game có thể thua.
- Không nhấp xuyên UI, softlock hướng dẫn, kẹt wave, hay đồng thời thắng/thua.
- Đo thời lượng, âm lượng, FPS và allocation trên máy mục tiêu; không khẳng định 60 FPS từ thiết lập targetFrameRate.
- Có bản PC chạy thử thành công và đủ giấy phép asset trong repo.
