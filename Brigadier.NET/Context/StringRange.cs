using System;

namespace Brigadier.NET.Context
{
	public readonly struct StringRange
	{
        public StringRange(int start, int end)
		{
			Start = start;
			End = end;
		}

		public static StringRange At(int pos)
		{
			return new StringRange(pos, pos);
		}

		public static StringRange Between(int start, int end)
		{
			return new StringRange(start, end);
		}

		public static StringRange Encompassing(StringRange a, StringRange b)
		{
			return new StringRange(Math.Min(a.Start, b.Start), Math.Max(a.End, b.End));
		}

		public int Start { get; }

		public int End { get; }

		public string Get(IImmutableStringReader reader)
		{
			return reader.String.Substring(Start, End - Start);
		}

		public string Get(string source)
		{
			return source.Substring(Start, End - Start);
		}


		public bool IsEmpty => Start == End;

		public int Length => End - Start;

        public override bool Equals(object obj)
        {
            return obj is StringRange other && Equals(other);
        }

        public bool Equals(StringRange other)
        {
            return Equals(Start, other.Start) && Equals(End, other.End);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Start, End);
        }

		public override string ToString()
		{
			return $"StringRange{{start={Start}, end={End}}}";
		}
	}
}
