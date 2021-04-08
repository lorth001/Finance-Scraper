/*
 * Author:  Luke Orth
 * Date:    01.05.2021
 * Version: 0.0.1
 * Updates: Initial version
 * 
 * About:   This "DbConnection" class handles everything associated with the database.
 *          The class can be used to open a connection, close a connection, and execute queries.
*/
using System;
using System.Data.SqlClient;

namespace FinancialScraper
{
    class DbConnection
    {
        string ConnectionString = "Data Source=#####;Initial Catalog=FinancialScraper;Integrated Security=True";
        SqlConnection con;

        /* Open database connection */
        public void OpenConnection()
        {
            try
            {
                con = new SqlConnection(ConnectionString);
                con.Open();
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /* Close database connection */
        public void CloseConnection()
        {
            con.Close();
        }

        /* Execute database queries and commands */
        public void ExecuteQueries(string Query_)
        {
            SqlCommand cmd = new SqlCommand(Query_, con);
            cmd.ExecuteNonQuery();
        }
    }
}
