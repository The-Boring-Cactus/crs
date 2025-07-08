using System.Text;
using FunctEngine.Exceptions;
namespace FunctEngine
{
    public class StringLiteralEscapeCodeParser
    {
        public StringLiteralEscapeCodeParser()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        /// <summary>
		/// This function translates all literal escape codes into their ASCII equivalent
		/// </summary>
		/// <param name="inputString">The input string containing escape codes</param>
		/// <returns>The output string with all escape codes converted.</returns>
		public static string ParseEscapeCodes(string inputString)
        {
            // If there aren't any escape codes, we don't need to go through this at all...
            // let's break out early...
            if (inputString.Contains("\\") == false) return inputString;

            char[] strArray = new char[inputString.Length];
            strArray = inputString.ToCharArray();

            StringBuilder retString = new StringBuilder();

            // Now We Loop Through all characters
            // of the input string looking for escape 
            // sequences.
            string curEscapeSequence = "";
            bool inEscape = false;

            for (int x = 0; x < strArray.Length; x++)
            {

                if (inEscape == false)
                {
                    if (strArray[x] == '\\')
                    {
                        inEscape = true;
                        continue;
                    }
                    retString.Append(strArray[x]);
                }
                else // If We Are Currently in an escape string
                {


                    if ((char.IsDigit(strArray[x]) == false) && (curEscapeSequence.Length > 0))
                    {
                        int num = int.Parse(curEscapeSequence);
                        curEscapeSequence = "";
                        char newChar = (char)num;
                        retString.Append(newChar);
                        inEscape = false;
                        retString.Append(strArray[x]);
                        continue;
                    }

                    if ((char.IsDigit(strArray[x]) == true))
                    {
                        curEscapeSequence += strArray[x];
                        continue;
                    }

                    else if (strArray[x] == '\\')
                    {
                        retString.Append("\\");
                        curEscapeSequence = "";
                        inEscape = false;
                        continue;
                    }
                    else if (strArray[x] == 't')
                    {
                        retString.Append("\t");
                        curEscapeSequence = "";
                        inEscape = false;
                        continue;
                    }
                    else if (strArray[x] == 'n')
                    {
                        retString.Append("\n");
                        curEscapeSequence = "";
                        inEscape = false;
                        continue;
                    }
                    else if (strArray[x] == 'r')
                    {
                        retString.Append("\r");
                        curEscapeSequence = "";
                        inEscape = false;
                        continue;
                    }
                    else if (strArray[x] == '"')
                    {
                        retString.Append("\"");
                        curEscapeSequence = "";
                        inEscape = false;
                        continue;
                    }
                    else
                    {
                        throw new GeneralParseException("Error Parsing String Literal [" + inputString + "]");
                    }


                }

            }
            // Let's Make Sure that We Don't have a last escape sequence trailing
            if (inEscape == true)
            {
                int num = int.Parse(curEscapeSequence);
                curEscapeSequence = "";
                char newChar = (char)num;
                retString.Append(newChar);
            }


            return retString.ToString();

        }
    }
}
