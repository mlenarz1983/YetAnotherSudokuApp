using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using YetAnotherSudokuPlayer.Components;
using System.IO;
using DejaVu;
using System.Diagnostics;

namespace YetAnotherSudokuPlayer.WinForms
{
    public partial class MainForm : Form
    {
        SudokuBoard _board = new SudokuBoard();
        Dictionary<Point, SudokuSquareControl> _squareLookup = new Dictionary<Point, SudokuSquareControl>();
        Point _selectedSquare = new Point(0, 0);
   
        public MainForm()
        {
            InitializeComponent();

            _board.PropertyChanged += new PropertyChangedEventHandler(_board_PropertyChanged);
            UndoRedoManager.CommandDone += new EventHandler<CommandDoneEventArgs>(UndoRedoManager_CommandDone);
        }

        private void RefreshBoard()
        {
            SudokuSquareControl selectedSquare = _squareLookup[_selectedSquare];

            foreach (Point position in _squareLookup.Keys)
            {
                SudokuSquareControl curr = _squareLookup[position];

                if (!curr.SudokuSquare.UserValue.HasValue && (curr != selectedSquare))
                {
                    if (Program.Preferences.DismissedValueVisibility == DismissedValueVisibility.Always)
                    {
                        curr.DisplayMode = SquareDisplayMode.DismissedValues;
                    }
                    else if (Program.Preferences.DismissedValueVisibility == DismissedValueVisibility.Never)
                    {
                        curr.DisplayMode = SquareDisplayMode.Normal;
                    }
                    else if (Program.Preferences.DismissedValueVisibility == DismissedValueVisibility.AroundSelectedSquare)
                    {
                        //show non-dismissed values for relevant squares w/o a value alrady set
                        if ((curr.SudokuSquare.Position.X == selectedSquare.SudokuSquare.Position.X)
                            || (curr.SudokuSquare.Position.Y == selectedSquare.SudokuSquare.Position.Y)
                            || (YetAnotherSudokuPlayer.Components.Utils.GetSuperCell(curr.SudokuSquare.Position)
                                == YetAnotherSudokuPlayer.Components.Utils.GetSuperCell(selectedSquare.SudokuSquare.Position)))
                        {
                            curr.DisplayMode = SquareDisplayMode.DismissedValues;
                        }
                        else
                        {
                            curr.DisplayMode = SquareDisplayMode.Normal;
                        }
                    }
                }
                else
                {
                    curr.DisplayMode = SquareDisplayMode.Normal;
                }

                //_squareLookup[position].Refresh();
            }
        }
        private void RefreshGameHistory()
        {
            int moveNum = 1;
            lvUndo.Items.Clear();
            for (int j = UndoRedoManager.UndoCommands.Count() - 1; j >= 0; j--)
            {
                string moveName = UndoRedoManager.UndoCommands.ToList()[j];
                lvUndo.Items.Add(new ListViewItem(new string[] { moveNum.ToString(), moveName }) { Tag = UndoRedoManager.UndoCommands.ToList().IndexOf(moveName) });
                moveNum++;
            }
            lvRedo.Items.Clear();
            foreach (string str in UndoRedoManager.RedoCommands)
            {
                lvRedo.Items.Add(new ListViewItem(new string[] { moveNum.ToString(), str }) { Tag = UndoRedoManager.RedoCommands.ToList().IndexOf(str) });
                moveNum++;
            }
        }

        void MainForm_Load(object sender, EventArgs e)
        {
            lblStatus.Text = "";
            lblStatus.Invalidate();
            lblGameDifficulty.Text = "";
            lblGameDifficulty.Invalidate();
            lblGameComments.Text = "";
            lblGameComments.Invalidate();
            undoToolStripMenuItem.Enabled = UndoRedoManager.CanUndo;
            redoToolStripMenuItem.Enabled = UndoRedoManager.CanRedo;
            toolStripProgressBar1.Visible = false;

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    TableLayoutPanel subPanel = (TableLayoutPanel)tableLayoutPanelMain.GetControlFromPosition(i, j);
                    for (int subi = 0; subi < 3; subi++)
                    {
                        for (int subj = 0; subj < 3; subj++)
                        {
                            SudokuSquareControl ctl = new SudokuSquareControl();
                            ctl.Margin = new Padding(0);
                            ctl.Dock = DockStyle.Fill;
                            subPanel.Controls.Add(ctl, subi, subj);
                            Point position = new Point(i * 3 + subi, j * 3 + subj);
                            ctl.PropertyChanged += new PropertyChangedEventHandler(ctl_PropertyChanged);
                            _squareLookup.Add(position, ctl);
                            _squareLookup[position].SudokuSquare = _board.GetSquare(position);
                        }
                    }
                }
            }

            RefreshBoard();

            _squareLookup[_selectedSquare].Focus();

