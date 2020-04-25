using System;
using System.Text;
using Brigadier.NET.Context;

namespace Brigadier.NET.Suggestion
{
	public class Suggestion : IComparable<Suggestion>, IEquatable<Suggestion>
	{
        public Suggestion(StringRange range, string text, IMessage tooltip = null)
		{
			Range = range;
			Text = text;
			Tooltip = tooltip;
		}

		public StringRange Range { get; }

		public string Text { get; }

		public IMessage Tooltip { get; }

		public string Apply(string input)
		{
			if (Range.Start == 0 && Range.End == input.Length)
			{
				return Text;
			}
			var result = new StringBuilder();
			if (Range.Start > 0)
			{
				result.Append(input.Substring(0, Range.Start));
			}
			result.Append(Text);
			if (Range.End < input.Length)
			{
				result.Append(input.Substring(Range.End));
			}
			return result.ToString();
		}

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || 
                   obj is Suggestion other && EqualsInternal(other);
        }

        public bool Equals(Suggestion other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || EqualsInternal(other);
        }

        private bool EqualsInternal(Suggestion other)
        {
            return Equals(Range, other.Range) &&
                   Equals(Text, other.Text) &&
                   Equals(Tooltip, other.Tooltip);
		}

        public override int GetHashCode()
		{
			return HashCode.Combine(Range, Text, Tooltip);
        }

		public override string ToString()
		{
			return $"Suggestion{{range={Range}, text='{Text}', tooltip='{Tooltip}}}";
		}

		public virtual int CompareTo(Suggestion o)
		{
			return string.Compare(Text, o.Text, StringComparison.Ordinal);
		}

		public virtual int CompareToIgnoreCase(Suggestion b)
		{
			return string.Compare(Text, b.Text, StringComparison.OrdinalIgnoreCase);
		}

		public Suggestion Expand(string command, StringRange range)
		{
			if (range.Equals(Range))
			{
				return this;
			}

			var result = new StringBuilder();
			if (range.Start < Range.Start)
			{
				result.Append(command.Substring(range.Start, Range.Start - range.Start));
			}
			result.Append(Text);
			if (range.End > Range.End)
			{
				result.Append(command.Substring(Range.End, range.End - Range.End));
			}
			return new Suggestion(range, result.ToString(), Tooltip);
		}
	}
}
