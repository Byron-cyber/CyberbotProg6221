namespace CyberbotGUI.Helpers
{
    public static class Validator
    {
        public static bool IsValid(string input)
        {
            return !string.IsNullOrWhiteSpace(input) && input.Trim().Length >= 1;
        }
    }
}
