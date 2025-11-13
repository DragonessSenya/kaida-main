using Kaida.AuthServer.Data;
using Kaida.AuthServer.Models;

namespace Kaida.AuthServer.Services;

    public class UserService(AuthServerDbContext db)
    {
    private readonly AuthServerDbContext _db = db;
    public User? ValidateUser(string username, string password)
    {
        var validatedUser = _db.Users
            .FirstOrDefault(u => u.UserName == username && u.PasswordHash == password);

        return validatedUser;
    }
}

