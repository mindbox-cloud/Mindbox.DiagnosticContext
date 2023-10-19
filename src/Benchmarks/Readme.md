# diagnostic-context-benchmarks

BenchmarkDotNet=v0.13.1, OS=macOS 13.6 (22G120) [Darwin 22.6.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK=7.0.302
  [Host]     : .NET 7.0.5 (7.0.523.17405), Arm64 RyuJIT
  DefaultJob : .NET 7.0.5 (7.0.523.17405), Arm64 RyuJIT


|          Method |      Mean |     Error |    StdDev |   Gen 0 |  Gen 1 | Allocated |
|---------------- |----------:|----------:|----------:|--------:|-------:|----------:|
|    WithoutSteps |  4.545 us | 0.0105 us | 0.0098 us |  1.9302 | 0.0305 |     12 KB |
| ConsequentSteps | 13.398 us | 0.0678 us | 0.0634 us |  5.9662 | 0.1373 |     37 KB |
|     NestedSteps | 16.334 us | 0.0374 us | 0.0312 us |  6.5308 | 0.2441 |     40 KB |
|  ComplexContext | 99.471 us | 0.1681 us | 0.1490 us | 25.3906 | 1.7090 |    156 KB |

```
// * Legends *

Mean      : Arithmetic mean of all measurements
Error     : Half of 99.9% confidence interval
StdDev    : Standard deviation of all measurements
Gen 0     : GC Generation 0 collects per 1000 operations
Gen 1     : GC Generation 1 collects per 1000 operations
Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
1 us      : 1 Microsecond (0.000001 sec)
```
