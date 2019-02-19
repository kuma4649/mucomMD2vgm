using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class clsLoopInfo
    {
        /// <summary>
        /// Lコマンド使用フラグ
        /// </summary>
        public bool use = false;

        /// <summary>
        /// Lコマンドの位置
        /// </summary>
        public long clockPos = 0;

        /// <summary>
        /// Lコマンド後のデータの長さ
        /// </summary>
        public long length = 0;

        /// <summary>
        /// 全体の演奏データを揃えるのに必要な、Lコマンド後のデータを演奏する回数
        /// </summary>
        public int playingTimes = 0;

        /// <summary>
        /// 演奏データを揃える処理を開始している
        /// </summary>
        public bool startFlag = false;

        /// <summary>
        /// 残りの必要なループ回数
        /// </summary>
        public int loopCount = 0;

        public long totalCounter = 0;
        public long loopCounter = 0;

        /// <summary>
        /// ループ時の戻り位置
        /// </summary>
        public int mmlPos = 0;

        /// <summary>
        /// 最長かどうか
        /// </summary>
        public bool isLongMml = false;

        public bool lastOne = false;
    }
}
