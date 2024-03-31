# diagnostic-context-benchmarks

## net8: baseline

BenchmarkDotNet=v0.13.1, OS=macOS 14.3.1 (23D60) [Darwin 23.3.0]
Apple M1 Pro, 1 CPU, 10 logical and 10 physical cores
.NET SDK=8.0.201
  [Host]     : .NET 8.0.2 (8.0.224.6711), Arm64 RyuJIT
  DefaultJob : .NET 8.0.2 (8.0.224.6711), Arm64 RyuJIT


|          Method |      Mean |     Error |    StdDev |   Gen 0 |  Gen 1 | Allocated |
|---------------- |----------:|----------:|----------:|--------:|-------:|----------:|
|    WithoutSteps |  3.514 us | 0.0180 us | 0.0168 us |  1.9035 | 0.0305 |     12 KB |
| ConsequentSteps | 11.629 us | 0.1400 us | 0.1310 us |  5.9509 | 0.1373 |     37 KB |
|     NestedSteps | 13.720 us | 0.2059 us | 0.2022 us |  6.5155 | 0.2441 |     40 KB |
|  ComplexContext | 87.755 us | 0.9788 us | 0.8174 us | 25.3906 | 1.7090 |    156 KB |

```
// * Hints *
Outliers
  DiagnosticContextBenchmarks.NestedSteps: Default    -> 3 outliers were removed (14.38 us..14.81 us)
  DiagnosticContextBenchmarks.ComplexContext: Default -> 2 outliers were removed (91.97 us, 93.45 us)

// * Legends *
  Mean      : Arithmetic mean of all measurements
  Error     : Half of 99.9% confidence interval
  StdDev    : Standard deviation of all measurements
  Gen 0     : GC Generation 0 collects per 1000 operations
  Gen 1     : GC Generation 1 collects per 1000 operations
  Allocated : Allocated memory per single operation (managed only, inclusive, 1KB = 1024B)
  1 us      : 1 Microsecond (0.000001 sec)
```

## net7: baseline

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
