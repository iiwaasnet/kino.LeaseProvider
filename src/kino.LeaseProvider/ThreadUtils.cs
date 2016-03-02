using System;
using System.Threading;

namespace kino.LeaseProvider
{
    public static class ThreadUtils
    {
        public static void Sleep(this TimeSpan delay)
        {
            using (var @lock = new ManualResetEvent(false))
            {
                @lock.WaitOne(delay);
            }
        }
    }
}