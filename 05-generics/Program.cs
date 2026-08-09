// -------------------------------------------------------------------
// Rung 05: Generics.
//
// Under the hood (reification):
//   - Reference-type args (List<string>, List<Customer>, List<object>)
//     SHARE one native implementation (all references are pointer-sized).
//   - Value-type args (List<int>, List<long>) each get their OWN
//     specialized native code -> ints stored inline, NO boxing.
// This is why .NET generics avoid the boxing Java's erasure can't.
// -------------------------------------------------------------------

// --- Constraints: promise T is comparable, so CompareTo is available ---
static T Max<T>(T a, T b) where T : IComparable<T>
    => a.CompareTo(b) > 0 ? a : b;

Console.WriteLine($"Max(3, 9)            = {Max(3, 9)}");
Console.WriteLine($"Max(\"apple\", \"pear\") = {Max("apple", "pear")}");
Console.WriteLine();

// --- Covariance (out T): IEnumerable only PRODUCES T, so this is safe ---
IEnumerable<string> words = new List<string> { "a", "b", "c" };
IEnumerable<object> asObjects = words;          // legal: string -> object
Console.WriteLine($"covariant read: {string.Join(", ", asObjects)}");

// --- Contravariance (in T): Action only CONSUMES T, so this is safe ---
Action<object> printAny = o => Console.WriteLine($"contravariant: got '{o}'");
Action<string> printString = printAny;          // legal: object -> string
printString("hello");

// --- Invariance: List<T> both produces AND consumes T, so this is banned:
//     List<object> bad = new List<string>();   // compile error, on purpose
//     bad.Add(42);                             // would corrupt a List<string>
Console.WriteLine("\nList<T> is invariant by design (Add(T) makes covariance unsafe).");
