using RigourTech;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
namespace TennisCountTest
{
    public class SimulationViewModel:baseVm,IChangeViewSizeCmd
    {
        TennisCount court;
        /// <summary>
        /// 更新窗口尺寸
        /// </summary>
        public baseCmd ChangeViewSizeCmd
        {
            get { return new ChangeWindowSizeCmd(this); }
        }

        public TennisCount Court
        {
            get
            {
                if (court == null)
                {
                    court = new TennisCount();
                }
                return court;
            }
        }

        /// <summary>
        /// 当前人的坐标
        /// </summary>
        public Point3D Person
        {
            get
            {
                return person;
            }

            set
            {
                person = value;
            }
        }

        /// <summary>
        /// 所有的轨迹
        /// </summary>
        public ObservableCollection<Geometry> Tracks
        {
            get
            {
                return tracks;
            }

            set
            {
                tracks = value;
            }
        }

        Point3D person;

        ObservableCollection<Geometry> tracks = new ObservableCollection<Geometry>();
       

        public void ChangeTransform(double height, double width)
        {
            court.ViewHeight = height;
            court.ViewWidth = width;
            court.ChangeView();
        }
    }
}
