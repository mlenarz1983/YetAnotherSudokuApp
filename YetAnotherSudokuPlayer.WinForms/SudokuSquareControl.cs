using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YetAnotherSudokuPlayer.Components;
using YetAnotherSudokuPlayer.WinForms.Properties;
using DejaVu;

namespace YetAnotherSudokuPlayer.WinForms
{
    public partial class SudokuSquareControl : UserControl, INotifyPropertyChanged
    {
        SudokuSquare _obj;
        SquareSelectionStatus _selectionStatus = SquareSelectionStatus.Normal;
        SquareDisplayMode _displayMode = SquareDisplayMode.Normal;
        Dictionary<int, Label> _dismissedLabelLookup = new Dictionary<int, Label>();

        public SudokuSquareControl()
        {
            InitializeComponent();

            _dismissedLabelLookup.Add(1, lblDismiss1);
            _dismissedLabelLookup.Add(2, lblDismiss2);
            _dismissedLabelLookup.Add(3, lblDismiss3);
            _dismissedLabelLookup.Add(4, lblDismiss4);
            _dismissedLabelLookup.Add(5, lblDismiss5);
            _dismissedLabelLookup.Add(6, lblDismiss6);
            _dismissedLabelLookup.Add(7, lblDismiss7);
            _dismissedLabelLookup.Add(8, lblDismiss8);
            _dismissedLabelLookup.Add(9, lblDismiss9);           
        }

        public SudokuSquare SudokuSquare
        {
            get { return _obj; }
            set
            {
                _obj = value;
                _obj.PropertyChanged += new PropertyChangedEventHandler(_obj_PropertyChanged);
                lblSquareValue.Text = _obj.UserValue.ToString();
                RefreshUserNonDismissed();
            }
        }
        public SquareSelectionStatus SelectionStatus
        {
            get { return _selectionStatus; }
            set
            {
                bool changed = (_selectionStatus != value);
                _selectionStatus = value;
                if (changed)
                {
                    OnPropertyChanged("Selected");
                    this.Invalidate();
                }
            }
        }
        public SquareDisplayMode DisplayMode
        {
            get { return _displayMode; }
            set
            {
                bool changed = (_displayMode != value);
                _displayMode = value;
                if (changed)
                {
                    OnPropertyChanged("DisplayMode");
                    OnDisplayModeChanged();
                }
            }
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            int? newValue = null;

            if (e.KeyCode == Keys.D1)
                newValue = 1;
            else if (e.KeyCode == Keys.D2)
                newValue = 2;
            else if (e.KeyCode == Keys.D3)
                newValue = 3;
            else if (e.KeyCode == Keys.D4)
                newValue = 4;
            else if (e.KeyCode == Keys.D5)
                newValue = 5;
            else if (e.KeyCode == Keys.D6)
                newValue = 6;
            else if (e.KeyCode == Keys.D7)
                newValue = 7;
            else if (e.KeyCode == Keys.D8)
                newValue = 8;
            else if (e.KeyCode == Keys.D9)
                newValue = 9;
            else if (e.KeyCode == Keys.Delete)
                newValue = null;
            else
                return;

            if (_obj.UserValue != newValue)
            {
                using (UndoRedoManager.Start(string.Format("Square {0}: {1}", _obj.Position, newValue)))
                {
                    _obj.UserValue = newValue;

                    UndoRedoManager.Commit();
                }
            }
        }
        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);

