using CountryModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WeatherServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController(
        UserManager<WorldCityUser> userManager,
        JWTHandler jWTHandler
    ) : ControllerBase
    {
        [HttpPost("login")]
        public async void Login(string username, string password)
        {

        }
    }
}
