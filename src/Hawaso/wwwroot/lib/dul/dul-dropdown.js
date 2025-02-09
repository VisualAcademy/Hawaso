/* 드롭다운 열고 닫기 함수 */
function dulToggleDropdown() {
    document.getElementById("dulDropdownContent").classList.toggle("dul-show");
}

// 드롭다운 외부를 클릭하면 리스트 닫기
window.onclick = function (event) {
    if (!event.target.matches('.dul-btn')) {
        var dropdowns = document.getElementsByClassName("dul-dropdown-content");
        for (var i = 0; i < dropdowns.length; i++) {
            var openDropdown = dropdowns[i];
            if (openDropdown.classList.contains('dul-show')) {
                openDropdown.classList.remove('dul-show');
            }
        }
    }
}
