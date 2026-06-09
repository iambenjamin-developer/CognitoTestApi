using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CognitoTestApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthTestingController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        [HttpGet("/WeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        #region Auth Endpoints for testing

        [HttpGet("auth-public")]
        [AllowAnonymous]
        public IActionResult Public()
        {
            return Ok("Public endpoint");
        }

        [HttpGet("auth-me")]
        public IActionResult Me()
        {
            return Ok("Authenticated");
        }

        [HttpGet("auth-admin")]
        //[Authorize(Policy = AppPolicies.RequireAdmin)]
        //[Authorize(Roles = AppRoles.Administrator)]
        public IActionResult AdminOnly()
        {
            return Ok("Admin area");
        }

        [HttpGet("auth-user")]
        //[Authorize(Policy = AppPolicies.RequireUser)]
        //[Authorize(Roles = AppRoles.User)]
        public IActionResult UserOnly()
        {
            return Ok("User area");
        }

        [HttpGet("auth-dashboard")]
        //[Authorize(Policy = AppPolicies.RequireAdminOrUser)]
        //[Authorize(Roles = AppRoles.Administrator + "," + AppRoles.User)]
        public IActionResult Dashboard()
        {
            return Ok("Admin or User area");
        }

        #endregion
    }
}
