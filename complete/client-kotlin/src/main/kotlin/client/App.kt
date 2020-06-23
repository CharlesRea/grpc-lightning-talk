package client

import greet.Greet.HelloRequest
import greet.GreeterGrpcKt.GreeterCoroutineStub
import io.grpc.ManagedChannel
import io.grpc.ManagedChannelBuilder
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.asExecutor
import kotlinx.coroutines.async
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.runBlocking
import java.io.Closeable
import java.util.concurrent.TimeUnit

class HelloWorldClient constructor(
        private val channel: ManagedChannel
) : Closeable {
    private val stub: GreeterCoroutineStub = GreeterCoroutineStub(channel)

    suspend fun greet(name: String) = coroutineScope {
        val request = HelloRequest.newBuilder().setName(name).build()

        val response = async { stub.sayHello(request) }

        println("Received: ${response.await().message}")
    }

    override fun close() {
        channel.shutdown().awaitTermination(5, TimeUnit.SECONDS)
    }
}

fun main(args: Array<String>) = runBlocking {
    val client = HelloWorldClient(
            ManagedChannelBuilder.forAddress("localhost", 5000)
                    .usePlaintext()
                    .executor(Dispatchers.Default.asExecutor())
                    .build())

    client.greet("Kotlin")
}