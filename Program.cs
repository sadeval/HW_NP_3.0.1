using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace RestaurantOrderClient
{
    class Program
    {
        static TcpClient? client;
        static NetworkStream? stream;
        static StreamReader? reader;
        static StreamWriter? writer;

        static void Main()
        {
            try
            {
                client = new TcpClient("127.0.0.1", 5500);
                using (stream = client.GetStream())
                using (reader = new StreamReader(stream, Encoding.UTF8))
                using (writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true })
                {
                    Thread listenThread = new Thread(ListenForNotifications);
                    listenThread.Start();

                    while (true)
                    {
                        Console.WriteLine("Выберите действие: 1. Добавить заказ  2. Проверить статус заказа  3. Отменить заказ  4. Выход");
                        string? choice = Console.ReadLine();

                        switch (choice)
                        {
                            case "1":
                                AddOrder();
                                break;
                            case "2":
                                CheckOrderStatus();
                                break;
                            case "3":
                                CancelOrder();
                                break;
                            case "4":
                                Disconnect();
                                return;
                            default:
                                Console.WriteLine("Неверный выбор.");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }

        private static void AddOrder()
        {
            Console.Write("Введите название ресторана: ");
            string? restaurantName = Console.ReadLine();
            Console.Write("Введите детали заказа: ");
            string? orderDetails = Console.ReadLine();

            var request = new ClientRequest
            {
                Command = "ADD_ORDER",
                RestaurantName = restaurantName,
                OrderDetails = orderDetails
            };

            SendRequest(request);
        }

        private static void CheckOrderStatus()
        {
            Console.Write("Введите ID заказа: ");
            if (int.TryParse(Console.ReadLine(), out int orderId))
            {
                var request = new ClientRequest
                {
                    Command = "CHECK_STATUS",
                    OrderId = orderId
                };

                SendRequest(request);
            }
            else
            {
                Console.WriteLine("Некорректный ID заказа.");
            }
        }

        private static void CancelOrder()
        {
            Console.Write("Введите ID заказа: ");
            if (int.TryParse(Console.ReadLine(), out int orderId))
            {
                var request = new ClientRequest
                {
                    Command = "CANCEL_ORDER",
                    OrderId = orderId
                };

                SendRequest(request);
            }
            else
            {
                Console.WriteLine("Некорректный ID заказа.");
            }
        }

        private static void SendRequest(ClientRequest request)
        {
            if (writer != null)
            {
                string jsonRequest = JsonConvert.SerializeObject(request);
                writer.WriteLine(jsonRequest);
            }
        }

        private static void ListenForNotifications()
        {
            try
            {
                while (true)
                {
                    string? notification = reader?.ReadLine();
                    if (!string.IsNullOrEmpty(notification))
                    {
                        Console.WriteLine($"Уведомление от сервера: {notification}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении уведомлений: {ex.Message}");
            }
        }

        private static void Disconnect()
        {
            try
            {
                writer?.Close();
                reader?.Close();
                stream?.Close();
                client?.Close();
                Console.WriteLine("Соединение с сервером закрыто.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отключении: {ex.Message}");
            }
        }
    }

    public class ClientRequest
    {
        public string? Command { get; set; }
        public string? RestaurantName { get; set; }
        public string? OrderDetails { get; set; }
        public int OrderId { get; set; }
    }
}
