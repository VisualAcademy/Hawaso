using Microsoft.AspNetCore.Mvc;

namespace Hawaso.Controllers
{
    /// <summary>
    /// 특정 Application의 JSON Form Data를 HTML로 변환하여 보여주는 컨트롤러
    /// </summary>
    public class ApplicationFormDataViewerController : Controller
    {
        public IActionResult Index(string formName = "", long id = 0, long applicationId = 0)
        {
            return View();
        }
    }
}
