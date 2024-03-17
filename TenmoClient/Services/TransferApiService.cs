using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenmoClient.Models;

namespace TenmoClient.Services
{
    public class TransferApiService : AuthenticatedApiService
    {
        public TransferApiService(string apiUrl) : base(apiUrl)
        {
            
        }
        public Transfer GetTransferById(int transferId)
        {
            RestRequest request = new RestRequest("/transfer/" + transferId);
            IRestResponse<Transfer> response = client.Get<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }

        public IList<UsernameTransfer> GetListOfTransfersByAccountId(int accountId)
        {
            RestRequest request = new RestRequest("/transfer/" + accountId + "/list");
            IRestResponse<IList<UsernameTransfer>> response = client.Get<IList<UsernameTransfer>>(request);
            CheckForError(response);
            return response.Data;
        }

        public decimal GetAmountById(int transferId)
        {
            RestRequest request = new RestRequest("/transfer/" + transferId + "/amount");
            IRestResponse<decimal> response = client.Get<decimal>(request);
            CheckForError(response);
            return response.Data;
        }

        public Transfer AddTransfer(Transfer transfer)
        {
            RestRequest request = new RestRequest("/transfer");
            request.AddJsonBody(transfer);
            IRestResponse<Transfer> response = client.Post<Transfer>(request);
            CheckForError(response);
            return response.Data;
        }
        
        public Transfer UpdateTransfer(int transferId, Transfer transfer)
        {
            RestRequest request = new RestRequest("/transfer/" + transferId);
            request.AddJsonBody(transfer);

            IRestResponse<Transfer> response = client.Put<Transfer>(request);

            CheckForError(response);

            return response.Data;
        }
    }
}
