using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using TenmoClient.Models;
using TenmoClient.Services;

namespace TenmoClient
{
    public class TenmoApp
    {
        private readonly TenmoConsoleService console = new TenmoConsoleService();
        private readonly TenmoApiService tenmoApiService;
        private readonly AccountApiService accountApiService;
        private readonly UserApiService userApiService;
        private readonly TransferApiService transferApiService;
        public TenmoApp(string apiUrl)
        {
            tenmoApiService = new TenmoApiService(apiUrl);
            accountApiService = new AccountApiService(apiUrl);
            userApiService = new UserApiService(apiUrl);
            transferApiService = new TransferApiService(apiUrl);
        }

        public void Run()
        {
            bool keepGoing = true;
            while (keepGoing)
            {
                // The menu changes depending on whether the user is logged in or not
                if (tenmoApiService.IsLoggedIn)
                {
                    keepGoing = RunAuthenticated();
                }
                else // User is not yet logged in
                {
                    keepGoing = RunUnauthenticated();
                }
            }
        }

        private bool RunUnauthenticated()
        {
            console.PrintLoginMenu();
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 2, 1);
            while (true)
            {
                if (menuSelection == 0)
                {
                    return false;   // Exit the main menu loop
                }

                if (menuSelection == 1)
                {
                    // Log in
                    Login();
                    return true;    // Keep the main menu loop going
                }

                if (menuSelection == 2)
                {
                    // Register a new user
                    Register();
                    return true;    // Keep the main menu loop going
                }
                console.PrintError("Invalid selection. Please choose an option.");
                console.Pause();
            }
        }

        private bool RunAuthenticated()
        {
            console.PrintMainMenu(tenmoApiService.Username);
            int menuSelection = console.PromptForInteger("Please choose an option", 0, 6);
            if (menuSelection == 0)
            {
                // Exit the loop
                return false;
            }

            if (menuSelection == 1)
            {
                // View your current balance
                ViewCurrentBalance();
            }

            if (menuSelection == 2)
            {
                // View your past transfers
                ViewPastTransfers();
            }

            if (menuSelection == 3)
            {
                // View your pending requests
                ViewPendingTransfers();
            }

            if (menuSelection == 4)
            {
                // Send TE bucks
                SendTeBucksTo();
            }

            if (menuSelection == 5)
            {
                // Request TE bucks
                RequestTeBucksTo();
            }

            if (menuSelection == 6)
            {
                // Log out
                tenmoApiService.Logout();
                console.PrintSuccess("You are now logged out");
            }

            return true;    // Keep the main menu loop going
        }

        private void Login()
        {
            LoginUser loginUser = console.PromptForLogin();
            if (loginUser == null)
            {
                return;
            }

            try
            {
                ApiUser user = tenmoApiService.Login(loginUser);
                if (user == null)
                {
                    console.PrintError("Login failed.");
                }
                else
                {
                    console.PrintSuccess("You are now logged in");
                }
            }
            catch (Exception)
            {
                console.PrintError("Login failed.");
            }
            console.Pause();
        }

        private void Register()
        {
            LoginUser registerUser = console.PromptForLogin();
            if (registerUser == null)
            {
                return;
            }
            try
            {
                bool isRegistered = tenmoApiService.Register(registerUser);
                if (isRegistered)
                {
                    console.PrintSuccess("Registration was successful. Please log in.");
                }
                else
                {
                    console.PrintError("Registration was unsuccessful.");
                }
            }
            catch (Exception)
            {
                console.PrintError("Registration was unsuccessful.");
            }
            console.Pause();
        }

        public void ViewCurrentBalance()
        {
            Account userAccount = accountApiService.GetAccountOfCurrentUser();
            Console.WriteLine($"Your Current Balance Is: ${userAccount.Balance}");
            console.Pause();
            RunAuthenticated();
        }


