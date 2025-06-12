namespace Cat.Api.Models;

public record SubmitAnswerRequest(string CandidateId, string QuestionId, string Answer);