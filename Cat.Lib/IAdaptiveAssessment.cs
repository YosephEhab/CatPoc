using Cat.Lib.Models;

namespace Cat.Lib;

public interface IAdaptiveAssessment
{
    /// <summary>
    /// Initializes a new assessment session for a candidate.
    /// </summary>
    /// <param name="candidateId">Unique identifier for the test taker.</param>
    void StartSession(string candidateId);

    /// <summary>
    /// Returns the next question tailored to the candidate's ability.
    /// </summary>
    /// <returns>The next question object.</returns>
    Question GetNextQuestion();

    /// <summary>
    /// Submits the answer for the current question and updates the ability estimate.
    /// </summary>
    /// <param name="questionId">ID of the answered question.</param>
    /// <param name="answer">The submitted answer.</param>
    void SubmitAnswer(string questionId, string answer);

    /// <summary>
    /// Determines if the assessment session should end based on stopping rules.
    /// </summary>
    /// <returns>True if session should end, false otherwise.</returns>
    bool ShouldEndSession();

    /// <summary>
    /// Gets the current ability estimate of the candidate.
    /// </summary>
    /// <returns>A floating-point value representing ability.</returns>
    double GetCurrentAbilityEstimate();

    /// <summary>
    /// Ends the assessment session and returns a result object.
    /// </summary>
    /// <returns>The final result of the assessment.</returns>
    AssessmentResult EndSession();
}
