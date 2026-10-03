using App.Data.Entities;
using App.Infrastructure.Enums;

namespace App.Services.Context;

public interface IAuthContext
{
    User? GetUser();
    RolaUzytkownika GetRola();
    void SetUser(int userId, RolaUzytkownika rola);
}
