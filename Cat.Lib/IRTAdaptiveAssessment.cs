using Cat.Lib.Models;

namespace Cat.Lib;

public class IRTAdaptiveAssessment(List<Question> questionBank) : IAdaptiveAssessment
{
    private readonly List<(Question question, bool isCorrect)> answeredQuestions = [];
    private string candidateId;
    private double abilityEstimate = 0.0; // logit scale
    private readonly int maxQuestions = 4;
    private DateTime startTime;

    public void StartSession(string candidateId)
    {
        this.candidateId = candidateId;
        startTime = DateTime.UtcNow;
        answeredQuestions.Clear();
        abilityEstimate = 0.0;
    }

    public Question GetNextQuestion()
    {
        // Pick item closest to current ability
        var next = questionBank
            .Where(q => !answeredQuestions.Any(h => h.question.Id == q.Id))
            .OrderBy(q => Math.Abs(MapSkillLevelToDifficulty(q.SkillLevel) - abilityEstimate))
            .FirstOrDefault();

        return next ?? throw new InvalidOperationException("No more questions available.");
    }

    public void SubmitAnswer(string questionId, string answer)
    {
        bool isCorrect = questionBank.First(q => q.Id == questionId).Options.First(o => string.Equals(o.Text, answer, StringComparison.OrdinalIgnoreCase)).IsCorrect;
        var question = questionBank.First(q => q.Id == questionId);
        answeredQuestions.Add((question, isCorrect));
        UpdateAbilityEstimate3PL();
    }

    public bool ShouldEndSession() => answeredQuestions.Count >= maxQuestions;

    public double GetCurrentAbilityEstimate() => abilityEstimate;

    public AssessmentResult EndSession() => new(candidateId, MapDifficultyToSkillLevel(abilityEstimate, true), answeredQuestions.Count, DateTime.UtcNow - startTime);

    private void UpdateAbilityEstimate1PL()
    {
        // Use Newton-Raphson update to estimate theta (ability)
        double theta = abilityEstimate;

        for (int iter = 0; iter < 5; iter++) // max 5 iterations
        {
            double likelihood = 0.0;
            double information = 0.0;

            foreach (var (question, isCorrect) in answeredQuestions)
            {
                double p = 1.0 / (1.0 + Math.Exp(-(theta - MapSkillLevelToDifficulty(question.SkillLevel))));
                likelihood += (isCorrect ? 1.0 : 0.0) - p;
                information += p * (1 - p);

            }

            theta += likelihood / (information + 1e-6); // avoid division by zero
            double delta = likelihood / information;

            // Optional: clamp or check for divergence
            if (Math.Abs(delta) < 0.01) break;
        }

        abilityEstimate = Math.Max(-3.0, Math.Min(3.0, theta)); // clamp to a reasonable range
    }

    private void UpdateAbilityEstimate3PL()
    {
        double theta = abilityEstimate;

        for (int iter = 0; iter < 5; iter++) // Newton-Raphson loop
        {
            double likelihood = 0.0;
            double information = 0.0;

            foreach (var (question, isCorrect) in answeredQuestions)
            {
                double b = MapSkillLevelToDifficulty(question.SkillLevel);
                double a = question.Discrimination; // requires Discrimination in model
                double c = question.Guessing;       // requires Guessing in model

                double expTerm = Math.Exp(-a * (theta - b));
                double p = c + (1 - c) / (1 + expTerm); // 3PL probability

                double dP = a * (1 - c) * expTerm / Math.Pow(1 + expTerm, 2); // derivative of P(θ)

                // Guard against log(0) and division by zero
                p = Math.Clamp(p, 1e-6, 1 - 1e-6);

                // Gradient (1st derivative of log-likelihood)
                likelihood += ((isCorrect ? 1.0 : 0.0) - p) * dP / (p * (1 - p));

                // Fisher Information
                information += Math.Pow(dP, 2) / (p * (1 - p));
            }

            // Update ability estimate
            if (information > 0)
            {
                double delta = likelihood / information;
                theta += delta;

                if (Math.Abs(delta) < 0.01)
                    break; // converged
            }
        }

        // Clamp ability estimate to a reasonable range
        abilityEstimate = Math.Clamp(theta, -3.0, 3.0);
    }

    private static double MapSkillLevelToDifficulty(int skillLevel)
    {
        // skillLevel ∈ [1, 6]
        const int minSkill = 1;
        const int maxSkill = 6;
        const double minLogit = -2.5;
        const double maxLogit = 2.5;

        double scale = (maxLogit - minLogit) / (maxSkill - minSkill);
        return minLogit + (skillLevel - minSkill) * scale;
    }

    private static int MapDifficultyToSkillLevel(double difficulty, bool conservative = false)
    {
        const int minSkill = 1;
        const int maxSkill = 6;
        const double minLogit = -2.5;
        const double maxLogit = 2.5;
        double scale = (maxLogit - minLogit) / (maxSkill - minSkill);

        double rawSkill = (difficulty - minLogit) / scale + minSkill;

        int skill = conservative
            ? (int)Math.Floor(rawSkill)
            : (int)Math.Round(rawSkill);

        return Math.Clamp(skill, minSkill, maxSkill);
    }
}