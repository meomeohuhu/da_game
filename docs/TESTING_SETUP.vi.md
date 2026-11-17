# Chạy kiểm thử khi chưa cài Unity

## Đường ngắn nhất: Unity Hub + Test Runner

Không cần cài công cụ kiểm thử trả phí hoặc gửi mật khẩu cho người khác.

1. Tải **Unity Hub** từ https://unity.com/download và đăng nhập tài khoản Unity của bạn.
2. Trong Hub, kích hoạt license phù hợp với tài khoản. Nếu đủ điều kiện dùng Unity Personal, có thể dùng lựa chọn miễn phí mà Hub cung cấp.
3. Cài **Unity Editor 2022.3.62f1 LTS**. Nếu Hub không liệt kê bản này, mở kho chính thức https://unity.com/releases/editor/archive và tìm đúng phiên bản; dùng nút mở bằng Hub. Chỉ cần Editor để test; thêm Windows Build Support khi muốn xuất bản PC.
4. Clone hoặc tải ZIP repo `meomeohuhu/da_game`; giải nén nếu tải ZIP.
5. Hub → **Projects → Add → Add project from disk**, chọn thư mục chứa `Assets`, `Packages`, `ProjectSettings`. Mở bằng đúng bản Editor, chờ import hoàn tất.
6. Mở **Window → General → Test Runner**. Chọn **EditMode → Run All**, sau đó **PlayMode → Run All**.
7. Chạy **Eco Guardian → Validate Asset Imports**. Console không được có lỗi đỏ. Mở `Assets/EcoGuardian/Scenes/MainMenu.unity`, bấm Play để kiểm tra hình, chuột và âm thanh.

Nếu Test Runner báo không có test, đó là lỗi cần điều tra; không coi là đã đạt. Kiểm tra Console và chờ Unity Test Framework import xong. Đừng nâng phiên bản Editor hoặc thay render pipeline trong lúc xử lý import.

## Chạy tự động bằng một lệnh

Sau khi Editor được cài và mở project/kích hoạt thành công ít nhất một lần, đóng project trong Editor trước khi chạy batch mode. Công cụ dưới đây dùng **Python 3, chỉ thư viện chuẩn**, không cần pip.

Windows PowerShell, tại thư mục repo:

```powershell
py tools/run_unity_tests.py --unity "C:\Program Files\Unity\Hub\Editor\2022.3.62f1\Editor\Unity.exe"
```

macOS:

```sh
python3 tools/run_unity_tests.py --unity /Applications/Unity/Hub/Editor/2022.3.62f1/Unity.app/Contents/MacOS/Unity
```

Linux hoặc khi đã đặt biến `UNITY_EDITOR`:

```sh
python3 tools/run_unity_tests.py
```

Không có Python thì dùng Test Runner trong Editor ở phần trên; Python không phải điều kiện để chơi game hoặc test thủ công.

Công cụ chạy EditMode rồi PlayMode thật, xuất:

- `TestResults/local-unity/EditMode.xml`, `PlayMode.xml`: báo cáo NUnit.
- `EditMode.log`, `PlayMode.log`: log import, compiler, license và test.
- `summary.json`: kết quả tổng hợp.

Mã thoát: **0** chỉ khi tất cả test được thực thi và đạt; **1** khi Unity lỗi/test fail/report thiếu hoặc rỗng; **2** khi chưa tìm thấy Editor. Script xóa XML cũ trước mỗi lần chạy để không báo đạt nhầm từ kết quả trước. Test bị skipped/inconclusive cũng không được coi là đã nghiệm thu đầy đủ. Script chạy với `-nographics`, vì vậy vẫn phải kiểm tra hình ảnh và tương tác trong cửa sổ Unity.

Có thể chỉ chạy một nhóm:

```sh
python3 tools/run_unity_tests.py --mode EditMode --unity /path/to/Unity
```

## GitHub Actions

Workflow **Core rules** luôn chạy được, không cần Unity/license: 145 assertion quy tắc, 100 phiên ngẫu nhiên kèm phát lại, 30 lượt bot chiến thuật và sáu test chống báo kết quả Unity sai. Log được tải thành artifact `core-test-results`.

Workflow **Unity acceptance (requires license)** chạy thủ công từ Actions. Lần thử thực tế ngày 17/11/2026 bị chặn trước khi chạy test: `Licensing method: <none>` và `License activation strategy could not be determined`. Xem lần chạy: https://github.com/meomeohuhu/da_game/actions/runs/44025731844.

Muốn dùng CI Unity, chủ repo cấu hình phương thức kích hoạt hợp lệ theo tài liệu hiện hành của GameCI:

- https://game.ci/docs/github/getting-started/
- https://game.ci/docs/github/activation/
- https://game.ci/docs/github/test-runner/

Workflow hiện hỗ trợ các biến GitHub Secrets `UNITY_LICENSE`, hoặc cặp `UNITY_EMAIL`/`UNITY_PASSWORD`; `UNITY_SERIAL` được truyền nếu có. Việc áp dụng được phương thức nào phụ thuộc license và dịch vụ Unity. Bước preflight báo thiếu cấu hình trước khi tải image Unity. Các mode chạy tuần tự để hạn chế xung đột seat license. Không đưa secret vào code, file `.env` đã commit, log hay chat. Dùng Hub trên máy cá nhân là lựa chọn đơn giản nếu không muốn cấu hình tài khoản cho CI.

## Dữ liệu lưu khi kiểm thử

PlayMode test đặt `ECO_GUARDIAN_SAVE_DIR` vào thư mục tạm riêng cho từng test và khôi phục sau test, để không đọc/ghi tiến trình đang chơi. Khi chạy game bình thường không cần đặt biến này; game vẫn lưu ở `Application.persistentDataPath`.

## Điều gì vẫn cần chơi trực tiếp?

Test tự động không xác nhận hình ảnh đẹp, font/glyph đúng trong mọi độ phân giải, nhấp chuột xuyên panel, âm lượng dễ chịu hay FPS trên máy bạn. Hãy làm thêm checklist `TEST_PLAN.vi.md`. Kết quả bot là kiểm tra mô phỏng với dữ liệu `Campaign`, không thay việc Unity import và đọc các ScriptableObject.
