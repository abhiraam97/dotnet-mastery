# 02 - Value vs reference types

## The one rule everything hangs on

A variable holds different things depending on the type:

- **Value type** (`struct`, and all the built-ins: `int`, `bool`, `double`,
  `char`, `DateTime`, enums, tuples): the variable holds the **data itself**.
- **Reference type** (`class`, `string`, arrays, delegates, most things): the
  variable holds a **reference** (an arrow/address) to an object on the heap.
  It does NOT hold the object.

Assignment (`b = a`) and passing to a method both **copy what the variable
holds**:

```
struct:  s1 = {X:1}     s2 = s1  copies the BYTES  ->  s2 = {X:1}  (separate)
         s2.X = 99  ->  s1 still 1

class:   c1 ---> [ X:1 ]    c2 = c1  copies the ARROW  ->  c2 ---> same object
         c2.X = 99  ->  c1 sees 99
```

## Why the method case behaves the same way

C# passes arguments **by value by default**. For a struct that means the whole
struct is copied into the method, so mutations inside don't escape. For a class
the **reference** is copied (still by value!), so both the caller and the method
point at the same object, and mutations through it are visible to the caller.

(There's more here later: `ref` / `in` / `out`, and `ref struct`. Parked.)

## The trap that got me, and the fix

I initially predicted struct assignment would share and class assignment would
copy. It is the **reverse**. The way to never flip it again: ask "what does the
variable literally hold?" Struct holds the data; class holds the arrow. Copy the
arrow and you still share the object.

## Interview myth to kill next: "structs always live on the stack"

Not true. A local struct lives on the stack, but a struct that is a **field of a
class** lives on the **heap**, inside its parent object. Where a value lives
depends on where it is declared, not on the fact that it is a value type. This
is the doorway to boxing (rung 03).

## In my own words

_(Ram: write the rule from memory and one sentence on why `b.X` became 500.)_

because structs are value types and classes are reference types.

### Review corrections

- That's the **label**, not the mechanism. Tighter answer: passing `b` (a class)
  copied the **reference**, so the method mutated the **same object** the caller
  holds -> 500. In an interview, state the mechanism, not the category.
