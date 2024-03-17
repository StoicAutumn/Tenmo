using System.Collections;
using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO.Interfaces
{
    public interface IAccountDao
    {
        IList<Account> GetAccounts();
        Account GetAccountByUserId(int userId);
        Account GetAccountByUsername(string username);
        Account UpdateAccount(Account account);
    }
}
