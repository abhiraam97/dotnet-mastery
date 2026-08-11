// -------------------------------------------------------------------
// Rung 07: GC and IDisposable.
//
// The GC reclaims MEMORY by REACHABILITY: an object is live if a chain
// of references from a ROOT (stack locals, statics, registers) reaches
// it. Not reachable = garbage. It does NOT reference-count, so cycles
// (A<->B with no root) collect fine.
//
// Generations exploit "most objects die young": gen 0 (new) is small,
// collected often + fast; survivors promote to gen 1 then gen 2 (old,
// collected rarely). A gen-0 collection scans gen 0, not the whole heap.
//
// The GC does NOT promptly release NON-memory resources (file handles,
// DB connections). Those need DETERMINISTIC cleanup via IDisposable +
// `using`, which calls Dispose() the instant the scope exits.
// -------------------------------------------------------------------

Console.WriteLine("enter using-block:");
using (var conn = new Resource("db-connection"))
{
    Console.WriteLine("  ...working...");
}   // conn.Dispose() runs HERE, guaranteed (even on exception)
Console.WriteLine("left block: resource already released\n");

Console.WriteLine("using-declaration form:");
using var file = new Resource("file-handle");
Console.WriteLine("  ...working with file...");
// file.Dispose() runs when this top-level scope ends (see last line of output)

sealed class Resource : IDisposable
{
    private readonly string _name;
    public Resource(string name)
    {
        _name = name;
        Console.WriteLine($"  [open]    {_name}");
    }
    public void Dispose() => Console.WriteLine($"  [dispose] {_name}");
}
