(function () {

    // ---------------------------------------------------------
    // 0) 로그인/공개 페이지에서는 모든 자동 체크 비활성화
    // ---------------------------------------------------------
    var path = window.location.pathname.toLowerCase();

    if (
        path.startsWith('/identity/account/login') ||
        path.startsWith('/identity/account/register') ||
        path.startsWith('/identity/account/forgotpassword') ||
        path.startsWith('/identity/account/resetpassword')
    ) {
        console.log("🔕 auth/ping + inactivity auto-check disabled on Identity pages.");
        return;
    }

    console.log("🔧 auth/ping + inactivity auto-check script loaded.");

    // ---------------------------------------------------------
    // 1) (A) 서버 세션 만료 체크: 31분마다 /auth/ping
    // ---------------------------------------------------------
    setInterval(function () {

        console.log("⏱️ Sending ping request to /auth/ping ...");

        fetch('/auth/ping')
            .then(r => {
                console.log("📡 Response received. Status:", r.status);

                if (r.status === 401) {
                    console.log("❌ Not authenticated (401). Redirecting to login...");

                    const currentUrl = window.location.pathname + window.location.search;
                    const loginUrl = "/Identity/Account/Login?returnUrl=" + encodeURIComponent(currentUrl);

                    console.log("➡️ Redirect URL:", loginUrl);
                    window.location.href = loginUrl;
                } else {
                    console.log("✅ User still authenticated.");
                }
            })
            .catch(err => {
                console.log("⚠️ Ping request failed:", err);
            });

    }, 31 * 60 * 1000); // 31분


    // ---------------------------------------------------------
    // 2) (B) 클라이언트 inactivity 감지: 25분 동안 아무 이벤트 없으면 로그아웃
    // ---------------------------------------------------------

    var INACTIVITY_LIMIT = 25 * 60 * 1000; // 25분
    var inactivityTimer = null;

    function resetInactivityTimer() {
        if (inactivityTimer) {
            clearTimeout(inactivityTimer);
        }

        inactivityTimer = setTimeout(function () {
            console.log("😴 User inactive for 25 minutes. Redirecting to login...");

            const currentUrl = window.location.pathname + window.location.search;
            const loginUrl = "/Identity/Account/Login?returnUrl=" + encodeURIComponent(currentUrl);

            window.location.href = loginUrl;
        }, INACTIVITY_LIMIT);
    }

    // 사용자 활동 이벤트 → 타이머 리셋
    ['click', 'mousemove', 'keydown', 'scroll', 'touchstart'].forEach(function (eventName) {
        document.addEventListener(eventName, resetInactivityTimer, true);
    });

    // 최초 타이머 시작
    resetInactivityTimer();

})();
