using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ParserLib
{
    public class Parser
    {
        public string[] CollectIndices(string text)
        {
            var set = new HashSet<string>();
            var stream = new InputStringStream(text);
            var next = stream.IndexOf(Consts.FullIndexCommand);
            while (next != InputStringStream.NoPos)
            {
                stream.Skip(next + Consts.FullIndexCommand.Length);
                var levels = ParseEntry(stream);
                var setInternal = new HashSet<string>();

                var wereAllAdded = true;
                foreach (var level in levels)
                {
                    wereAllAdded = setInternal.Add(level.Text);

                    foreach (var see in level.See)
                    {
                        setInternal.Add(see);
                    }

                    foreach (var seeAlso in level.SeeAlso)
                    {
                        wereAllAdded &= setInternal.Add(seeAlso);
                    }
                }
                
                if (!wereAllAdded)
                {
                    throw new ArgumentException();
                }

                set.UnionWith(setInternal);
                next = stream.IndexOf(Consts.FullIndexCommand);
            }
            return set.ToArray();
        }

        public string EnumerateIndices(string text, string[] indices)
        {
            var indicesNums = indices.Select((index, i) => (index, i)).ToDictionary(x => x.index, x => x.i);
            var builder = new StringBuilder();
            var stream = new InputStringStream(text);
            var nextStream = stream.IndexOf(Consts.FullIndexCommand);
            var prevText = 0;
            var nextText = text.IndexOf(Consts.FullIndexCommand) + Consts.FullIndexCommand.Length;
            while (nextStream != InputStringStream.NoPos)
            {
                builder.Append(text.Substring(prevText, nextText - prevText));
                stream.Skip(nextStream + Consts.FullIndexCommand.Length);
                var levels = ParseEntry(stream);

                var nums = new List<int>();
                foreach (var level in levels)
                {
                    nums.Add(indicesNums[level.Text]);

                    foreach (var see in level.See)
                    {
                        nums.Add(indicesNums[see]);
                    }

                    foreach (var seeAlso in level.SeeAlso)
                    {
                        nums.Add(indicesNums[seeAlso]);
                    }
                }

                var numsText = string.Join('_', nums.ToArray());
                builder.Append('{' + numsText + '}');

                nextStream = stream.IndexOf(Consts.FullIndexCommand);

                prevText = nextText;
                nextText = text.IndexOf(Consts.FullIndexCommand, nextText) + Consts.FullIndexCommand.Length;
            }
            builder.Append(text.Substring(prevText));

            return builder.ToString();
        }

        internal enum State
        {
            None,
            Level,
            See,
            SeeAlso,
            PageNumStyle,
            Stop,
        }

        private bool IsFollowingCommand(InputStringStream stream, string command)
        {
            var str = stream.Peek(command.Length + 1);
            return str.StartsWith(command) && (str.Last() == ' ' || str.Last() == Consts.ArgOpen);
        }

        private string[] ParseSee(InputStringStream stream)
        {
            stream.Skip(stream.IndexOf(Consts.ArgOpen));
            stream.Skip(1);
            var see = stream.Get(stream.IndexOf(Consts.ArgClose));
            stream.Skip(1);
            return see.Split(',').Select(x => x.Trim()).ToArray();
        }

        private (string Text, string Visual) SeparateVisual(string input)
        {
            var startVisual = 0;
            do
            {
                startVisual = input.IndexOf(Consts.Visual, startVisual);
            }
            while (startVisual != -1 && input[startVisual - 1] == Consts.IndexEscape);

            if (startVisual != -1)
            {
                return (input.Substring(0, startVisual), input.Substring(startVisual + 1));
            }
            else
            {
                return (input, string.Empty);
            }
        }

        private IndexLevel CreateLevel(string input, string[] see, string[] seeAlso)
        {
            var (text, visual) = SeparateVisual(input);
            return new IndexLevel(text, visual, see, seeAlso);
        }

        private IReadOnlyCollection<IndexLevel> ReadLevels(InputStringStream stream)
        {
            var state = State.None;
            var currentLevel = string.Empty;

            var levels = new List<IndexLevel>();

            while (state != State.Stop)
            {
                switch (state)
                {
                    case State.Level:
                        {
                            var curr = stream.Get();

                            var isEscaped = false;
                            if (curr == Consts.IndexEscape)
                            {
                                curr = stream.Get();
                                isEscaped = true;
                            }

                            if (!isEscaped && curr == Consts.Level)
                            {
                                levels.Add(CreateLevel(currentLevel, new string[] { }, new string[] { }));
                                currentLevel = string.Empty;
                            }
                            else if (!isEscaped && curr == Consts.Subcommand)
                            {
                                if (stream.Peek() == Consts.PageRangeOpen || stream.Peek() == Consts.PageRangeClose)
                                {
                                    stream.Skip(1);
                                }
                                else if (IsFollowingCommand(stream, Consts.SeeCommand))
                                {
                                    state = State.See;
                                    stream.Skip(Consts.SeeCommand.Length);
                                }
                                else if (IsFollowingCommand(stream, Consts.SeeAlsoCommand))
                                {
                                    state = State.SeeAlso;
                                    stream.Skip(Consts.SeeAlsoCommand.Length);
                                }
                                else
                                {
                                    state = State.PageNumStyle;
                                }
                            }
                            else if (!isEscaped && curr == Consts.ArgClose)
                            {
                                levels.Add(CreateLevel(currentLevel, new string[] { }, new string[] { }));
                                state = State.Stop;
                            }
                            else
                            {
                                currentLevel += curr;
                            }
                        }
                        break;
                    case State.See:
                        var see = ParseSee(stream);
                        levels.Add(CreateLevel(currentLevel, see, new string[] { }));
                        currentLevel = string.Empty;
                        state = State.None;
                        break;
                    case State.SeeAlso:
                        var seeAlso = ParseSee(stream);
                        levels.Add(CreateLevel(currentLevel, new string[] { }, seeAlso));
                        currentLevel = string.Empty;
                        state = State.None;
                        break;
                    case State.None:
                        {
                            var curr = stream.Get();
                            if (char.IsWhiteSpace(curr)) { }
                            else if (curr == Consts.ArgOpen)
                            {
                                state = State.Level;
                            }
                            else if (curr == Consts.ArgClose)
                            {
                                state = State.Stop;
                            }
                            else
                            {
                                throw new InvalidOperationException();
                            }
                        }
                        break;
                    case State.PageNumStyle:
                        if (stream.Get() == Consts.ArgClose)
                        {
                            state = State.Stop;
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            return levels;
        }

        private IReadOnlyCollection<IndexLevel> ParseEntry(InputStringStream stream)
        {
            if (stream.Peek() != Consts.ArgOpen) throw new InvalidOperationException();

            var levels = ReadLevels(stream);
            return levels;
        }
    }
}
