using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TenmoServer.DAO.Interfaces;
using TenmoServer.Models;

namespace TenmoServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransferController : ControllerBase
    {
        private readonly ITransferDao transferDao;

        public TransferController(ITransferDao transferDao)
        {
            this.transferDao = transferDao;
        }

        [HttpGet("{id}")]
        public ActionResult<Transfer> GetTransferById (int id)
        {
            Transfer transfer = transferDao.GetTransferById(id);

            if (transfer != null)
            {
                return Ok(transfer);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/list")]
        public ActionResult<IList<UsernameTransfer>> GetListOfTransfersByAccountId(int id)
        {
            IList<UsernameTransfer> listOfTransfers = transferDao.GetUsernameOfSentTransfersByAccountId(id);

            IList<UsernameTransfer> receivedTransfers = transferDao.GetUsernameOfReceivedTransfersByAccountId(id);
            for (int i = 0; i < receivedTransfers.Count; i++)
            {
                listOfTransfers.Add(receivedTransfers[i]);
            }

            if (listOfTransfers != null)
            {
                return Ok(listOfTransfers);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id}/amount")]
        public ActionResult<decimal> GetAmountById(int id)
        {
            decimal amount = transferDao.GetTransferById(id).Amount;

            if (amount != 0)
            {
                return Ok(amount);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public ActionResult<Transfer> AddTransfer (Transfer transfer)
        {
            Transfer addedTransfer = transferDao.AddTransfer(transfer);
            return Created($"/transfer", addedTransfer);
        }

        [HttpPut("{id}")]
        public ActionResult<Transfer> UpdateTransfer(Transfer transfer)
        {
            Transfer updatedTransfer = transferDao.UpdateTransfer(transfer);

            if (updatedTransfer != null)
            {
                return Ok(updatedTransfer);

            }
            else
            {
                return NotFound();
            }
        }
    }
}
