using System;
using Brigadier.NET.Context;

namespace Brigadier.NET.Suggestion
{
	public class IntegerSuggestion : Suggestion, IEquatable<IntegerSuggestion>
	{
        

        public IntegerSuggestion(StringRange range, int value, IMessage tooltip = null)
			: base(range, value.ToString(), tooltip)
		{
			Value = value;
		}

		public int Value { get; }

		public override bool Equals(object obj)
		{
			return ReferenceEquals(this, obj) || 
                   obj is IntegerSuggestion other && EqualsInternal(other);
		}

		public bool Equals(IntegerSuggestion other)
		{
			if (other is null) return false;
			return ReferenceEquals(this, other) || EqualsInternal(other);
        }

        private bool EqualsInternal(IntegerSuggestion other)
        {
            return Value == other.Value
                   && base.Equals(other);
		}

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Value);
        }

		public override string ToString()
		{
			return $"IntegerSuggestion{{value={Value}, range={Range}, text='{Text}', tooltip='{Tooltip}'}}";
		}

		public override int CompareTo(Suggestion o)
		{
			if (o is IntegerSuggestion integerSuggestion)
			{
				return integerSuggestion.Value.CompareTo(Value);
			}
			return base.CompareTo(o);
		}

		public override int CompareToIgnoreCase(Suggestion b)
		{
			return CompareTo(b);
		}
	}
}
