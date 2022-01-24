using System;
using System.Diagnostics;

namespace Parser
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class InputStringStream
    {
        public const int NoPos = -1;

        private readonly string _text;
        private int _pos = NoPos;

        private string DebuggerDisplay { get => _text.Substring(_pos); }

        public InputStringStream(string text)
        {
            _text = text ?? throw new ArgumentNullException(nameof(text));
            _pos = 0;
        }

        public bool IsEof => _pos >= _text.Length;

        public char Get() => _text[_pos++];

        public string Get(int length)
        {
            var res = _text.Substring(_pos, length);
            _pos += length;
            return res;
        }

        public char Peek() => _text[_pos];

        public string Peek(int length) => _text.Substring(_pos, length);

        public void Skip(int delta)
        {
            if (delta < 0)
            {
                throw new ArgumentException(nameof(delta));
            }
            _pos += delta;
        }

        public int IndexOf(char target)
        {
            var pos = _text.IndexOf(target, _pos);
            if (pos == -1) return NoPos;
            return pos - _pos;
        }

        public int IndexOf(string target)
        {
            var pos = _text.IndexOf(target, _pos);
            if (pos == -1) return NoPos;
            return pos - _pos;
        }
    }
}
