# Các giới hạn hiện tại

Đây là bản nguồn Unity đầu tiên, chưa phải release đã nghiệm thu. Không có Unity Editor hoặc Unity license trên máy thực hiện, nên chưa thể xác nhận import, Play Mode, âm thanh thực tế hoặc xuất/chạy `.exe`.

Các phần cần tiếp tục để đáp ứng mức hoàn thiện toàn bộ brief:

- Chơi thử và cân bằng đủ 5 màn với tài nguyên hợp lệ; thời lượng 20–40 phút chưa đo. Kiểm thử tự động tiêu diệt địch bằng API chỉ xác nhận bookkeeping, không chứng minh màn chơi thắng được bằng chiến thuật.
- Dựng Tilemap/prefab để chỉnh map trong Editor; hiện bản đồ được vẽ bằng GUI theo waypoint. Bố cục các màn dùng chung trục đường chính và thêm nhánh; chưa phải năm bản đồ mỹ thuật độc lập.
- Có ô ô nhiễm và lửa nhưng chưa có lớp sông/nước hoàn chỉnh. Không cho xây ngoài các ô được chỉ định.
- Hội thoại và panel kết thúc đã có; cinematic lướt camera, máy chặt cây, tái sinh rừng và cử chỉ nhân vật An chưa được dựng thành animation.
- Tháp có hình riêng theo loại; cấp được phân biệt bằng kích thước và dấu cấp, chưa có ba bộ sprite/animation riêng. Đóng băng/độc/nhiễu/khóa có chỉ báo; cần kiểm tra glyph biểu tượng của font trong Unity.
- Đạn đang là hiệu ứng hình ảnh; sát thương xử lý trực tiếp khi bắn, chưa dùng projectile va chạm.
- Một nền ambient dùng chung cho chiến dịch; chưa có nhạc/soundscape riêng từng màn và voice-over. Thêm hiệu ứng kỹ năng, nâng cấp và kết thúc ở giai đoạn audio polish.
- Chưa đo hiệu năng/GC; OnGUI và vòng quét mục tiêu có thể cần tối ưu khi tăng quy mô.
- File `.bak` được giữ để khôi phục thủ công; chưa tự khôi phục từ backup khi save JSON bị hỏng.
- Chưa có binary Windows/WebGL trong repo; không coi workflow Unity thủ công là test đã chạy. Cần license Unity hợp lệ để chạy workflow đó.

Xem `PLAN.vi.md` để biết thứ tự hoàn thiện và `TEST_PLAN.vi.md` để nghiệm thu trước khi tạo release.
