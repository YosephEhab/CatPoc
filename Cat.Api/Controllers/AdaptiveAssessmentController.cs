using System.Collections.Concurrent;
using Cat.Api.Models;
using Cat.Lib;
using Cat.Lib.Models;
using Microsoft.AspNetCore.Mvc;

namespace Cat.Api.Controllers;

[Route("api/adaptive-assessment")]
[ApiController]
public class AdaptiveAssessmentController : ControllerBase
{
    private static readonly List<Question> QuestionBank =
    [
        new Question("Q1", "1 + 1 = ?", [new("2", true), new("3", false), new("4", false)], 1),
        new Question("Q2", "5 * 6 = ?", [new("30", true), new("25", false), new("20", false)], 2),
        new Question("Q3", "Derivative of x^2 = ?", [new("2x", true), new("x", false), new("x^2", false)], 3),
        new Question("Q4", "Limit of sin(x)/x as x→0 = ?", [new("1", true), new("0", false), new("∞", false)], 4),
        new Question("Q5", "∫(2x)dx", [new("x^2 + C", true), new("2x^2 + C", false), new("x + C", false)], 5)
    ];

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
