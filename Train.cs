using RigourTech.Tennisball.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RigourTech.Tennisball.Model;
namespace RigourTech.Service
{
    public class Train: baseRun
    {
        public Train(TTrain train)
        {
            Id = train.TainID;
            StarteTime = DateTime.Parse(train.StartTime);
            Array.ForEach(train.Trainers.ToArray(), (n) => 
            {
                Players.Add(new Trainner(n));
            });
            Court = train.Court;
        }


        public override void AddNewTrack(Track newTrack)
        {

            if (!SetTrackOwner(newTrack))
            {
                return;
            }

            DeterminHandType(newTrack);

            //添加训练轨迹到数据库中
            SaveTrace(newTrack);

            //添加训练事件
            train_event ev = CreateNewModel(newTrack);
            if (DeterminTrackTouchDown(newTrack))
            {
                ev.EventResult = "1";
            }
            else
            {
                ev.EventResult = "0";
            }

            DataProviderService.DataProviderServices.instance.AddTrainEvent(ev);
        }

        /// <summary>
        /// 创建训练事件数据库对象
        /// </summary>
        /// <param name="newTrack"></param>
        /// <returns></returns>
        private train_event CreateNewModel(Track newTrack)
        {
            train_event ev = new train_event();
            ev.ID = Guid.NewGuid().ToString();
            ev.EventTypeID = ((Trainner)newTrack.Owner).TrainName;
            TimeSpan sp = new TimeSpan(((newTrack.FirstPosition.FrameNumber - newTrack.Owner.AllPositions.First().FrameNumber) / 180) * TimeSpan.TicksPerSecond);
            ev.EventTime = StarteTime + sp;
            return ev;
        }

        /// <summary>
        /// 保存轨迹
        /// </summary>
        private void SaveTrace(Track t)
        {
            train_track tm = new train_track();
            tm.ID = Guid.NewGuid().ToString();
            tm.TrainID = Id;
            tm.TrainnerID = t.Owner.Userinfo.UserID;
            tm.TrackData = t.TraceString;
            tm.MaxSpeed = (decimal)t.MaxSpeed;
            tm.MaxRotateSpeed = (decimal)t.MaxRotateSpeed;
            tm.ForeOrBack = (int)t.Hand;
            tm.Direction = t.Direction;
            tm.OverNetHeight = (decimal)t.OverNetHeight;
            tm.TrackStatus = t.Status.ToString();
            DataProviderService.DataProviderServices.instance.AddTrainTrack(tm);
        }

        public bool DeterminTrackTouchDown(Track newTrack)
        {
            //判断指定轨迹的落地点是否指定的训练范围中
            Trainner t = newTrack.Owner as Trainner;
            if (t != null)
            {
                foreach (System.Windows.Rect rect in t.TargetMarks)
                {
                    if (rect.Contains(newTrack.Touchdown_P1.Point2D) || rect.Contains(newTrack.Touchdown_P2.Point2D))
                    {
                        return true;
                    }
                }
            }
            else
            {
                LogText.Error("DeterminTrackTouchDown", "轨迹的所有者不是训练者");
            }
            return false;
        }

        /// <summary>
        /// 保存运动员的轨迹
        /// </summary>
        public void SavePlayersTrack()
        {
            Parallel.ForEach(Players, (o) =>
            {
                trainner_track mt = new trainner_track();
                mt.ID = Guid.NewGuid().ToString();
                mt.TrainID = Id;
                mt.TrainnerID = o.Userinfo.UserID;
                mt.TrackData = string.Join(";", o.AllPositions.Select(n => n.FrameNumber + "," + n.Position.ToString()));
                mt.Duration = (int)(o.AllPositions.Last().FrameNumber - o.AllPositions.First().FrameNumber) / 180;
                DataProviderService.DataProviderServices.instance.AddTrainnerTrack(mt);
            });
        }

    }
}
