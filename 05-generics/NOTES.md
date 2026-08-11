# 05 - Generics

## Why they exist

Write type-safe, reusable code once for many types, with **no boxing** and no
unsafe casts. `List<T>` replaced the old `ArrayList` (which stored `object` and
boxed every value type).

## Under the hood: reification (the CLR-level answer)

.NET generics are **real at runtime** (reified), not erased. When the JIT
instantiates a generic type it splits on the value-vs-reference line:

- **Reference-type arguments share ONE implementation.** `List<string>`,
  `List<Customer>`, `List<object>` all use a single native code body, because
  every reference is pointer-sized and handled identically.
- **Value-type arguments each get their OWN specialized code.** `List<int>` and
  `List<long>` get separate native bodies built to that type's exact size, so
  values are stored **inline** (a real `int[]` backing array) with no boxing.

Example: `List<string>`, `List<Customer>`, `List<object>`, `List<int>`,
`List<long>` -> **3** implementations (1 shared reference body + int + long).

Contrast **Java erasure**: `List<T>` becomes `List<Object>` under the hood, which
is why Java can't avoid boxing primitives in generic collections. Cost of
reification: a little extra JITed code per value-type instantiation.

## Constraints

Inside a generic body, the compiler only lets you use what it can PROVE every
`T` supports. With no constraint, `T` is effectively `object` (only
`ToString`/`Equals`/`GetHashCode`). Constraints are promises that unlock more:

```csharp
T Max<T>(T a, T b) where T : IComparable<T>   // now CompareTo is available
    => a.CompareTo(b) > 0 ? a : b;
```

Common: `where T : class`, `: struct`, `: new()`, a base class/interface,
`: notnull`, `: unmanaged`. (.NET 7+: `where T : INumber<T>` for real operators.)

## Variance (in / out) - only on interfaces & delegates, reference types only

The axis is whether the type **outputs** or **takes in** T:

- **`out` (covariant):** only produces T. `IEnumerable<out T>`. Safe derived->base:
  `IEnumerable<string>` fits `IEnumerable<object>` (every string is an object;
  you can only read).
- **`in` (contravariant):** only consumes T. `Action<in T>`, `IComparer<in T>`.
  Safe base->derived: `Action<object>` fits `Action<string>` (handles any object,
  so certainly a string).
- **invariant:** does both. `List<T>`. No substitution.

Why `List<string>` -> `List<object>` is banned:

```csharp
List<object> bad = stringList;   // if this compiled...
bad.Add(42);                     // ...you'd put an int into a List<string>
```

`Add(T)` consumes T, which makes covariance unsafe. That's the whole reason.

Mnemonic: **out comes out (producer), in goes in (consumer).**

## In my own words

_(Ram: from memory - how many native implementations for List<string>,
List<Customer>, List<int>, List<long>? And why is IEnumerable covariant but
List invariant?)_

Because the IEnumerable is compatible with it's base type IEnumerable<object> which is only read only, where as List has a add feature which makes it invariant.

### Review corrections

- Variance reasoning correct. Missing the count: `List<string>`, `List<Customer>`,
  `List<int>`, `List<long>` -> **3** implementations (one shared for the two
  reference types, plus int, plus long).