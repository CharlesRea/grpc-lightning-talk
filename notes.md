# gRPC

## What is gRPC?
* Remote Procedure Calls
  * Service to service communication
  * Write code as it it were a normal method call
  * Mainly designed around inter-service calls in microservices
* Contract first
  * Write code in a language independent format, to define the interface up front
  * Code generation for the language of your choice
  
## Demo

### Create greet.proto
```
syntax = "proto3";

option csharp_namespace = "Server";

package greet;

service Greeter {
  rpc SayHello (HelloRequest) returns (HelloReply);
}

message HelloRequest {
  string name = 1;
}

message HelloReply {
  string message = 1;
}
```

### Implement Server
Add `services/GreeterService`
```csharp
public class GreeterService : Greeter.GreeterBase
{
    public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
        return Task.FromResult(new HelloReply
        {
            Message = "Hello " + request.Name
        });
    }
}
```

Add to `startup.cs`:
```csharp
endpoints.MapGrpcService<GreeterService>();
```

Run `dotnet run`

### Call from Kotlin client
```kotlin
val request = HelloRequest.newBuilder().setName(name).build()

val response = async { stub.sayHello(request) }

println("Received: ${response.await().message}")
```

### Show in BloomRPC

## Streaming demo

### Add weather.proto
```
syntax = "proto3";

import "google/protobuf/empty.proto";
import "google/protobuf/timestamp.proto";

option csharp_namespace = "Weather";

package weatherForecast;

service Weather {
  rpc GetWeather (google.protobuf.Empty) returns (stream WeatherData);
}

message WeatherData {
  google.protobuf.Timestamp dateTimeStamp = 1;
  int32 temperatureC = 2;
  int32 temperatureF = 3;
}
```

### Implement on the server
```csharp
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
```

Add to `startup.cs`:
```csharp
endpoints.MapGrpcService<WeatherService>();
```

### Implement on the client:
```csharp
using var channel = GrpcChannel.ForAddress("https://localhost:5001");

var client = new Weather.Weather.WeatherClient(channel);

using var response = client.GetWeather(new Empty());

await foreach (var weatherData in response.ResponseStream.ReadAllAsync())
{
    Console.WriteLine($"{weatherData.DateTimeStamp.ToDateTime():s}   |   {weatherData.TemperatureC} C\t|   {weatherData.TemperatureF} F");
}
```

## gRPC Web
### Set up client
In `protos`, run `protoc greet.proto --js_out=import_style=commonjs:../Server/wwwroot/scripts --grpc-web_out=import_style=commonjs,mode=grpcwebtext:../server/wwwroot/scripts`

Show generated JS

In `index.ts`
```typescript
var request = new HelloRequest();
request.setName(nameInput.value);

client.sayHello(request, {}, (err, response) => {
    resultText.innerHTML = response.getMessage();
});
```

In `startup.cs`
```csharp
app.UseGrpcWeb();
app.UseEndpoints(endpoints =>
{
    endpoints.MapGrpcService<GreeterService>().EnableGrpcWeb();
    endpoints.MapGrpcService<WeatherService>();
});
```

To run:
`yarn build`
`npx serve . -l 5005`

* Don't have sufficient control over HTTP2 frames in the browser, so isn't a complete implementation of GRPC
* Server needs a proxy to convert requests to proper GRPC - Envoy, nginx, or ASP.NET has support out of the box

## Downsides
* Have to define your contract in language independent format - so can't use language specific features
* HTTP2 makes things a bit more difficult
  * HTTP2 generally assumes SSL. If you're hosting behind a proxy with SSL termination, you need to do some specific config to make sure that HTTP communication uses HTTP2.
* Doesn't work in IIS / Azure App Service
* Can't debug via browser dev tools

## When to use it?
* High performance, service-to-service comms
* When you need streaming
* Polyglot environment - different languages