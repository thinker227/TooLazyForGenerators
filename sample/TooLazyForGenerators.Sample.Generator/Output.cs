﻿namespace TooLazyForGenerators.Sample.Generator;

public sealed class Output : ISourceOutput
{
    public Task GetSource(ISourceOutputContext ctx)
    {
        ctx.AddSource("""
        /// <auto-generated />

        using System;

        public static class Generated
        {
            public static void Hello() =>
                Console.WriteLine("Hello, World!"); 
        }
        """, "Generated.g.cs");

        return Task.CompletedTask;
    }
}
