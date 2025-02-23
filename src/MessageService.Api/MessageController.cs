using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using MessageService.Dal;
using Microsoft.OpenApi.Models;

namespace MessageService.Api
{
    [ApiController]
    [Route("api/v1/messages")]
    public class MessageController : ControllerBase
    {
        private readonly IMessageRepository _messageRepository;
        private readonly WebSocketHandler _webSocketHandler;

        public MessageController(IMessageRepository messageRepository, WebSocketHandler webSocketHandler)
        {
            _messageRepository = messageRepository;
            _webSocketHandler = webSocketHandler;
        }

        /// <summary>
        /// Получить все сообщения
        /// </summary>
        [HttpGet("all")]
        [ProducesResponseType(typeof(List<Message>), 200)]
        public async Task<ActionResult<List<Message>>> GetAll()
        {
            var messages = await _messageRepository.GetAllMessagesAsync();
            return Ok(messages);
        }

        /// <summary>
        /// Получить сообщение по ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Message), 200)]
        public async Task<ActionResult<Message>> Get(int id)
        {
            var message = await _messageRepository.GetMessageByIdAsync(id);
            if (message == null) return NotFound(new { message = "Message not found" });

            return Ok(message);
        }

        /// <summary>
        /// Получить сообщения за временной период
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<List<Message>>> GetMessagesByPeriod([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            if (from >= to)
                return BadRequest(new { message = "Date 'from' should be less than date 'to'" });

            var messages = await _messageRepository.GetMessagesByPeriodAsync(from, to);
            return Ok(messages);
        }

        /// <summary>
        /// Отправить сообщение
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Message), 201)]
        public async Task<ActionResult> Create([FromBody] CreateMessageRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { message = "Message is empty" });

            if (request.MessageId <= 0)
                return BadRequest(new { message = "MessageId is required and must be greater than 0" });

            var existingMessage = await _messageRepository.GetMessageByIdAsync(request.MessageId);
            if (existingMessage != null)
                return Conflict(new { message = "MessageId must be unique" });

            var message = new Message(request.Text, request.MessageId);
            await _messageRepository.AddMessageAsync(message);
            await _webSocketHandler.SendMessageToClients(message);

            return CreatedAtAction(nameof(Get), new { id = message.Id }, message);
        }

        /// <summary>
        /// Удалить сообщение по ID
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existingMessage = await _messageRepository.GetMessageByIdAsync(id);
            if (existingMessage == null)
                return NotFound(new { message = "Message not found" });

            await _messageRepository.DeleteMessageAsync(id);
            return NoContent();
        }

    }

    /// <summary>
    /// DTO для создания сообщения
    /// </summary>
    public class CreateMessageRequest
    {
        public string Text { get; set; }
        public int MessageId { get; set; }

        public CreateMessageRequest(string text, int messageId)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            MessageId = messageId;
        }
    }

}
