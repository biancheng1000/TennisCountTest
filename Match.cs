using RigourTech;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RigourTech.Tennisball.Model;
namespace RigourTech.Service
{
    public class Match: baseRun
    {
        public Match(TMatch match)
        {
            Id = match.MatchID;
            StarteTime = DateTime.Parse(match.StartTime);
            Players = match.Users.Select(o => new User(o)).ToList();
            Court = match.Court;
        }

        /// <summary>
        /// 保存运动员的轨迹
        /// </summary>
        public  void SavePlayersTrack()
        {
            Parallel.ForEach(Players, (o) => 
            {
                match_player_track mt = new match_player_track();
                mt.ID = Guid.NewGuid().ToString();
                mt.MatchID = Id;
                mt.PlayerID = o.Userinfo.UserID;
                mt.TrackData = string.Join(";", o.AllPositions.Select(n => n.FrameNumber + "," + n.Position.ToString()));
                mt.Duration = (int)(o.AllPositions.Last().FrameNumber - o.AllPositions.First().FrameNumber) / 180;
                DataProviderService.DataProviderServices.instance.AddMachPlayerTrack(mt);
            });
        }
    }
}
