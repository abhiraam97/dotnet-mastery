// -------------------------------------------------------------------
// Rung 06: Async/await internals.
//
// async does NOT create a thread. The compiler rewrites the method
// into a STATE MACHINE: locals become fields, your position becomes an
// int 'state'. At an await on an incomplete task it SAVES state and
// RETURNS a Task to the caller (no thread parked). When the task
// completes, a continuation calls MoveNext() again and the switch
// jumps back to where it left off.
//
// WHERE the continuation runs is decided by the SynchronizationContext
// captured at the await: UI apps resume on the UI thread; console /
// ASP.NET Core have no context, so it resumes on a thread-pool thread.
// -------------------------------------------------------------------

Console.WriteLine($"caller thread: {Environment.CurrentManagedThreadId}");

Task<int> t = WorkAsync();   // runs synchronously until the first real await
Console.WriteLine("WorkAsync handed back a Task; caller keeps going meanwhile");

int r = await t;
Console.WriteLine($"result = {r}");

async Task<int> WorkAsync()
{
    // Runs on the CALLER's thread up to the first incomplete await.
    Console.WriteLine($"  WorkAsync start:  thread {Environment.CurrentManagedThreadId} (same as caller)");

    await Task.Delay(200);   // no thread is parked during this wait

    // In a console app there is no SynchronizationContext, so this
    // continuation resumes on a THREAD-POOL thread (often a different id).
    Console.WriteLine($"  WorkAsync resume: thread {Environment.CurrentManagedThreadId} (may differ)");
    return 42;
}
