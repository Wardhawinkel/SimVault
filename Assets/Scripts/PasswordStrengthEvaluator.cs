using System.Text.RegularExpressions;

/// <summary>
/// Evalueert de sterkte van een wachtwoord en geeft educatieve feedback.
/// Score 0 (zeer zwak) t/m 4 (zeer sterk).
/// </summary>
public static class PasswordStrengthEvaluator
{
    public static EvaluationResult Evaluate(string password)
    {
        if (string.IsNullOrEmpty(password))
            return new EvaluationResult(0, "Voer een wachtwoord in.");

        int score = 0;
        var tips  = new System.Collections.Generic.List<string>();

        // Lengte
        if (password.Length >= 8)  score++;
        else tips.Add("minimaal 8 tekens");

        if (password.Length >= 12) score++;
        else if (password.Length < 12) tips.Add("gebruik bij voorkeur 12+ tekens");

        // Hoofdletters
        if (Regex.IsMatch(password, @"[A-Z]")) score++;
        else tips.Add("voeg een hoofdletter toe");

        // Cijfers
        if (Regex.IsMatch(password, @"\d")) score++;
        else tips.Add("voeg een cijfer toe");

        // Speciale tekens
        if (Regex.IsMatch(password, @"[^a-zA-Z0-9]")) score++;
        else tips.Add("voeg een speciaal teken toe (!, @, #, ...)");

        // Score maximum 4
        score = System.Math.Min(score, 4);

        // Veelgebruikte zwakke wachtwoorden
        string[] common = { "wachtwoord", "password", "123456", "qwerty", "admin", "welkom" };
        foreach (var c in common)
        {
            if (password.ToLower().Contains(c))
            {
                score = 0;
                tips.Clear();
                tips.Add("dit wachtwoord is veel te voorspelbaar — kies iets unieks");
                break;
            }
        }

        string feedback = tips.Count == 0
            ? "✅ Uitstekend wachtwoord!"
            : "💡 Tip: " + string.Join(", ", tips) + ".";

        return new EvaluationResult(score, feedback);
    }
}

/// <summary>Resultaat van een wachtwoordevaluatie.</summary>
public class EvaluationResult
{
    public int    Score;      // 0 – 4
    public string Feedback;

    public EvaluationResult(int score, string feedback)
    {
        Score    = score;
        Feedback = feedback;
    }
}
