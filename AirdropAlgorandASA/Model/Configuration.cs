using AirdropAlgorandASA.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirdropAlgorandASA.Model
{
    public class Configuration
    {
        /// <summary>
        /// Algod configuration
        /// </summary>
        public AlgodConfiguration Algod { get; set; } = new AlgodConfiguration();

        /// <summary>
        /// Indexer configuration
        /// </summary>
        public IndexerConfiguration Indexer { get; set; } = new IndexerConfiguration();
        /// <summary>
        /// Airdrop
        /// </summary>
        public AirdropConfiguration Airdrop { get; set; } = new AirdropConfiguration();
    }
}