            SelectionStatus = SquareSelectionStatus.Selected;
        }
        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);

            SelectionStatus = SquareSelectionStatus.Normal;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_obj.Correct)
            {
                switch (_selectionStatus)
                {
                    case SquareSelectionStatus.Normal:
                        this.BackColor = Color.Transparent;
                        break;
                    case SquareSelectionStatus.HotTracked:
                        this.BackColor = Program.Preferences.HotTrackingColor;
                        break;
                    case SquareSelectionStatus.Selected:
                        this.BackColor = Program.Preferences.SelectedColor;
                        break;
                    case SquareSelectionStatus.Highlighted:
                        this.BackColor = Program.Preferences.RelatedSquareColor;
                        break;
                }
            }
            else
            {
                this.BackColor = Program.Preferences.ErrorColor;
            }
        }
        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (_selectionStatus != SquareSelectionStatus.Selected)
                this.SelectionStatus = SquareSelectionStatus.HotTracked;
        }
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_selectionStatus != SquareSelectionStatus.Selected)
                this.SelectionStatus = SquareSelectionStatus.Normal;
        }
        protected override bool IsInputKey(Keys keyData)
        {
            bool ret = true;

            switch (keyData)
            {
                case Keys.Left:
                    break;
                case Keys.Right:
                    break;
                case Keys.Up:
                    break;
                case Keys.Down:
                    break;
                default:
                    ret = base.IsInputKey(keyData);
                    break;
            }
            return ret;
        }

        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        void OnDisplayModeChanged()
        {
            if (_displayMode == SquareDisplayMode.Normal)
            {
                tableLayoutPanel1.Hide();
                lblSquareValue.Show();
            }
            else
            {
                lblSquareValue.Hide();
                tableLayoutPanel1.Show();
            }
        }
        void RefreshUserNonDismissed()
        {
            List<int> nonDismissedValues = _obj.UserNonDismissedValues;
            for (int i = 1; i <= 9; i++)
            {
                if (nonDismissedValues.Contains(i))
                {
                    _dismissedLabelLookup[i].ForeColor = Color.Black;
                }
                else
                {
                    _dismissedLabelLookup[i].ForeColor = Color.Transparent;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void lblSquareValue_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e); //pass on to parent
        }
        private void lblSquareValue_MouseEnter(object sender, EventArgs e)
        {
            OnMouseEnter(e);//pass on to parent
        }
        private void lblSquareValue_MouseLeave(object sender, EventArgs e)
        {
            OnMouseLeave(e);//pass on to parent
        }
        void _obj_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "UserValue")
            {
                Invoke(new DoMethod(delegate
                {
                    lblSquareValue.Text = _obj.UserValue.ToString();
                    if (_obj.UserValue.HasValue)
                        DisplayMode = SquareDisplayMode.Normal;
                }));
                Application.DoEvents();
            }
            else if (e.PropertyName == "UserNonDismissedValues")
            {
                Invoke(new DoMethod(delegate
                {
                    RefreshUserNonDismissed();
                }));
                Application.DoEvents();
            }
            else if (e.PropertyName == "Correct")
            {
                this.Refresh();
            }
        }       
        private void SudokuSquareControl_Load(object sender, EventArgs e)
        {
            OnDisplayModeChanged();
        }
        private void lblDismiss_MouseDown(object sender, MouseEventArgs e)
        {
            Label lbl = sender as Label;
            int value = Convert.ToInt32(lbl.Text);

            bool dismissed = _obj.UserNonDismissedValues.Contains(value);

            using (UndoRedoManager.Start(string.Format("Square {0}: {1} {2}", _obj.Position, value, (dismissed ? "dismissed" : "included"))))
            {
                _obj.UpdateUserDismissedValue(value, dismissed); //toggle

                UndoRedoManager.Commit();
            }
            
            //OnMouseDown(e); //pass on to parent
        }
        private void lblDismiss_MouseEnter(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            //lbl.BorderStyle = BorderStyle.FixedSingle;
            lbl.BackColor = Program.Preferences.HotTrackingColor;

            //OnMouseEnter(e);//pass on to parent
        }
        private void lblDismiss_MouseLeave(object sender, EventArgs e)
        {
            Label lbl = sender as Label;
            //lbl.BorderStyle = BorderStyle.None;
            lbl.BackColor = Color.Transparent;

           //OnMouseLeave(e);//pass on to parent
        }
        private void lblDismiss_MouseHover(object sender, EventArgs e)
        {
            //Label lbl = sender as Label;
            //lbl.BorderStyle = BorderStyle.FixedSingle;
            //lbl.BackColor = Program.Preferences.HotTrackingColor;
        }
    }

    public enum SquareSelectionStatus
    {
        Normal, HotTracked, Selected, Highlighted
    }

    public enum SquareDisplayMode
    {
        Normal, DismissedValues
    }
}
