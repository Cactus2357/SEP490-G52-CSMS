/**
 * Centralized FaceIO Configuration and Helper Script for CSMS
 * Shared between Login, FaceRegister, and Admin/Manager RegisterFace views.
 */

// CENTRALIZED CONFIG: Configure your FaceIO Public App ID here (e.g. "fioXXXXX").
// Leave as empty string "" to run in Simulated/Offline Mode (using MediaPipe webcam simulation).
const CentralizedFaceIOConfig = {
    appId: "" 
};

const CSMSFaceIO = {
    // Check if real FaceIO library is loaded from CDN
    isLoaded: function() {
        return typeof faceIO !== 'undefined';
    },

    // Get the configured App ID.
    // Checks window.FACEIO_APP_ID first (can be set per-page or via the UI override input).
    // Falls back to CentralizedFaceIOConfig.appId.
    // Returns empty string if neither is set (simulated/offline mode).
    getAppId: function() {
        // window.FACEIO_APP_ID can be:
        //   - undefined (not set by page) → use centralized config
        //   - "" or null (explicitly cleared) → use centralized config
        //   - "fioXXXXX" (set by page or UI override) → use it
        const windowVal = window.FACEIO_APP_ID;
        if (windowVal && typeof windowVal === 'string' && windowVal.trim() !== '') {
            return windowVal.trim();
        }
        return CentralizedFaceIOConfig.appId;
    },

    /**
     * Perform Face Enrollment (Registration)
     * @param {Object} options - Configuration object
     * @param {string} options.username - The username of the employee (used for simulated unique Face ID)
     * @param {number} options.employeeId - The database ID of the employee
     * @param {string} options.email - The registered email of the employee
     * @param {function} options.onSuccess - Callback on success: function(facialId, isSimulated)
     * @param {function} options.onError - Callback on error: function(errorMessage)
     * @param {function} [options.onSimulate] - Custom simulation callback: function(simulatedId)
     */
    enroll: function(options) {
        // Use appId from options if explicitly provided and non-empty, else fall back to centralized config.
        const appId = (options.appId && options.appId.trim() !== '') ? options.appId.trim() : this.getAppId();
        const { username, employeeId, email, onSuccess, onError, onSimulate } = options;

        if (appId) {
            // Real FaceIO Enrollment Flow
            if (!this.isLoaded()) {
                onError("Thư viện FaceIO chưa được tải. Vui lòng kiểm tra kết nối mạng hoặc CORS.");
                return;
            }

            try {
                const faceio = new faceIO(appId);
                faceio.enroll({
                    locale: "auto",
                    payload: {
                        employeeId: employeeId,
                        email: email,
                        username: username
                    }
                }).then(userInfo => {
                    console.log("Real FaceIO Enrollment Success:", userInfo);
                    onSuccess(userInfo.facialId, false); // false = not simulated
                }).catch(errCode => {
                    console.error("Real FaceIO Enrollment Error Code:", errCode);
                    let errMsg = "Không hoàn tất đăng ký khuôn mặt.";
                    
                    if (typeof fioErrCode !== 'undefined') {
                        if (errCode === fioErrCode.PERMISSION_REFUSED) {
                            errMsg = "Quyền truy cập Camera bị từ chối.";
                        } else if (errCode === fioErrCode.NO_FACES_DETECTED) {
                            errMsg = "Không tìm thấy khuôn mặt trong ống kính.";
                        } else if (errCode === fioErrCode.DUPLICATE_ENROLLMENT) {
                            errMsg = "Khuôn mặt này đã được liên kết với tài khoản khác.";
                        }
                    }
                    onError(`Lỗi FaceIO: ${errMsg} (Mã: ${errCode})`);
                });
            } catch (err) {
                console.error("Lỗi khi gọi enroll trên faceIO:", err);
                onError("Lỗi khởi chạy tiến trình đăng ký FaceIO: " + err.message);
            }
        } else {
            // Simulated/Offline Enrollment Flow
            // We use 'fio_sim_' + username to ensure a unique simulated ID that matches during simulated login
            const simulatedId = "fio_sim_" + username.trim().toLowerCase();
            console.log(`[Simulated FaceIO] Enrolled username '${username}' with Simulated ID: ${simulatedId}`);

            if (onSimulate) {
                onSimulate(simulatedId);
            } else {
                onSuccess(simulatedId, true); // true = isSimulated
            }
        }
    },

    /**
     * Perform Face Authentication (Login matching)
     * @param {Object} options - Configuration object
     * @param {function} options.onSuccess - Callback on success: function(facialId, isSimulated)
     * @param {function} options.onError - Callback on error: function(errorMessage)
     * @param {function} [options.onSimulate] - Custom simulation callback: function(promptAction)
     */
    authenticate: function(options) {
        // Use appId from options if explicitly provided and non-empty, else fall back to centralized config.
        const appId = (options.appId && options.appId.trim() !== '') ? options.appId.trim() : this.getAppId();
        const { onSuccess, onError, onSimulate } = options;

        if (appId) {
            // Real FaceIO Authentication Flow
            if (!this.isLoaded()) {
                onError("Thư viện FaceIO chưa được tải. Vui lòng kiểm tra kết nối mạng hoặc CORS.");
                return;
            }

            try {
                const faceio = new faceIO(appId);
                faceio.authenticate({
                    locale: "auto"
                }).then(userData => {
                    console.log("Real FaceIO Authentication Success:", userData);
                    onSuccess(userData.facialId, false);
                }).catch(errCode => {
                    console.error("Real FaceIO Authentication Error Code:", errCode);
                    let errMsg = "Nhận diện khuôn mặt không thành công.";
                    
                    if (typeof fioErrCode !== 'undefined') {
                        if (errCode === fioErrCode.PERMISSION_REFUSED) {
                            errMsg = "Quyền truy cập Camera bị từ chối.";
                        } else if (errCode === fioErrCode.NO_FACES_DETECTED) {
                            errMsg = "Không nhận diện được khuôn mặt.";
                        }
                    }
                    onError(`Lỗi FaceIO: ${errMsg} (Mã: ${errCode})`);
                });
            } catch (err) {
                console.error("Lỗi khi gọi authenticate trên faceIO:", err);
                onError("Lỗi khởi chạy tiến trình nhận diện FaceIO: " + err.message);
            }
        } else {
            // Simulated/Offline Authentication Flow
            // Instead of a disruptive browser prompt(), render an inline input form
            const promptAction = () => {
                // Try to show an inline input if the faceStatus element exists
                const statusEl = document.getElementById('faceStatus');
                if (statusEl) {
                    statusEl.innerHTML = `
                        <div style="margin-top:8px;">
                            <label style="font-size:0.85rem;display:block;margin-bottom:4px;">
                                <strong>[Chế độ mô phỏng]</strong> Nhập Username để giả lập nhận diện:
                            </label>
                            <div style="display:flex;gap:8px;justify-content:center;flex-wrap:wrap;">
                                <input id="faceSimUsernameInput" type="text" 
                                    placeholder="Nhập tên tài khoản của bạn"
                                    style="padding:6px 10px;border:1px solid #aaa;border-radius:4px;font-size:0.9rem;min-width:200px;" />
                                <button id="faceSimSubmitBtn" type="button"
                                    style="padding:6px 14px;background:#111827;color:#fff;border:none;border-radius:4px;cursor:pointer;font-size:0.9rem;">
                                    Xác nhận
                                </button>
                            </div>
                        </div>`;

                    const input = document.getElementById('faceSimUsernameInput');
                    const btn = document.getElementById('faceSimSubmitBtn');

                    const handleSubmit = () => {
                        const username = input ? input.value : '';
                        // Restore normal status text
                        statusEl.innerHTML = '';
                        if (username && username.trim() !== '') {
                            const simulatedId = 'fio_sim_' + username.trim().toLowerCase();
                            console.log(`[Simulated FaceIO] Authenticated simulated ID: ${simulatedId}`);
                            onSuccess(simulatedId, true);
                        } else {
                            onError('Đã hủy quét giả lập hoặc Tên tài khoản trống.');
                        }
                    };

                    if (btn) btn.addEventListener('click', handleSubmit);
                    if (input) input.addEventListener('keydown', (e) => { if (e.key === 'Enter') handleSubmit(); });
                    if (input) setTimeout(() => input.focus(), 50);
                } else {
                    // Fallback to prompt() if faceStatus element not available
                    const username = prompt('Nhập Username để giả lập nhận diện khuôn mặt:');
                    if (username && username.trim() !== '') {
                        const simulatedId = 'fio_sim_' + username.trim().toLowerCase();
                        console.log(`[Simulated FaceIO] Authenticated simulated ID: ${simulatedId}`);
                        onSuccess(simulatedId, true);
                    } else {
                        onError('Đã hủy quét giả lập hoặc Tên tài khoản trống.');
                    }
                }
            };

            if (onSimulate) {
                onSimulate(promptAction);
            } else {
                promptAction();
            }
        }
    }
};
