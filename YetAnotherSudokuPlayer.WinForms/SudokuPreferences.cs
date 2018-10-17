using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.Xml.Serialization;
using YetAnotherSudokuPlayer.Components;

namespace YetAnotherSudokuPlayer.WinForms
{
    public class SudokuPreferences
    {
        [XmlIgnore]
        [Category("Appearance")]
        [Description("The color of the square which is selected.")]
        public Color SelectedColor { get; set; }
        [XmlIgnore]
        [Category("Appearance")]
        [Description("The color of a square when the mouse moves over it.")]
        public Color HotTrackingColor { get; set; }
        [XmlIgnore]
        [Category("Appearance")]
        [Description("The color of a square when an invalid value is entered.")]
        public Color ErrorColor { get; set; }
        [XmlIgnore]
        [Category("Appearance")]
        [Description("The color to use when highlighting related squares.")]
        public Color RelatedSquareColor { get; set; }
        [Category("Appearance")]
        [Description("Set to 'True' to show the move history of the game.")]
        public bool ShowMoveHistory { get; set; }
        [Category("Behavior")]
        [Description("When saving a game, if this value is set to 'True', the saved game will include the user-dismissed values.")]
        public bool IncludeDismissedValues { get; set; }
        [Category("Behavior")]
        [Description("Indicates when dismissed value(s) will be shown on the board.")]
        public DismissedValueVisibility DismissedValueVisibility { get; set; }
        [Category("Behavior")]
        [Description("The difficulty of generated games.")]
        public GameDifficulty GameDifficulty { get; set; }


        [Browsable(false)]
        public string SelectedColorValue
        {
            get { return ColorTranslator.ToHtml(SelectedColor); }
            set {SelectedColor = ColorTranslator.FromHtml(value);}
        }
        [Browsable(false)]
        public string HotTrackingColorValue
        {
            get { return ColorTranslator.ToHtml(HotTrackingColor); }
            set { HotTrackingColor = ColorTranslator.FromHtml(value); }
        }
        [Browsable(false)]
        public string ErrorColorValue
        {
            get { return ColorTranslator.ToHtml(ErrorColor); }
            set { ErrorColor = ColorTranslator.FromHtml(value); }
        }
        [Browsable(false)]
        public string RelatedSquareColorValue
        {
            get { return ColorTranslator.ToHtml(RelatedSquareColor); }
            set { RelatedSquareColor = ColorTranslator.FromHtml(value); }
        }
    }
}
