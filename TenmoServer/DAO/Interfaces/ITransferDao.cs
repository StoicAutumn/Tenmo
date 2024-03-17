using System.Collections;
using System.Collections.Generic;
using TenmoServer.Models;

namespace TenmoServer.DAO.Interfaces
{
    public interface ITransferDao
    {
        Transfer GetTransferById(int transferId);
        public IList<UsernameTransfer> GetUsernameOfSentTransfersByAccountId(int accountId);
        public IList<UsernameTransfer> GetUsernameOfReceivedTransfersByAccountId(int accountId);
        Transfer UpdateTransfer(Transfer updatedTransfer);
        Transfer AddTransfer(Transfer newTransfer);
    }
}
