﻿//using Orangebeard.Shared.Extensibility;
//using Orangebeard.Shared.Extensibility.Commands.CommandArgs;
using Orangebeard.Client;
using Orangebeard.Client.Entities;
using Orangebeard.SpecFlowPlugin.LogHandler;
using System;

namespace Orangebeard.SpecFlowPlugin.ClientExecution.Logging
{
    public class LogScope : BaseLogScope
    {
        public LogScope(ILogContext logContext, ILogScope root, ILogScope parent, string name) 
            : base(logContext)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Log scope name cannot be null of empty.", nameof(name));
            }

            Root = root;
            Parent = parent;
            Name = name;
        }

        public override ILogScope Parent { get; }

        public override string Name { get; }

        //TODO!- For debugging.
        public void PrintTree(int indentation)
        {
            
            Console.WriteLine($"Name=${Name}".PadLeft(indentation));
            if (Parent != null && Parent is LogScope)
            {
                (Parent as LogScope).PrintTree(indentation + 2);
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            ContextAwareLogHandler.CommandsSource_OnEndLogScopeCommand(Context, new ClientExtensibility.Commands.CommandArgs.LogScopeCommandArgs(this));
            Context.Log = Parent;
        }
    }
}