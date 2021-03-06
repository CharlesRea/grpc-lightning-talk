﻿import { HelloRequest, HelloReply } from './generated/greet_pb';
import { GreeterClient } from './generated/GreetServiceClientPb'

var client = new GreeterClient('https://localhost:5001');

var nameInput = document.getElementById('name') as HTMLInputElement;
var sendInput = document.getElementById('send') as HTMLInputElement;
var resultText = document.getElementById('result') as HTMLElement;

sendInput.onclick = function () {
    var request = new HelloRequest();
    request.setName(nameInput.value);

    client.sayHello(request, {}, (err, response) => {
        resultText.innerHTML = response.getMessage();
    });
};
