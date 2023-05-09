using System.Text;
using Newtonsoft.Json;
using OnTheFly.Connections;
using OnTheFly.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static void Main(string[] args)
    {
        const string QUEUE_NAME = "Sales";

        var factory = new ConnectionFactory() { HostName = "localhost" };

        SaleConnection saleConnection = new SaleConnection();

        using (var connection = factory.CreateConnection())
        {
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: QUEUE_NAME,
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

                while (true)
                {
                    try
                    {
                        var consumer = new EventingBasicConsumer(channel);
                        consumer.Received += (model, ea) =>
                        {
                            var body = ea.Body.ToArray();
                            var returnMessage = Encoding.UTF8.GetString(body);
                            var sale = JsonConvert.DeserializeObject<Sale>(returnMessage);
                            //Console.WriteLine(ticket.ToString());
                            var finish = false;
                            do
                            {
                                try
                                {
                                    saleConnection.Insert(sale);
                                    finish = false;
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("falha ao persistir os dados");
                                    finish = true;
                                    Thread.Sleep(3000);
                                }


                            } while (finish);




                            //if(t == null)
                        };

                        channel.BasicConsume(queue: QUEUE_NAME,
                                             autoAck: true,
                                             consumer: consumer);

                        Thread.Sleep(2000);
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }
        }
    }
}