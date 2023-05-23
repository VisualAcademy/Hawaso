// Microsoft의 Identity 프레임워크를 사용하기 위해 참조를 추가합니다.
using Microsoft.AspNetCore.Identity;

// 'Hawaso.Areas.Identity.Models' 네임스페이스는 Identity 관련 모델들을 포함하고 있습니다.
namespace Hawaso.Areas.Identity.Models
{
    // 'ApplicationUser' 클래스는 사용자 정보를 확장하기 위해 'IdentityUser' 클래스를 상속받습니다.
    // 이 클래스를 통해 기본적인 사용자 정보 이외에도 추가적인 사용자 정보를 관리할 수 있습니다.
    public class ApplicationUser : IdentityUser
    {
        // 'FirstName' 프로퍼티는 사용자의 이름(성이 아닌 부분)을 저장합니다. 
        // 이 정보는 선택적이므로 null일 수 있습니다(? 표시).
        public string? FirstName { get; set; }

        // 'LastName' 프로퍼티는 사용자의 성을 저장합니다. 
        // 이 정보도 선택적이므로 null일 수 있습니다(? 표시).
        public string? LastName { get; set; }

        // 'Timezone' 프로퍼티는 사용자의 시간대를 저장합니다. 
        // 이를 통해 사용자별로 다른 시간대를 처리할 수 있습니다. 이 정보도 선택적입니다(? 표시).
        public string? Timezone { get; set; }

        // TODO: 필요한 추가 프로퍼티들을 여기에 선언하세요.
    }
}