        //Todo Format later to clean up
        public void ViewPastTransfers()
        {
            int AccountId = accountApiService.GetAccountOfCurrentUser().AccountId;
            Account currentUser = accountApiService.GetAccountOfCurrentUser();
            string currentUsername = userApiService.GetUserById(currentUser.UserId).Username;

            Console.WriteLine("|----------------------------------------|");
            Console.WriteLine("| Transfers                              |");
            Console.WriteLine("|----------------------------------------|");
            Console.WriteLine("{0, -10} {1, -20} {2, 10}", "| ID", "From/To", "Amount |");
            Console.WriteLine("|----------------------------------------|");
            IList<UsernameTransfer> transfers = transferApiService.GetListOfTransfersByAccountId(AccountId);
            foreach (UsernameTransfer transfer in transfers)
            {
                if (transfer.TransferStatusId == 2)
                {
                    int transferId = transfer.TransferId;
                    string fromOrTo = "";
                    decimal transferAmount = transfer.Amount;
                    if (transfer.TransferTypeId == 1 && transfer.usernameFrom == currentUsername)
                    {
                        fromOrTo = $"To: {transfer.usernameTo}";
                    }
                    else if (transfer.TransferTypeId == 1 && transfer.usernameTo == currentUsername)
                    {
                        fromOrTo = $"From: {transfer.usernameFrom}";
                    }
                    else if (transfer.TransferTypeId == 2 && transfer.usernameFrom == currentUsername)
                    {
                        fromOrTo = $"To: {transfer.usernameTo}";
                    }
                    else if (transfer.TransferTypeId == 2 && transfer.usernameTo == currentUsername)
                    {
                        fromOrTo = $"From: {transfer.usernameFrom}";
                    }

                    Console.WriteLine("{0, -10} {1, -20} {2, 10}", $"| {transferId}", fromOrTo, $"${transferAmount} |");
                }             
            }
            Console.WriteLine("|----------------------------------------|");
            ViewTransferDetails(transfers);
        }

