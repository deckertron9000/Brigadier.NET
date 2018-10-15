// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.Threading.Tasks;
using Brigadier.NET.Builder;
using Brigadier.NET.Context;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Brigadier.NET.Tests
{
	public class CommandSuggestionsTest {
		private readonly CommandDispatcher<object> _subject;
		private readonly object _source = Substitute.For<object>();

		public CommandSuggestionsTest()
		{
			_subject = new CommandDispatcher<object>();
		}

		private void TestSuggestions(string contents, int cursor, StringRange range, params string[] suggestions) {
			var result = _subject.GetCompletionSuggestions(_subject.Parse(contents, _source), cursor);
			result.Range.Should().BeEquivalentTo(range);

			var expected = new List<Suggestion.Suggestion>();
			foreach (var suggestion in suggestions) {
				expected.Add(new Suggestion.Suggestion(range, suggestion));
			}

			result.List.Should().BeEquivalentTo(expected);
		}

		private static StringReader InputWithOffset(string input, int offset) {
			var result = new StringReader(input)
			{
				Cursor = offset
			};
			return result;
		}

		[Fact]
		public void getCompletionSuggestions_rootCommands(){
			_subject.Register(r => r.Literal("foo"));
			_subject.Register(r => r.Literal("bar"));
			_subject.Register(r => r.Literal("baz"));

			var result = _subject.GetCompletionSuggestions(_subject.Parse("", _source));

			result.Range.Should().BeEquivalentTo(StringRange.At(0));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.At(0), "bar"), new Suggestion.Suggestion(StringRange.At(0), "baz"), new Suggestion.Suggestion(StringRange.At(0), "foo")});
		}

		[Fact]
		public void getCompletionSuggestions_rootCommands_withInputOffset(){
			_subject.Register(r => r.Literal("foo"));
			_subject.Register(r => r.Literal("bar"));
			_subject.Register(r => r.Literal("baz"));

			var result = _subject.GetCompletionSuggestions(_subject.Parse(InputWithOffset("OOO", 3), _source));

			result.Range.Should().BeEquivalentTo(StringRange.At(3));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.At(3), "bar"), new Suggestion.Suggestion(StringRange.At(3), "baz"), new Suggestion.Suggestion(StringRange.At(3), "foo")});
		}

		[Fact]
		public void getCompletionSuggestions_rootCommands_partial(){
			_subject.Register(r => r.Literal("foo"));
			_subject.Register(r => r.Literal("bar"));
			_subject.Register(r => r.Literal("baz"));

			var result = _subject.GetCompletionSuggestions(_subject.Parse("b", _source));

			result.Range.Should().BeEquivalentTo(StringRange.Between(0, 1));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.Between(0, 1), "bar"), new Suggestion.Suggestion(StringRange.Between(0, 1), "baz")});
		}

		[Fact]
		public void getCompletionSuggestions_rootCommands_partial_withInputOffset(){
			_subject.Register(r => r.Literal("foo"));
			_subject.Register(r => r.Literal("bar"));
			_subject.Register(r => r.Literal("baz"));

			var result = _subject.GetCompletionSuggestions(_subject.Parse(InputWithOffset("Zb", 1), _source));

			result.Range.Should().BeEquivalentTo(StringRange.Between(1, 2));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.Between(1, 2), "bar"), new Suggestion.Suggestion(StringRange.Between(1, 2), "baz")});
		}

		[Fact]
		public void getCompletionSuggestions_SubCommands(){
			_subject.Register(r =>
				r.Literal("parent")
					.Then(r.Literal("foo"))
					.Then(r.Literal("bar"))
					.Then(r.Literal("baz"))
			);

			var result = _subject.GetCompletionSuggestions(_subject.Parse("parent ", _source));

			result.Range.Should().BeEquivalentTo(StringRange.At(7));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.At(7), "bar"), new Suggestion.Suggestion(StringRange.At(7), "baz"), new Suggestion.Suggestion(StringRange.At(7), "foo")});
		}

		[Fact]
		public void getCompletionSuggestions_movingCursor_SubCommands(){
			_subject.Register(r =>
				r.Literal("parent_one")
					.Then(r.Literal("faz"))
					.Then(r.Literal("fbz"))
					.Then(r.Literal("gaz"))
			);

			_subject.Register(r =>
				r.Literal("parent_two")
			);

			TestSuggestions("parent_one faz ", 0, StringRange.At(0), "parent_one", "parent_two");
			TestSuggestions("parent_one faz ", 1, StringRange.Between(0, 1), "parent_one", "parent_two");
			TestSuggestions("parent_one faz ", 7, StringRange.Between(0, 7), "parent_one", "parent_two");
			TestSuggestions("parent_one faz ", 8, StringRange.Between(0, 8), "parent_one");
			TestSuggestions("parent_one faz ", 10, StringRange.At(0));
			TestSuggestions("parent_one faz ", 11, StringRange.At(11), "faz", "fbz", "gaz");
			TestSuggestions("parent_one faz ", 12, StringRange.Between(11, 12), "faz", "fbz");
			TestSuggestions("parent_one faz ", 13, StringRange.Between(11, 13), "faz");
			TestSuggestions("parent_one faz ", 14, StringRange.At(0));
			TestSuggestions("parent_one faz ", 15, StringRange.At(0));
		}

		[Fact]
		public void getCompletionSuggestions_SubCommands_partial(){
			_subject.Register(r =>
				r.Literal("parent")
					.Then(r.Literal("foo"))
					.Then(r.Literal("bar"))
					.Then(r.Literal("baz"))
			);

			var parse = _subject.Parse("parent b", _source);
			var result = _subject.GetCompletionSuggestions(parse);

			result.Range.Should().BeEquivalentTo(StringRange.Between(7, 8));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.Between(7, 8), "bar"), new Suggestion.Suggestion(StringRange.Between(7, 8), "baz")});
		}

		[Fact]
		public void getCompletionSuggestions_SubCommands_partial_withInputOffset(){
			_subject.Register(r =>
				r.Literal("parent")
					.Then(r.Literal("foo"))
					.Then(r.Literal("bar"))
					.Then(r.Literal("baz"))
			);

			var parse = _subject.Parse(InputWithOffset("junk parent b", 5), _source);
			var result = _subject.GetCompletionSuggestions(parse);

			result.Range.Should().BeEquivalentTo(StringRange.Between(12, 13));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.Between(12, 13), "bar"), new Suggestion.Suggestion(StringRange.Between(12, 13), "baz")});
		}

		[Fact]
		public void getCompletionSuggestions_redirect(){
			var actual = _subject.Register(r => r.Literal("actual").Then(r.Literal("sub")));
			_subject.Register(r => r.Literal("redirect").Redirect(actual));

			var parse = _subject.Parse("redirect ", _source);
			var result = _subject.GetCompletionSuggestions(parse);

			result.Range.Should().BeEquivalentTo(StringRange.At(9));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.At(9), "sub")});
		}

		[Fact]
		public void getCompletionSuggestions_redirectPartial(){
			var actual = _subject.Register(r => r.Literal("actual").Then(r.Literal("sub")));
			_subject.Register(r => r.Literal("redirect").Redirect(actual));

			var parse = _subject.Parse("redirect s", _source);
			var result = _subject.GetCompletionSuggestions(parse);

			result.Range.Should().BeEquivalentTo(StringRange.Between(9, 10));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.Between(9, 10), "sub")});
		}

		[Fact]
		public void getCompletionSuggestions_movingCursor_redirect(){
			var actualOne = _subject.Register(r => r.Literal("actual_one")
				.Then(r.Literal("faz"))
				.Then(r.Literal("fbz"))
				.Then(r.Literal("gaz"))
			);

			_subject.Register(r => r.Literal("actual_two"));

			_subject.Register(r => r.Literal("redirect_one").Redirect(actualOne));
			_subject.Register(r => r.Literal("redirect_two").Redirect(actualOne));

			TestSuggestions("redirect_one faz ", 0, StringRange.At(0), "actual_one", "actual_two", "redirect_one", "redirect_two");
			TestSuggestions("redirect_one faz ", 9, StringRange.Between(0, 9), "redirect_one", "redirect_two");
			TestSuggestions("redirect_one faz ", 10, StringRange.Between(0, 10), "redirect_one");
			TestSuggestions("redirect_one faz ", 12, StringRange.At(0));
			TestSuggestions("redirect_one faz ", 13, StringRange.At(13), "faz", "fbz", "gaz");
			TestSuggestions("redirect_one faz ", 14, StringRange.Between(13, 14), "faz", "fbz");
			TestSuggestions("redirect_one faz ", 15, StringRange.Between(13, 15), "faz");
			TestSuggestions("redirect_one faz ", 16, StringRange.At(0));
			TestSuggestions("redirect_one faz ", 17, StringRange.At(0));
		}

		[Fact]
		public void getCompletionSuggestions_redirectPartial_withInputOffset(){
			var actual = _subject.Register(r => r.Literal("actual").Then(r.Literal("sub")));
			_subject.Register(r => r.Literal("redirect").Redirect(actual));

			var parse = _subject.Parse(InputWithOffset("/redirect s", 1), _source);
			var result = _subject.GetCompletionSuggestions(parse);

			result.Range.Should().BeEquivalentTo(StringRange.Between(10, 11));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> {new Suggestion.Suggestion(StringRange.Between(10, 11), "sub")});
		}

		[Fact]
		public void getCompletionSuggestions_redirect_lots()
		{
			var loop = _subject.Register(r => r.Literal("redirect"));
			_subject.Register(r =>
				r.Literal("redirect")
					.Then(
						r.Literal("loop")
							.Then(
								r.Argument("loop", Arguments.Integer())
									.Redirect(loop)
							)
					)
			);

			var result = _subject.GetCompletionSuggestions(_subject.Parse("redirect loop 1 loop 02 loop 003 ", _source));

			result.Range.Should().BeEquivalentTo(StringRange.At(33));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion> { new Suggestion.Suggestion(StringRange.At(33), "loop") });
		}

		[Fact]
		public void getCompletionSuggestions_execute_simulation()
		{
			var execute = _subject.Register(r => r.Literal("execute"));
			_subject.Register(r =>
				r.Literal("execute")
					.Then(
						r.Literal("as")
							.Then(
								r.Argument("name", Arguments.Word())
									.Redirect(execute)
							)
					)
					.Then(
						r.Literal("store")
							.Then(
								r.Argument("name", Arguments.Word())
									.Redirect(execute)
							)
					)
					.Then(
						r.Literal("run")
							.Executes(e => 0)
				)
				);

			var parse = _subject.Parse("execute as Dinnerbone as", _source);
			var result = _subject.GetCompletionSuggestions(parse);

			result.IsEmpty().Should().Be(true);
		}

		[Fact]
		public void getCompletionSuggestions_execute_simulation_partial()
		{
			var execute = _subject.Register(r => r.Literal("execute"));
			_subject.Register(r =>
				r.Literal("execute")
					.Then(
						r.Literal("as")
							.Then(r.Literal("bar").Redirect(execute))
							.Then(r.Literal("baz").Redirect(execute))
					)
					.Then(
						r.Literal("store")
							.Then(
								r.Argument("name", Arguments.Word())
									.Redirect(execute)
							)
					)
					.Then(
						r.Literal("run").Executes(e => 0)
				)
				);

			var parse = _subject.Parse("execute as bar as ", _source);
			var result = _subject.GetCompletionSuggestions(parse);

			result.Range.Should().BeEquivalentTo(StringRange.At(18));
			result.List.Should().BeEquivalentTo(new List<Suggestion.Suggestion>
			{
				new Suggestion.Suggestion(StringRange.At(18), "bar"),
				new Suggestion.Suggestion(StringRange.At(18), "baz")
			});
		}
	}
}