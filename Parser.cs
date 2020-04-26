using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace HashCodeProblem
{
    public static class Parser
    {
        public static Input ParseInput(string inputFilePath)
        {
            var input = new Input();
            using (var reader = new StreamReader(File.OpenRead(inputFilePath)))
            {
                var line = reader.ReadLine().Split();
                input.BooksCount = int.Parse(line[0]);
                input.LibrariesCount = int.Parse(line[1]);
                input.DayForScanning = int.Parse(line[2]);
                input.Libraries = new List<Library>(input.LibrariesCount);

                var books = reader.ReadLine()
                    .Split()
                    .Select((value, index) => new Book() { Id = index, Score = int.Parse(value) })
                    .ToList();

                input.Books = books;

                var allPoints = books.Sum(_ => _.Score);

                Debug.WriteLine($"Max score: {allPoints}");

                for (int i = 0; i < input.LibrariesCount; ++i)
                {
                    line = reader.ReadLine().Split();

                    var library = new Library
                    {
                        Id = i,
                        BooksCount = int.Parse(line[0]),
                        SignupProcess = int.Parse(line[1]),
                        ShipPerDay = int.Parse(line[2]),
                    };

                    var booksInLibrary = reader.ReadLine().Split().Select(int.Parse).ToList();

                    library.Books = booksInLibrary
                        .Select(val => books[val])
                        .OrderByDescending(_ => _.Score)
                        .ToList();

                    library.TotalScore = library.Books.Sum(_ => _.Score);

                    input.Libraries.Add(library);
                }

            }

            return input;
        }

        public static void WriteOutput(Output output, string filePath)
        {
            using (var writer = File.CreateText(filePath))
            {
                writer.WriteLine($"{output.Libraries.Count}");
                //Console.WriteLine($"{output.Libraries.Count}");
                foreach (var library in output.Libraries)
                {
                    writer.WriteLine($"{library.Id} {library.BooksIds.Count}");
                    writer.WriteLine(string.Join(' ', library.BooksIds));

                    //Console.WriteLine($"{library.Id} {library.BooksIds.Count}");
                    //Console.WriteLine(string.Join(' ', library.BooksIds));
                }
            }
        }
    }
}
