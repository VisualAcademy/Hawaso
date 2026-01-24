namespace Hawaso.Controllers;

public class HomeController(ILogger<HomeController> logger) : Controller
{
    private readonly ILogger<HomeController> _logger = logger;

    public IActionResult Index()
    {
        _logger.LogInformation("Home/Index accessed");
        return View();
    }

    public IActionResult Privacy()
    {
        _logger.LogInformation("Home/Privacy accessed");
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogError(
            "Error page accessed. RequestId: {RequestId}",
            requestId
        );

        return View(new ErrorViewModel
        {
            RequestId = requestId
        });
    }
}
