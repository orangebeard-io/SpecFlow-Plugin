﻿using System;
using System.Diagnostics;
using System.Reflection;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.ErrorHandling;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Tracing;

namespace Orangebeard.SpecFlowPlugin
{
    internal class SafeBindingInvoker : BindingInvoker
    {
        public SafeBindingInvoker(SpecFlowConfiguration reqnrollConfiguration, IErrorProvider errorProvider,
            ISynchronousBindingDelegateInvoker synchronousBindingDelegateInvoker)
            : base(reqnrollConfiguration, errorProvider, synchronousBindingDelegateInvoker)
        {
        }
        
        public override object InvokeBinding(IBinding binding, IContextManager contextManager, object[] arguments,
            ITestTracer testTracer, out TimeSpan duration)
        {
            object result = null;

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                result = base.InvokeBinding(binding, contextManager, arguments,
                    testTracer, out duration);
            }
            catch (Exception ex)
            {
                PreserveStackTrace(ex);

                if (binding is IHookBinding == false)
                {
                    throw;
                }

                var hookBinding = (IHookBinding)binding;

                if (hookBinding.HookType == HookType.BeforeScenario
                    || hookBinding.HookType == HookType.BeforeScenarioBlock
                    || hookBinding.HookType == HookType.BeforeScenario
                    || hookBinding.HookType == HookType.BeforeStep
                    || hookBinding.HookType == HookType.AfterStep
                    || hookBinding.HookType == HookType.AfterScenario
                    || hookBinding.HookType == HookType.AfterScenarioBlock)
                {
                    stopwatch.Stop();

                    duration = stopwatch.Elapsed;

                    testTracer.TraceError(ex, duration);
                    SetTestError(contextManager.ScenarioContext, ex);
                }
            }
            finally
            {
                stopwatch.Stop();

                duration = stopwatch.Elapsed;
            }

            return result;
        }

        private static void SetTestError(ScenarioContext context, Exception ex)
        {
            if (context != null && context.TestError == null)
            {
                context.GetType().GetProperty("ScenarioExecutionStatus")
                    ?.SetValue(context, ScenarioExecutionStatus.TestError);

                context.GetType().GetProperty("TestError")
                    ?.SetValue(context, ex);
            }
        }

        private static void PreserveStackTrace(Exception ex)
        {
            typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.Invoke(ex, Array.Empty<object>());
        }
    }
}