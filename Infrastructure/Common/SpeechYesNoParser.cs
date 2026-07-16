using System.Globalization;
using System.Text;

namespace NiquiBackend.Infrastructure.Common;

// Interpreta la transcripción de twilio (SpeechResult) como si/no/Indeterminado
public static class SpeechYesNoParser
{
  private static readonly string[] YesWords = { "si", "s", "claro", "correcto", "afirmativo", "exacto", "vale", "dale", "aja" };
  private static readonly string[] NoWords = { "no", "nel", "negativo", "nunca", "jamas" };

  public static bool? Parse(string? speechResult)
  {
    if(string.IsNullOrWhiteSpace(speechResult)) return null;

    var normalized = RemoveAccents(speechResult.Trim().ToLowerInvariant());
    var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (words.Any(w => YesWords.Contains(w))) return true;
    if (words.Any(w => NoWords.Contains(w))) return false;

    return null;
  }

  private static string RemoveAccents(string input)
  {
    var descomposed = input.Normalize(NormalizationForm.FormD);
    var sb = new StringBuilder();
    foreach (var c in descomposed)
    {
      if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
          sb.Append(c);
    }
    return sb.ToString();
  }
}