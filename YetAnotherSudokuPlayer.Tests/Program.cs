using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using YetAnotherSudokuPlayer.Tests.Tests;

namespace YetAnotherSudokuPlayer.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            SolverTests tests = new SolverTests();
            //tests.CheckForPairsTest2();
            tests.SolveVeryEasy1();
        }
    }
}
