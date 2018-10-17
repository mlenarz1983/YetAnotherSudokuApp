using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Diagnostics;
using DejaVu;
using DejaVu.Collections.Generic;

namespace YetAnotherSudokuPlayer.Components
{
    [DebuggerDisplay("Position={Position}, UserValue={UserValue}, ActualValue={ActualValue}")]
    public class SudokuSquare : INotifyPropertyChanged
    {
        readonly UndoRedo<int?> _userValue = new UndoRedo<int?>();
        readonly UndoRedoList<bool> _userDismissedValues = new UndoRedoList<bool>();
        bool[] _dismissedValues;
        bool? _correct;

        public SudokuSquare()
        {
            _userDismissedValues = new UndoRedoList<bool>(new bool[9]);
            _dismissedValues = new bool[9];

            _userValue.Changed += new EventHandler<MemberChangedEventArgs>(_userValue_Changed);
            _userDismissedValues.Changed += new EventHandler<MemberChangedEventArgs>(_userDismissedValues_Changed);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int? UserValue
        {
            get { return _userValue.Value; }
            set { _userValue.Value = value; }
        }
        public int? ActualValue { get; set; }
        public List<int> UserNonDismissedValues
        {
            get
            {
                List<int> nonDismissedValues = new List<int>();
                for (int i = 0; i < 9; i++)
                {
                    if (!_userDismissedValues[i])
                        nonDismissedValues.Add(i + 1);
                }
                return nonDismissedValues;
            }
        }
        public List<int> NonDismissedValues
        {
            get
            {
                List<int> nonDismissedValues = new List<int>();
                for (int i = 0; i < 9; i++)
                {
                    if (!_dismissedValues[i])
                        nonDismissedValues.Add(i + 1);
                }
                return nonDismissedValues;
            }
        }
        public SudokuBoard Board { get; set; }
        public Point Position { get; set; }
        public bool Correct
        {
            get
            {
                if (!_correct.HasValue)
                {
                    if (UserValue.HasValue)
                    {
                        _correct = Board.IsValueValid(Position, UserValue.Value, true);
                    }
                    else
                    {
                        _correct = true; //no way to know otherwise
                    }
                }

                return _correct.Value;
            }
        }

        public void UpdateUserDismissedValue(int value, bool dismissed)
        {
            UpdateUserDismissedValues(new int[] { value }, dismissed);
        }
        public void UpdateUserDismissedValues(int[] values, bool dismissed)
        {
            for (int i = 0; i < values.Length; i++)
            {
                _userDismissedValues[values[i] - 1] = dismissed;
            }
            //OnPropertyChanged("UserNonDismissedValues");
        }
        public void UpdateDismissedValue(int value, bool dismissed)
        {
            UpdateDismissedValues(new int[] { value }, dismissed);
        }
        public void UpdateDismissedValues(int[] values, bool dismissed)
        {
            for (int i = 0; i < values.Length; i++)
            {
                _dismissedValues[values[i] - 1] = dismissed;
            }
            OnPropertyChanged("NonDismissedValues");
        }

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        void ClearCache()
        {
            _correct = null;
        }

        void _userDismissedValues_Changed(object sender, MemberChangedEventArgs e)
        {
            OnPropertyChanged("UserNonDismissedValues");
        }
        void _userValue_Changed(object sender, MemberChangedEventArgs e)
        {
            OnPropertyChanged("UserValue");

            ClearCache();
            OnPropertyChanged("Correct");
        }
    }
}
