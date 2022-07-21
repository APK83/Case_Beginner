using System;
using Npgsql;


namespace NetChecker
{
    class PostgresCheck
    {
        public static NpgsqlConnection Connect(string connectionString)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            try
            {
                conn.Open();
                Console.WriteLine($"Строка подключения: {connectionString} \nСтатус: доступен");

            }
            catch
            {
                Console.WriteLine($"База данных Postgres: ({connectionString}), \nСтатус: недоступен.");
            }
            return conn;
        }
    }
}
