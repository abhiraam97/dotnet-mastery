# 07 - GC and IDisposable

## How the GC decides what's garbage: REACHABILITY

An object is **live** if there is a chain of references from a **root** to it.
Roots = local variables of running methods (stack), static fields, CPU
registers. If **no root can reach** an object, it's garbage.

- "Reachable," NOT "recently used." An object you'll never touch again but is
  still held by a static field is not collected.
- .NET does **not** use reference counting. So a **cycle** (A -> B -> A) with no
  root reaching it is collected fine. Reference counting (Python, old COM) would
  leak that cycle; tracing reachability does not. (Classic interview question:
  "do circular refs leak in .NET?" -> No.)

## Generations: exploiting "most objects die young"

Empirically most allocations are short-lived temporaries; a few live long. So:

- **Gen 0**: new objects. Small, collected **often** and **fast**. Most objects
  die here.
- **Gen 1**: survived one gen-0 collection (buffer).
- **Gen 2**: long-lived. Collected **rarely** (expensive).
- Survive a collection -> **promoted** up a generation.
- A gen-0 collection scans **gen 0**, not the whole heap -> reclaims most garbage
  cheaply.
- Objects >= ~85,000 bytes go on the **Large Object Heap** (collected with gen 2).

**Trigger:** not a timer, not constant. Mainly when **gen 0 fills** (allocation
pressure). (`GC.Collect()` exists; don't call it in normal code.)

Note: DI lifetimes (transient/scoped/singleton) are a **DI container** concept,
separate from GC generations. Don't fuse them.

## IDisposable: deterministic cleanup for NON-memory resources

The GC handles memory well, but releases things **whenever** it runs. For a file
handle, socket, or DB connection (scarce, exclusively held) that's too late,
you'd hold locks and starve the pool.

`IDisposable.Dispose()` releases the resource **when you say so**. The `using`
statement guarantees it:

```csharp
using (var conn = new SqlConnection(cs)) { ... }  // Dispose() at block exit, even on throw
using var file = File.OpenRead(path);             // Dispose() at end of scope
```

Wrap `DbContext`, `SqlConnection`, `FileStream`, etc.

## Finalizer vs Dispose (the "destructor" trap)

`~MyClass()` is a **finalizer**, not a destructor and not deterministic. It runs
at GC time (same timing problem), and a finalizable object **survives an extra
collection** (finalization queue), making GC work harder.

- **Dispose()** = the real, deterministic cleanup tool.
- **Finalizer** = last-ditch backstop only for directly-held unmanaged handles.
- Full pattern: call `GC.SuppressFinalize(this)` in `Dispose()` so the finalizer
  is skipped once you've cleaned up.

## In my own words

_(Ram: from memory - why don't circular references leak in .NET, and why isn't
the GC enough for a database connection?)_

Because the objects are collected based on their root object tracing rather than individual counts.

### Review corrections

- First half correct (reachability tracing, not reference counts, so cycles
  collect). Missing second half: the GC isn't enough for a DB connection because
  GC timing is **non-deterministic** - the connection would stay open and locked
  until some random future collection, starving the pool. `Dispose()` (via
  `using`) releases it **the instant** you're done.
