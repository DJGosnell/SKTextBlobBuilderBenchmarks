using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using SkiaSharp;

namespace SKTextBlobBuilderBenchmarks
{
    //[NativeMemoryProfiler]
    public class Benchmarks
    {
        protected readonly SKFont _font;
        protected readonly SKPaint _paint;
        protected readonly ushort[] _glpyhs;

        //private const int InternalIteration = 500000; for DotTrace
        private const int InternalIteration = 500;

        public Benchmarks()
        {
            _font = new SKFont(SKTypeface.Default);
            _paint = new SKPaint(_font);
            _glpyhs = _paint.GetGlyphs("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.");
        }

        private void MultiThread(ThreadStart action, int threads)
        {
            var threadList = new List<Thread>();
            for (int i = 0; i < threads; i++)
            {
                var thread = new Thread(action);
                threadList.Add(thread);
            }

            // Start all close together
            foreach (var thread in threadList)
                thread.Start();

            // Wait for completion.
            foreach (var thread in threadList)
                thread.Join();
            
        }

        [Benchmark(Baseline = true)]
        public void Baseline()
        {
            MultiThread(() =>
            {
                for (int j = 0; j < InternalIteration; j++)
                {
                    using var builder = new SKTextBlobBuilder();

                    var runBuffer = builder.AllocatePositionedRun(_font, _glpyhs.Length);

                    var glyphSpan = runBuffer.GetGlyphSpan();
                    var positionSpan = runBuffer.GetPositionSpan();

                    var pos = 0;
                    for (int i = 0; i < _glpyhs.Length; i++)
                    {
                        glyphSpan[i] = _glpyhs[i];
                        positionSpan[i] = new SKPoint(pos, 0);
                        pos += 2;
                    }

                    builder.Build().Dispose();
                }
            }, Environment.ProcessorCount);
        }

        [Benchmark]
        public void PR()
        {
            var cache = new ConcurrentBag<SKTextBlobBuilder>();
            
            MultiThread(() =>
            {
                for (int j = 0; j < InternalIteration; j++)
                {
                    if (!cache.TryTake(out var builder))
                    {
                        builder = new SKTextBlobBuilder();
                    }

                    var runBuffer = builder.AllocatePositionedRun(_font, _glpyhs.Length);

                    var glyphSpan = runBuffer.GetGlyphSpan();
                    var positionSpan = runBuffer.GetPositionSpan();

                    var pos = 0;
                    for (int i = 0; i < _glpyhs.Length; i++)
                    {
                        glyphSpan[i] = _glpyhs[i];
                        positionSpan[i] = new SKPoint(pos, 0);
                        pos += 2;
                    }

                    builder.Build().Dispose();

                    cache.Add(builder);
                }
            }, Environment.ProcessorCount);
        }
    }
}
