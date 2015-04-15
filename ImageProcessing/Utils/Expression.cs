using System.Collections.Generic;
using System.Linq;

namespace ImageProcessing.Utils
{
    public class Expression
    {
        public string Command { get; private set; }
        public string[] Args { get; private set; }
        public int ArgsCount { get { return Args.Length; } }

        public Expression(string line)
        {
            var args = new List<string>();
            var str = "";
            var inQuotes = false;
            var c = '\0';
            foreach (var ac in line)
            {
                var pc = c;
                c = ac;
                if (c == '"')
                {
                    if (inQuotes && pc == '\\')
                    {
                        str += c;
                        continue;
                    }
                    inQuotes = !inQuotes;
                    continue;
                }
                if (inQuotes)
                {
                    str+=c;
                    continue;
                }
                if (char.IsWhiteSpace(c))
                {
                    if (str.Length < 1)
                        continue;
                    args.Add(str);
                    str = "";
                    continue;
                }
                str += c;
            }

            if (str.Length > 0)
                args.Add(str);
            

            if (args.Count > 0)
            {
                Command = args[0];
                Args = args.Skip(1).ToArray();
            }
            else
            {
                Command = "";
                Args = args.ToArray();
            }
        }
    }
}
