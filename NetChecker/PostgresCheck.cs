using System;
using Npgsql;


namespace NetChecker
{
    class PostgresCheck
    {
        public static bool Connect(string connectionString)
        {
            NpgsqlConnection conn = new NpgsqlConnection(connectionString);
            try
            {
                conn.Open();
                Console.WriteLine($"Строка подключения: {connectionString} \nСтатус: БД доступна.");
                return true; 

            }
            catch
            {
                Console.WriteLine($"База данных Postgres: ({connectionString}), \nСтатус: БД недоступна.");
            }
            return false;
        }
    }
}
