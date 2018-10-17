using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using YetAnotherSudokuPlayer.Components;
using YetAnotherSudokuPlayer.Tests.Properties;
using DejaVu;
using System.IO;

namespace YetAnotherSudokuPlayer.Tests.Tests
{
    [TestFixture]
    public class SolverTests
    {
        [Test]
        public void BacktrackingTest1()
        {
            SudokuBoard board = new SudokuBoard();
            using (UndoRedoManager.Start("SolveTest1"))
            {
                board.LoadGame(Resources.test1);
                UndoRedoManager.Commit();

            }
            board.PerformBacktracking();

            Assert.IsTrue(board.GetSquare(3, 0).ActualValue == 9);
            Assert.IsTrue(board.GetSquare(1, 1).ActualValue == 9);
            Assert.IsTrue(board.GetSquare(5, 1).ActualValue == 1);
            Assert.IsTrue(board.GetSquare(5, 2).ActualValue == 4);
            Assert.IsTrue(board.GetSquare(1, 4).ActualValue == 7);
            Assert.IsTrue(board.GetSquare(5, 5).ActualValue == 5);
            Assert.IsTrue(board.GetSquare(1, 7).ActualValue == 8);
            Assert.IsTrue(board.GetSquare(8, 8).ActualValue == 8);
        }

        [Test]
        public void BacktrackingTest2()
        {
            SudokuBoard board = new SudokuBoard();
            using (UndoRedoManager.Start("SolveTest1"))
            {
                board.LoadGame(Resources.test2);
                UndoRedoManager.Commit();
            }
            board.PerformBacktracking();

            Assert.IsTrue(board.GetSquare(6, 0).ActualValue == 5);
            Assert.IsTrue(board.GetSquare(3, 1).ActualValue == 9);
            Assert.IsTrue(board.GetSquare(8, 1).ActualValue == 1);
            Assert.IsTrue(board.GetSquare(0, 2).ActualValue == 3);
            Assert.IsTrue(board.GetSquare(4, 3).ActualValue == 3);
            Assert.IsTrue(board.GetSquare(1, 4).ActualValue == 6);
            Assert.IsTrue(board.GetSquare(8, 5).ActualValue == 8);
            Assert.IsTrue(board.GetSquare(3, 7).ActualValue == 5);
            Assert.IsTrue(board.GetSquare(7, 8).ActualValue == 2);
        }

