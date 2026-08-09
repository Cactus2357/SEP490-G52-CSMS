/**
 * CSMS Self-Hosted Face Recognition Utility (face-api.js Wrapper)
 * Loaded on Face Login and Face Registration views.
 * 
 * Runs face detection, landmark detection, and feature extraction (128-dimensional vector)
 * entirely in the browser using Tensorflow.js WebGL/WASM runtimes.
 */

const CSMSFaceAPI = {
    _modelsLoadedPromise: null,

    /**
     * Load the required face-api.js model weights from '/models/' directory.
     * Caches the loading promise to prevent redundant loads.
     */
    loadModels: function () {
        if (!this._modelsLoadedPromise) {
            console.log("[CSMSFaceAPI] Loading face-api.js models from local server...");
            this._modelsLoadedPromise = Promise.all([
                faceapi.nets.ssdMobilenetv1.loadFromUri('/models'),
                faceapi.nets.faceLandmark68Net.loadFromUri('/models'),
                faceapi.nets.faceRecognitionNet.loadFromUri('/models')
            ]).then(() => {
                console.log("[CSMSFaceAPI] All models loaded successfully.");
            }).catch(err => {
                console.error("[CSMSFaceAPI] Failed to load models:", err);
                this._modelsLoadedPromise = null; // Reset on failure so we can try again
                throw err;
            });
        }
        return this._modelsLoadedPromise;
    },

    /**
     * Detect a single face in a video element and extract its landmarks and 128-d descriptor.
     * @param {HTMLVideoElement} videoEl - The webcam video element.
     * @returns {Promise<Object|null>} - Returns the face detection result or null.
     */
    detectFace: async function (videoEl) {
        await this.loadModels();
        
        if (!videoEl || videoEl.paused || videoEl.ended || videoEl.readyState < 2) {
            return null;
        }

        try {
            // SsdMobilenetv1Options uses default settings: minConfidence: 0.5
            const result = await faceapi.detectSingleFace(videoEl, new faceapi.SsdMobilenetv1Options({ minConfidence: 0.5 }))
                .withFaceLandmarks()
                .withFaceDescriptor();

            return result;
        } catch (err) {
            console.error("[CSMSFaceAPI] Error detecting face:", err);
            return null;
        }
    },

    /**
     * Check if a detected face is positioned well within the camera frame constraints.
     * @param {Object} detection - The detection result from detectFace.
     * @param {HTMLVideoElement} videoEl - The source video element.
     * @returns {Object} - { valid: boolean, message: string }
     */
    validatePosition: function (detection, videoEl) {
        if (!detection) {
            return {
                valid: false,
                message: "Không tìm thấy khuôn mặt. Vui lòng nhìn thẳng vào camera."
            };
        }

        const box = detection.detection.box;
        const width = videoEl.videoWidth || 640;
        const height = videoEl.videoHeight || 480;

        // Margin constraints to ensure face is not cut off
        const edgeMargin = 30; // pixels
        const isNearEdge = box.x <= edgeMargin ||
                           box.y <= edgeMargin ||
                           (box.x + box.width) >= (width - edgeMargin) ||
                           (box.y + box.height) >= (height - edgeMargin);

        // Size constraint: Face must cover at least 20% of webcam frame width
        const isTooSmall = box.width < (width * 0.20);

        if (isNearEdge) {
            return {
                valid: false,
                message: "CẢNH BÁO: Khuôn mặt bị lệch hoặc bị khuất! Hãy căn giữa khung hình."
            };
        }

        if (isTooSmall) {
            return {
                valid: false,
                message: "CẢNH BÁO: Quá xa! Di chuyển khuôn mặt lại gần camera hơn."
            };
        }

        return {
            valid: true,
            message: "Điểm đặc điểm đầy đủ. Giữ yên vị trí..."
        };
    },

    /**
     * Capture a mirrored image of the current video frame on a canvas, returning base64.
     * @param {HTMLVideoElement} videoEl - The source video.
     * @returns {string} - The JPEG base64 data URL.
     */
    captureSnapshot: function (videoEl) {
        try {
            const canvas = document.createElement('canvas');
            canvas.width = videoEl.videoWidth || 640;
            canvas.height = videoEl.videoHeight || 480;
            const ctx = canvas.getContext('2d');

            // Mirror image horizontally to match the display preview
            ctx.translate(canvas.width, 0);
            ctx.scale(-1, 1);

            ctx.drawImage(videoEl, 0, 0, canvas.width, canvas.height);
            return canvas.toDataURL('image/jpeg');
        } catch (err) {
            console.error("[CSMSFaceAPI] Error capturing face snapshot:", err);
            return "https://placehold.co/400x300?text=Camera+Snapshot+Error";
        }
    }
};
