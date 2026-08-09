// -------------------------------------------------------------------
// Rung 04: Delegates, lambdas, closures.
//
// A delegate = a reference-type value holding a method reference
// (really: a TARGET object + a method pointer). A lambda is just an
// inline, unnamed delegate. A CLOSURE captures the VARIABLE, not its
// value at capture time. That last point is the trap below.
// -------------------------------------------------------------------

Console.WriteLine("A) for-loop capture (the trap):");
var trap = new List<Action>();
for (int i = 0; i < 3; i++)
    trap.Add(() => Console.Write(i + " "));   // all three capture the SAME i
foreach (var a in trap) a();
Console.WriteLine("   <- prints 3 3 3: one shared 'i', left at 3 after the loop\n");

Console.WriteLine("B) for-loop with a fresh temp (the fix):");
var fixedList = new List<Action>();
for (int i = 0; i < 3; i++)
{
    int copy = i;                             // NEW variable each iteration
    fixedList.Add(() => Console.Write(copy + " "));
}
foreach (var a in fixedList) a();
Console.WriteLine("   <- prints 0 1 2: each lambda captured its own 'copy'\n");

Console.WriteLine("C) foreach captures per-iteration since C# 5:");
var fe = new List<Action>();
foreach (var n in new[] { 0, 1, 2 })
    fe.Add(() => Console.Write(n + " "));
foreach (var a in fe) a();
Console.WriteLine("   <- prints 0 1 2 with no temp: foreach makes a fresh var for you");
