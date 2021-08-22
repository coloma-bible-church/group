namespace Group.Common.Strings
{
    public static class StringExtensions
    {
        public static string Truncate(this string s, int length) =>
            s.Length <= length
                ? s
                : s[..length];
    }
}