using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace YetAnotherSudokuPlayer.Components
{
    public static class Utils
    {
        public static Point CalculatePoint(int superCell, int position)
        {
            return CalculatePoint(superCell, position - Convert.ToInt32(position / 3) * 3, Convert.ToInt32(position / 3));
        }
        public static Point CalculatePoint(int superCell, int x, int y)
        {
            return new Point((superCell - (Convert.ToInt32(superCell / 3) * 3)) * 3 + x,
                Convert.ToInt32(superCell / 3) * 3 + y);
        }
        public static int GetSuperCell(Point position)
        {
            return Convert.ToInt32(position.X / 3) + Convert.ToInt32(position.Y / 3) * 3 + 1;
        }
    }
}
