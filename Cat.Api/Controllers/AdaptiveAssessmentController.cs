using System.Collections.Concurrent;
using Cat.Api.Helpers;
using Cat.Api.Models;
using Cat.Lib;
using Cat.Lib.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cat.Api.Controllers;

[Route("api/adaptive-assessment")]
[ApiController]
public class AdaptiveAssessmentController : ControllerBase
{
    private static readonly List<Question> QuestionBank = QuestionGenerator.GenerateSampleQuestions(30);
    private static readonly ConcurrentDictionary<string, IAdaptiveAssessment> Sessions = new();

    [HttpPost("start")]
    public IActionResult Start([FromBody] string candidateId)
    {
        var session = new IRTAdaptiveAssessment(QuestionBank);
        session.StartSession(candidateId);
        Sessions[candidateId] = session;

        var firstQuestion = session.GetNextQuestion();
        return Ok(firstQuestion);
    }

    [HttpPost("submit")]
    public IActionResult Submit([FromBody] SubmitAnswerRequest request)
    {
        if (!Sessions.TryGetValue(request.CandidateId, out var session))
            return NotFound("Session not found.");

        session.SubmitAnswer(request.QuestionId, request.Answer);

        if (session.ShouldEndSession())
        {
            return Ok(new
            {
                message = "Assessment complete",
                shouldEnd = true
            });
        }

        var nextQuestion = session.GetNextQuestion();
        return Ok(new
        {
            shouldEnd = false,
            nextQuestion
        });
    }

    [HttpPost("end")]
    public IActionResult End([FromBody] string candidateId)
    {
        if (!Sessions.TryGetValue(candidateId, out var session))
            return NotFound("Session not found.");

        var result = session.EndSession();
        Sessions.TryRemove(candidateId, out _);

        return Ok(result);
    }
}
