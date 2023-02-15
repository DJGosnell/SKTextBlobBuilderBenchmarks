using BenchmarkDotNet.Running;

namespace SKTextBlobBuilderBenchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
            JetBrains.Profiler.Api.MeasureProfiler.StartCollectingData();
            //new Benchmarks().Baseline();
            new Benchmarks().PR();
            JetBrains.Profiler.Api.MeasureProfiler.StopCollectingData();*/

            // If arguments are available use BenchmarkSwitcher to run benchmarks
            if (args.Length > 0)
            {
                var summaries = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly)
                    .Run(args, BenchmarkConfig.Get());
                return;
            }
            // Else, use BenchmarkRunner
            var summary = BenchmarkRunner.Run<Benchmarks>(BenchmarkConfig.Get());
        }
    }
}