            splitContainer1.Panel2Collapsed = !Program.Preferences.ShowMoveHistory;
        }
        void ctl_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SudokuSquareControl ctl = (SudokuSquareControl)sender;
            if (e.PropertyName == "Selected")
            {
                if (ctl.SelectionStatus == SquareSelectionStatus.Selected)
                {
                    _selectedSquare = ctl.SudokuSquare.Position;
                    ctl.DisplayMode = SquareDisplayMode.Normal; //don't show dismissed values on the focused square


                    lblStatus.Text = ctl.SudokuSquare.Position.ToString();

                    foreach (Point position in _squareLookup.Keys)
                    {
                        SudokuSquareControl curr = _squareLookup[position];
                        if (curr != ctl)
                        {
                            if (ctl.SudokuSquare.UserValue.HasValue && curr.SudokuSquare.UserValue == ctl.SudokuSquare.UserValue)
                            {
                                curr.SelectionStatus = SquareSelectionStatus.Highlighted;
                            }
                            else
                            {
                                curr.SelectionStatus = SquareSelectionStatus.Normal;
                            }
                        }
                    }

                    RefreshBoard();
                }
            }
        }
        void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoRedoManager.ClearHistory();
            using (UndoRedoManager.Start("New Game"))
            {
                _board.NewGame(Program.Preferences.GameDifficulty);
                UndoRedoManager.Commit();
            }
            RefreshBoard();
        }
        void solveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            _board.ShowSolution();
            Cursor.Current = Cursors.Default;
        }
        void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        void _board_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SolutionProgress")
            {
                Invoke(new DoMethod(delegate
                {
                    toolStripProgressBar1.Value = _board.SolutionProgress;
                    toolStripProgressBar1.Visible = (_board.SolutionProgress > 0 && _board.SolutionProgress < 100);
                }));
                Application.DoEvents();
            }
            else if (e.PropertyName == "Difficulty")
            {
                Invoke(new DoMethod(delegate
                {
                    if (_board.Difficulty.HasValue)
                    {
                        GameDifficulty category = GameDifficulty.VeryEasy;
                        if (_board.Difficulty < 2.0)
                            category = GameDifficulty.VeryEasy;
                        else if ((_board.Difficulty >= 2.0) && (_board.Difficulty < 4.0))
                            category = GameDifficulty.Easy;
                        else if ((_board.Difficulty >= 4.0) && (_board.Difficulty < 6.0))
                            category = GameDifficulty.Medium;
                        else if ((_board.Difficulty >= 6.0) && (_board.Difficulty < 8.0))
                            category = GameDifficulty.Hard;
                        else if ((_board.Difficulty >= 8.0))
                            category = GameDifficulty.VeryHard;
                        lblGameDifficulty.Text = string.Format("Difficulty: {0:F1} ({1})",
                            _board.Difficulty, category);
                    }
                    else
                    {
                        lblGameDifficulty.Text = "";
                    }
                }));
                Application.DoEvents();
            }
            else if (e.PropertyName == "Message")
            {
                Invoke(new DoMethod(delegate
                {
                    lblGameComments.Text = _board.Message;
                }));
                Application.DoEvents();
            }
        }
        void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                UndoRedoManager.ClearHistory();
                using (UndoRedoManager.Start(string.Format("Loaded Game: {0}", Path.GetFileNameWithoutExtension(openFileDialog1.FileName))))
                {
                    _board.ClearSquares(); //clear prev. game
                    _board.LoadGame(File.ReadAllText(openFileDialog1.FileName));
                    UndoRedoManager.Commit();
                }

                RefreshBoard();

                _board.SolveAsync(false); //we don't have the solution yet, so figure it out in the background
            }
        }
        void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(saveFileDialog1.FileName, _board.SaveGame(true, GameLoadOptions.UserValues, Program.Preferences.IncludeDismissedValues));
            }
        }
        void optionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OptionsForm options = new OptionsForm();
            options.Options = Program.Preferences;
            options.ShowDialog();

            RefreshBoard();

            splitContainer1.Panel2Collapsed = !Program.Preferences.ShowMoveHistory;
        }
        void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (_selectedSquare.Y < 8)
                    _selectedSquare.Y++;
                else
                    _selectedSquare.Y = 0;
            }
            else if (e.KeyCode == Keys.Up)
            {
                if (_selectedSquare.Y > 0)
                    _selectedSquare.Y--;
                else
                    _selectedSquare.Y = 8;
            }
            else if (e.KeyCode == Keys.Left)
            {
                if (_selectedSquare.X > 0)
                    _selectedSquare.X--;
                else
                    _selectedSquare.X = 8;
            }
            else if (e.KeyCode == Keys.Right)
            {
                if (_selectedSquare.X < 8)
                    _selectedSquare.X++;
                else
                    _selectedSquare.X = 0;
            }
            else
            {
                return;
            }

            //_squareLookup[_selectedSquare].SelectionStatus = SquareSelectionStatus.Selected;
            bool b = _squareLookup[_selectedSquare].Focus();
            e.Handled = true;
        }
        void UndoRedoManager_CommandDone(object sender, CommandDoneEventArgs e)
        {
            undoToolStripMenuItem.Enabled = UndoRedoManager.CanUndo;
            redoToolStripMenuItem.Enabled = UndoRedoManager.CanRedo;

            RefreshGameHistory();
        }
        void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoRedoManager.Undo();
        }
        void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoRedoManager.Redo();
        }
        void lvUndo_ItemActivate(object sender, EventArgs e)
        {
            if (lvUndo.SelectedItems.Count > 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                int movesToUndo = Convert.ToInt32(lvUndo.SelectedItems[0].Tag);
                for (int i = 0; i < movesToUndo; i++)
                    UndoRedoManager.Undo();
                Cursor.Current = Cursors.Default;
            }
        }
        void lvRedo_ItemActivate(object sender, EventArgs e)
        {
            if (lvRedo.SelectedItems.Count > 0)
            {
                Cursor.Current = Cursors.WaitCursor;
                int movesToRedo = Convert.ToInt32(lvRedo.SelectedItems[0].Tag);
                for (int i = 0; i < movesToRedo; i++)
                    UndoRedoManager.Redo();
                Cursor.Current = Cursors.Default;
            }
        }
        void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UndoRedoManager.ClearHistory();
            RefreshGameHistory();
            using (UndoRedoManager.Start("Clear Game"))
            {
                _board.ClearSquares();
                UndoRedoManager.Commit();
            }
            RefreshBoard();
        }
    }

    public delegate void DoMethod();
}
