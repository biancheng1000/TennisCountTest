using RigourTech;
using RigourTech.Service;
using RigourTech.Tennisball.Public;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace DataProviderService
{
    public class EventAnaliyServiceEmp: TennisDataAnaliy.Iface
    {
        public enum Runmode
        {
            Match,
            Train,
            WarmUp
        }

        #region 属性
        RigourTech.Service.Match cmatch;
        Train ctrain;
        Runmode rmode;
        string currentSpeed;
        TennisCourt _court;

        public RigourTech.Service.Match Cmatch
        {
            get
            {
                return cmatch;
            }

            set
            {
                cmatch = value;
            }
        }

        public Train Ctrain
        {
            get
            {
                return ctrain;
            }

            set
            {
                ctrain = value;
            }
        }

        /// <summary>
        /// 当前服务的运行模式：m 比赛 t 训练 w 热身
        /// </summary>
        public Runmode Rmode
        {
            get
            {
                return rmode;
            }

            set
            {
                rmode = value;
            }
        }

        public string CurrentSpeed
        {
            get
            {
                return currentSpeed;
            }

            set
            {
                currentSpeed = value;
            }
        }

        public TennisCourt Court
        {
            get
            {
                return _court;
            }

            set
            {
                _court = value;
            }
        }

        #endregion

        #region Iface
        /// <summary>
        /// 开始比赛
        /// </summary>
        /// <param name="match"></param>
        public bool StartMatch(TMatch match)
        {
            rmode = Runmode.Match;
            if (Cmatch == null || Cmatch.Id != match.MatchID)
            {
                Cmatch = new RigourTech.Service.Match(match);
                LogText.Info("StartMatch", "*******************************************开始新比赛************************************");
            }
            else
            {
                LogText.Info("StartMatch", "*******************************************回复比赛************************************");
            }
      
           
            return true;
        }

        public bool StartTrain(TTrain tain)
        {
            rmode = Runmode.Train;
            Ctrain = new Train(tain);
            LogText.Info("StartTrain", "开始训练");
            return true;
        }

        /// <summary>
        /// 结束比赛
        /// </summary>
        /// <param name="match"></param>
        public bool EndMatch(string matchID)
        {
            //结束最后一个回合
            Cmatch.CompleteRand();

            //保存运动员的轨迹
            Cmatch.SavePlayersTrack();

            //保存人员的轨迹信息
            LogText.Info("EndMatch", "结束比赛");
            return true;
        }

        /// <summary>
        /// 结束训练
        /// </summary>
        /// <param name="trian"></param>
        public bool EndTrain(string trianID)
        {
            Ctrain.SavePlayersTrack();
            Ctrain = null;
            //保存训练人员的轨迹信息
            LogText.Info("EndTrain", "训练结束");
            return true;
        }
        /// <summary>
        /// 交换产地
        /// </summary>
        public bool ChangeGroud()
        {
            if (rmode == Runmode.Match)
            {
                cmatch.ExChangeGround();
            }
            else if (rmode == Runmode.Train)
            {
                Ctrain.ExChangeGround();
            }
            LogText.Info("ChangeGroud", "交换场地");
            return true;
        }

        /// <summary>
        /// 接收来自底层的数据
        /// </summary>
        /// <param name="path"></param>
        public bool InsertPathToDB(string path)
        {
           // SaveDataToFile(path);
            if (path.StartsWith("0|"))
            {
                MatchPerson(path);
            }
            else if (path.StartsWith("1|"))
            {
                Track t = Track.Parse(path);
                if (Rmode == Runmode.Match)
                {
                    Cmatch?.AddNewTrack(t);
                }
                else
                {
                    Ctrain?.AddNewTrack(t);
                }
            }
            else
            {
                matchSpeed(path);
            }
            return true;
        }
        /// <summary>
        /// 缓存数据
        /// </summary>
        /// <param name="path"></param>
        private void SaveDataToFile(string strvalue)
        {
            string path = @"E:\网球\03网球鹰眼顶级赛事版\02doc\00-项目文档\NewLog.txt";
            StreamWriter writer = new StreamWriter(path,true);
            writer.WriteLine(strvalue);
            writer.Flush();
            writer.Close();
        }

        #endregion

        #region 接收到底层坐标信息进行事件分析
        /// <summary>
        /// 匹配人员坐标
        /// </summary>
        /// <param name="SubjectString"></param>
        private void MatchPerson(string SubjectString)
        {
            try
            {
                string strPosition = Regex.Match(SubjectString, "\\|[0-9]*\\|-?[0-9]*\\.[0-9]*,-?[0-9]*\\.[0-9]*,-?[0-9]*\\.[0-9]*").Value;
                if (!string.IsNullOrEmpty(strPosition))
                {
                    FramePosition position = FramePosition.Parse(strPosition.Substring(1,strPosition.LastIndexOf("|")-1),strPosition.Substring(strPosition.LastIndexOf("|")+1));
                    if (position != null)
                    {
                        if (Rmode == Runmode.Match)
                        {
                            Cmatch?.AddPosition(position);
                        }
                        else if(rmode==Runmode.Train)
                        {
                            Ctrain?.AddPosition(position);
                        }
                    }
                }
            }
            catch (ArgumentException ex)
            {
                LogText.Error("MatchPerson",ex.Message);
            }
        }


        /// <summary>
        /// 匹配数据
        /// </summary>
        /// <param name="SubjectString"></param>
        private void matchSpeed(string SubjectString)
        {
            try
            {
                string[] vls = SubjectString.Split('|');
                if (vls.Length == 3)
                {
                    currentSpeed = vls[2];
                    string de = vls[1];
                }
                //currentSpeed = Regex.Match(SubjectString, "[0-9]*\\.[0-9]*").Value;
               // Console.WriteLine("实时速度:"+ currentSpeed);
            }
            catch (ArgumentException ex)
            {
                // Syntax error in the regular expression
            }
        }

        /// <summary>
        /// 匹配轨迹数据
        /// </summary>
        /// <param name="subjectString"></param>
        private void matchTrack(string subjectString)
        {
            if (Rmode == Runmode.Match)
            {
                
            }
            else if (rmode == Runmode.Train)
            {
               
            }
        }


        #endregion

    }

    
}
