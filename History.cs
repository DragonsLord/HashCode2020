using System;
using System.Collections.Generic;
using System.Linq;

namespace HashCodeProblem
{
    public class AlgHistory
    {
        public static List<LibraryScanPlan> FindPlan(List<Book> books, int days, List<Library> libraries)
        {
            var currentDay = 0;
            var scanPlans = new List<LibraryScanPlan>();
            var unscanedBooks = books;

            var unSignedLibs = libraries.Where(x => x.SignupProcess + currentDay < days).ToList();

            while (currentDay <= days && unSignedLibs.Any())
            {
                var bestLibPlan = GetScanPlan(unscanedBooks, currentDay, days, unSignedLibs);

                bestLibPlan.Library.IsSignedUp = true;
                scanPlans.Add(bestLibPlan);
                currentDay += bestLibPlan.Library.SignupProcess;

                unscanedBooks = unscanedBooks.Except(bestLibPlan.BooksToScanOld).ToList();

                unSignedLibs = libraries.Where(x => !x.IsSignedUp && x.SignupProcess + currentDay < days).ToList();
            }

            return scanPlans.Where(lp => lp.TotalScoreProduced > 0).ToList();
        }

        public static LibraryScanPlan GetScanPlan(List<Book> unscannedBooks, int day, int allDays, List<Library> unSignedUpLibraries)
        {
            var allPlans = unSignedUpLibraries
                //.OrderByDescending(l => l.TotalScore / (l.SignupProcess + (l.BooksCount / l.ShipPerDay))).Take(10)
                .Select(library =>
                {
                    var scanDays = allDays - library.SignupProcess - day;
                    var BooksToScanOld = library.Books.Intersect(unscannedBooks).Take(scanDays * library.ShipPerDay);

                    return new LibraryScanPlan()
                    {
                        Library = library,
                        BooksToScanOld = BooksToScanOld,
                        TotalScoreProduced = BooksToScanOld.Sum(b => b.Score)
                    };
                });

            LibraryScanPlan bestPlan = new LibraryScanPlan() { TotalScoreProduced = 0 };

            foreach (var plan in allPlans)
            {
                if (plan.TotalScoreProduced >= bestPlan.TotalScoreProduced)
                {
                    bestPlan = plan;
                }
            }

            bestPlan.BooksToScanOld = bestPlan.BooksToScanOld.ToList();

            return bestPlan;
        }

        public static List<LibraryScanPlan> FindPlan2(List<Book> books, int days, List<Library> libraries)
        {
            var currentDay = 0;
            var scanPlans = new List<LibraryScanPlan>();
            var unscanedBooks = books;

            var unSignedLibs = libraries.Where(x => x.SignupProcess + currentDay < days).ToList();

            while (currentDay <= days && unSignedLibs.Any())
            {
                var bestLibPlan = GetScanPlan2(unscanedBooks, currentDay, days, unSignedLibs);

                bestLibPlan.Library.IsSignedUp = true;
                scanPlans.Add(bestLibPlan);
                currentDay += bestLibPlan.Library.SignupProcess;

                unscanedBooks = unscanedBooks.Except(bestLibPlan.BooksToScanOld).ToList();

                unSignedLibs = libraries.Where(x => !x.IsSignedUp && x.SignupProcess + currentDay < days).ToList();
            }

            return scanPlans.Where(lp => lp.TotalScoreProduced > 0).ToList();
        }

        public static LibraryScanPlan GetScanPlan2(List<Book> unscannedBooks, int day, int allDays, List<Library> unSignedUpLibraries)
        {
            var allPlans = unSignedUpLibraries
                //.OrderByDescending(l => l.TotalScore / (l.SignupProcess + (l.BooksCount / l.ShipPerDay))).Take(10)
                .Select(library =>
                {
                    var scanDays = allDays - library.SignupProcess - day;
                    var BooksToScanOldCount = scanDays * library.ShipPerDay;
                    var score = 0;
                    foreach (var book in library.Books)
                    {
                        if (unscannedBooks.Contains(book))
                        {
                            --BooksToScanOldCount;
                            score += book.Score;
                        }
                        if (BooksToScanOldCount == 0)
                        {
                            break;
                        }
                    }
                    // var BooksToScanOld = library.Books.Intersect(unscannedBooks).Take(scanDays * library.ShipPerDay);

                    return new LibraryScanPlan()
                    {
                        Library = library,
                        TotalScoreProduced = score
                    };
                });

            LibraryScanPlan bestPlan = new LibraryScanPlan() { TotalScoreProduced = 0 };

            foreach (var plan in allPlans)
            {
                if (plan.TotalScoreProduced >= bestPlan.TotalScoreProduced)
                {
                    bestPlan = plan;
                }
            }

            bestPlan.BooksToScanOld = bestPlan.Library.Books
                .Intersect(unscannedBooks)
                .Take((allDays - bestPlan.Library.SignupProcess - day) * bestPlan.Library.ShipPerDay);

            return bestPlan;
        }

