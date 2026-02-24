
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RPA_Api.Model;
using RPA_Api.Repository;


namespace RPA_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController:ControllerBase
    {
        private readonly IUserRepository _userRepository;
        public UserController(IUserRepository userRepository) { 
            _userRepository = userRepository;
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromBody] Model.UserRecord request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (request == null) 
                return BadRequest(new { error = "No user sent" });

            try
            {
                var result = _userRepository.CreateAsync(request);
                return Ok(new
                {
                    success = true,
                    id = request.Id
                });
            }
            catch (Exception ex) {
                return StatusCode(500, new
                {
                    success = false,
                    error = ex.Message
                });

            }

            

            
        }
    }
}
