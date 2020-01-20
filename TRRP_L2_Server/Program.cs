using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading;

//Сервер на стороне раббита /deprecated
namespace TRRP_L2_Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("1)Queue");
            Console.WriteLine("2)Socket");
            int switcher = Convert.ToInt32(Console.ReadLine());
            if (switcher == 1)
            {
                StartQueueServer(args);
            }
            else
            {
                StartSocketServer();
            }
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }

        static void StartQueueServer(string[] args)
        {
            //var pcList = Normalizer.GetPCList("Data Source=" + "bands.db;");

            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);
            AmqpTcpEndpoint amqpTcpEndpoint = new AmqpTcpEndpoint(ipAddr.ToString(), 15672);
            //var factory = new ConnectionFactory() { Endpoint = amqpTcpEndpoint };
            var factory = new ConnectionFactory() { HostName = "localhost" };
            factory.UserName = "guest";
            factory.Password = "guest";
            //factory.VirtualHost = vhost;
            //factory.HostName = hostName;
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    //channel.QueueDeclare(queue: "Test",
                    //                     durable: true,
                    //                     exclusive: false,
                    //                     autoDelete: false,
                    //                     arguments: null);

                    //var consumer = new EventingBasicConsumer(channel);
                    //consumer.Received += (model, ea) =>
                    //{
                    //    var body = ea.Body;
                    //    var message = Encoding.UTF8.GetString(body);
                    //    Console.WriteLine(" [x] Received {0}", message);
                    //};
                    //channel.BasicConsume(queue: "Test",
                    //                     autoAck: true,
                    //                     consumer: consumer);


                    //channel.QueueDeclare(queue: "Test",
                    //                 durable: true,
                    //                 exclusive: false,
                    //                 autoDelete: false,
                    //                 arguments: null);

                    //var message = GetMessage(args);
                    //var body = Encoding.UTF8.GetBytes(message);

                    //var properties = channel.CreateBasicProperties();
                    //properties.Persistent = true;

                    //channel.BasicPublish(exchange: "",
                    //                     routingKey: "Test",
                    //                     basicProperties: properties,
                    //                     body: body);
                    //Console.WriteLine(" [x] Sent {0}", message);

                    channel.QueueDeclare(queue: "Test",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                    Console.WriteLine(" [*] Waiting for messages.");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        //

                        var pcList = JsonConvert.DeserializeObject<List<Normalizer.PCModel>>(message);
                        Normalizer.PCTableNormalizer.SetPCList(pcList, "Server = 127.0.0.1; User Id = postgres; " + "Password=a54g5x; Database=PC_DB;");
                        //
                        Console.WriteLine(" [x] Received {0}", message);

                        int dots = message.Split('.').Length - 1;
                        Thread.Sleep(dots * 1000);

                        Console.WriteLine(" [x] Done");

                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    };
                    channel.BasicConsume(queue: "Test",
                                         autoAck: false,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();

                }
                Console.ReadKey();
            }
        }

        static void StartSocketServer()
        {
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет Tcp/Ip
            Socket sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);

                // Начинаем слушать соединения
                while (true)
                {
                    Console.WriteLine("Ожидаем соединение через порт {0}", ipEndPoint);

                    // Программа приостанавливается, ожидая входящее соединение
                    Socket handler = sListener.Accept();
                    string data = null;

                    // Мы дождались клиента, пытающегося с нами соединиться

                    byte[] bytes = new byte[1024];
                    int bytesRec = handler.Receive(bytes);

                    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    var pcList = JsonConvert.DeserializeObject<List<Normalizer.PCModel>>(data);
                    Normalizer.PCTableNormalizer.SetPCList(pcList, "Server = 127.0.0.1; User Id = postgres; " + "Password=a54g5x; Database=PC_DB;");

                    // Показываем данные на консоли
                    Console.Write("Полученный текст: " + data + "\n\n");

                    // Отправляем ответ клиенту\
                    //string reply = "Спасибо за запрос в " + data.Length.ToString()
                    //        + " символов";
                    //byte[] msg = Encoding.UTF8.GetBytes(reply);
                    //handler.Send(msg);

                    if (data.IndexOf("<TheEnd>") > -1)
                    {
                        Console.WriteLine("Сервер завершил соединение с клиентом.");
                        break;
                    }

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
}
