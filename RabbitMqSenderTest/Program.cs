using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMqSenderTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "128.1.10.11", UserName = "Username", Password = "Password" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "hello-exchange",
                        type: ExchangeType.Direct,
                        durable: true,
                        autoDelete: false);

                    while (true)
                    {
                        var message = Console.ReadLine();
                        if (message == "q")
                        {
                            break;
                        }
                        var body = Encoding.UTF8.GetBytes(message);


                        channel.ConfirmSelect();
                        var isOk = channel.WaitForConfirms();
                        var a = channel.NextPublishSeqNo;
                        channel.BasicPublish("hello-exchange", "hello",null, body);
                    }
                }
            }
        }
    }
}
