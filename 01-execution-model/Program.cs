using System.Diagnostics;

// -------------------------------------------------------------------
// Rung 01: The execution model.
//
// C#  --(Roslyn)-->  assembly (IL + metadata)  --(JIT, at runtime)-->
// native machine code  -->  CPU
//
// The JIT compiles each METHOD to native code the FIRST time it is
// called, then CACHES the result. Later calls run the cached native
// code directly. A method that is never called is never JITed.
//
// The one-time first-call cost is what we call WARM-UP.
// This program lets you watch it happen.
// -------------------------------------------------------------------

Console.WriteLine(".NET execution model: watching JIT warm-up\n");

// Some real work so the method is worth compiling.
static long Work(int n)
{
    long acc = 0;
    for (int i = 0; i < n; i++)
        acc += (i ^ (i << 1)) % 97;
    return acc;
}

// The FIRST EVER call to Work(): the JIT compiles it right on this line.
var sw = Stopwatch.StartNew();
long r1 = Work(1_000_000);
sw.Stop();
double firstUs = sw.Elapsed.TotalMicroseconds;

// Every later call: native code already compiled + cached. Take the best.
double bestLaterUs = double.MaxValue;
for (int k = 0; k < 20; k++)
{
    sw.Restart();
    Work(1_000_000);
    sw.Stop();
    bestLaterUs = Math.Min(bestLaterUs, sw.Elapsed.TotalMicroseconds);
}

Console.WriteLine($"First call  (includes JIT): {firstUs,10:F1} us");
Console.WriteLine($"Best later call (native):   {bestLaterUs,10:F1} us");
Console.WriteLine($"Warm-up cost (roughly):     {firstUs - bestLaterUs,10:F1} us\n");

Console.WriteLine("The first call paid a one-time JIT cost. That gap is warm-up.");
Console.WriteLine("Run with `dotnet run -c Release` for the cleanest numbers.");

_ = r1; // keep the result so the loop can't be optimized away
