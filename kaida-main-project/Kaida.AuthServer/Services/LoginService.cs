using Kaida.AuthServer.Data;
using Kaida.AuthServer.Entities;


namespace Kaida.AuthServer.Services;

public class LoginService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AuthServerDbContext _db;
    public LoginService(IHttpContextAccessor httpContextAccessor, AuthServerDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }


    public bool handleFailedLoginAttempts()
    {
        var context = _httpContextAccessor.HttpContext;
        if (context == null)
        {
            return false;
        }
        var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
     ?? context.Connection.RemoteIpAddress?.ToString();



        var hasFailedLoginAttempts = _db.FailedLoginAttempts.FirstOrDefault(f => f.IpAddress == ipAddress);

        if (hasFailedLoginAttempts != null && hasFailedLoginAttempts.LoginAttempts >= 5)
            return true;
        

        if (hasFailedLoginAttempts == null)
        {
            hasFailedLoginAttempts = new FailedLoginAttempts
            {
                Id = 0,
                IpAddress = ipAddress,
                LoginAttempts = 1,

            };
            _db.FailedLoginAttempts.Add(hasFailedLoginAttempts);
        }
        else
        {
            hasFailedLoginAttempts.LoginAttempts += 1;
        }
        _db.SaveChanges();

       
        return false;
    }
}

