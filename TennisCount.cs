
using RigourTech;
using RigourTech.Tennisball;
using RigourTech.Tennisball.Public;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RigourTech
{
    /// <summary>
    /// 球场 1mm=3.780 WPF单位
    /// 默认球场的单位是毫米
    /// </summary>
    public class TennisCount:baseVm
    {
        public TennisCount()
        {
            readConfigure();

          //  InitStadimOutLook();
        }
        /// <summary>
        /// 通过配置文件读取场地信息
        /// </summary>
        private void readConfigure()
        {
            TennisCountWidth = GetConfig("TennisCountWidth") * GlobalParam.unit;
            TennisCountHeight = GetConfig("TennisCountLenght") * GlobalParam.unit;

            StandardLine = GetConfig("StandardLine") * GlobalParam.unit;
            ServeLine = GetConfig("ServeLine") * GlobalParam.unit;
            SingleBorderLine = GetConfig("SigleBorderLine") * GlobalParam.unit;
            DoubleBorderLine = GetConfig("Std_DoorLenght") * GlobalParam.unit;
        }

        Dictionary<int, Rect> allMarkRect = new Dictionary<int, Rect>();

      
        /// <summary>
        /// 获得场地参数信息，只能读取double类型的值配置
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private double GetConfig(string key)
        {
            try
            {
                string value = System.Configuration.ConfigurationManager.AppSettings[key];
                if (!string.IsNullOrEmpty(value))
                {
                    double dvalue;
                    if (double.TryParse(value, out dvalue))
                    {
                        return dvalue;
                    }
                }
            }
            catch(Exception ex)
            {
                LogText.Error("GetConfig", "读取配置文件出错：" + ex.Message);
            }
            return 0;
       
        }
       
        public void ReinitTs()
        {
            double factor = Math.Min(ViewHeight /TennisCountHeight, ViewWidth / TennisCountWidth);
            Matrix moffset = new Matrix();
            moffset.OffsetX = -TennisCountWidth / 2;
            moffset.OffsetY = -TennisCountHeight / 2;

            Matrix mscale = new Matrix();
            mscale.M22 = -factor;
            mscale.M11 = factor;

            Matrix mviewOff = new Matrix();
            mviewOff.OffsetX = ViewWidth / 2;
            mviewOff.OffsetY = ViewHeight / 2;

            Matrix resultMatrix = moffset * mscale * mviewOff;
            MatrixTransform nts = new MatrixTransform(resultMatrix);
            Ts = nts;

        }


        /// <summary>
        /// 场地的外观
        /// 根据场地的参数动态显示球场的外观
        /// </summary>
        public Geometry StadiumOutLook
        {
            get
            {
                return stadiumOutLook;
            }
        }

        /// <summary>
        /// 根据场地的类型，初始化场地
        /// </summary>
        private void InitStadimOutLook()
        {
            //GeometryGroup root = new GeometryGroup();
            GeometryGroup group = new GeometryGroup();


            //边框
            RectangleGeometry border = new RectangleGeometry(new System.Windows.Rect(basePoint.X, basePoint.Y, TennisCountWidth, TennisCountHeight));
            group.Children.Add(border);


            Vector vx = new Vector(basePoint.X + TennisCountWidth, basePoint.Y);

            Vector vy = new Vector(basePoint.X, basePoint.Y + TennisCountHeight);

            //基准线 ，球网所在的线
            LineGeometry standardLine = new LineGeometry(
                basePoint.Point2D + (vy / 2), basePoint.Point2D + (vy / 2) + vx);

            //左侧单打边线
            Vector vleftSingleBorder = vx / 2;
            vleftSingleBorder.X -= SingleBorderLine;
            LineGeometry leftSignalBorderGeo = new LineGeometry(
                 basePoint.Point2D + vleftSingleBorder, basePoint.Point2D + vleftSingleBorder + vy);

            //右侧单打边线
            Vector vrightSingleBorder = vx / 2;
            vrightSingleBorder.X += SingleBorderLine;
            LineGeometry rightSignalBorderGeo = new LineGeometry(
               basePoint.Point2D + vrightSingleBorder, basePoint.Point2D + vrightSingleBorder + vy);

            Vector vupserv = vy / 2;
            vupserv.Y += ServeLine;
            //上发球线
            LineGeometry upServeLine = new LineGeometry(
             basePoint.Point2D + vleftSingleBorder + vupserv, basePoint.Point2D + vrightSingleBorder + vupserv);

            //下发球线
            Vector vdownserv = vy / 2;
            vdownserv.Y -= ServeLine;
            LineGeometry downServeLine = new LineGeometry(
           basePoint.Point2D + vleftSingleBorder + vdownserv, basePoint.Point2D + vrightSingleBorder + vdownserv);

            //中线
            LineGeometry middleLine = new LineGeometry(
            basePoint.Point2D + vupserv + vx / 2, basePoint.Point2D + vdownserv + vx / 2);

            group.Children.Add(middleLine);
            group.Children.Add(standardLine);
            group.Children.Add(downServeLine);
            group.Children.Add(upServeLine);
            group.Children.Add(leftSignalBorderGeo);
            group.Children.Add(rightSignalBorderGeo);

            group.Transform = ts;
            stadiumOutLook = group;
        }
        /// <summary>
        /// 对网球场地进行标定
        /// </summary>
        private void InitMarkRects()
        {
            Point bp = basePoint.Point2D;

            Vector vx = new Vector(bp.X + TennisCountWidth, bp.Y);

            Vector vy = new Vector(bp.X, bp.Y + TennisCountHeight);

            
            Vector vserve = new Vector(0, ServeLine);

            Vector vleftSingleBorder = vx / 2;
            vleftSingleBorder.X -= SingleBorderLine;

            Vector vrightSingleBorder = vx / 2;
            vrightSingleBorder.X += SingleBorderLine;

            //区域1
            AllMarkRect.Add(1, new Rect(bp + vy / 2, bp + vleftSingleBorder + vy));

            //区域2
            allMarkRect.Add(2, new Rect(bp + vleftSingleBorder+vy/2+vserve, bp + vrightSingleBorder + vy));

            //区域3
          
            allMarkRect.Add(3, new Rect(bp + vleftSingleBorder + vy/2, bp+vx/2+vy/2+vserve));
            //区域4
            allMarkRect.Add(4, new Rect(bp+vx/2+vy/2,bp+vrightSingleBorder+vy/2+vserve));
            //区域5
            allMarkRect.Add(5, new Rect(bp + vrightSingleBorder + vy / 2, bp + vy + vx));
            //区域6
            allMarkRect.Add(6, new Rect(bp, bp + vleftSingleBorder + vy / 2));
            //区域7
            allMarkRect.Add(7, new Rect(bp + vleftSingleBorder, bp + vrightSingleBorder + vy / 2 - vserve));
            //区域8
            allMarkRect.Add(8, new Rect(bp + vleftSingleBorder + vy / 2 - vserve, bp + vy / 2 + vx / 2));
            //区域9
            allMarkRect.Add(9, new Rect(bp + vx / 2 + vy / 2 - vserve, bp + vrightSingleBorder + vy / 2));
            //区域10
            allMarkRect.Add(10, new Rect(bp + vrightSingleBorder, bp + vx + vy / 2));
        }

       

        /// <summary>
        /// 场地中所有的线的颜色
        /// </summary>
        public Brush LineBrush
        {
            get
            {
                return Brushes.White;
            }
        }
        /// <summary>
        /// 场地中划线的粗细，默认2
        /// </summary>
        public int LineStrokeThinckness
        {
            get
            {
                return lineStrokeness;
            }

            set
            {
                lineStrokeness = value;
            }
        }

        //完全由外面控制
        /// <summary>
        /// 从实际场地到视图上的投影转换
        /// 默认实际坐标和桌面坐标一致，原点坐标一致，场地实际长宽比和显示长宽比例一致
        /// 所以只需要计算缩放投影
        /// 这里需要进行矩阵运算，将Y轴坐标方向取反
        /// </summary>
        public Transform Ts
        {
            get
            {
                return ts;
            }
            set
            {
                ts = value;
            }
        }

        /// <summary>
        /// 窗口尺寸发生变化
        /// </summary>
        /// <param name="viewHeight"></param>
        /// <param name="viewWidth"></param>
        public void ChangeView()
        {
            double factor = Math.Min(ViewHeight / TennisCountHeight, ViewWidth / TennisCountWidth);
            Matrix moffset = new Matrix();
            moffset.OffsetX = -TennisCountWidth / 2;
            moffset.OffsetY = -TennisCountHeight / 2;

            Matrix mscale = new Matrix();
            mscale.M22 = -factor;
            mscale.M11 = factor;

            Matrix mviewOff = new Matrix();
            mviewOff.OffsetX = ViewWidth / 2;
            mviewOff.OffsetY = ViewHeight / 2;

            Matrix resultMatrix = moffset * mscale * mviewOff;
            MatrixTransform ts = new MatrixTransform(resultMatrix);

            Ts = ts;
            InitStadimOutLook();
            OnProperyChanged("StadiumOutLook");
        }

      

        public double TennisCountWidth { get;  set; }
        public double TennisCountHeight { get;  set; }
        public double StandardLine { get;  set; }
        public double ServeLine { get;  set; }
        public double SingleBorderLine { get;  set; }
        public double DoubleBorderLine { get;  set; }
        public double ViewHeight { get;  set; }
        public double ViewWidth { get;  set; }

        public Dictionary<int, Rect> AllMarkRect
        {
            get
            {
                return allMarkRect;
            }

            set
            {
                allMarkRect = value;
            }
        }

        public Brush HightLightBrush
        {
            get
            {
                return hightLightBrush;
            }

            set
            {
                hightLightBrush = value;
            }
        }

        public Geometry HightGeo
        {
            get
            {
                return hightGeo;
            }

            set
            {
                hightGeo = value;
            }
        }

        public bool IsHightLightGeo
        {
            get
            {
                return isHightLightGeo;
            }

            set
            {
                isHightLightGeo = value;
            }
        }

        public Rect SideA
        {
            get
            {
                return new Rect(basePoint.Point2D, new Vector(TennisCountWidth / 2,TennisCountHeight / 2));
            }
        }

        public Rect SideB
        {
            get
            {
                return new Rect(basePoint.Point2D + new Vector(0, TennisCountHeight / 2), new Vector(TennisCountWidth / 2, TennisCountHeight / 2));
            }
        }

        Transform ts;
        private Point3D basePoint = new Point3D(0, 0, 0);
        private int lineStrokeness=1;
        private Geometry stadiumOutLook;

        Brush hightLightBrush;
        Geometry hightGeo;
        bool isHightLightGeo;
    }

}
