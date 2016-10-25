using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RigourTech.Service
{
    /// <summary>
    /// 正反手
    /// </summary>
    public enum HandType
    {
        ForeHand,
        BackHand
    }

    /// <summary>
    /// 轨迹的状态
    /// </summary>
    public enum MatchEventResult
    {
        /// <summary>
        /// 未知道
        /// </summary>
        UNKNOW,
        /// <summary>
        /// 得分
        /// </summary>
        SCORE,

        /// <summary>
        /// 界内
        /// </summary>
        INSIDE,

        /// <summary>
        /// 界外
        /// </summary>
        OUT_SIDE,

        /// <summary>
        /// 下网
        /// </summary>
        DownNet
    }

    /// <summary>
    /// 比赛事件列表
    /// 内角一发    
	//外角一发
 //   内角二发
 //   外角二发
 //   普通接发球
 //   截杀揭发球
 //   截杀回去
 //   普通回球
    /// </summary>
    public enum MatchEventType
    {
        InnerFirstServe=1,
        OutFistServe=2,
        InnerSecondServe=3,
        OutSecondServe=4,
        CommonRecive=5,
        CutRecive=6,
        CutReturn=7,
        CommonReturn=8
    }

}
