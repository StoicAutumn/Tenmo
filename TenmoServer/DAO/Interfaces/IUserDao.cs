using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO.Interfaces
{
    public interface IUserDao
    {
        User GetUserById(int id);
        User GetUserByUsername(string username);
        User CreateUser(string username, string password);
        IList<User> GetUsers();
        IList<User> GetOtherUsernamesAndIds(int userId);
    }
}
