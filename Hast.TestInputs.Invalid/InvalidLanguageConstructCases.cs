namespace Hast.TestInputs.Invalid
{
    public class InvalidLanguageConstructCases
    {
        public void BreakStatements()
        {
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) break;
            }
        }
    }
}
