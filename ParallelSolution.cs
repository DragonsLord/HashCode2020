using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace HashCodeProblem
{
    public class ParallelSolution
    {
        public static Output Resolve(Input input)
        {
            var plan = FindPlan(input.DayForScanning, input.Libraries);

            var score = ScoreCalulator.ShowResult(plan);

            //Debug.WriteLine("Current score: " + score);

            return new Output()
            {
                Libraries = plan.Select(p => new LibraryOutput()
                {
                    Id = p.Library.Id,
                    BooksIds = p.BooksToScan.Select(b => b.Id).ToList()
                }).ToList()
            };
        }

        public static int EvaluateScore(Library library, int daysLeft)
        {
            //var scanDays = daysLeft - library.SignupProcess;
            //var booksToScanCount = scanDays * library.ShipPerDay;
            //var score = 0;
            //var actualBooksCount = scanDays <= 0 ? 0 : Math.Min(booksToScanCount, library.Books.Count);
            //for (int i = 0; i < actualBooksCount; ++i)
            //{
            //    score += library.Books[i].Score;
            //}

            //return score;

            //return library.Books.Count * 1000000 / library.SignupProcess;

            return library.BooksCount * 1000000 / (library.SignupProcess + library.BooksCount / library.ShipPerDay);
        }

        public static int EvaluateScoreForDuplicates(Library library)
        {
            //var scanDays = daysLeft - library.SignupProcess;
            //var booksToScanCount = scanDays * library.ShipPerDay;
            //var score = 0;
            //var actualBooksCount = scanDays <= 0 ? 0 : Math.Min(booksToScanCount, library.Books.Count);
            //for (int i = 0; i < actualBooksCount; ++i)
            //{
            //    score += library.Books[i].Score;
            //}

            //return score;

            //return library.Books.Count * 1000000 / library.SignupProcess;

            return library.Books.Sum(_ => _.Score);
        }

        public static ICollection<Book> ScanBooks(List<LibraryScanPlan> scanPlans, int day)
        {
            //Debug.WriteLine("Prepare books to scan...");
            var bookInLibrary = new Dictionary<Book, List<LibraryScanPlan>>();

            for (int planIndex = 0; planIndex < scanPlans.Count; ++planIndex)
            {
                var plan = scanPlans[planIndex];
                if (day >= plan.FirstScanDay && plan.Library.Books.Count > 0)
                {
                    for (int bookIdx = 0; bookIdx < plan.Library.ShipPerDay && bookIdx < plan.Library.Books.Count; ++bookIdx)
                    {
                        var book = plan.Library.Books[bookIdx];
                        if (bookInLibrary.ContainsKey(book))
                        {
                            bookInLibrary[book].Add(plan);
                        }
                        else
                        {
                            bookInLibrary[book] = new List<LibraryScanPlan>(scanPlans.Count);
                            bookInLibrary[book].Add(plan);
                        }

                    }
                }
            }

            bool duplicatesFound;
            do
            {
                var tempArr = ArrayPool<LibraryScanPlan>.Shared.Rent(bookInLibrary.Count);
                var tempIdx = 0;
                duplicatesFound = false;
                foreach (var bookLibsPair in bookInLibrary)
                {
                    if (bookLibsPair.Value.Count > 1)
                    {
                        duplicatesFound = true;

                        var score = int.MinValue;
                        var bestIdx = -1;
                        for (int i = 0; i < bookLibsPair.Value.Count; ++i)
                        {
                            var plan = bookLibsPair.Value[i];
                            var libScore = EvaluateScoreForDuplicates(plan.Library);

                            if (libScore >= score)
                            {
                                score = libScore;
                                if (bestIdx >= 0)
                                {
                                    var oldPlan = bookLibsPair.Value[bestIdx];
                                    bookLibsPair.Value.RemoveAt(bestIdx);
                                    --i;
                                    oldPlan.Library.Books.Remove(bookLibsPair.Key);
                                    tempArr[tempIdx++] = oldPlan;
                                }
                                bestIdx = i;
                            }
                            else
                            {
                                bookLibsPair.Value.RemoveAt(i);
                                --i;
                                plan.Library.Books.Remove(bookLibsPair.Key);
                                tempArr[tempIdx++] = plan;
                            }
                        }
                    }
                }

                for (int i = 0; i < tempIdx; ++i)
                {
                    var plan = tempArr[i];
                    if (plan.Library.Books.Count >= plan.Library.ShipPerDay)
                    {
                        var nextBook = plan.Library.Books[plan.Library.ShipPerDay - 1];
                        if (bookInLibrary.ContainsKey(nextBook))
                        {
                            bookInLibrary[nextBook].Add(plan);
                        }
                        else
                        {
                            bookInLibrary.Add(nextBook, new List<LibraryScanPlan> { plan });
                        }
                    }
                }

                ////Debug.WriteLine($"Duplicates are found in {string.Join(", ", tempArr.Take(tempIdx+1))}");
                ArrayPool<LibraryScanPlan>.Shared.Return(tempArr);

            } while (duplicatesFound);

            foreach (var bookLibsPair in bookInLibrary)
            {
                bookLibsPair.Value[0].BooksToScan.Add(bookLibsPair.Key);
                bookLibsPair.Value[0].Library.Books.Remove(bookLibsPair.Key);
            }

            return bookInLibrary.Keys;
        }

        public static void SignUpLib(int day, int days, ref int nextSignUpDay, List<LibraryScanPlan> scanPlans, List<Library> unSignedLibs)
        {
            var scanPlan = GetScanPlan(day, days, unSignedLibs);
            if (scanPlan.TotalScoreProduced > 0)
            {
                scanPlans.Add(scanPlan);
                //Debug.WriteLine($"new lib {scanPlan.Library.Id} started signup process");
            }
            nextSignUpDay += scanPlan.Library.SignupProcess;
            unSignedLibs.Remove(scanPlan.Library);

            // filter libs which will not able to scan books anymore
            for (int i = 0; i < unSignedLibs.Count; ++i)
            {
                if (unSignedLibs[i].SignupProcess + day >= days)
                {
                    unSignedLibs.RemoveAt(i);
                    --i;
                }
            }
        }

        public static List<LibraryScanPlan> FindPlan(int days, List<Library> libraries)
        {
            var nextSignUpDay = 0;
            var scanPlans = new List<LibraryScanPlan>();

            var unSignedLibs = libraries;

            for (int day = 0; day < days; ++day)
            {
                //Debug.WriteLine($"{day} of {days}");
                if (day == nextSignUpDay && unSignedLibs.Count > 0)
                {
                    SignUpLib(day, days, ref nextSignUpDay, scanPlans, unSignedLibs);
                }

                var scannedBooks = ScanBooks(scanPlans, day);

                foreach (var lib in unSignedLibs)
                {
                    foreach (var book in scannedBooks)
                    {
                        lib.Books.Remove(book);
                    }
                }

                //Debug.WriteLine("Books scanned");
            }

            return scanPlans;
        }

        public static LibraryScanPlan GetScanPlan(int day, int allDays, List<Library> unSignedUpLibraries)
        {
            var currBestIdx = 0;
            var currBestScore = int.MinValue;
            var daysLeft = allDays - day;

            for (int i = 0; i < unSignedUpLibraries.Count; ++i)
            {
                var temp = EvaluateScore(unSignedUpLibraries[i], daysLeft);
                if (temp >= currBestScore)
                {
                    currBestIdx = i;
                    currBestScore = temp;
                }
            }

            LibraryScanPlan bestPlan = new LibraryScanPlan()
            {
                Library = unSignedUpLibraries[currBestIdx],
                FirstScanDay = day + unSignedUpLibraries[currBestIdx].SignupProcess,
                TotalScoreProduced = currBestScore,
                BooksToScan = new List<Book>()
            };

            return bestPlan;
        }
    }
}
