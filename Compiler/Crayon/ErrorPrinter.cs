using System.Collections.Generic;

namespace Crayon
{
    class ErrorPrinter
    {
        public static void ShowErrors(IList<Wax.Error> errors, bool throwAnyway)
        {
            List<string> errorLines = new List<string>();
            foreach (Wax.Error error in errors)
            {
                string errorString;
                if (error.Line != 0)
                {
                    errorString = error.FileName + ", Line " + error.Line + ", Column " + error.Column + ": " + error.Message;
                }
                else if (error.FileName != null)
                {
                    errorString = error.FileName + ": " + error.Message;
                }
                else
                {
                    errorString = error.Message;
                }
                errorLines.Add(errorString);
            }

            string finalErrorString = string.Join("\n\n", errorLines);

            if (throwAnyway)
            {
                throw new System.Exception(finalErrorString);
            }
            Common.ConsoleWriter.Print(Common.ConsoleMessageType.PARSER_ERROR, finalErrorString);
        }
    }
}
