namespace Cat.Lib.Models;

public record Question(string Id, string Text, List<Option> Options, int SkillLevel);

public record Option(string Text, bool IsCorrect);