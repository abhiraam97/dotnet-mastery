// -------------------------------------------------------------------
// Rung 02: Value vs reference types.
//
// A variable of a VALUE type (struct) holds the DATA itself.
// A variable of a REFERENCE type (class) holds a REFERENCE (arrow)
// to an object on the heap.
//
// Assignment and method-passing copy WHAT THE VARIABLE HOLDS:
//   value type  -> copies the whole value  (independent)
//   reference type -> copies the arrow     (both point at one object)
// -------------------------------------------------------------------

struct PointS { public int X; }
class  PointC { public int X; }

class Program
{
    static void Bump(PointS p) { p.X = 500; }   // mutates a COPY
    static void Bump(PointC p) { p.X = 500; }   // mutates the SHARED object

    static void Main()
    {
        // --- Assignment ---
        var s1 = new PointS { X = 1 };
        var s2 = s1;            // copies the bytes
        s2.X = 99;

        var c1 = new PointC { X = 1 };
        var c2 = c1;            // copies the arrow
        c2.X = 99;

        Console.WriteLine($"s1.X = {s1.X}   (expect 1  - struct copy is independent)");
        Console.WriteLine($"c1.X = {c1.X}   (expect 99 - both arrows point at one object)");

        // --- Method passing ---
        var a = new PointS { X = 1 };
        Bump(a);
        var b = new PointC { X = 1 };
        Bump(b);

        Console.WriteLine($"a.X  = {a.X}    (expect 1   - struct copied into the method)");
        Console.WriteLine($"b.X  = {b.X}  (expect 500 - class reference copied, real object changed)");
    }
}
