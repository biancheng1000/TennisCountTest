
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using RigourTech.Tennisball.Model;
namespace RigourTech.Service
{
    /// <summary>
    /// 管理用户坐标变化、轨迹等
    /// </summary>
    public class User
    {
        userInfo user;
        public User(userInfo _user)
        {
            user = _user;
        }

        /// <summary>
        /// 更新用户信息（交换场地)
        /// </summary>
        /// <param name="u"></param>
        public void UpdateUser(userInfo u)
        {
            user = u;
        }


        /// <summary>
        /// 判断是否属于当前用户
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private bool Contain(FramePosition p)
        {
            return p.Direction*this.Userinfo.Direction>0;
        }

        /// <summary>
        /// 添加坐标
        /// </summary>
        /// <param name="p"></param>
        public  void AddPosition(FramePosition p)
        {
            if (Contain(p))
            {
                allPositions.Add(p);
            }
          
        }

        ///// <summary>
        ///// 添加轨迹
        ///// </summary>
        ///// <param name="ps"></param>
        //public void AddTracks(Track track)
        //{
        //    Vector d = track.FirstPosition.Position.Point2D - track.EndPosition.Position.Point2D;
        //    if (user.Derection*d.Y>0)
        //    {
        //        Alltracks.Add(track);
        //    }
        //}

        /// <summary>
        /// 清空人的轨迹坐标和球的轨迹数据
        /// </summary>
        public void Clear()
        {
            AllPositions.Clear();
            //Alltracks.Clear();
        }

        ///// <summary>
        ///// 分析当前的轨迹 
        ///// </summary>
        ///// <param name="track"></param>
        //public void AnaliyEvent(Track track)
        //{

        //}

        public userInfo Userinfo
        {
            get
            {
                return user;
            }

            set
            {
                user = value;
            }
        }
        /// <summary>
        /// 坐标
        /// </summary>
        public IList<FramePosition> AllPositions
        {
            get
            {
                return allPositions;
            }

            set
            {
                allPositions = value;
            }
        }
        ///// <summary>
        ///// 轨迹
        ///// </summary>
        //public IList<Track> Alltracks
        //{
        //    get
        //    {
        //        return alltracks;
        //    }

        //    set
        //    {
        //        alltracks = value;
        //    }
        //}

        IList<FramePosition> allPositions = new List<FramePosition>();
        //IList<Track> alltracks = new List<Track>();
        
    }
}
