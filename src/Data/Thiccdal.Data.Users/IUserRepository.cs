namespace Thiccdal.Data.Users;
public interface IUserRepository
{
    User GetUser(string name, UserSource source);
}
