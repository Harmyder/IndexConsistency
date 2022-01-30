using System;
using System.IO;
using System.Text;
using ParserLib;

namespace IndexCopy
{
    class Program
    {
        private static readonly string[] Files = new[] {
            "ch.01.intro.tex",
            "ch.02.lexical.tex",
            "ch.03.datatypes.tex",
            "ch.04.expressions.tex",
            "ch.05.statements.tex",
            "ch.06.objects.tex",
            "ch.07.arrays.tex",
            "ch.08.functions.tex",
        };

        private const string BaseInputPath = @"D:\Yola\Books\js7\js7.before.Olga\source.enumerated";
        private const string BaseOutputPath = @"D:\Yola\Books\js7\js7.before.Olga\source";

        static void Main()
        {
            foreach (var file in Files)
            {
                var textOrig = File.ReadAllText(Path.Combine(BaseInputPath, file));
                var textTran = File.ReadAllText(Path.Combine(BaseOutputPath, file));

                var builder = new StringBuilder();
                var currOrig = textOrig.IndexOf(Consts.FullIndexCommand) + Consts.FullIndexCommand.Length;
                var nextTran = textTran.IndexOf(Consts.FullIndexCommand) + Consts.FullIndexCommand.Length;
                var currTran = 0;
                while (currOrig != Consts.FullIndexCommand.Length - 1)
                {
                    builder.Append(textTran.Substring(currTran, nextTran - currTran));
                    currTran = nextTran;

                    var start = currOrig;
                    currOrig = textOrig.IndexOf(Consts.ArgClose, start) + 1;
                    var stop = currOrig;
                    var numbers = textOrig.Substring(start, stop - start);
                    builder.Append(numbers);

                    currOrig = textOrig.IndexOf(Consts.FullIndexCommand, currOrig) + Consts.FullIndexCommand.Length;
                    nextTran = textTran.IndexOf(Consts.FullIndexCommand, currTran) + Consts.FullIndexCommand.Length;
                }
                builder.Append(textTran.Substring(currTran));
                File.WriteAllText(Path.Combine(BaseOutputPath, file), builder.ToString());
            }
        }
    }
}