        [Test]
        public void SingletonTest1()
        {
            SudokuBoard board = new SudokuBoard();

            board.GetSquare(Utils.CalculatePoint(0, 0)).ActualValue = 1;
            board.GetSquare(Utils.CalculatePoint(0, 1)).ActualValue = 2;
            board.GetSquare(Utils.CalculatePoint(0, 2)).ActualValue = 3;
            board.GetSquare(Utils.CalculatePoint(0, 3)).ActualValue = 4;
            board.GetSquare(Utils.CalculatePoint(0, 4)).ActualValue = 5;
            board.GetSquare(Utils.CalculatePoint(0, 5)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 6, 7, 8 }, true); //should be  9
            board.GetSquare(Utils.CalculatePoint(0, 6)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 6, 7, 9 }, true); //8
            board.GetSquare(Utils.CalculatePoint(0, 7)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 8, 9 }, true); //can't figure out yet
            board.GetSquare(Utils.CalculatePoint(0, 8)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 8, 9 }, true); //can't figure out yet

            board.CheckForSingletons(ConstraintType.SuperCell);

            Assert.IsTrue(board.GetSquare(Utils.CalculatePoint(0, 5)).ActualValue == 9);
            Assert.IsTrue(board.GetSquare(Utils.CalculatePoint(0, 6)).ActualValue == 8);
            Assert.IsTrue(!board.GetSquare(Utils.CalculatePoint(0, 7)).ActualValue.HasValue);
            Assert.IsTrue(!board.GetSquare(Utils.CalculatePoint(0, 8)).ActualValue.HasValue);
        }

        [Test]
        public void CheckForPairsTest1()
        {
            SudokuBoard board = new SudokuBoard();

            board.GetSquare(Utils.CalculatePoint(0, 0)).ActualValue = 1;
            board.GetSquare(Utils.CalculatePoint(0, 1)).ActualValue = 2;
            board.GetSquare(Utils.CalculatePoint(0, 2)).ActualValue = 3;
            board.GetSquare(Utils.CalculatePoint(0, 3)).ActualValue = 4;
            board.GetSquare(Utils.CalculatePoint(0, 4)).ActualValue = 5;
            board.GetSquare(Utils.CalculatePoint(0, 5)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5 }, true); //6 and 7 should be dismissed
            board.GetSquare(Utils.CalculatePoint(0, 6)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5 }, true); //6 and 7 should be dismissed
            board.GetSquare(Utils.CalculatePoint(0, 7)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 8, 9 }, true); //6-7 pair is unique
            board.GetSquare(Utils.CalculatePoint(0, 8)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 8, 9 }, true); //6-7 pair is unique

            board.CheckForPairs(ConstraintType.SuperCell);

            Assert.IsTrue(!board.GetSquare(Utils.CalculatePoint(0, 5)).NonDismissedValues.Contains(6));
            Assert.IsTrue(!board.GetSquare(Utils.CalculatePoint(0, 5)).NonDismissedValues.Contains(7));
            Assert.IsTrue(!board.GetSquare(Utils.CalculatePoint(0, 6)).NonDismissedValues.Contains(6));
            Assert.IsTrue(!board.GetSquare(Utils.CalculatePoint(0, 6)).NonDismissedValues.Contains(7));
        }

        [Test]
        public void CheckForPairsTest2()
        {
            SudokuBoard board = new SudokuBoard();

            board.GetSquare(Utils.CalculatePoint(0, 0)).ActualValue = 1;
            board.GetSquare(Utils.CalculatePoint(0, 1)).ActualValue = 2;
            board.GetSquare(Utils.CalculatePoint(0, 2)).ActualValue = 3;
            board.GetSquare(Utils.CalculatePoint(0, 3)).ActualValue = 4;
            board.GetSquare(Utils.CalculatePoint(0, 4)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 6, 7 }, true);
            board.GetSquare(Utils.CalculatePoint(0, 5)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 6, 7 }, true);
            board.GetSquare(Utils.CalculatePoint(0, 6)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 6, 7 }, true);
            board.GetSquare(Utils.CalculatePoint(0, 7)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 9 }, true);  //6-7 pairs only found here
            board.GetSquare(Utils.CalculatePoint(0, 8)).UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 8, 9 }, true);//6-7 pairs only found here -> 8 should be dismissed

            board.CheckForPairs(ConstraintType.SuperCell);

            Assert.IsTrue(!board.GetSquare(Utils.CalculatePoint(0, 7)).NonDismissedValues.Contains(8));
        }

        [Test]
        public void SolveTest1()
        {
            SudokuBoard board = new SudokuBoard();
            board.LoadGame(Resources.test3, GameLoadOptions.SolutionValues);

            SolutionResults results = board.Solve();
        }

        [Test]
        public void SolveVeryEasy1()
        {
            SudokuBoard board = new SudokuBoard();
            board.LoadGame(Resources.veryeasy, GameLoadOptions.SolutionValues);

            try
            {
                SolutionResults results = board.Solve();
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception occured while solving board: " + ex.ToString());
            }
        }

        [Test]
        public void SolveMedium1()
        {
            SudokuBoard board = new SudokuBoard();
            board.LoadGame(Resources.medium, GameLoadOptions.SolutionValues);

            try
            {
                SolutionResults results = board.Solve();
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception occured while solving board: " + ex.ToString());
            }
        }

        [Test]
        public void SolveHard1()
        {
            SudokuBoard board = new SudokuBoard();
            board.LoadGame(Resources.hard, GameLoadOptions.SolutionValues);

            try
            {
                SolutionResults results = board.Solve();
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception occured while solving board: " + ex.ToString());
            }
        }

        [Test]
        public void SolveVeryHard1()
        {
            SudokuBoard board = new SudokuBoard();
            board.LoadGame(Resources.veryhard, GameLoadOptions.SolutionValues);

            try
            {
                SolutionResults results = board.Solve();
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception occured while solving board: " + ex.ToString());
            }
        }

        [Test]
        public void SolveVeryHard2()
        {
            SudokuBoard board = new SudokuBoard();
            board.LoadGame(Resources.veryhard2, GameLoadOptions.SolutionValues);

            try
            {
                SolutionResults results = board.Solve();
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception occured while solving board: " + ex.ToString());
            }
        }


    }
}
