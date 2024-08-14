using FeelinCute.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace FeelinCute.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly TokenServices _tokenService;

        public APIController(TokenServices tokenService)
        {
            _tokenService = tokenService;
        }
    }
}
