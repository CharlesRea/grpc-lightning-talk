using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;

namespace Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");

            var client = new Weather.Weather.WeatherClient(channel);

            using var response = client.GetWeather(new Empty());

            await foreach (var weatherData in response.ResponseStream.ReadAllAsync())
            {
                Console.WriteLine($"{weatherData.DateTimeStamp.ToDateTime():s}   |   {weatherData.TemperatureC} C\t|   {weatherData.TemperatureF} F");
            }
        }
    }
}
