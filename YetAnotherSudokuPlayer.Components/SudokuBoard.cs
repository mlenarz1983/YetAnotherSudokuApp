using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Diagnostics;
using DejaVu;
using YetAnotherSudokuPlayer.Components.Properties;
using System.Threading;

namespace YetAnotherSudokuPlayer.Components
{
    public class SudokuBoard : Component, INotifyPropertyChanged
    {
        private BackgroundWorker bwSolver;
        SudokuSquare[] _squares = new SudokuSquare[81];
        int _solutionProgress = 0;
        string _msg;
        double? _difficulty = 0;

        public SudokuBoard()
        {
            InitializeComponent();

            for (int i = 0; i < _squares.Length; i++)
            {
                _squares[i] = new SudokuSquare();
                _squares[i].Board = this;
                _squares[i].Position = new Point(i - Convert.ToInt32(i / 9) * 9, Convert.ToInt32(i / 9));
            }
        }

        public int SolutionProgress
        {
            get { return _solutionProgress; }
            set
            {
                bool changed = (_solutionProgress != value);
                _solutionProgress = value;
                if (changed)
                    OnPropertyChanged("SolutionProgress");
            }
        }
        public string Message
        {
            get { return _msg; }
            set
            {
                bool changed = (_msg != value);
                _msg = value;
                if (changed)
                    OnPropertyChanged("Message");
            }
        }
        public double? Difficulty
        {
            get { return _difficulty; }
            set
            {
                bool changed = (_difficulty != value);
                _difficulty = value;
                if (changed)
                    OnPropertyChanged("Difficulty");
            }
        }

        public SudokuSquare GetSquare(int position)
        {
            return _squares[position];
        }
        public SudokuSquare GetSquare(Point position)
        {
            return GetSquare(position.X, position.Y);
        }
        public SudokuSquare GetSquare(int x, int y)
        {
            return _squares[x + 9 * y];
        }
        public bool IsValueValid(int position, int value, bool useUserValue)
        {
            return IsValueValid(new Point(position - Convert.ToInt32(position / 9) * 9, Convert.ToInt32(position / 9)), value, useUserValue);
        }
        public bool IsValueValid(Point position, int value, bool useUserValue)
        {
            //search row
            for (int i = 0; i < 9; i++)
            {
                Point p = new Point(i, position.Y);
                if (p != position)
                {
                    if (useUserValue)
                    {
                        if (GetSquare(p).UserValue == value)
                            return false;
                    }
                    else
                    {
                        if (GetSquare(p).ActualValue == value)
                            return false;
                    }
                }
            }

            //search column
            for (int j = 0; j < 9; j++)
            {
                Point p = new Point(position.X, j);
                if (p != position)
                {
                    if (useUserValue)
                    {
                        if (GetSquare(p).UserValue == value)
                            return false;
                    }
                    else
                    {
                        if (GetSquare(p).ActualValue == value)
                            return false;
                    }
                }
            }

            //search box
            Point boxLocation = new Point(Convert.ToInt32(position.X / 3), Convert.ToInt32(position.Y / 3));
            for (int i = boxLocation.X * 3; i < boxLocation.X * 3 + 3; i++)
            {
                for (int j = boxLocation.Y * 3; j < boxLocation.Y * 3 + 3; j++)
                {
                    Point p = new Point(i, j);
                    if (p != position)
                    {
                        if (useUserValue)
                        {
                            if (GetSquare(p).UserValue == value)
                                return false;
                        }
                        else
                        {
                            if (GetSquare(p).ActualValue == value)
                                return false;
                        }
                    }
                }
            }

            return true;
        }
        public SolutionResults Solve()
        {
            return SolveCore(null, false);
        }
        public void SolveAsync(bool showSolution)
        {
            bwSolver.RunWorkerAsync(showSolution);
        }
        public void NewGame(GameDifficulty difficulty)
        {
            int squaresToReveal = 0;
            switch (difficulty)
            {
                case GameDifficulty.VeryEasy:
                    squaresToReveal = 40;
                    break;
                case GameDifficulty.Easy:
                    squaresToReveal = 34;
                    break;
                case GameDifficulty.Medium:
                    squaresToReveal = 30;
                    break;
                case GameDifficulty.Hard:
                    squaresToReveal = 28;
                    break;
                case GameDifficulty.VeryHard:
                    squaresToReveal = 25;
                    break;
            }

            NewGame(squaresToReveal);
        }
        public void NewGame(int squaresToReveal)
        {
            ClearSquares();

            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    //formula comes from http://www.gamedev.net/community/forums/topic.asp?topic_id=347124
                    GetSquare(new Point(i, j)).ActualValue = (i + 3 * j + j / 3) % 9 + 1;
                }
            }

