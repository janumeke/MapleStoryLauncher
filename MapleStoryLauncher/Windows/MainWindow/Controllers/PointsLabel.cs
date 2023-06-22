using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapleStoryLauncher
{
    public partial class MainWindow : Form
    {
        public void Controller_PointsLabel()
        {
            this.Load += (sender, _) =>
            {
                this.Tip.SetToolTip(pointsLabel, "雙擊更新點數");
            };

            void UpdatePoints()
            {
                int rightX = pointsLabel.Location.X + pointsLabel.Width;

                int points = beanfun.GetRemainingPoints();
                if (points < 0)
                    pointsLabel.Text = "-- 點";
                else
                    pointsLabel.Text = $"{points} 點";

                pointsLabel.Location = new Point(rightX - pointsLabel.Width, pointsLabel.Location.Y);
            }

            SyncEvents.LoggedIn_Loading += username =>
            {
                UpdatePoints();
            };

            pointsLabel.DoubleClick += (sender, _) =>
            {
                UpdatePoints();
            };
        }
    }
}
