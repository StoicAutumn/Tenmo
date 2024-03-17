using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenmoClient.Models;
using System.Net.Http;

namespace TenmoClient.Services
{
    public class AccountApiService : AuthenticatedApiService
    {
        public AccountApiService(string apiUrl) : base(apiUrl)
        {

        }

        public IList<Account> GetAccounts()
        {
            RestRequest request = new RestRequest("/account");

            IRestResponse<IList<Account>> response = client.Get<IList<Account>>(request);

            CheckForError(response);

            return response.Data;
        }

        public Account GetAccountByUserId(int userId)
        {
            RestRequest request = new RestRequest("/account/" + userId);

            IRestResponse<Account> response = client.Get<Account>(request);

            CheckForError(response);

            return response.Data;
        }

        public Account GetAccountByUsername(string username)
        {
            RestRequest request = new RestRequest("/useraccount/" + username);

            IRestResponse<Account> response = client.Get<Account>(request);

            CheckForError(response);

            return response.Data;
        }

        public Account GetAccountOfCurrentUser()
        {
            RestRequest request = new RestRequest("/account");

            IRestResponse<Account> response = client.Get<Account>(request);

            CheckForError(response);

            return response.Data;
        }

        public Account UpdateAccount(int userId, Account account)
        {
            RestRequest request = new RestRequest("/account/" + userId);
            request.AddJsonBody(account);

            IRestResponse<Account> response = client.Put<Account>(request);

            CheckForError(response);

            return response.Data;
        }
    }
}
