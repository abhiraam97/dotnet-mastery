# 04 - Delegates, lambdas, closures, events

## What a delegate is

A **delegate** is a reference type whose instance holds a **method reference**.
Precisely, it holds two things:

1. a **method pointer**, and
2. a **target object** (the instance the method runs on; `null` for `static`).

`Func<int,int> f = Square;` stores a reference to `Square` in `f`. Calling
`f(5)` follows that reference and runs the method. `Action`, `Func<>`, and
`Predicate<>` are just built-in delegate types.

That target reference matters: a delegate keeps its target object **alive**. This
is the classic **event memory leak** (see below).

## Lambdas

A lambda (`x => x * x`) is an **inline, unnamed delegate**. The compiler emits a
real method for it behind the scenes. Nothing magic.

## Closures: capture the VARIABLE, not the value

When a lambda uses a variable from its enclosing scope, it **captures the
variable itself**, not a snapshot of its value. The compiler hoists that
variable into a generated closure class; every lambda sharing it points at one
instance.

### The trap

```csharp
for (int i = 0; i < 3; i++)
    actions.Add(() => Console.WriteLine(i));   // -> 3 3 3
```

`i` is ONE variable for the whole `for` loop. All three lambdas share it. After
the loop `i == 3`, so all three print 3.

### The fix

```csharp
for (int i = 0; i < 3; i++) {
    int copy = i;                              // fresh variable each iteration
    actions.Add(() => Console.WriteLine(copy));// -> 0 1 2
}
```

### for vs foreach (interview gold)

- `for` loop variable: one shared variable -> `3 3 3`.
- `foreach` variable (since C# 5): a fresh variable per iteration -> `0 1 2`.

Same-looking code, opposite result. Know why.

## Multicast delegates -> events

Delegates are **multicast**: `d += handler` adds to an invocation list,
`d -= handler` removes. Invoking calls each in order (for non-void, you only get
the LAST return value). An **event** is a delegate field with restricted access:
outside code can only `+=` / `-=`, not invoke or overwrite it. That is the whole
difference between an event and a plain delegate field.

## The event memory leak

Because a delegate holds its target, subscribing `longLived.SomeEvent += obj.Handler`
keeps `obj` alive as long as `longLived` lives. Forget to unsubscribe and `obj`
never gets collected. Fix: `-=` when done (or weak event patterns).

## In my own words

_(Ram: from memory - why does the `for` loop print 3 3 3, and what is the one
difference between an event and a public delegate field?)_

Because the for loop uses the same shared variable not it's value as a snapshot.

### Review corrections

- First half correct and crisp. Missing second half: an **event** exposes only
  `+=` / `-=` to outside code - it can't be invoked or overwritten from outside.
  A public delegate field could be fired or nulled by anyone. That restriction
  is the whole difference.