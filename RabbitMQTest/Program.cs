using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQTest
{


    class Program
    {
        static void Main(string[] args)
        {
            RabbitMqFactory rmf = null;
            try
            {
                 rmf = RabbitMqFactory.Get();

                rmf.Init("192.168.174.128", "user", "123456");
                rmf.GetChannel("消息队列1", (channel) =>
                {
                    channel.ExchangeDeclare(exchange: "hello-exchange", type: ExchangeType.Direct, durable: true, autoDelete: false);
                    channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false);
                    channel.QueueBind(queue: "hello-queue", exchange: "hello-exchange", routingKey: "hello");

                    var consumer = new EventingBasicConsumer(channel);
                    var consumerTag = channel.BasicConsume("hello-queue", false, consumer);

                    consumer.Received += (model, e) =>
                    {
                        channel.BasicAck(e.DeliveryTag, false);
                        var message = Encoding.UTF8.GetString(e.Body);
                        Console.WriteLine("Received {0}", message);
                    };
                });

            }
            catch (Exception ex)
            {
                if (rmf != null)
                {
                    rmf.Dispose();
                }
                //记录日志

                Console.WriteLine(ex.Message + " stack" + ex.StackTrace);
            }

            Console.ReadLine();

           

            //var factory = new ConnectionFactory() { HostName = "192.168.174.128", UserName = "user", Password = "123456" };

            //常规写法1
            //while (true)
            //{
            //    //获取一个连接

            //    using (var connection = factory.CreateConnection())
            //    {
            //        //如果连接没被意外关闭
            //        while (connection.IsOpen)
            //        {
            //            //获取渠道中未处理消息
            //            using (var channel = connection.CreateModel())
            //            {
            //                channel.ExchangeDeclare(exchange: "hello-exchange", type: ExchangeType.Direct, durable: true, autoDelete: false);
            //                channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false);
            //                channel.QueueBind(queue: "hello-queue", exchange: "hello-exchange", routingKey: "hello");

            //                //第二个参数是自动应答
            //                QueueingBasicConsumer consumer = new QueueingBasicConsumer(channel);


            //                channel.BasicConsume("hello-queue", false, consumer);

            //                while (true)
            //                {
            //                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();//接收消息并出列
            //                    var body = ea.Body;//消息主体
            //                    var message = Encoding.UTF8.GetString(body);
            //                    Console.WriteLine("Received {0}", message);
            //                    if (message == "exit")
            //                    {
            //                        // 处理应答
            //                        Console.WriteLine("exit!");
            //                        break;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}
            
            //常规写法2

            //var factory = new ConnectionFactory() { HostName = "192.168.174.128", UserName = "user", Password = "123456" };
            //using (var connection = factory.CreateConnection())
            //{
            //    using (var channel = connection.CreateModel())
            //    {
            //        channel.ExchangeDeclare(exchange: "hello-exchange", type: ExchangeType.Direct, durable: true, autoDelete: false);
            //        channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false);
            //        channel.QueueBind(queue: "hello-queue", exchange: "hello-exchange", routingKey: "hello");

            //        var consumer = new EventingBasicConsumer(channel);
            //        var consumerTag = channel.BasicConsume("hello-queue", false, consumer);

            //        consumer.Received += (model, e) =>
            //        {
            //            channel.BasicAck(e.DeliveryTag, false);
            //            var message = Encoding.UTF8.GetString(e.Body);
            //            Console.WriteLine("Received {0}", message);
            //        };

            //        if ("q" == Console.ReadLine())
            //        {
            //            channel.BasicCancel(consumerTag);
            //        }
            //    }
            //}
        }
    }
}
