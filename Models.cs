using System.Collections.Generic;

namespace HashCodeProblem
{
    public class Input
    {
        public int BooksCount { get; set; }

        public int LibrariesCount { get; set; }

        public int DayForScanning { get; set; }

        public List<Library> Libraries { get; set; }

        public List<Book> Books { get; set; }
    }

    public class Library
    {
        public int Id { get; set; }

        public int BooksCount { get; set; }

        public int SignupProcess { get; set; }

        public int ShipPerDay { get; set; }

        public List<Book> Books { get; set; }

        public int TotalScore { get; set; }

        public bool IsSignedUp { get; set; }
    }

    public class Book
    {
        public int Id { get; set; }

        public int Score { get; set; }
    }

    public class LibraryScanPlan
    {
        public Library Library { get; set; }

        public int TotalScoreProduced { get; set; }

        public int FirstScanDay { get; set; }

        public List<Book> BooksToScan { get; set; }

        public IEnumerable<Book> BooksToScanOld { get; set; }

        public override string ToString()
        {
            return $"(id-{Library.Id} bc-{Library.Books.Count})";
        }
    }

    public class Output
    {
        public List<LibraryOutput> Libraries { get; set; }
    }

    public class LibraryOutput
    {
        public int Id { get; set; }

        public List<int> BooksIds { get; set; }
    }
}
