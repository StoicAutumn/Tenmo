using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using TenmoServer.DAO.Interfaces;
using TenmoServer.Exceptions;
using TenmoServer.Models;

namespace TenmoServer.DAO
{
    
    public class TransferSqlDao : ITransferDao
    {
        private string connectionString = "";
        public TransferSqlDao(string dbConnectionString)
        {
            connectionString = dbConnectionString;
        }

        public Transfer GetTransferById(int transferId)
        {
            Transfer transfer = null;

            string sql = "SELECT * FROM transfer WHERE transfer_id = @transfer_id;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@transfer_id", transferId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        transfer = MapRowToTransfer(reader);
                    }                   
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return transfer;
        }

        public IList<UsernameTransfer> GetUsernameOfSentTransfersByAccountId(int accountId)
        {
            IList<UsernameTransfer> userTransfers = new List<UsernameTransfer>();
            

            string sql = @"SELECT transfer_id, username AS username_from, amount, transfer_type_id, transfer_status_id
                            FROM tenmo_user
                            JOIN account ON account.user_id = tenmo_user.user_id
                            JOIN transfer ON transfer.account_from = account.account_id
                            WHERE account_from = @account_from
                            ORDER BY transfer_id;";
            string sql2 = @"SELECT username AS username_to
                            FROM tenmo_user
                            JOIN account ON account.user_id = tenmo_user.user_id
                            JOIN transfer ON transfer.account_to = account.account_id
                            WHERE account_from = @account_from
                            ORDER BY transfer_id;";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        UsernameTransfer usernameTransfer = MapRowToUsernameTransfer(reader);
                        userTransfers.Add(usernameTransfer);
                    }
                }   
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql2, conn);
                    cmd.Parameters.AddWithValue("@account_from", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    int i = 0;
                    while (reader.Read())
                    {
                        userTransfers[i].usernameTo = Convert.ToString(reader["username_to"]);
                        i++;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return userTransfers;
        }

        public IList<UsernameTransfer> GetUsernameOfReceivedTransfersByAccountId(int accountId)
        {
            IList<UsernameTransfer> userTransfers = new List<UsernameTransfer>();


            string sql = @"SELECT transfer_id, username AS username_from, amount, transfer_type_id, transfer_status_id
                            FROM tenmo_user
                            JOIN account ON account.user_id = tenmo_user.user_id
                            JOIN transfer ON transfer.account_from = account.account_id
                            WHERE account_to = @account_to
                            ORDER BY transfer_id;";
            string sql2 = @"SELECT username AS username_to
                            FROM tenmo_user
                            JOIN account ON account.user_id = tenmo_user.user_id
                            JOIN transfer ON transfer.account_to = account.account_id
                            WHERE account_to = @account_to
                            ORDER BY transfer_id;";
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@account_to", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        UsernameTransfer usernameTransfer = MapRowToUsernameTransfer(reader);
                        userTransfers.Add(usernameTransfer);
                    }
                }
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql2, conn);
                    cmd.Parameters.AddWithValue("@account_to", accountId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    int i = 0;
                    while (reader.Read())
                    {
                        userTransfers[i].usernameTo = Convert.ToString(reader["username_to"]);
                        i++;
                    }
                }
                
                
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return userTransfers;
        }

        public Transfer AddTransfer(Transfer newTransfer)
        {
            string sql = @"INSERT INTO transfer (transfer_type_id, transfer_status_id, account_from, account_to, amount) 
                           OUTPUT INSERTED.transfer_id
                           VALUES (@transfer_type_id, @transfer_status_id, @account_from, @account_to, @amount);";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@transfer_type_id", newTransfer.TransferTypeId);
                    cmd.Parameters.AddWithValue("@transfer_status_id", newTransfer.TransferStatusId);
                    cmd.Parameters.AddWithValue("@account_from", newTransfer.AccountFrom);
                    cmd.Parameters.AddWithValue("@account_to", newTransfer.AccountTo);
                    cmd.Parameters.AddWithValue("@amount", newTransfer.Amount);
                    newTransfer.TransferId = (int)cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }

            return newTransfer;
        }

        public Transfer UpdateTransfer(Transfer updatedTransfer)
        {
            string sql = "UPDATE transfer SET transfer_status_id = @transfer_status_id WHERE transfer_id = @transfer_id;";

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@transfer_status_id", updatedTransfer.TransferStatusId);
                    cmd.Parameters.AddWithValue("@transfer_id", updatedTransfer.TransferId);

                    int count = cmd.ExecuteNonQuery();
                    if (count == 1)
                    {
                        return updatedTransfer;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (SqlException ex)
            {
                throw new DaoException("SQL exception occurred", ex);
            }
        }

        private Transfer MapRowToTransfer(SqlDataReader reader)
        {
            Transfer transfer = new Transfer();
            transfer.TransferId = Convert.ToInt32(reader["transfer_id"]);
            transfer.TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]);
            transfer.TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]);
            transfer.AccountFrom = Convert.ToInt32(reader["account_from"]);
            transfer.AccountTo = Convert.ToInt32(reader["account_to"]);
            transfer.Amount = Convert.ToDecimal(reader["amount"]);

            return transfer;
        }

        private UsernameTransfer MapRowToUsernameTransfer(SqlDataReader reader)
        {
            UsernameTransfer transfer = new UsernameTransfer();
            transfer.TransferId = Convert.ToInt32(reader["transfer_id"]);
            transfer.TransferTypeId = Convert.ToInt32(reader["transfer_type_id"]);
            transfer.TransferStatusId = Convert.ToInt32(reader["transfer_status_id"]);
            transfer.usernameFrom = Convert.ToString(reader["username_from"]);           
            transfer.Amount = Convert.ToDecimal(reader["amount"]);

            return transfer;
        }
    }
}
