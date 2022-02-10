using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace ProcessMemory.Interfaces
{
    public interface ITracer
    {
        public void TraceInformation(string message, [CallerMemberName] string memberName = "");

        public void TraceInformation(string message, [CallerMemberName] string memberName = "", params object?[]? args);

        public void TraceWarning(string message, [CallerMemberName] string memberName = "");

        public void TraceWarning(string message, [CallerMemberName] string memberName = "", params object?[]? args);

        public void TraceError(string message, [CallerMemberName] string memberName = "");

        public void TraceError(string message, [CallerMemberName] string memberName = "", params object?[]? args);

        public void TraceError(Exception exc, string message, [CallerMemberName] string memberName = "");

        public void TraceError(Exception exc, string message, [CallerMemberName] string memberName = "", params object?[]? args);

        public void AddListener(TraceListener traceListener);

        public IEnumerable<TraceListener> GetListeners();
    }
}
