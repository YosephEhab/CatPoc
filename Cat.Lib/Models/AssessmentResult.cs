namespace Cat.Lib.Models;

public record AssessmentResult(string CandidateId, double FinalSkillLevelEstimate, int TotalQuestionsAnswered, TimeSpan Duration);