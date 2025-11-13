//using Duende.IdentityServer.Models;
//using Duende.IdentityServer.Services;
//using Microsoft.AspNetCore.Identity;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace Kaida.AuthServer.Services
//{
//    public class AspNetIdentityProfileService : IProfileService
//    {
//        private readonly UserManager<IdentityUser> _userManager;

//        public AspNetIdentityProfileService(UserManager<IdentityUser> userManager)
//        {
//            _userManager = userManager;
//        }

//        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
//        {
//            var user = await _userManager.GetUserAsync(context.Subject);
//            var claims = new List<Claim>
//            {
//                new Claim("sub", user.Id),
//                new Claim("email", user.Email ?? string.Empty)
//            };

//            context.IssuedClaims.AddRange(claims);
//        }

//        public async Task IsActiveAsync(IsActiveContext context)
//        {
//            var user = await _userManager.GetUserAsync(context.Subject);
//            context.IsActive = user != null;
//        }
//    }
//}