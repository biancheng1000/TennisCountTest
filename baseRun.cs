using RigourTech.Tennisball.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RigourTech.Tennisball.Model;
using DataProviderService;

namespace RigourTech.Service
{
    /// <summary>
    /// 开始的一个周期
    /// </summary>
    public class baseRun
    {
        public baseRun()
        {
           
        }
        DateTime starteTime;
        DateTime endTime;
        string id;
        IList<Round> rounds = new List<Round>();
      
        Round currentRound;
        long startFrameNumber;
        IList<User> players = new List<User>();
        TennisCourt _court;
        /// <summary>
        /// 当前的运动员（比赛人员或训练人员）
        /// </summary>
        public virtual IList<User> Players
        {
            get
            {
                return players;
            }

            set
            {
                players = value;
            }
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StarteTime
        {
            get
            {
                return starteTime;
            }

            set
            {
                starteTime = value;
            }
        }

        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime
        {
            get
            {
                return endTime;
            }

            set
            {
                endTime = value;
            }
        }

        /// <summary>
        /// 比赛或训练的ID
        /// </summary>
        public string Id
        {
            get
            {
                return id;
            }

            set
            {
                id = value;
            }
        }

        /// <summary>
        ///回合
        /// </summary>
        public IList<Round> Rounds
        {
            get
            {
                return rounds;
            }

            set
            {
                rounds = value;
            }
        }
        public long StartFrameNumber
        {
            get
            {
                return startFrameNumber;
            }

            set
            {
                startFrameNumber = value;
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


        /// <summary>
        /// 交换场地 方向取反
        /// </summary>
        public void ExChangeGround()
        {
            Parallel.ForEach(Players, (o) => { o.Userinfo.Direction *= -1; });
        }

        /// <summary>
        /// 添加新的轨迹
        /// </summary>
        public virtual void AddNewTrack(Track newTrack)
        {
            if (!SetTrackOwner(newTrack))
            {
                return;
            }

            DeterminHandType(newTrack);

            CaculateOverNetHeight(newTrack);

            //判断跟上条估计间的时间差
            if (currentRound == null)
            {
                currentRound = new Round();
                rounds.Add(currentRound);
                currentRound.Tracks.Add(newTrack);
               
            }
            else
            {

                long lastFrame = currentRound.Tracks.Last().EndPosition.FrameNumber;

                //与上条轨迹想差3秒认为是新的回合开始
                if (((double)(newTrack.FirstPosition.FrameNumber - lastFrame)) / 180 > 2.0d)
                {
                    //上个回合完成，可以进行计算回合内的统计
                    LogText.Info("AddNewTrack", "上帧号：{0},当前帧号：{1}与上条轨迹间大于2秒钟，进行新回合判断",lastFrame, newTrack.FirstPosition.FrameNumber);
                    if (CompleteRand())
                    {
                        SaveTrack(newTrack, currentRound);
                        currentRound = new Round();
                        LogText.Info("AddNewTrack", "开启了一个新的回合");
                    }
                    else
                    {
                        //如果是一发失败的，不进行开始新的回合，仍然继续该回合
                        currentRound.Tracks.Add(newTrack);
                    }
                    rounds.Add(currentRound);
                    
                }
                else
                {
                    currentRound.Tracks.Add(newTrack);
                }
            }
            LogText.Info("AddNewTrack", "当前回合轨迹数为：" + rounds.Count);
        }

        public virtual void SaveTrack(Track tr,Round rd)
        {
            match_track tm = new match_track();
            tm.ID = Guid.NewGuid().ToString();
            tm.MatchID = Id;
            tm.RoundID = rd.Id;
            tm.PlayerID = tr.Owner.Userinfo.UserID;
            tm.TrackData = tr.TraceString;
            tm.MaxRotateSpeed = (decimal)tr.MaxRotateSpeed;
            tm.MaxSpeed = (decimal)tr.MaxSpeed;
            tm.OverNetHeight = (decimal)tr.OverNetHeight;
            tm.ForeOrBack = (int)tr.Hand;
            tm.Direction = tr.Direction;
            tm.TrackStatus = tr.Status.ToString() ;
            DataProviderServices.instance.AddMatchTrack(tm);
            LogText.Info("SaveTrack", "保存当前回合数据到数据库");
        }
        /// <summary>
        /// 计算轨迹中的过网高度
        /// </summary>
        /// <param name="newTrack"></param>
        private void CaculateOverNetHeight(Track newTrack)
        {
            double standerLine = Court.Height / 2;
            int nextIndex = -1;
            if (newTrack.FirstPosition.Position.Y > standerLine)
            {
                for (int i = 0; i < newTrack.Trace.Count; i++)
                {
                    if (newTrack.Trace[i].Position.Y < standerLine)
                    {
                        nextIndex = i;
                    }
                }
            }
            else
            {
                for (int i = 0; i < newTrack.Trace.Count; i++)
                {
                    if (newTrack.Trace[i].Position.Y > standerLine)
                    {
                        nextIndex = i;
                    }
                }
            }

            //可能是网前
            if (nextIndex == 0)
            {
                newTrack.OverNetHeight= newTrack.Trace[nextIndex].Position.Z;
            }

            if (nextIndex > 0 && nextIndex >= 1)
            {
                newTrack.OverNetHeight = (newTrack.Trace[nextIndex].Position.Z + newTrack.Trace[nextIndex-1].Position.Z)/2;
            }

            LogText.Info("CaculateOverNetHeight", "当前轨迹的过网高度为："+newTrack.OverNetHeight);
            
        }

        /// <summary>
        /// 找到该轨迹的发起人
        /// </summary>
        /// <param name="newTrack"></param>
        /// <returns></returns>
        public bool SetTrackOwner(Track newTrack)
        {
            //轨迹的所属人
            User u = Players.First(o => o.Userinfo.Direction * newTrack.Direction > 0);
            if (u != null)
            {
                //估计的所属人
                newTrack.Owner = u;

                FramePosition position = newTrack.Owner.AllPositions.LastOrDefault(o => o.FrameNumber<=newTrack.FirstPosition.FrameNumber);
                if (position != null)
                {
                    newTrack.OwnerStartPosition = position;
                    LogText.Info("SetTrackOwner01", "轨迹起始坐标：{0}，人的坐标{1}，人的场地{2}",newTrack.FirstPosition,position,u.Userinfo.Direction>0?"A":"B");
                }
                else
                {
                    //设置人的最后一个坐标为当前轨迹起始时，人的轨迹坐标
                    newTrack.OwnerStartPosition = newTrack.Owner.AllPositions.LastOrDefault();
                    LogText.Info("SetTrackOwner02", "将当前轨迹所有人的最后一个坐标点设置为轨迹起始时，对应的人的坐标，可能会导致左右手判断出错");
                    LogText.Info("SetTrackOwner01", "轨迹起始坐标：{0}，人的坐标{1}，人的场地{2}", newTrack.FirstPosition, position, u.Userinfo.Direction > 0 ? "A" : "B");
                }
                return true;
            }
            else
            {
                LogText.Error("SetTrackOwner03", "没有找到该轨迹对应的人员的同帧号坐标,轨迹起始帧号：" + newTrack.FirstPosition.FrameNumber);
            }
            return false;
        }
        /// <summary>
        /// 当前回合完成，可以分析当前回合内的数据
        /// 一发失败
        /// </summary>
        public bool CompleteRand()
        {
            IList<Track> tracks = currentRound.Tracks;
            if (tracks.Count == 0)
            {
                LogText.Error("CompleteRand", "当前回合的轨迹数为0，发生数据错误");
                return false;
            }
          
            //一发
            if (tracks.Count == 1)
            {
                LogText.Info("CompleteRand", "当前回合只有1条轨迹");

                FirstServeEvent(tracks[0]);

                SetWinner(currentRound, tracks[0]);

                if (tracks[0].Status == MatchEventResult.SCORE)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else if (tracks.Count == 2)
            {
                Track firstT = tracks[0];

                ///一发失败，从第二个轨迹开始开始计算
                ///第一条轨迹已经判断过了，并且一二两条轨迹都是同一个人
                if (firstT.Status != MatchEventResult.UNKNOW && firstT.Owner.Equals(tracks[1].Owner))
                {
                    LogText.Info("CompleteRand", "一发失误，开始进行二发");

                    SecondServ();
                    //设定二发的赢球人
                    SetWinner(currentRound, tracks[1]);
                    return true;
                }
                else
                {
                    currentRound.CompetitionCount = 1;
                    match_event reciveEvent=ProcessRecive(tracks);
                    DataProviderServices.instance.AddMatchEvent(reciveEvent);
                    SetWinner(currentRound, tracks[1]);
                    LogText.Info("CompleteRand", "普通的两条轨迹处理完成");
                }
            }
            else if (tracks.Count > 2)
            {
                LogText.Info("CompleteRand", "大于2条轨迹回合处理");
                match_event serv= ProcessRecive(tracks);

                for (int i = 2; i <= tracks.Count - 1; i++)
                {
                    match_event ev = CreateNewEvent();
                    if (tracks[i-1].Status == MatchEventResult.OUT_SIDE)
                    {
                        tracks[i - 1].Status = MatchEventResult.INSIDE;
                        serv.EventResult=(int)MatchEventResult.INSIDE;
                        DataProviderServices.instance.AddMatchEvent(serv);
                    }
                    ev.EventResult= (int)processSingleTrack(tracks[i],false);
                    serv = ev;
                }
                DataProviderServices.instance.AddMatchEvent(serv);

                SetWinner(currentRound, tracks[tracks.Count - 1]);
            }
            currentRound.Save();
            return true;
        }

        /// <summary>
        /// 处理接发球
        /// </summary>
        /// <param name="tracks"></param>
        private match_event ProcessRecive(IList<Track> tracks)
        {
            match_event serv = ServeEvent(tracks[0]);

            if (tracks[0].Status != MatchEventResult.INSIDE)
            {
                LogText.Error("CompleteRand", "数据出现异常，roundID:" + currentRound.Id);
            }

            match_event ev = CreateNewEvent();

            //修正如果上条轨迹是没有落地点，判断是不是截杀揭发球
            if (tracks[0].Status == MatchEventResult.OUT_SIDE)
            {
                tracks[0].Status = MatchEventResult.INSIDE;
                serv.EventResult = (int)MatchEventResult.INSIDE;
               
                ev.EventType = (int)MatchEventType.CutRecive;
                LogText.Info("ProcessRecive", "修改上条轨迹结果，将出界改为界内，当前事件为截杀事件");
            }
            else if (tracks[0].Status == MatchEventResult.INSIDE)
            {
                ev.EventType = (int)MatchEventType.CommonRecive;
                LogText.Info("ProcessRecive", "普通接发球");
            }
            DataProviderServices.instance.AddMatchEvent(serv);

            ev.EventResult = (int)processSingleTrack(tracks[1], false);

            LogText.Info("ProcessRecive", "记录当前事件的结果，并保存上一条轨迹事件到数据库中");

            return ev;
        }

        /// <summary>
        /// 二发处理
        /// </summary>
        private void SecondServ()
        {
            match_event ev = CreateNewEvent();

            //判断是内角还是外角发球
            double middleValue = Court.Width / 2;

            ///判断二发是内角还是外角发球
            if (Math.Abs(middleValue - currentRound.Tracks[1].FirstPosition.Position.X) > 2057.5)
            {
                ev.EventType = (int)MatchEventType.OutSecondServe;
            }
            else
            {
                ev.EventType = (int)MatchEventType.InnerSecondServe;
            }
            currentRound.CompetitionCount = 0;

            //二发结果
            ev.EventResult = (int)processSingleTrack(currentRound.Tracks[1], true);

            DataProviderServices.instance.AddMatchEvent(ev);
            LogText.Info("SecondServ", "二发事件：" + ev.EventType.ToString());
        }

        /// <summary>
        ///一发处理
        /// </summary>
        private void FirstServeEvent(Track fistTrack)
        {
            match_event ev = CreateNewEvent();

            //判断是内角还是外角发球
            double middleValue = Court.Width / 2;

            ///判断是否内角发球
            if (Math.Abs(middleValue - fistTrack.FirstPosition.Position.X) > 2057.5)
            {
                ev.EventType = (int)MatchEventType.OutFistServe;
            }
            else
            {
                ev.EventType = (int)MatchEventType.InnerFirstServe;
            }

            ev.EventResult = (int)processSingleTrack(fistTrack);

            if (ev.EventResult != (int)MatchEventResult.SCORE)
            {
                LogText.Error("CompleteRand", "数据出错：轨迹{0}", fistTrack.TraceString); ;
            }

            DataProviderServices.instance.AddMatchEvent(ev);

            LogText.Info("FirstServeEvent", "一发判断："+ev.EventResult.ToString());
        }

        /// <summary>
        /// 普通的发球
        /// </summary>
        /// <param name="fistTrack"></param>
        /// <returns></returns>
        private match_event ServeEvent(Track fistTrack)
        {
            match_event ev = CreateNewEvent();

            //判断是内角还是外角发球
            double middleValue = Court.Width / 2;

            ///判断是否内角发球
            if (Math.Abs(middleValue - fistTrack.FirstPosition.Position.X) > 2057.5)
            {
                ev.EventType = (int)MatchEventType.OutFistServe;
            }
            else
            {
                ev.EventType = (int)MatchEventType.InnerFirstServe;
            }

            ev.EventResult = (int)processSingleTrack(fistTrack);

            if (ev.EventResult != (int)MatchEventResult.SCORE)
            {
                LogText.Error("CompleteRand", "数据出错：轨迹{0}", fistTrack.TraceString); ;
            }

            return ev;
        }


        /// <summary>
        /// 创建新的比赛事件
        /// </summary>
        public match_event CreateNewEvent()
        {
            match_event ev = new match_event();
            ev.ID = Guid.NewGuid().ToString();
            ev.MatchID = Id;
            ev.RoundID = currentRound.Id;
            return ev;
        }

        /// <summary>
        /// 设置赢球方
        /// </summary>
        private void SetWinner(Round round,Track track)
        {
            if (track.Status == MatchEventResult.SCORE)
            {
                round.Winner = track.Owner.Userinfo;
            }
            else
            {
                User otherUser = Players.Where(o => o.Userinfo.UserID != track.Owner.Userinfo.UserID).FirstOrDefault();
                if (otherUser != null)
                {
                    round.Winner = otherUser.Userinfo;
                }
            }
            LogText.Info("SetWinner", "当前回合获胜者是:"+round.Winner.UserID);
        }

        /// <summary>
        /// 处理一条估计
        /// </summary>
        /// <returns></returns>
        private MatchEventResult processSingleTrack(Track firstTrack,bool isFirstServe=true)
        {

            //出界
            if (firstTrack.TouchdonwMarkNumber.Equals("0"))
            {
                //轨迹的终点距离网半米内，认为是触网
                if (Math.Abs(firstTrack.EndPosition.Position.Y - Court.Height / 2) < 500)
                {
                    LogText.Info("processSingleTrack", "单条轨迹结果：下网,{0}",firstTrack.TraceString);
                    return firstTrack.Status=MatchEventResult.DownNet;
                }
                else
                {
                    LogText.Info("processSingleTrack", "单条轨迹结果：出界,{0}", firstTrack.TraceString);
                    //出界
                    return firstTrack.Status = MatchEventResult.OUT_SIDE;

                }
            }
            else
            {
                if (isFirstServe)
                {
                    //发球失误，不在发球指定的发球区域中
                    //判断轨迹所有这所在的位置，判断发球的目标区域
                    bool leftside = firstTrack.OwnerStartPosition.Position.X > Court.Width / 2;
                    string targetServeMark;
                    //A
                    if (firstTrack.Owner.Userinfo.Direction > 0)
                    {
                        if (leftside)
                        {
                            targetServeMark = "9";
                        }
                        else
                        {
                            targetServeMark = "8";
                        }
                    }
                    else
                    {
                        if (leftside)
                        {
                            targetServeMark = "4";
                        }
                        else
                        {
                            targetServeMark = "3";
                        }
                    }

                    //一发得分
                    if (firstTrack.TouchdonwMarkNumber.Equals(targetServeMark))
                    {
                        LogText.Info("processSingleTrack", "单条轨迹结果：一发得分,{0}", firstTrack.TraceString);
                        return firstTrack.Status = MatchEventResult.SCORE;
                    }
                    else
                    {
                        //出界
                        LogText.Info("processSingleTrack", "单条轨迹结果：一发出界,{0}", firstTrack.TraceString);
                        return firstTrack.Status = MatchEventResult.OUT_SIDE;
                    }
                }
                else
                {
                    LogText.Info("processSingleTrack", "单条轨迹结果：单条结果：界内，{0}", firstTrack.TraceString);
                    return firstTrack.Status = MatchEventResult.INSIDE;
                }
            }
        }

        /// <summary>
        /// 判断是左手还是右手
        /// </summary>
        /// <param name="newTrack"></param>
        public void DeterminHandType(Track newTrack)
        {
            if (newTrack.OwnerStartPosition != null)
            {
                if (newTrack.Owner.Userinfo.Hand == MainhandType.LEFT)
                {
                    //A面
                    if (newTrack.Owner.Userinfo.Direction > 0)
                    {
                        if (newTrack.FirstPosition.Position.X >= newTrack.OwnerStartPosition.Position.X)
                        {
                            //正手
                            newTrack.Hand = HandType.ForeHand;
                        }
                        else
                        {
                            //反手
                            newTrack.Hand = HandType.BackHand;
                        }
                    }
                    else
                    {
                        if (newTrack.FirstPosition.Position.X <= newTrack.OwnerStartPosition.Position.X)
                        {
                            //正手
                            newTrack.Hand = HandType.ForeHand;
                        }
                        else
                        {
                            //反手
                            newTrack.Hand = HandType.BackHand;
                        }
                    }
                }
                else
                {
                    //A面
                    if (newTrack.Owner.Userinfo.Direction > 0)
                    {
                        if (newTrack.FirstPosition.Position.X <= newTrack.OwnerStartPosition.Position.X)
                        {
                            //正手
                            newTrack.Hand = HandType.ForeHand;
                        }
                        else
                        {
                            //反手
                            newTrack.Hand = HandType.BackHand;
                        }
                    }
                    else
                    {
                        if (newTrack.FirstPosition.Position.X >= newTrack.OwnerStartPosition.Position.X)
                        {
                            //正手
                            newTrack.Hand = HandType.ForeHand;
                        }
                        else
                        {
                            //反手
                            newTrack.Hand = HandType.BackHand;
                        }
                    }
                }
            }
            LogText.Info("DeterminHandType", "当前轨迹为"+newTrack.Hand.ToString()+"球");
           
        }

        /// <summary>
        /// 添加运动员的坐标
        /// </summary>
        /// <param name="fr"></param>
        public void AddPosition(FramePosition fr)
        {
            //为坐标点设置方向
            fr.Direction = fr.Position.Y > Court.Height / 2 ? 1 : -1;

            Parallel.ForEach(Players, (n) => { n.AddPosition(fr); });

            ///记录该场比赛的起始帧号
            if (StartFrameNumber == 0)
            {
                StartFrameNumber = fr.FrameNumber;
            }
        }
    }
}