            Random rnd = new Random();

            //randomize rows (within supercells)
            RandomizeBoard(ConstraintType.Row);
            RandomizeBoard(ConstraintType.Column);

            List<int> positionsWithClues = new List<int>();
            for (int i = 0; i < _squares.Length; i++)
                positionsWithClues.Add(i);

            while (positionsWithClues.Count > squaresToReveal)
            {
                int positionIndex = rnd.Next(positionsWithClues.Count);
                positionsWithClues.RemoveAt(positionIndex);
            }

            for (int positionIndex = 0; positionIndex < positionsWithClues.Count; positionIndex++)
            {
                _squares[positionsWithClues[positionIndex]].UserValue = _squares[positionsWithClues[positionIndex]].ActualValue;
            }
        }
        public void LoadGame(string gameData)
        {
            LoadGame(gameData, GameLoadOptions.All);
        }
        public void LoadGame(string gameData, GameLoadOptions options)
        {
            int position = 0;
            bool readingNonDismissedValues = false;
            foreach (char c in gameData)
            {
                if (char.IsDigit(c))
                {
                    if (!readingNonDismissedValues)
                    {
                        if ((options & GameLoadOptions.SolutionValues) == GameLoadOptions.SolutionValues)
                            _squares[position].ActualValue = Convert.ToInt32(c.ToString()); //we're assuming that the values being read in are correct
                        if ((options & GameLoadOptions.UserValues) == GameLoadOptions.UserValues)
                            _squares[position].UserValue = Convert.ToInt32(c.ToString());

                        position++;
                    }
                    else
                    {
                        if ((options & GameLoadOptions.SolutionValues) == GameLoadOptions.SolutionValues)
                            _squares[position].UpdateDismissedValue(Convert.ToInt32(c.ToString()), false);
                        if ((options & GameLoadOptions.UserValues) == GameLoadOptions.UserValues)
                            _squares[position].UpdateUserDismissedValue(Convert.ToInt32(c.ToString()), false);
                    }
                }
                else if (c == '.')
                {
                    position++;
                }
                else if (c == '(')
                {
                    readingNonDismissedValues = true;

                    if ((options & GameLoadOptions.SolutionValues) == GameLoadOptions.SolutionValues)
                        _squares[position].UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, true);
                    if ((options & GameLoadOptions.UserValues) == GameLoadOptions.UserValues)
                        _squares[position].UpdateUserDismissedValues(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, true);
                }
                else if (c == ')')
                {
                    readingNonDismissedValues = false;
                    position++;
                }
            }
        }
        public string SaveGame()
        {
            return SaveGame(true);
        }
        public string SaveGame(bool addNewLines)
        {
            return SaveGame(true, GameLoadOptions.UserValues);
        }
        public string SaveGame(bool addNewLines, GameLoadOptions options)
        {
            return SaveGame(addNewLines, options, false);
        }
        public string SaveGame(bool addNewLines, GameLoadOptions options, bool includeNonDismissed)
        {
            if ((options == GameLoadOptions.None)
                || (options == GameLoadOptions.All))
                throw new ArgumentException("Options must specify EITHER the user values OR the actual values - not both.", "options");

            StringBuilder gameData = new StringBuilder();
            for (int i = 0; i < _squares.Length; i++)
            {
                if ((options & GameLoadOptions.SolutionValues) == GameLoadOptions.SolutionValues)
                {
                    if (_squares[i].ActualValue.HasValue)
                    {
                        gameData.Append(_squares[i].ActualValue.Value);
                    }
                    else
                    {
                        if (includeNonDismissed)
                        {
                            gameData.AppendFormat("({0})", string.Join("", _squares[i].NonDismissedValues.ConvertAll<string>(
                                delegate(int val) { return val.ToString(); }).ToArray()));
                        }
                        else
                        {
                            gameData.Append(".");
                        }
                    }
                }
                else if ((options & GameLoadOptions.UserValues) == GameLoadOptions.UserValues)
                {
                    if (_squares[i].UserValue.HasValue)
                    {
                        gameData.Append(_squares[i].UserValue.Value);
                    }
                    else
                    {
                        if (includeNonDismissed)
                        {
                            gameData.AppendFormat("({0})", string.Join("", _squares[i].UserNonDismissedValues.ConvertAll<string>(
                                delegate(int val) { return val.ToString(); }).ToArray()));
                        }
                        else
                        {
                            gameData.Append(".");
                        }
                    }
                }

                if (addNewLines && ((i + 1) % 9 == 0))
                    gameData.Append(Environment.NewLine);
            }
            return gameData.ToString();
        }
        public int PerformBacktracking()
        {
            int effectedSquares = 0;

            for (int i = 0; i < _squares.Length; i++)
            {
                SudokuSquare square = _squares[i];
                Point position = new Point(i - Convert.ToInt32(i / 9) * 9, Convert.ToInt32(i / 9));

                if (!square.ActualValue.HasValue)
                {
                    List<int> validValues = new List<int>();
                    List<int> candidates = square.NonDismissedValues;
                    for (int j = 0; j < candidates.Count; j++)
                    {
                        if (IsValueValid(position, candidates[j], false))
                        {
                            validValues.Add(candidates[j]);
                        }
                        else
                        {
                            square.UpdateDismissedValue(candidates[j], true); //cache for later
                            effectedSquares++;
                        }
                    }

                    if (validValues.Count == 1)
                    {
                        square.ActualValue = validValues[0];
                        effectedSquares++;
                    }
                }
            }

            return effectedSquares;
        }
        public int CheckForSingletons()
        {
            int effectedSquares = 0;
            effectedSquares += CheckForSingletons(ConstraintType.Column);
            effectedSquares += CheckForSingletons(ConstraintType.Row);
            effectedSquares += CheckForSingletons(ConstraintType.SuperCell);
            return effectedSquares;
        }
        public int CheckForSingletons(ConstraintType constraint)
        {
            int effectedSquares = 0;

            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, List<SudokuSquare>> possibilities = ExtractPossibilities(constraint, i);

                foreach (int possibileValue in possibilities.Keys)
                {
                    if (possibilities[possibileValue].Count == 1)
                    {
                        if (IsValueValid(possibilities[possibileValue][0].Position, possibileValue, false))
                        {
                            Debug.Assert(!possibilities[possibileValue][0].ActualValue.HasValue, string.Format("Square {0} already has a value.", possibilities[possibileValue][0].Position));

                            //this only works if the value is valid
                            possibilities[possibileValue][0].ActualValue = possibileValue;
                            effectedSquares++;
                        }
                        else
                        {
                            //possibilities[possibileValue][0].UpdateDismissedValue(possibileValue, true); //cache for later
                            //effectedSquares++;
                        }
                    }
                }
            }

            return effectedSquares;
        }
        public int CheckForPairs()
        {
            int effectedSquares = 0;
            effectedSquares += CheckForPairs(ConstraintType.Column);
            effectedSquares += CheckForPairs(ConstraintType.Row);
            effectedSquares += CheckForPairs(ConstraintType.SuperCell);
            return effectedSquares;
        }
        public int CheckForPairs(ConstraintType constraint)
        {
            int effectedSquares = 0;

            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, List<SudokuSquare>> possibilities = ExtractPossibilities(constraint, i);

                effectedSquares += DismissUnnecessaryIntraPairPossibilities(possibilities);
                effectedSquares += DismissUnnecessaryExtraPairPossibilities(possibilities);
            }

            return effectedSquares;
        }
        public void ShowSolution()
        {
            using (UndoRedoManager.Start("Show solution"))
            {
                for (int i = 0; i < _squares.Length; i++)
                    _squares[i].UserValue = _squares[i].ActualValue;

                UndoRedoManager.Commit();
            }
        }
        public void ClearSquares()
        {
            SolutionProgress = 0;
            Message = "";
            Difficulty = null;
            for (int i = 0; i < _squares.Length; i++)
            {
                _squares[i].ActualValue = null;
                _squares[i].UpdateDismissedValues(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, false);
                _squares[i].UpdateUserDismissedValues(new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, false);
                _squares[i].UserValue = null;
            }
        }

        void RandomizeBoard(ConstraintType constraint)
        {
            Random rnd = new Random();

            if (constraint == ConstraintType.SuperCell)
                throw new ArgumentException("Can't randomize supercells", "constraint");

            for (int x = 0; x < 3; x++)
            {
                //6 possible ways to re-organize rows/columns
                int configuration = rnd.Next(5);

                int newPosForGroupOne = Convert.ToInt32(configuration / 2) + 1;
                int newPosForGroupTwo = 3 - (configuration % 3);
                int newPosForGroupThree = (Convert.ToInt32((configuration + 3) / 2) % 3) + 1;

                List<int> groupOne = new List<int>();
                for (int i = 0; i < 9; i++)
                {
                    if (constraint == ConstraintType.Row)
                        groupOne.Add(GetSquare(new Point(i, x * 3)).ActualValue.Value);
                    else
                        groupOne.Add(GetSquare(new Point(x * 3, i)).ActualValue.Value);
                }
                List<int> groupTwo = new List<int>();
                for (int i = 0; i < 9; i++)
                {
                    if (constraint == ConstraintType.Row)
                        groupTwo.Add(GetSquare(new Point(i, x * 3 + 1)).ActualValue.Value);
                    else
                        groupTwo.Add(GetSquare(new Point(x * 3 + 1, i)).ActualValue.Value);
                }
                List<int> groupThree = new List<int>();
                for (int i = 0; i < 9; i++)
                {
                    if (constraint == ConstraintType.Row)
                        groupThree.Add(GetSquare(new Point(i, x * 3 + 2)).ActualValue.Value);
                    else
                        groupThree.Add(GetSquare(new Point(x * 3 + 2, i)).ActualValue.Value);
                }

                if (newPosForGroupOne != 1)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (newPosForGroupOne == 2)
                        {
                            if (constraint == ConstraintType.Row)
                                GetSquare(new Point(i, x * 3)).ActualValue = groupTwo[i];
                            else
                                GetSquare(new Point(x * 3, i)).ActualValue = groupTwo[i];

                        }
                        else if (newPosForGroupOne == 3)
                        {
                            if (constraint == ConstraintType.Row)
                                GetSquare(new Point(i, x * 3)).ActualValue = groupThree[i];
                            else
                                GetSquare(new Point(x * 3, i)).ActualValue = groupThree[i];
                        }
                    }
                }
                if (newPosForGroupTwo != 2)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (newPosForGroupTwo == 1)
                        {
                            if (constraint == ConstraintType.Row)
                                GetSquare(new Point(i, x * 3 + 1)).ActualValue = groupOne[i];
                            else
                                GetSquare(new Point(x * 3 + 1, i)).ActualValue = groupOne[i];
                        }
                        else if (newPosForGroupTwo == 3)
                        {
                            if (constraint == ConstraintType.Row)
                                GetSquare(new Point(i, x * 3 + 1)).ActualValue = groupThree[i];
                            else
                                GetSquare(new Point(x * 3 + 1, i)).ActualValue = groupThree[i];
                        }
                    }
                }
                if (newPosForGroupThree != 3)
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (newPosForGroupThree == 1)
                        {
                            if (constraint == ConstraintType.Row)
                                GetSquare(new Point(i, x * 3 + 2)).ActualValue = groupOne[i];
                            else
                                GetSquare(new Point(x * 3 + 2, i)).ActualValue = groupOne[i];
                        }
                        else if (newPosForGroupThree == 2)
                        {
                            if (constraint == ConstraintType.Row)
                                GetSquare(new Point(i, x * 3 + 2)).ActualValue = groupTwo[i];
                            else
                                GetSquare(new Point(x * 3 + 2, i)).ActualValue = groupTwo[i];
                        }
                    }
                }
            }
        }
        void InitializeComponent()
        {
            this.bwSolver = new System.ComponentModel.BackgroundWorker();
            // 
            // bwSolver
            // 
            this.bwSolver.WorkerReportsProgress = true;
            this.bwSolver.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwSolver_DoWork);
            this.bwSolver.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bwSolver_RunWorkerCompleted);
            this.bwSolver.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bwSolver_ProgressChanged);

        }
        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        int GetSolvedSquareCount()
        {
            int solvedSquares = 0;
            for (int i = 0; i < _squares.Length; i++)
            {
                SudokuSquare square = _squares[i];
                if (square.ActualValue.HasValue)
                    solvedSquares++;
            }
            return solvedSquares;
        }
        SolutionResults SolveCore(BackgroundWorker bw, bool showSolution)
        {
            int squaresToSolve = _squares.Length;
            SolutionResults result = new SolutionResults();

            //arbitrary measure of the amount of effort needed to solve the puzzle
            int effortNeeded = 0;

            while (true)
            {
                if (bw != null)
                    bw.ReportProgress(Convert.ToInt32((double)GetSolvedSquareCount() / (double)squaresToSolve * 100));

                if (showSolution)
                    ShowSolution();

                effortNeeded += 1;

                if (PerformBacktracking() == 0)
                {
                    effortNeeded += 3;  //checking for singltons is more difficult

                    if (CheckForSingletons() == 0)
                    {
                        effortNeeded += 12;  //checking for pairs is way more difficult

                        if (CheckForPairs() == 0)
                        {
                            if (!result.Comments.Contains(Resources.AmbiguousSolutionMsg))
                                result.Comments.Add(Resources.AmbiguousSolutionMsg);

                            //we're not getting anywhere - make a guess
                            for (int i = 0; i < _squares.Length; i++)
                            {
                                if (!_squares[i].ActualValue.HasValue)
                                {
                                    Debug.Assert(_squares[i].NonDismissedValues.Count > 0);
                                    _squares[i].ActualValue = _squares[i].NonDismissedValues[0];
                                    break;
                                }
                            }
                        }
                    }
                }

                if (GetSolvedSquareCount() == squaresToSolve)
                    break;
            }

            //difficulty of 10 is the max
            if (effortNeeded > 1024)
                effortNeeded = 1024;

            result.Difficulty = Math.Log(effortNeeded, 2);

            return result;
        }
        Dictionary<int, List<SudokuSquare>> ExtractPossibilities(ConstraintType constraint, int i)
        {
            Dictionary<int, List<SudokuSquare>> possibilities = new Dictionary<int, List<SudokuSquare>>();
            for (int j = 0; j < 9; j++)
            {
                SudokuSquare square = null;

                switch (constraint)
                {
                    case ConstraintType.Column:
                        square = GetSquare(new Point(i, j));
                        break;
                    case ConstraintType.Row:
                        square = GetSquare(new Point(j, i));
                        break;
                    case ConstraintType.SuperCell:
                        square = GetSquare(Utils.CalculatePoint(i, j));
                        break;
                }

                if (!square.ActualValue.HasValue)
                {
                    List<int> candidates = square.NonDismissedValues;
                    foreach (int candidate in candidates)
                    {
                        if (possibilities.ContainsKey(candidate))
                        {
                            possibilities[candidate].Add(square);
                        }
                        else
                        {
                            possibilities.Add(candidate, new List<SudokuSquare>(new SudokuSquare[] { square }));
                        }
                    }
                }
            }
            return possibilities;
        }
        int DismissUnnecessaryIntraPairPossibilities(Dictionary<int, List<SudokuSquare>> possibilities)
        {
            int effectedSquares = 0;
            List<int> potentialPairs = new List<int>();

            foreach (int possibileValue in possibilities.Keys)
            {
                if (possibilities[possibileValue].Count == 2)
                {
                    potentialPairs.Add(possibileValue);
                }
            }

            //List<ValueCollection> pairs = new List<ValueCollection>();

            foreach (int possibileValue in potentialPairs)
            {
                foreach (int innerPossibileValue in potentialPairs)
                {
                    if (possibileValue == innerPossibileValue)
                        continue;

                    if (possibilities[possibileValue].Contains(possibilities[innerPossibileValue][0])
                        && possibilities[possibileValue].Contains(possibilities[innerPossibileValue][1]))
                    {
                        //ValueCollection pair = new ValueCollection() { Values = new int[] { possibileValue, innerPossibileValue } };
                        //if (!pairs.Contains(pair))
                        //{

                        foreach (int nonDismissed in possibilities[possibileValue][0].NonDismissedValues)
                        {
                            if (nonDismissed != possibileValue && nonDismissed != innerPossibileValue)
                            {
                                possibilities[possibileValue][0].UpdateDismissedValue(nonDismissed, true);
                                effectedSquares++;
                            }
                        }
                        foreach (int nonDismissed in possibilities[possibileValue][1].NonDismissedValues)
                        {
                            if (nonDismissed != possibileValue && nonDismissed != innerPossibileValue)
                            {
                                possibilities[possibileValue][1].UpdateDismissedValue(nonDismissed, true);
                                effectedSquares++;
                            }
                        }

                        //pairs.Add(pair); //only do once per pair
                        //}
                    }
                }
            }

            return effectedSquares;
        }
        int DismissUnnecessaryExtraPairPossibilities(Dictionary<int, List<SudokuSquare>> possibilities)
        {
            int effectedSquares = 0;
            List<SudokuSquare> potentialSquares = new List<SudokuSquare>();

            foreach (int possibileValue in possibilities.Keys)
            {
                foreach (SudokuSquare square in possibilities[possibileValue])
                {
                    if (!potentialSquares.Contains(square))
                    {
                        potentialSquares.Add(square);
                    }
                }
            }

            foreach (SudokuSquare possibileSquare in potentialSquares)
            {
                foreach (SudokuSquare innerPossibileSquare in potentialSquares)
                {
                    if (possibileSquare == innerPossibileSquare)
                        continue;

                    if ((possibileSquare.NonDismissedValues.Count != 2)
                        || (innerPossibileSquare.NonDismissedValues.Count != 2))
                        continue;

                    bool match = true;
                    foreach (int nonDismissed in possibileSquare.NonDismissedValues)
                    {
                        if (!innerPossibileSquare.NonDismissedValues.Contains(nonDismissed))
                        {
                            match = false;
                            break;
                        }
                    }

                    if (match)
                    {
                        foreach (SudokuSquare otherSquare in potentialSquares)
                        {
                            if ((otherSquare != possibileSquare)
                                && (otherSquare != innerPossibileSquare))
                            {
                                //this was added so that the effectedSquares variable only gets incremented when a 
                                //meaningful update is being made
                                bool updateNecessary = false;
                                foreach (int i in possibileSquare.NonDismissedValues)
                                {
                                    if (otherSquare.NonDismissedValues.Contains(i))
                                    {
                                        updateNecessary = true;
                                        break;
                                    }
                                }
                                if (updateNecessary)
                                {
                                    otherSquare.UpdateDismissedValues(possibileSquare.NonDismissedValues.ToArray(), true);
                                    effectedSquares++;
                                }
                            }
                        }
                    }
                }
            }

            return effectedSquares;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void bwSolver_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            bool showSolution = (bool)e.Argument;

            e.Result = SolveCore(bw, showSolution);
        }
        private void bwSolver_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            SolutionProgress = e.ProgressPercentage;
        }
        private void bwSolver_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            SolutionResults results = e.Result as SolutionResults;
            SolutionProgress = 100;
            Difficulty = results.Difficulty;
            Message = string.Join(Environment.NewLine, results.Comments.ToArray());
        }
    }

    public enum GameDifficulty
    {
        VeryEasy, Easy, Medium, Hard, VeryHard
    }

    public enum ConstraintType
    {
        Column, Row, SuperCell
    }

    [Flags]
    public enum GameLoadOptions
    {
        None = 0,
        UserValues = 1,
        SolutionValues = 2,
        All = 3
    }

    public class SolutionResults
    {
        public SolutionResults()
        {
            Comments = new List<string>();
        }

        public double Difficulty { get; set; }
        public List<string> Comments { get; set; }
    }
}
