namespace UtilityLibraries2;

public static class StringLibrary
{
    public static bool StartsWithUpper2(this string? str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        char ch = str[0];
        return char.IsUpper(ch);
    }
}