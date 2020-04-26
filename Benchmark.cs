using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace HashCodeProblem
{
    [MemoryDiagnoser]
    public class TestWithRandomInput
    {
        public int Days { get; set; } = 50;
        public int TotalBooks { get; set; } = 20;
        public int TotalLibs { get; set; } = 5;

        private List<Book> _books;
        private List<Library> _libs;

        [GlobalSetup]
        public void SetUp()
        {
            var rnd = new Random(101);

            _books = Enumerable.Range(0, TotalBooks)
                .Select(i => new Book() { Id = i, Score = rnd.Next(1, 20) })
                .ToList();
            _libs = Enumerable.Range(0, TotalLibs)
                .Select(i => new Library()
                {
                    Id = i,
                    SignupProcess = rnd.Next(3, 10),
                    ShipPerDay = rnd.Next(1, 4),
                    Books = _books
                }).ToList();
        }

        [Benchmark(Baseline = true)]
        public IList<LibraryScanPlan> Algorythm() => AlgHistory.FindPlan(_books, Days, _libs);

        [Benchmark]
        public IList<LibraryScanPlan> Algorythm2() => AlgHistory.FindPlan2(_books, Days, _libs);

        [Benchmark]
        public IList<LibraryScanPlan> Algorythm3() => AlgHistory.FindPlan3(_books, Days, _libs);

        [Benchmark]
        public IList<LibraryScanPlan> Algorythm4() => AlgHistory.FindPlan4(_books, Days, _libs);
    }

    [MemoryDiagnoser]
    public class TestWithFileInput
    {
        [Params("a_example")]
        public string FilePath { get; set; }

        private int _days;
        private List<Book> _books;
        private List<Library> _libs;

        [GlobalSetup]
        public void SetUp()
        {
            //var a = "a_example";
            //var b = "b_read_on";
            //var c = "c_incunabula";
            //var d = "d_tough_choices";
            //var e = "e_so_many_books";
            //var f = "f_libraries_of_the_world";

            var input = Parser.ParseInput($"inputs/{FilePath}.txt");
            _days = input.DayForScanning;
            _books = input.Books;
            _libs = input.Libraries;
        }

        //[Benchmark]
        //public IList<LibraryScanPlan> Algorythm1() => AlgHistory.FindPlan(_books, _days, _libs);

        //[Benchmark]
        //public IList<LibraryScanPlan> Algorythm2() => AlgHistory.FindPlan2(_books, _days, _libs);

        //[Benchmark]
        //public IList<LibraryScanPlan> Algorythm3() => AlgHistory.FindPlan3(_books, _days, _libs);

        //[Benchmark]
        //public IList<LibraryScanPlan> Algorythm4() => AlgHistory.FindPlan4(_books, _days, _libs);

        [Benchmark]
        public IList<LibraryScanPlan> ReworkedAlghorytm() => ParallelSolution.FindPlan(_days, _libs);

        [Benchmark]
        public IList<LibraryScanPlan> Algorythm5() => AlgHistory.FindPlan5(_books, _days, _libs);
    }

    public class Benchmark
    {
        public static void Run()
        {
            ManualConfig.CreateEmpty() // A configuration for our benchmarks
                .With(Job.Default.WithWarmupCount(0)); // Disable warm-up stage
            _ = BenchmarkRunner.Run<TestWithFileInput>();
        }
    }
}