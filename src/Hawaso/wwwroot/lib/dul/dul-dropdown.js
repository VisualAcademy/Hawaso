document.addEventListener("DOMContentLoaded", function () {
    // 모든 dul-btn 버튼에 클릭 이벤트 추가
    document.querySelectorAll(".dul-btn").forEach(button => {
        button.addEventListener("click", function (event) {
            event.stopPropagation(); // 이벤트 전파 방지

            // 현재 버튼과 연결된 드롭다운 찾기
            let dropdown = this.nextElementSibling;
            if (dropdown && dropdown.classList.contains("dul-dropdown-content")) {
                // 모든 드롭다운 닫기 (하나만 열도록)
                document.querySelectorAll(".dul-dropdown-content").forEach(d => {
                    if (d !== dropdown) {
                        d.classList.remove("dul-show");
                    }
                });

                // 현재 드롭다운 토글
                dropdown.classList.toggle("dul-show");
            }
        });
    });

    // 문서 클릭 시 모든 드롭다운 닫기 (외부 클릭 감지)
    document.addEventListener("click", function () {
        document.querySelectorAll(".dul-dropdown-content").forEach(dropdown => {
            dropdown.classList.remove("dul-show");
        });
    });
});
