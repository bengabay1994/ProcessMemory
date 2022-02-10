using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using ProcessMemory.Interfaces;

namespace ProcessMemory
{
    public class Tracer : ITracer
    {
        private static ITracer tracerInstance;

        private Tracer() { }

        public static ITracer GetTracer() 
        {
            if (tracerInstance == null) 
            {
                tracerInstance = new Tracer();
            }
            return tracerInstance;
        }

        public void AddListener(TraceListener traceListener)
        {
            Trace.Listeners.Add(traceListener);
        }

        public IEnumerable<TraceListener> GetListeners()
        {
            return (IEnumerable<TraceListener>)Trace.Listeners;
            
        }

        public void TraceError(Exception exc, string message, [CallerMemberName] string memberName = "")
        {
            Trace.TraceError($"function: {memberName}\n{message}\nException: {exc.Message}");
        }

        public void TraceError(Exception exc, string message, [CallerMemberName] string memberName = "", params object?[]? args)
        {
            Trace.TraceError($"function: {memberName}\n{message}\nException: {exc.Message}", args);
        }

        public void TraceWarning(string message, [CallerMemberName] string memberName = "")
        {
            Trace.TraceWarning($"function: {memberName}\n{message}");
        }

        public void TraceWarning(string message, [CallerMemberName] string memberName = "", params object?[]? args)
        {
            Trace.TraceWarning($"function: {memberName}\n{message}", args);
        }

        public void TraceInformation(string message, [CallerMemberName] string memberName = "")
        {
            Trace.TraceInformation($"function: {memberName}\n{message}");
        }

        public void TraceInformation(string message, [CallerMemberName] string memberName = "", params object?[]? args)
        {
            Trace.TraceInformation($"function: {memberName}\n{message}", args);
        }

        public void TraceError(string message, [CallerMemberName] string memberName = "")
        {
            Trace.TraceError($"function: {memberName}\n{message}");
        }

        public void TraceError(string message, [CallerMemberName] string memberName = "", params object[] args)
        {
            Trace.TraceError($"function: {memberName}\n{message}", args);
        }
    }
}
