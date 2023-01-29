using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirdropAlgorandASA.Model
{
    public class AirdropConfiguration
    {
        /// <summary>
        /// Airdrop asset
        /// </summary>
        public ulong Asset { get; set; }

        /// <summary>
        /// Rule 
        /// 
        /// OnOptIn
        /// </summary>
        public string Rule { get; set; } = "OnOptIn";
        /// <summary>
        /// Start from round
        /// </summary>
        public ulong StartFromRound { get; set; }
        /// <summary>
        /// From
        /// </summary>
        public string From { get; set; }
    }
}
