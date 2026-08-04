using System.Globalization;
using System.Text;

namespace NiquiBackend.Infrastructure.Common;

public static class SpeechYesNoParser
{
    private static readonly string[] YesPhrases =
    {
        "esta bien", "claro que si", "por supuesto", "totalmente de acuerdo",
        "de acuerdo", "si gracias", "listo gracias", "de una gracias",
        "muchas gracias", "de una", "eso es", "ah bueno", "bueno senor",
        "bueno senora", "listo pues", "si por favor"
    };

    private static readonly string[] YesWords =
    {
        "si", "s", "claro", "correcto", "afirmativo", "exacto", "dale", "aja",
        "sizas", "sisas", "hagale", "firme", "firmes", "obvio", "listo",
        "copiado", "sale", "gracias", "perfecto", "bueno", "vale", "ok"
    };

    private static readonly string[] NoPhrases =
    {
        "para nada", "no senor", "no senora", "no gracias", "nanay nicanor",
        "que va", "ni de fundas", "ni a palos", "nada que ver"
    };

    private static readonly string[] NoWords =
    {
        "no", "nel", "negativo", "nunca", "jamas", "tampoco", "incorrecto",
        "nanay", "imposible"
    };

    public static bool? Parse(string? speechResult)
    {
        if (string.IsNullOrWhiteSpace(speechResult)) return null;

        var normalized = RemoveAccents(speechResult.Trim().ToLowerInvariant());

        var cleaned = new string(normalized.Where(c => !char.IsPunctuation(c)).ToArray());

        if (YesPhrases.Any(phrase => cleaned.Contains(phrase))) return true;
        if (NoPhrases.Any(phrase => cleaned.Contains(phrase))) return false;

        var words = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Any(w => YesWords.Contains(w))) return true;
        if (words.Any(w => NoWords.Contains(w))) return false;

        return null;
    }

    private static string RemoveAccents(string input)
    {
        var decomposed = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString();
    }
}