namespace ParserLib
{
    internal class IndexLevel
    {
        public string Text { get; }
        public string Visual { get; }
        public string[] See { get; }
        public string[] SeeAlso { get; }

        public IndexLevel(string text, string visual, string[] see, string[] seeAlso)
        {
            Text = text;
            Visual = visual;
            See = see;
            SeeAlso = seeAlso;
        }
    }
}
