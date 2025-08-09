using System.Text;

public class TextFormatterUtility
{
    public static string ConvertToUpperUnderscore(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var result = new StringBuilder();
        result.Append(char.ToUpperInvariant(input[0]));

        for (int i = 1; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && !char.IsUpper(input[i - 1]))
            {
                result.Append('_');
            }
            result.Append(char.ToUpperInvariant(input[i]));
        }

        return result.ToString();
    }
}
