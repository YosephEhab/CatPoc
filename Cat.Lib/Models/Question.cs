namespace Cat.Lib.Models;

public record Question(string Id, string Text, List<Option> Options, int SkillLevel, double Discrimination, double Guessing);

public record Option(string Text, bool IsCorrect);