        public void ViewTransferDetails(IList<UsernameTransfer> transfers)
        {
            try
            {
                Console.Write("Please enter a transfer ID to view details (0 to cancel): ");
                int transferId = int.Parse(Console.ReadLine());
                if (transferId == 0)
                {
                    Console.WriteLine("Returning to main menu");
                    console.Pause();
                    RunAuthenticated();
                }

                bool transferFound = false;
                for (int i = 0; i < transfers.Count; i++)
                {
                    if (transferId == transfers[i].TransferId)
                    {
                        Console.WriteLine("------------------------------");
                        Console.WriteLine("Transfer Details");
                        Console.WriteLine("------------------------------");
                        Console.WriteLine($"Id: {transferId}");
                        Console.WriteLine($"From: {transfers[i].usernameFrom}");
                        Console.WriteLine($"To: {transfers[i].usernameTo}");
                        if (transfers[i].TransferTypeId == 1)
                        {
                            Console.WriteLine("Type: Request");
                        }
                        else
                        {
                            Console.WriteLine("Type: Send");
                        }

                        if (transfers[i].TransferStatusId == 1)
                        {
                            Console.WriteLine("Status: Pending");
                        }
                        else if (transfers[i].TransferStatusId == 2)
                        {
                            Console.WriteLine("Status: Approved");
                        }
                        else
                        {
                            Console.WriteLine("Status: Rejected");
                        }
                        Console.WriteLine($"Amount: ${transfers[i].Amount}");
                        Console.WriteLine();
                        transferFound = true;
                        console.Pause();
                        ViewPastTransfers();
                    }
                }
                if (transferFound == false)
                {
                    Console.WriteLine("Please enter a valid transfer id");
                    console.Pause();
                    ViewPastTransfers();
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Only numbers are accepted");
                Console.WriteLine();
                console.Pause();
                ViewPastTransfers();
            }           
        }

        public void ViewPendingTransfers()
        {
            Account currentUser = accountApiService.GetAccountOfCurrentUser();
            int accountId = currentUser.AccountId;
            string currentUsername = userApiService.GetUserById(currentUser.UserId).Username;
            UsernameTransfer pendingTransfer = null;

            Console.WriteLine("|----------------------------------------|");
            Console.WriteLine("| Pending Transfers                      |");
            Console.WriteLine("|----------------------------------------|");
            Console.WriteLine("{0, -10} {1, -20} {2, 10}", "| ID", "To", "Amount |");
            Console.WriteLine("|----------------------------------------|");
            IList<UsernameTransfer> transfers = transferApiService.GetListOfTransfersByAccountId(accountId);
            foreach (UsernameTransfer transfer in transfers)
            {
                if (transfer.TransferStatusId == 1 && transfer.TransferTypeId == 1 && transfer.usernameFrom == currentUsername)
                {
                    Console.WriteLine("{0, -10} {1, -20} {2, 10}", $"| {transfer.TransferId}", transfer.usernameTo, $"${transfer.Amount} |");
                }               
            }
            Console.WriteLine("|----------------------------------------|");

            try
            {
                Console.WriteLine("Please enter transfer ID to approve/reject (0 to return to main menu)");
                int transferId = int.Parse(Console.ReadLine());

                if (transferId == 0)
                {
                    RunAuthenticated();
                }

                bool transferFound = false;
                foreach (UsernameTransfer transfer in transfers)
                {
                    if (transferId == transfer.TransferId)
                    {
                        transferFound = true;
                        pendingTransfer = transfer;
                        break;
                    }
                }

                if (transferFound == false)
                {
                    Console.WriteLine("Please enter existing User Id");
                    Console.WriteLine();
                    console.Pause();
                    ViewPendingTransfers();
                }
                else
                {
                    ApproveOrReject(pendingTransfer);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Only numbers are accepted");
                Console.WriteLine();
                console.Pause();
                ViewPendingTransfers();
            }            
        }

        public void ApproveOrReject(UsernameTransfer pendingTransfer)
        {
            Console.WriteLine();
            Console.WriteLine("1: Approve");
            Console.WriteLine("2: Reject");
            Console.WriteLine("0: Don't approve or reject");
            Console.WriteLine("---------------------------");           

            try
            {
                Console.Write("Please choose an option: ");
                int userResponse = int.Parse(Console.ReadLine());

                if (userResponse == 1)
                {
                    ApproveTransaction(pendingTransfer);
                    Console.WriteLine("Transaction approved");
                    console.Pause();                   
                }
                else if (userResponse == 2)
                {
                    RejectTransaction(pendingTransfer);
                    Console.WriteLine("Transaction rejected");
                    console.Pause();
                }
                else if (userResponse == 0)
                {
                    Console.WriteLine("Returning to pending transfers");
                    console.Pause();
                }
                else
                {
                    Console.WriteLine("Please enter valid option");
                    ApproveOrReject(pendingTransfer);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Only numbers are accepted");
                Console.WriteLine();
                console.Pause();
                ApproveOrReject(pendingTransfer);
            }
            ViewPendingTransfers();
        }

        public void ApproveTransaction(UsernameTransfer pendingTransfer)
        {
            Transfer transferToUpdate = new Transfer();
            transferToUpdate.TransferId = pendingTransfer.TransferId;
            transferToUpdate.TransferTypeId = pendingTransfer.TransferTypeId;
            transferToUpdate.TransferStatusId = 2;
            transferToUpdate.AccountFrom = 0;
            transferToUpdate.AccountTo = 0;
            transferToUpdate.Amount = pendingTransfer.Amount;

            //Check if there are enough funds in current user's account
            decimal transferAmount = transferApiService.GetTransferById(pendingTransfer.TransferId).Amount;
            if (transferAmount > accountApiService.GetAccountOfCurrentUser().Balance)
            {
                Console.WriteLine("Transfer amount cannot be more than current balance");
                console.Pause();
                ViewPendingTransfers();
            }
            else
            {
                //Update the transfer's transfer status id to 2
                transferApiService.UpdateTransfer(transferToUpdate.TransferId, transferToUpdate);

                //Add amount to requester's account balance
                Account receiverAccount = accountApiService.GetAccountByUsername(pendingTransfer.usernameTo);
                receiverAccount.Balance += transferAmount;
                accountApiService.UpdateAccount(receiverAccount.AccountId, receiverAccount);

                //Subject amount from current user's account balance
                Account senderAccount = accountApiService.GetAccountOfCurrentUser();
                senderAccount.Balance -= transferAmount;
                accountApiService.UpdateAccount(senderAccount.UserId, senderAccount);
            }
        }
        public void RejectTransaction(UsernameTransfer pendingTransfer)
        {
            Transfer transferToUpdate = new Transfer();
            transferToUpdate.TransferId = pendingTransfer.TransferId;
            transferToUpdate.TransferTypeId = pendingTransfer.TransferTypeId;
            transferToUpdate.TransferStatusId = 3;
            transferToUpdate.AccountFrom = 0;
            transferToUpdate.AccountTo = 0;
            transferToUpdate.Amount = pendingTransfer.Amount;

            //Update the transfer's transfer status id to 3
            transferApiService.UpdateTransfer(transferToUpdate.TransferId, transferToUpdate);
        }

        //Todo Format later to clean up
        public void SendTeBucksTo()
        {
            Console.WriteLine("|-------------------");
            Console.WriteLine("| Id    | Username   ");
            Console.WriteLine("|-------------------");
            IList<User> users = userApiService.GetOtherUsernamesAndIds();
            foreach (User user in users)
            {
                Console.WriteLine($"| {user.UserId}  | {user.Username}");
            }
            Console.WriteLine("|-------------------");
            try 
            {
                Console.Write("Id of the user you are sending to (0 to return to main menu): ");
                int userId = int.Parse(Console.ReadLine());
                if (userId == 0)
                {
                    
                    RunAuthenticated();
                }

                bool userFound = false;
                foreach (User user in users)
                {
                    if (userId == user.UserId)
                    {
                        userFound = true;
                    }
                }
                if (userFound == false)
                {
                    Console.WriteLine("Please enter existing User Id");
                    Console.WriteLine();
                    console.Pause();
                    SendTeBucksTo();
                }

                Console.Write("Enter amount to send: ");
                decimal transferAmount = decimal.Parse(Console.ReadLine());
                if (transferAmount <= 0M)
                {
                    Console.WriteLine("Transfer amount cannot be zero or negative");
                    console.Pause();
                    SendTeBucksTo();
                }
                else if (transferAmount > accountApiService.GetAccountOfCurrentUser().Balance)
                {
                    Console.WriteLine("Transfer amount cannot be more than current balance");
                    console.Pause();
                    SendTeBucksTo();
                }
                else
                {
                    Transfer newTransfer = new Transfer();
                    newTransfer.TransferTypeId = 2;
                    newTransfer.TransferStatusId = 2;
                    newTransfer.AccountFrom = accountApiService.GetAccountOfCurrentUser().AccountId;
                    newTransfer.AccountTo = accountApiService.GetAccountByUserId(userId).AccountId;
                    newTransfer.Amount = transferAmount;
                    Transfer sendTransfer = transferApiService.AddTransfer(newTransfer);

                    int transferId = sendTransfer.TransferId;
                    //Adding funds to receiver's account
                    Account receiverAccount = accountApiService.GetAccountByUserId(userId);
                    receiverAccount.Balance += transferAmount;
                    accountApiService.UpdateAccount(userId, receiverAccount);

                    //Removing funds from sender's account                 
                    Account senderAccount = accountApiService.GetAccountOfCurrentUser();
                    senderAccount.Balance -= transferAmount;
                    accountApiService.UpdateAccount(senderAccount.UserId, senderAccount);

                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Only numbers are accepted");
                Console.WriteLine();
                console.Pause();
                SendTeBucksTo();
            }
            Console.WriteLine("Amount sent, returning to main menu");
            console.Pause();
            RunAuthenticated();
        }

        public void RequestTeBucksTo()
        {
            Console.WriteLine("|----------------");
            Console.WriteLine("| Id  | Username   ");
            Console.WriteLine("|----------------");
            IList<User> users = userApiService.GetOtherUsernamesAndIds();
            foreach (User user in users)
            {
                Console.WriteLine("| " + user.UserId + "| " + user.Username);
            }
            Console.WriteLine("|----------------");
            try
            {
                Console.Write("Id of the user you are requesting from (0 to return to main menu): ");
                int userId = int.Parse(Console.ReadLine());
                if (userId == 0)
                {
                    RunAuthenticated();
                }
                bool userFound = false;
                foreach (User user in users)
                {
                    if (userId == user.UserId)
                    {
                        userFound = true;
                    }
                }
                if (userFound == false)
                {
                    Console.WriteLine("Please enter existing User Id");
                    Console.WriteLine();
                    console.Pause();
                    RequestTeBucksTo();
                }

                Console.Write("Enter amount to request: ");
                decimal transferAmount = decimal.Parse(Console.ReadLine());
                if (transferAmount <= 0M)
                {
                    Console.WriteLine("Transfer amount cannot be zero or negative");
                    console.Pause();
                    RequestTeBucksTo();
                }               
                else
                {
                    Transfer newTransfer = new Transfer();
                    newTransfer.TransferTypeId = 1;
                    newTransfer.TransferStatusId = 1;
                    newTransfer.AccountFrom = accountApiService.GetAccountByUserId(userId).AccountId;
                    newTransfer.AccountTo = accountApiService.GetAccountOfCurrentUser().AccountId;                    
                    newTransfer.Amount = transferAmount;
                    Transfer requestTransfer = transferApiService.AddTransfer(newTransfer);
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Only numbers are accepted");
                console.Pause();
                RequestTeBucksTo();
            }

            Console.WriteLine("Amount requested, returning to main menu");
            console.Pause();
            RunAuthenticated();
        }

    }
}
