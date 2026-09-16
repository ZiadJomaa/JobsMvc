using JobsMvc.Services;
using Microsoft.AspNetCore.Mvc;

namespace JobMvc.Controllers
{
    public class AiController : Controller
    {
        private readonly AiService _aiService;

        public AiController(AiService aiService)
        {
            _aiService = aiService;
        }

        // GET: /Ai/Ask
        [HttpGet]
        public IActionResult Ask()
        {
            return View();
        }

        // POST: /Ai/Ask
        [HttpPost]
        public async Task<IActionResult> Ask([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserQuestion))
            {
                return Json(new { answer = "Please enter a valid question." });
            }

            // Get response from the AI service
            string aiResponse = await _aiService.GetAnswerFromAiAsync(request.UserQuestion);

            return Json(new { answer = aiResponse });
        }
    }

    public class ChatRequest
    {
        public string UserQuestion { get; set; }
    }
}