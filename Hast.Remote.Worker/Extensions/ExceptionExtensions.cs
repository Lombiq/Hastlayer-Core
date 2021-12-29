using System.Text;

namespace System
{
    public static class ExceptionExtensions
    {
        /// <summary>
        /// We don't want to show the stack trace to the user, just exception message, so building one by iterating all
        /// the nested exceptions.
        /// </summary>
        public static string WithoutStackTrace(this Exception ex)
        {
            var currentException = ex;
            var message = new StringBuilder();

            while (currentException != null)
            {
                message.AppendLine(currentException.Message);
                currentException = currentException.InnerException;
            }

            return message.ToString();
        }
    }
}
