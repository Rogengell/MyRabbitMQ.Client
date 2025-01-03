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
        private readonly RabbitMqClient _rabbitMqClient;

        public SenderController(RabbitMqClient rabbitMqClient)
        {
            _rabbitMqClient = rabbitMqClient;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] MessageRequest request)
        {
            var result = await _rabbitMqClient.SendAndReceiveAsync(request.Message, request.RoutingKey);

            if (result)
            {
                return Ok("Message processed successfully.");
            }
            else
            {
                return StatusCode(504, "Timeout waiting for response.");
            }
        }
    }
}