        // Remove scanned books from libraries book list
        public static List<LibraryScanPlan> FindPlan3(List<Book> books, int days, List<Library> libraries)
        {
            var currentDay = 0;
            var scanPlans = new List<LibraryScanPlan>();

            var unSignedLibs = libraries.Where(x => x.SignupProcess + currentDay < days).ToList();

            while (currentDay <= days && unSignedLibs.Any())
            {
                var bestLibPlan = GetScanPlan3(currentDay, days, unSignedLibs);

                bestLibPlan.Library.IsSignedUp = true;
                scanPlans.Add(bestLibPlan);
                currentDay += bestLibPlan.Library.SignupProcess;

                unSignedLibs = libraries.Where(x => !x.IsSignedUp && x.SignupProcess + currentDay < days).ToList();

                foreach (var scannedBook in bestLibPlan.BooksToScanOld)
                {
                    foreach (var lib in unSignedLibs)
                    {
                        lib.Books.Remove(scannedBook);
                    }
                }

            }

            return scanPlans.Where(lp => lp.TotalScoreProduced > 0).ToList();
        }

        public static LibraryScanPlan GetScanPlan3(int day, int allDays, List<Library> unSignedUpLibraries)
        {
            var allPlans = unSignedUpLibraries
                //.OrderByDescending(l => l.TotalScore / (l.SignupProcess + (l.BooksCount / l.ShipPerDay))).Take(10)
                .Select(library =>
                {
                    var scanDays = allDays - library.SignupProcess - day;
                    var BooksToScanOldCount = scanDays * library.ShipPerDay;
                    var score = 0;
                    var actualBooksCount = scanDays <= 0 ? 0 : Math.Min(BooksToScanOldCount, library.Books.Count);
                    for (int i = 0; i < actualBooksCount; ++i)
                    {
                        score += library.Books[i].Score;
                    }
                    // var BooksToScanOld = library.Books.Intersect(unscannedBooks).Take(scanDays * library.ShipPerDay);

                    return new LibraryScanPlan()
                    {
                        Library = library,
                        TotalScoreProduced = score
                    };
                });

            LibraryScanPlan bestPlan = new LibraryScanPlan() { TotalScoreProduced = 0 };

            foreach (var plan in allPlans)
            {
                if (plan.TotalScoreProduced >= bestPlan.TotalScoreProduced)
                {
                    bestPlan = plan;
                }
            }

            bestPlan.BooksToScanOld = bestPlan.Library.Books
                .Take((allDays - bestPlan.Library.SignupProcess - day) * bestPlan.Library.ShipPerDay);

            return bestPlan;
        }

        // Get rid of LINQ as much as possible
        public static List<LibraryScanPlan> FindPlan4(List<Book> books, int days, List<Library> libraries)
        {
            var currentDay = 0;
            var scanPlans = new List<LibraryScanPlan>();

            var unSignedLibs = libraries;

            while (currentDay <= days && unSignedLibs.Count > 0)
            {
                var bestLibPlan = GetScanPlan4(currentDay, days, unSignedLibs);

                bestLibPlan.Library.IsSignedUp = true;
                if (bestLibPlan.TotalScoreProduced > 0)
                {
                    scanPlans.Add(bestLibPlan);
                }

                currentDay += bestLibPlan.Library.SignupProcess;
                unSignedLibs.Remove(bestLibPlan.Library);

                for (int i = 0; i < unSignedLibs.Count; i++)
                {
                    if (unSignedLibs[i].SignupProcess + currentDay >= days)
                    {
                        unSignedLibs.RemoveAt(i);
                        --i;
                    }
                }

                foreach (var scannedBook in bestLibPlan.BooksToScanOld)
                {
                    foreach (var lib in unSignedLibs)
                    {
                        lib.Books.Remove(scannedBook);
                    }
                }

            }

            return scanPlans;
        }

