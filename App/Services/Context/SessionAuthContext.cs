using App.Data;
using App.Data.Entities;
using App.Infrastructure.Enums;

namespace App.Services.Context;

public class SessionAuthContext : IAuthContext
{
    public const string UserIdKey = "demo.userId";
    public const string RolaKey = "demo.rola";

    private readonly IHttpContextAccessor _http;
    private readonly AppDbContext _db;

    public SessionAuthContext(IHttpContextAccessor http, AppDbContext db)
    {
        _http = http;
        _db = db;
    }

    public User? GetUser()
    {
        var ctx = _http.HttpContext;
        if (ctx is null) return null;
        var id = ctx.Session.GetInt32(UserIdKey);
        if (id is null) return null;
        return _db.Users.FirstOrDefault(u => u.Id == id.Value);
    }

    public RolaUzytkownika GetRola()
    {
        var ctx = _http.HttpContext;
        var txt = ctx?.Session.GetString(RolaKey);
        return Enum.TryParse<RolaUzytkownika>(txt, out var r) ? r : RolaUzytkownika.Autor;
    }

    public void SetUser(int userId, RolaUzytkownika rola)
    {
        var ctx = _http.HttpContext ?? throw new InvalidOperationException("Brak HttpContext.");
        ctx.Session.SetInt32(UserIdKey, userId);
        ctx.Session.SetString(RolaKey, rola.ToString());
    }
}
