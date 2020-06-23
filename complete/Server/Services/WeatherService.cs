using System;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Weather;

namespace Server.Services
{
    public class WeatherService : Weather.Weather.WeatherBase
    {
        public override async Task GetWeather(
            Empty _,
            IServerStreamWriter<WeatherData> responseStream,
            ServerCallContext context)
        {
            var rng = new Random();
            var now = DateTime.UtcNow;

            var i = 0;

            while (!context.CancellationToken.IsCancellationRequested && i < 20)
            {
                var forecast = new WeatherData
                {
                    DateTimeStamp = Timestamp.FromDateTime(now.AddDays(i++)),
                    TemperatureC = rng.Next(-20, 55),
                    TemperatureF = rng.Next(0, 150),
                };

                await responseStream.WriteAsync(forecast);

                await Task.Delay(500);
            }
        }
    }
}
