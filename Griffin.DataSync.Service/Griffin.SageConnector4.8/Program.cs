using Griffin.SageConnector4._8.Infrustructure;
using System;

namespace Griffin.SageConnector
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var factory = new OdbcConnectionFactory();

                using (var connection = factory.CreateConnection())
                {
                    Console.WriteLine("Opening connection...");

                    connection.Open();

                    Console.WriteLine("Connection successful!");

                    Console.WriteLine(connection.ServerVersion);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                Console.WriteLine();

                Console.WriteLine(ex);
            }

            Console.WriteLine();

            Console.WriteLine("Press any key...");

            Console.ReadKey();
        }
    }
}