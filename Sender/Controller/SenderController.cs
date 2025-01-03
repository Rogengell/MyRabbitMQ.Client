using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SharedMessages;

namespace Sender.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class SenderController : ControllerBase
    {
        private readonly MessageClient _messageClient;

        public SenderController(MessageClient messageClient)
        {
            _messageClient = messageClient;
        }
    
        [HttpPost]
        public async Task<bool> Post()
        {
            var CorrelationIdGenerated = Guid.NewGuid().ToString();
            var message = new PongMessage{ Message = "Pong!", CorrelationId = CorrelationIdGenerated };

            var response = _messageClient.Responce(CorrelationIdGenerated);
            var Request = _messageClient.Send<PongMessage>(message, "Order");


            await Task.WhenAll(response, Request);

            return response.Result;
        }

    }
}