using System.Text.Json;
using GraduationAPI_EPOSHBOOKING.DTO;
using Microsoft.AspNetCore.Mvc;

namespace GraduationAPI_EPOSHBOOKING.Controllers.TelegramBot
{
    [ApiController]
    [Route("api/v1/telegram-bot")]
    public class TelegramBotController : Controller
    {
        private readonly IConfiguration _configuration;
        public TelegramBotController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("get-chat-id")]
        public async Task<IActionResult> GetChatID()
        {
            string botToken = _configuration["Serilog:WriteTo:2:Args:token"]!;
            string url = $"https://api.telegram.org/bot{botToken}/getUpdates";

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jsonResponse = System.Text.Json.JsonSerializer.Deserialize<TelegramResponse>(responseBody, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (jsonResponse?.Ok == true && jsonResponse.Result?.Any() == true)
                    {
                        var chatId = jsonResponse.Result.First().Message.Chat.Id;
                        return Ok(new { ChatId = chatId });
                    }

                    return Ok(new { Message = "Không tìm thấy cập nhật nào. Hãy nhắn tin cho bot trước." });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new { Error = ex.Message });
                }
            }
        }
    }
}
