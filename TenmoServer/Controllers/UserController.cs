using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TenmoServer.DAO;
using TenmoServer.DAO.Interfaces;
using TenmoServer.Models;



namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserDao userDao;

        public UserController(IUserDao userDao)
        {
            this.userDao = userDao;
        }

        //List of usernames minus the current user
        [HttpGet]
        public ActionResult<IList<User>> GetOtherUsernamesAndIds()
        {
            string userIdString = User.FindFirst("sub")?.Value;
            int userId = int.Parse(userIdString);

            IList<User> listOfUsers = userDao.GetOtherUsernamesAndIds(userId);
            return Ok(listOfUsers);
        }

        [HttpGet("{id}")]
        public ActionResult<User> GetUserById(int id)
        {
            User user = userDao.GetUserById(id);
            user.PasswordHash = "";
            user.Salt = "";
            user.Email = "";

            if (user != null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
