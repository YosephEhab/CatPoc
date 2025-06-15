using Cat.Lib.Models;

namespace Cat.Api.Helpers;

public static class QuestionGenerator
{
    public static List<Question> GenerateSampleQuestions(int count)
    {
        var questions = new List<Question>();
        var rand = new Random();

        for (int i = 1; i <= count; i++)
        {
            int skillLevel = (i - 1) % 6 + 1; // cycle from 1 to 6
            string id = $"Q{i:00}";
            string text = $"What is the answer to question {i}?";

            // Generate 4 options: 1 correct, 3 incorrect
            var options = new List<Option>();
            int correctIndex = rand.Next(0, 4);

            for (int j = 0; j < 4; j++)
            {
                bool isCorrect = j == correctIndex;
                options.Add(new Option($"Option {j + 1}", isCorrect));
            }

            // Discrimination: between 0.8 and 1.5
            double discrimination = Math.Round(0.8 + rand.NextDouble() * 0.7, 2);

            // Guessing: fixed at 0.25 for 4 options (can vary if desired)
            double guessing = 0.25;

            questions.Add(new Question(id, text, options, skillLevel, discrimination, guessing));
        }

        return questions;
    }
}