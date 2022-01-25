using System.Collections.Generic;
using System.IO;
using System.Linq;
using ParserLib;

namespace IndexConsistency
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
            "ch.09.classes.tex",
            "ch.10.modules.tex",
            "ch.11.builtintypes.tex",
            "ch.12.itergene.tex",
            "ch.13.async.tex",
            "ch.14.metaprogramming.tex",
            "ch.15.clientside.tex",
            "ch.16.serverside.tex",
            "ch.17.extensions.tex",
            "preface.tex"
        };

        private const string BaseInputPath = @"...\source";
        private const string BaseOutputPath = @"...\source.enumerated";

        static void Main()
        {
            var indices = new HashSet<string>();
            foreach (var file in Files)
            {
                var text = File.ReadAllText(Path.Combine(BaseInputPath, file));
                var localIndices = new Parser().CollectIndices(text);
                indices.UnionWith(localIndices);
            }

            var enumeratedIndices = indices.ToArray();
            Directory.CreateDirectory(BaseOutputPath);
            foreach (var file in Files)
            {
                var text = File.ReadAllText(Path.Combine(BaseInputPath, file));
                var modifiedText = new Parser().EnumerateIndices(text, enumeratedIndices);
                File.WriteAllText(Path.Combine(BaseOutputPath, file), modifiedText);
            }
        }
    }
}
