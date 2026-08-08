using System.Collections;

// -------------------------------------------------------------------
// Rung 03: Boxing.
//
// Boxing = wrapping a value type in a heap object so a reference-typed
// slot (object, or a non-generic collection) can hold it.
// Every box is a HEAP ALLOCATION. This program measures that in bytes.
// -------------------------------------------------------------------

const int N = 1_000_000;

static long Measure(Action work)
{
    long before = GC.GetAllocatedBytesForCurrentThread();
    work();
    return GC.GetAllocatedBytesForCurrentThread() - before;
}

// Generic collection: stores int inline. No boxing.
var list = new List<int>(N);
long genericBytes = Measure(() =>
{
    for (int i = 0; i < N; i++) list.Add(i);
});

// Non-generic collection: Add(object). Every int gets boxed.
var al = new ArrayList(N);
long boxedBytes = Measure(() =>
{
    for (int i = 0; i < N; i++) al.Add(i);   // <-- one heap allocation each
});

Console.WriteLine($"List<int>  added {N:N0} ints -> {genericBytes,14:N0} bytes allocated");
Console.WriteLine($"ArrayList  added {N:N0} ints -> {boxedBytes,14:N0} bytes allocated");
Console.WriteLine();
Console.WriteLine("List<int> allocates ~nothing during Add (ints stored inline).");
Console.WriteLine("ArrayList allocates a boxed int per Add (~24 bytes each on 64-bit).");
Console.WriteLine("That is boxing, and why generics replaced the old collections.");
