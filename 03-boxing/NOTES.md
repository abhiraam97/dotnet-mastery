# 03 - Boxing

## What it is

**Boxing**: the runtime wraps a value type in a newly allocated **heap object**
so that a reference-typed slot can hold it. The reference-typed slot then holds
an arrow to that box.

**Unboxing**: copying the value back out of the box (`int n = (int)o;`).

```
int i = 42;         // value, sitting inline
object o = i;       // BOX: allocate heap object, copy 42 in, o -> box
int back = (int)o;  // UNBOX: copy 42 out of the box
```

## The key correction I needed

`int` is **not a class**. `int` is `System.Int32`, a **struct** (value type).
What's true is that *every* type in .NET, value types included, derives from
`System.Object`. That shared root is what makes `object o = 42;` legal. But
honoring it at runtime requires a box, because `object` must hold a heap
reference and a bare value type has none.

## Why it matters

Every box is a **heap allocation** -> GC pressure. In a hot loop this is a real,
measurable performance cost. Boxing is a classic *invisible* cost: the code
looks innocent.

## Where boxing hides (spot-the-box)

- `object o = i;`                      -> boxes
- `List<int>.Add(i)`                   -> NO box (generic, stores int inline)
- `ArrayList.Add(i)`                   -> boxes (Add takes `object`)
- `int` into a `string.Format`/interpolation as `object` arg -> boxes
- calling a struct method via an interface reference -> boxes

## The lesson that connects to generics

`List<int>` keeps ints as ints; `ArrayList` (Add(object)) boxes every one. This
contrast is *why generics exist*: type safety AND no boxing. See rung 05.

## In my own words

_(Ram: from memory, what exactly does the runtime do on `object o = 42;`, and
why does `ArrayList.Add(5)` box but `List<int>.Add(5)` not?)_

because the ArrayList.Add(5) does not have a specific type and hence it just puts everything in a heap which does boxing with a pointer where as the latter just stacks in a collection.

### Review corrections

- Right that `ArrayList.Add(object)` boxes. But "the latter just stacks in a
  collection" is wrong: `List<int>`'s backing array is on the **heap** too. The
  ints sit **inline** in that array with **no box**. The real contrast is
  boxed-on-heap vs inline-in-a-heap-array, not heap vs stack.
