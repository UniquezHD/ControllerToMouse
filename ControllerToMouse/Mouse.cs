using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ControllerToMouse
{
    public static class Mouse
    {
        private const float MOUSE_SMOOTH = 5f;

        public static void MoveTo(int targetX, int targetY)
        {
            var targetPosition = new Point(targetX, targetY);
            var curPos = Cursor.Position;

            var diffX = targetPosition.X - curPos.X;
            var diffY = targetPosition.Y - curPos.Y;

            for (int i = 0; i <= MOUSE_SMOOTH; i++)
            {
                float x = curPos.X + (diffX / MOUSE_SMOOTH * i);
                float y = curPos.Y + (diffY / MOUSE_SMOOTH * i);
                Cursor.Position = new Point((int)x, (int)y);
                Thread.Sleep(1);
            }

            if (Cursor.Position != targetPosition)
            {
                MoveTo(targetPosition.X, targetPosition.Y);
            }
        }
    }

}
