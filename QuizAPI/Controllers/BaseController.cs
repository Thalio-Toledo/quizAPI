using Microsoft.AspNetCore.Mvc;
using QuizAPI.Helpers;

namespace QuizAPI.Controllers
{
    public class BaseController : ControllerBase
    {
        public IActionResult Resolve<T>(Result<T> result)
        {
            return StatusCode((int)result.StatusCode, result);
        }
    }
}
