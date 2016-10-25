using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RigourTech.Tennisball.Model;
namespace RigourTech.Service
{
    /// <summary>
    /// 回合概念，回合属于一次训练或比赛
    /// </summary>
    public class Round
    {
        string id;
        IList<Track> tacks = new List<Track>();
        DateTime startTime;
        DateTime endTime;
        userInfo winner;
        userInfo serve;
        baseRun run;
        int competitionCount;
        public IList<Track> Tracks
        {
            get
            {
                return tacks;
            }

            set
            {
                tacks = value;
            }
        }

        public DateTime StartTime
        {
            get
            {
                return startTime;
            }

            set
            {
                startTime = value;
            }
        }

        /// <summary>
        /// 单位秒
        /// </summary>
        public long Last
        {
            get
            {
                if (Tracks.Count == 0)
                {
                    return 0;
                }
                else
                {
                    return (Tracks.Last().EndPosition.FrameNumber - Tracks.First().FirstPosition.FrameNumber) / 180;
                }
            }

           
        }

        public userInfo Winner
        {
            get
            {
                return winner;
            }

            set
            {
                winner = value;
            }
        }

        public userInfo Serve
        {
            get
            {
                return serve;
            }

            set
            {
                serve = value;
            }
        }

        public baseRun Run
        {
            get
            {
                return run;
            }

            set
            {
                run = value;
            }
        }

        public int CompetitionCount
        {
            get
            {
                return competitionCount;
            }

            set
            {
                competitionCount = value;
            }
        }

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    id = Guid.NewGuid().ToString();
                }
                return id;
            }
        }

        /// <summary>
        /// 保存到数据库中
        /// </summary>
        public void Save()
        {
            match_round model = new match_round();
            model.ID = Id;
            model.matchID = this.Run?.Id;
            model.EventTime = StartTime;
            model.EventLength = (int)Last;
            model.Serve = Serve?.UserID;
            model.Winner = Winner.UserID;
            model.competitionCount = CompetitionCount;
            DataProviderService.DataProviderServices.instance.AddMatchRound(model);
        }
    }
}
