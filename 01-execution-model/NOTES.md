# 01 - The execution model

## The chain, precisely

1. **C# source** is compiled by **Roslyn** (the C# compiler).
2. Output is an **assembly** (a `.dll` or `.exe`): **IL** (Intermediate
   Language) plus **metadata** describing every type and member.
3. At runtime the **CLR** (Common Language Runtime) hosts execution.
4. The **JIT** (Just-In-Time compiler) turns IL into **native machine code**
   one **method** at a time, on that method's **first call**, then **caches**
   it. Later calls (including a million loop iterations) run the cached native
   code. A method never called is never JITed.
5. The CPU runs the native code.

## Why it matters: warm-up (cold start)

The first request to a freshly started service is slower because the methods it
touches are being JIT-compiled right then. By the thousandth request everything
it uses is compiled and cached, so it runs at full native speed. That first-hit
tax is **warm-up**. This is the difference between "I memorized 'JIT compiles
IL'" and "I understand what that costs."

Mitigations (for later): **ReadyToRun** and **Native AOT** pre-compile ahead of
time to cut warm-up.

## The nuance I should not fake in an interview

The clean "compiled once, never again" story is the right first model, but the
real runtime uses **tiered compilation**: the first JIT pass is quick and
lightly optimized (fast to produce), and if a method turns out to be **hot**
(called a lot), the runtime **re-JITs** it in the background at a higher
optimization tier. So "compiled once and cached" is true enough for reasoning
about warm-up, but the honest full answer is "compiled on first use, and hot
methods get re-optimized later."

## In my own words

_(Ram: rewrite the chain above from memory, without looking. If any arrow is
fuzzy, that's the bit to reread.)_

C# code -> Roslyn Analyzer -> (JIT) in CLR Host -> CPU code

### Review corrections

- It's the Roslyn **compiler**, not an "analyzer" (analyzers are a separate
  diagnostics feature - easy to mix up under pressure).
- Missing box: Roslyn emits **IL/assembly**, and the JIT compiles *that*. Full
  chain: C# -> Roslyn -> **IL/assembly** -> JIT (in the CLR) -> native -> CPU.
