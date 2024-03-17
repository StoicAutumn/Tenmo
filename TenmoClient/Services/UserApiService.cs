using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class UserApiService : AuthenticatedApiService
    {
        public UserApiService(string apiUrl) : base(apiUrl)
        {

        }
        public IList<User> GetOtherUsernamesAndIds()
        {
            RestRequest request = new RestRequest("/user");

            IRestResponse<IList<User>> response = client.Get<IList<User>>(request);

            CheckForError(response);

            return response.Data;
        }

        public User GetUserById(int id)
        {
            RestRequest request = new RestRequest("/user/" + id);

            IRestResponse<User> response = client.Get<User>(request);

            CheckForError(response);

            return response.Data;
        }
    }
}
