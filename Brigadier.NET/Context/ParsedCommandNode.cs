using System;
using Brigadier.NET.Tree;

namespace Brigadier.NET.Context
{
	public class ParsedCommandNode<TSource> : IEquatable<ParsedCommandNode<TSource>>
	{
        public ParsedCommandNode(CommandNode<TSource> node, StringRange range)
		{
			Node = node;
			Range = range;
		}

		public CommandNode<TSource> Node { get; }

		public StringRange Range { get; }

		public override string ToString()
		{
			return $"{Node}@{Range}";
		}

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || 
                   obj is ParsedCommandNode<TSource> other && EqualsInternal(other);
        }

        public bool Equals(ParsedCommandNode<TSource> other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || EqualsInternal(other);
        }

        private bool EqualsInternal(ParsedCommandNode<TSource> other)
        {
            return Equals(Node, other.Node) && Range.Equals(other.Range);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Node, Range);
        }
    }
}
