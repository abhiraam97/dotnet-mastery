# dotnet-mastery

Working through .NET from the CLR up, one concept per folder. Each folder is a
small runnable console app plus a `NOTES.md` in my own words. Committed as I go.

## The ladder

| # | Folder | Focus |
|---|--------|-------|
| 01 | execution-model | IL, assemblies, CLR, JIT, warm-up |
| 02 | value-vs-reference | stack/heap, the memory model |
| 03 | boxing | falls out of 02 |
| 04 | delegates-events | lambdas, closures, the traps |
| 05 | generics | constraints, variance |
| 06 | async-internals | the state machine, no magic |
| 07 | gc-and-disposal | GC generations, IDisposable |
| 08+ | ... | fold the fundamentals into a real system |

## Run any folder

```
cd 01-execution-model
dotnet run -c Release
```

Use `-c Release` when timing things: Debug disables optimizations and muddies
the numbers.
