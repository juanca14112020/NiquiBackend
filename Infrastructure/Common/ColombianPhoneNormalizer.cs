using System.Text.RegularExpressions;

namespace NiquiBackend.Infrastructure.Common;

//Normaloza cualquier variante de celilar colombiano al formato +573xxxxxx
//Devuelve null si no se puede reconocer como un celular colombiano valido.
public static class ColombianPhoneNormalizer
{
    public static string? Normalize(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
            return null;


        var digits = Regex.Replace(rawValue, @"[^\d]", "");

        if (string.IsNullOrEmpty(digits))
            return null;

            //Si ya viene con el indicativo de pais, se lo quitamos
            if(digits.Length == 12 && digits.StartsWith("57"))
                digits = digits[2..];

            if (digits.Length != 10 || digits[0] != '3')
                return null;

            return "+57" + digits;
    }
}