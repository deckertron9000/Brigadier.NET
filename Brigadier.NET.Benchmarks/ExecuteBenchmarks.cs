// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using Brigadier.NET;
using Brigadier.NET.Builder;

[MarkdownExporterAttribute.GitHub, MemoryDiagnoser]
public class ExecuteBenchmarks {
    private CommandDispatcher<object> dispatcher;
    private ParseResults<object> simple;
    private ParseResults<object> singleRedirect;
    private ParseResults<object> forkedRedirect;

    [GlobalSetup]
    public void setup() {
        dispatcher = new CommandDispatcher<object>();
        dispatcher.Register(r => r.Literal("command").Executes(c => 0));
        dispatcher.Register(r => r.Literal("redirect").Redirect(dispatcher.GetRoot()));
        dispatcher.Register(r => r.Literal("fork").Fork(dispatcher.GetRoot(), o => new List<object> {new object(), new object(), new object()}));
        simple = dispatcher.Parse("command", new object());
        singleRedirect = dispatcher.Parse("redirect command", new object());
        forkedRedirect = dispatcher.Parse("fork command", new object());
    }

    [Benchmark]
    public void execute_simple() {
        dispatcher.Execute(simple);
    }

	[Benchmark]
    public void execute_single_redirect() {
        dispatcher.Execute(singleRedirect);
    }

	[Benchmark]
    public void execute_forked_redirect() {
        dispatcher.Execute(forkedRedirect);
    }
}
