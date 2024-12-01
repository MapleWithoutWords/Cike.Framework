namespace Cike.FluentValidation;

public class AdvancedJsonCamelCaseNamingPolicy : JsonNamingPolicy
{
    public char[] Separators { get; set; } = new[] { ' ', '.' };

    public override string ConvertName(string name)
    {
        if (string.IsNullOrEmpty(name) || !char.IsUpper(name[0]))
        {
            return name;
        }

        return string.Create(name.Length, name, (chars, name) =>
        {
            name.CopyTo(chars);
            FixCasing(chars);
        });
    }

    private void FixCasing(Span<char> chars)
    {
        var wordIndex = 0;
        var skip = false;

        for (int i = 0; i < chars.Length; i++)
        {
            if (wordIndex++ == 1 && !char.IsUpper(chars[i]))
            {
                if (Separators.Contains(chars[i]))
                {
                    wordIndex = 0;
                }
                continue;
            }

            bool hasNext = (i + 1 < chars.Length);

            if (i > 0 && hasNext)
            {
                if (Separators.Contains(chars[i + 1]))
                {
                    if (char.IsUpper(chars[i]))
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    skip = false;
                    i++;
                    wordIndex = 0;
                    continue;
                }
                else if (skip)
                {
                    continue;
                }
                else if (!char.IsUpper(chars[i + 1]))
                {
                    if (char.IsUpper(chars[i]) && wordIndex == 1)
                    {
                        chars[i] = char.ToLowerInvariant(chars[i]);
                    }

                    skip = true;
                    continue;
                }
            }

            chars[i] = char.ToLowerInvariant(chars[i]);
        }
    }
}