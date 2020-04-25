using System;
using System.Collections.Generic;
using System.Linq;
using Brigadier.NET.Tree;

namespace Brigadier.NET.Context
{
	public class CommandContext<TSource> : IEquatable<CommandContext<TSource>>
	{
        private readonly IDictionary<string, IParsedArgument> _arguments;
		private readonly bool _forks;

	    public CommandContext(TSource source, string input, IDictionary<string, IParsedArgument> arguments, Command<TSource> command, CommandNode<TSource> rootNode, List<ParsedCommandNode<TSource>> nodes, StringRange range, CommandContext<TSource> child, RedirectModifier<TSource> modifier, bool forks)
		{
			Source = source;
			Input = input;
			_arguments = arguments;
			Command = command;
			RootNode = rootNode;
			Nodes = nodes;
			Range = range;
			Child = child;
			RedirectModifier = modifier;
			_forks = forks;
		}

		public CommandContext<TSource> CopyFor(TSource source)
		{
			if (Source.Equals(source))
			{
				return this;
			}
			return new CommandContext<TSource>(source, Input, _arguments, Command, RootNode, Nodes, Range, Child, RedirectModifier, _forks);
		}

		public CommandContext<TSource> Child { get; }

		public CommandContext<TSource> LastChild
		{
			get
			{
				var result = this;
				while (result.Child != null)
				{
					result = result.Child;
				}

				return result;
			}
		}

		public Command<TSource> Command { get; }

		public TSource Source { get; }

		public T GetArgument<T>(string name)
		{
			if (!_arguments.TryGetValue(name, out var argument))
			{
				throw new InvalidOperationException($"No such argument '{name}' exists on this command");
			}

			var result = argument.Result;

			if (result is T v)
			{
				return v;
			}
			else
			{
				throw new InvalidOperationException($"Argument {name}' is defined as {result.GetType().Name}, not {typeof(T).Name}");
			}
		}

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || 
                   obj is CommandContext<TSource> other && EqualsInternal(other);
        }

		public bool Equals(CommandContext<TSource> other)
        {
            if (other is null) return false;
            return ReferenceEquals(this, other) || EqualsInternal(other);
        }

        private bool EqualsInternal(CommandContext<TSource> other)
        {
            return Equals(_arguments, other._arguments)
                   && Equals(RootNode, other.RootNode)
                   && Equals(Nodes, other.Nodes)
                   && Equals(Command, other.Command)
                   && EqualityComparer<TSource>.Default.Equals(Source, other.Source)
                   && Equals(Child, other.Child);
		}

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, _arguments, Command, RootNode, Nodes, Child);
        }

        public RedirectModifier<TSource> RedirectModifier { get; }

		public StringRange Range { get; }

		public string Input { get; }

		public CommandNode<TSource> RootNode { get; }

		public List<ParsedCommandNode<TSource>> Nodes { get; }

		public bool HasNodes()
		{
			return Nodes.Count > 0;
		}

		public bool IsForked()
		{
			return _forks;
		}
	}
}