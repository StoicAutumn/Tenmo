using Microsoft.AspNetCore.Mvc;
using TenmoServer.DAO;
using TenmoServer.DAO.Interfaces;
using TenmoServer.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly IAccountDao accountDao;

        public AccountController(IAccountDao accountDao)
        {
            this.accountDao = accountDao;
        }

        [HttpGet]
        public ActionResult<Account> GetAccountOfCurrentUser()
        {
            string userIdString = User.FindFirst("sub")?.Value;
            int userId = int.Parse(userIdString);

            Account account = accountDao.GetAccountByUserId(userId);

            if (account != null)
            {
                return Ok(account);
            }
            else
            {
                return NotFound();
            }
        }

        // GET /account/{id}
        [HttpGet("{id}")]
        public ActionResult<Account> GetAccountById (int id)
        {
            Account account = accountDao.GetAccountByUserId(id);

            if (account != null)
            {
                return Ok(account);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("/useraccount/{username}")]
        public ActionResult<Account> GetAccountByUsername(string username)
        {
            Account account = accountDao.GetAccountByUsername(username);

            if (account != null)
            {
                return Ok(account);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut("{id}")]
        public ActionResult<Account> UpdateAccountBalance(Account account)
        {
            Account updatedAccount = accountDao.UpdateAccount(account);
            
            if (updatedAccount != null)
            {
                return Ok(updatedAccount);

            }
            else
            {
                return NotFound();
            }
        }
    }
}
