namespace ResxFormatterTests.TestFoundation
{
    internal static class TextNormalization
    {
        internal static string NormalizeLineEndings(string text)
        {
            return text.Replace("\r\n", "\n").Replace("\r", "\n");
        }
    }
}