        public static LibraryScanPlan GetScanPlan4(int day, int allDays, List<Library> unSignedUpLibraries)
        {
            var allPlans = unSignedUpLibraries
                //.OrderByDescending(l => l.TotalScore / (l.SignupProcess + (l.BooksCount / l.ShipPerDay))).Take(10)
                .Select(library =>
                {
                    var scanDays = allDays - library.SignupProcess - day;
                    var BooksToScanOldCount = scanDays * library.ShipPerDay;
                    var score = 0;
                    var actualBooksCount = scanDays <= 0 ? 0 : Math.Min(BooksToScanOldCount, library.Books.Count);
                    for (int i = 0; i < actualBooksCount; ++i)
                    {
                        score += library.Books[i].Score;
                    }

                    return new LibraryScanPlan()
                    {
                        Library = library,
                        TotalScoreProduced = score
                    };
                });

            LibraryScanPlan bestPlan = new LibraryScanPlan() { TotalScoreProduced = 0 };

            foreach (var plan in allPlans)
            {
                if (plan.TotalScoreProduced >= bestPlan.TotalScoreProduced)
                {
                    bestPlan = plan;
                }
            }

            var booksCount = Math.Min((allDays - bestPlan.Library.SignupProcess - day) * bestPlan.Library.ShipPerDay, bestPlan.Library.Books.Count);
            var BooksToScanOld = new Book[booksCount];
            bestPlan.Library.Books.CopyTo(0, BooksToScanOld, 0, booksCount);

            bestPlan.BooksToScanOld = BooksToScanOld;

            return bestPlan;
        }

        // Get rid of LINQ as much as possible
        public static int EvaluateScore5(Library library, int daysLeft)
        {
            var scanDays = daysLeft - library.SignupProcess;
            var BooksToScanOldCount = scanDays * library.ShipPerDay;
            var score = 0;
            var actualBooksCount = scanDays <= 0 ? 0 : Math.Min(BooksToScanOldCount, library.Books.Count);
            for (int i = 0; i < actualBooksCount; ++i)
            {
                score += library.Books[i].Score;
            }

            return score;
        }

        public static List<LibraryScanPlan> FindPlan5(List<Book> books, int days, List<Library> libraries)
        {
            var currentDay = 0;
            var scanPlans = new List<LibraryScanPlan>();

            var unSignedLibs = libraries;

            while (currentDay <= days && unSignedLibs.Count > 0)
            {
                var bestLibPlan = GetScanPlan5(currentDay, days, unSignedLibs);

                bestLibPlan.Library.IsSignedUp = true;
                if (bestLibPlan.TotalScoreProduced > 0)
                {
                    scanPlans.Add(bestLibPlan);
                }

                currentDay += bestLibPlan.Library.SignupProcess;
                unSignedLibs.Remove(bestLibPlan.Library);

                for (int i = 0; i < unSignedLibs.Count; i++)
                {
                    if (unSignedLibs[i].SignupProcess + currentDay >= days)
                    {
                        unSignedLibs.RemoveAt(i);
                        --i;
                    }
                }

                foreach (var scannedBook in bestLibPlan.BooksToScanOld)
                {
                    foreach (var lib in unSignedLibs)
                    {
                        lib.Books.Remove(scannedBook);
                    }
                }

            }

            return scanPlans;
        }

        public static LibraryScanPlan GetScanPlan5(int day, int allDays, List<Library> unSignedUpLibraries)
        {
            var currBestIdx = 0;
            var currBestScore = int.MinValue;
            var daysLeft = allDays - day;

            for (int i = 0; i < unSignedUpLibraries.Count; ++i)
            {
                var temp = EvaluateScore5(unSignedUpLibraries[i], daysLeft);
                if (temp >= currBestScore)
                {
                    currBestIdx = i;
                    currBestScore = temp;
                }
            }

            LibraryScanPlan bestPlan = new LibraryScanPlan()
            {
                Library = unSignedUpLibraries[currBestIdx],
                TotalScoreProduced = currBestScore
            };

            var booksCount = Math.Min((allDays - bestPlan.Library.SignupProcess - day) * bestPlan.Library.ShipPerDay, bestPlan.Library.Books.Count);
            var BooksToScanOld = new Book[booksCount];
            bestPlan.Library.Books.CopyTo(0, BooksToScanOld, 0, booksCount);

            bestPlan.BooksToScanOld = BooksToScanOld;

            return bestPlan;
        }
    }
}
