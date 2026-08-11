# 06 - Async/await internals

## The one misconception to kill

`async` does **not** spin up a thread. Marking a method `async` adds zero
threads. The body runs on the **calling thread**, synchronously, up to the first
`await` on something not already complete.

- Threads are for **CPU work** (you need one to compute -> `Task.Run`).
- Async is for **I/O waits** (no thread needed while waiting).

While an HTTP/DB call is in flight, **no thread waits on it** (the OS handles it
via completion ports). That's why a server can hold 10,000 in-flight requests on
a handful of threads. The old blocking model held one thread per waiting request
and fell over.

## What `await` compiles into: a state machine

The compiler rewrites the async method into a struct implementing
`IAsyncStateMachine`:

- your **local variables become fields** on the struct,
- your **position becomes an `int state`**,
- the body is chopped at each `await` into cases of a `switch` inside
  `MoveNext()`.

Flow:

1. First `MoveNext()` runs code up to the await, calls `GetAwaiter()`.
2. If the awaiter **isn't complete**: save `state`, register `MoveNext` as the
   continuation (`AwaitOnCompleted`), and **return** a `Task` to the caller. No
   thread parked.
3. When the awaited task completes, the continuation calls `MoveNext()` again;
   the `switch` jumps to the saved state, gets the result, runs the rest, and
   completes the Task.

Key insight: **async turns a call stack into a heap object.** "Where was I" is
data (an int + fields), not a paused thread.

## Where does the continuation resume? SynchronizationContext

At the `await`, the current `SynchronizationContext` is captured. The
continuation is posted back to it.

- **WPF / WinForms / Blazor Server / classic ASP.NET:** context = the UI thread.
  Code after `await` resumes on the UI thread (why you can touch controls after
  an await).
- **Console / ASP.NET Core:** no context -> continuation runs on a **thread-pool
  thread** (often a different thread id than before the await).

## ConfigureAwait(false)

"Don't resume on the captured context; use the pool." Library code uses it for
speed and to avoid deadlocks, because it doesn't care about your UI thread.

## The classic async deadlock

On a UI/classic-ASP.NET thread, calling `task.Result` or `.Wait()` blocks that
single thread. The continuation needs to post back to that same thread to
finish, but it's blocked. Deadlock. Fix: `await` instead of blocking, or
`ConfigureAwait(false)`. "Async all the way down."

## In my own words

_(Ram: from memory - what two things does the compiler turn your locals and your
position into, and what decides which thread runs the code after an await?)_
