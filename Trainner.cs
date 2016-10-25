using RigourTech.Tennisball.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RigourTech.Service
{
    /// <summary>
    /// 训练人员
    /// </summary>
    public class Trainner:User
    {
        public Trainner(Trainer p):base(p.User)
        {
            trainName=p.TrainName;
            GetMarksFromString(p.TargetTouchDowns);
        }

        private void GetMarksFromString(string targetTouchDowns)
        {
            if (string.IsNullOrEmpty(targetTouchDowns))
            {
                LogText.Error("GetMarksFromString", "训练的指定落地区域为空");
                return;
            }
            MatchCollection result= Regex.Matches(targetTouchDowns, "\\([0 - 9] *,[0 - 9] *\\)");
            if (result.Count > 0)
            {
                for(int i=0;i<result.Count/2;i++)
                {
                    Point p1 = Point.Parse(result[i*2].Value);
                    Point p2 = Point.Parse(result[i*2 + 1].Value);
                    TargetMarks.Add(new Rect(p1, p2));
                }
            }
            else
            {
                LogText.Error("GetMarksFromString","没有符合规则的训练指定区域信息，"+targetTouchDowns);
            }
        }

        string trainName;
        IList<Rect> targetMarks;

        public string TrainName
        {
            get
            {
                return trainName;
            }

            set
            {
                trainName = value;
            }
        }

        public IList<Rect> TargetMarks
        {
            get
            {
                return targetMarks;
            }

            set
            {
                targetMarks = value;
            }
        }
    }
}
