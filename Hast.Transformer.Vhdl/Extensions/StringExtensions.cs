namespace System
{
    internal static class StringExtensions
    {
        public static bool IsTaskFromResultMethodName(this string name) =>
            name.Contains("System.Threading.Tasks.Task::FromResult", StringComparison.InvariantCulture);

        public static bool IsTaskCompletedTaskPropertyName(this string name) =>
            name.Contains("System.Threading.Tasks.Task::CompletedTask", StringComparison.InvariantCulture);
    }
}
