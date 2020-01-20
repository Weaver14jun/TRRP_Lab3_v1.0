using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace TRRP_L2
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
                    SendMessageFromQueue();
                }
            else
            try
            {
                SendMessageFromSocket(11000);
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

        //private void GetContext(string connectionString)
        //{

        //}
        static void SendMessageFromQueue()
        {
            //ConnectionFactory factory = new ConnectionFactory();
            //IConnection conn = factory.CreateConnection();
            //IModel channel = conn.CreateModel();
            //model.QueueDeclare(queueName, false, false, false, null);
            //model.QueueBind(queueName, exchangeName, routingKey, null);


            //channel.Close();
            //conn.Close();
            var pcList = Normalizer.PCTableNormalizer.GetPCList("Data Source=bands.db;");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            string pcListString;
            using (StreamWriter sw = new StreamWriter("PCList.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, pcList);
            }
            using (StreamReader sr = new StreamReader("PCList.json"))
            {
                pcListString = sr.ReadToEnd();
            }

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "Test",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

                    //Console.WriteLine("Enter message...");
                    //string message = Console.ReadLine();

                    //string message = "Hello World!";

                    var body = Encoding.UTF8.GetBytes(pcListString);

                    channel.BasicPublish(exchange: "",
                                         routingKey: "Test",
                                         basicProperties: null,
                                         body: body);
                    //Console.WriteLine(" [x] Sent {0}", message);
                }
            }
        }

        static void SendMessageFromSocket(int port)
        {
            var pcList = Normalizer.PCTableNormalizer.GetPCList("Data Source=bands.db;");
            JsonSerializer serializer = new JsonSerializer();
            serializer.Formatting = Formatting.Indented;
            string pcListString;
            using (StreamWriter sw = new StreamWriter("PCList.json"))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, pcList);
            }
            using (StreamReader sr = new StreamReader("PCList.json"))
            {
                pcListString = sr.ReadToEnd();
            }
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];
            byte[] pcListByte = Encoding.UTF8.GetBytes(pcListString);
            // Соединяемся с удаленным устройством

            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("localhost");
            IPAddress ipAddr = ipHost.AddressList[1];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            Socket sender = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            sender.Connect(ipEndPoint);

            Console.Write("Введите сообщение: ");
            string message = Console.ReadLine();

            Console.WriteLine("Сокет соединяется с {0} ", sender.RemoteEndPoint.ToString());
            byte[] msg = Encoding.UTF8.GetBytes(message);

            // Отправляем данные через сокет
            int bytesSent = sender.Send(pcListByte);

            // Получаем ответ от сервера
            int bytesRec = sender.Receive(bytes);

            Console.WriteLine("\nОтвет от сервера: {0}\n\n", Encoding.UTF8.GetString(bytes, 0, bytesRec));

            // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
            if (message.IndexOf("<TheEnd>") == -1)
                SendMessageFromSocket(port);

            // Освобождаем сокет
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}
