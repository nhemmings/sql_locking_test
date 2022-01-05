using System;
using System.Data;
using System.Data.SqlClient;

namespace ItemBatch_Locking
{
    public class Program
    {
        private readonly string connectionString = @"Data Source=localhost;Integrated Security=True;";

        static void Main(string[] args)
        {
            Program prog = new Program();
            prog.Run();
            Console.ReadKey();
        }

        private void Run()
        {
            Console.WriteLine("Connecting...");
            SqlConnection connection = Connect();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Could not open SQL connection.");
                return;
            }
            Console.WriteLine("Connected.");
            Console.WriteLine("Databases:");
            bool dbExists = ListDbs(connection);
            if (dbExists)
            {
                Console.WriteLine("Database exists. Dropping it.");
                DropDb(connection);
                Console.WriteLine("Dropped.");
            }
            Console.WriteLine("Creating database.");
            CreateDb(connection);
            Console.WriteLine("Created.");
        }

        private SqlConnection Connect()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            return connection;
        }

        private bool ListDbs(SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(@"SELECT [name] FROM master.sys.databases", connection);
            DataTable dt = new DataTable();
            dt.Load(cmd.ExecuteReader());

            bool dbExists = false;
            foreach (DataRow row in dt.Rows)
            {
                string dbName = row["name"].ToString();
                Console.WriteLine(dbName);
                if (dbName.Equals("Lock_Test"))
                    dbExists = true;
            }
            return dbExists;
        }

        private void DropDb(SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(@"DROP DATABASE IF EXISTS [Lock_Test]", connection);
            cmd.ExecuteNonQuery();
        }

        private void CreateDb(SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(@"CREATE DATABASE [Lock_Test]", connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(@"USE [Lock_Test]", connection);
            cmd.ExecuteNonQuery();

            cmd = new SqlCommand(@"CREATE TABLE [ItemBatch] (id INT IDENTITY(1, 1) PRIMARY KEY, dummy INT NULL)", connection);
            cmd.ExecuteNonQuery();
        }

        private void DryRun(SqlConnection connection)
        {
            SqlCommand cmd = new SqlCommand(@"INSERT INTO [ItemBatch] VALUES(1); SELECT SCOPE_IDENTITY()", connection);
            cmd.ExecuteScalar();
        }
    }
}
