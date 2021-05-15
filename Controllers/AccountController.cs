using IM_Api.Db;
using IM_Api.Models;
using IM_Api.Tools;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IM_Api.Controllers
{
    //[EnableCors("any")]
    [Route("[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private UserManager<IdentityUser> userManager;
        private SignInManager<IdentityUser> signinManager;
        private UserDbContext context;

        public AccountController(UserDbContext _context, UserManager<IdentityUser> _userManager,
            SignInManager<IdentityUser> _signinManager)
        {
            this.userManager = _userManager;
            this.signinManager = _signinManager;
            this.context = _context;
        }

        [HttpPost("Register")]
        public async Task<ContentResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };

                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    return Content("ok");
                }
                else
                {
                    var str = "";
                    foreach(var err in result.Errors)
                    {
                        str += err.ToString();
                    }
                    return Content(str);
                }
            }
            else
            {
                return Content("err2");
            }
        }

        [HttpPost("Login")]
        public async Task<ContentResult> Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                var result = userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);
      
                if (result == PasswordVerificationResult.Success)               
                {
                    return Content(JwtTools.GenerateJwtToken(model.Email, "web"));
                }
                else
                {
                    return Content("err1");
                }
            }
            else
            {
                return Content("err2");
            }
        }
    }
}

