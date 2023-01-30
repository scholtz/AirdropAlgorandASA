using System.Linq;
using Algorand;
using Algorand.Algod;
using Algorand.Indexer;
using Algorand.Client;
using Newtonsoft.Json;
using Algorand.Algod.Model.Transactions;
using Algorand.Algod.Model;
using Algorand.Indexer.Model;
using Algorand.Utils;
using System.Security.Principal;
using Org.BouncyCastle.Utilities;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace AirdropAlgorandASA
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var config = JsonConvert.DeserializeObject<Model.Configuration>(System.IO.File.ReadAllText("appsettings.json"));
            if (config == null) { throw new Exception("Not configured"); }

            var sendFrom = new Algorand.Algod.Model.Account(config.Airdrop.From);


            var algodHttpClient = HttpClientConfigurator.ConfigureHttpClient(config.Algod.Host, config.Algod.Token, config.Algod.Header);
            DefaultApi algod = new(algodHttpClient);
            var param = await algod.TransactionParamsAsync();
            var indexerHttpClient = HttpClientConfigurator.ConfigureHttpClient(config.Indexer.Host, config.Indexer.Token, config.Indexer.Header);
            LookupApi indexerL = new(indexerHttpClient);
            var asset = await indexerL.lookupAssetByIDAsync(config.Airdrop.Asset);
            if (asset?.Asset?.CreatedAtRound == null)
            {
                throw new Exception("Asset CreatedAtRound not found");
            }
            SearchApi indexerS = new(indexerHttpClient);
            // process all past transactions

            var allTxs = await GetTxsFromIndexerAsync(indexerS, config, asset.Asset.CreatedAtRound.Value);
            ProcessTxs(allTxs);
            Console.WriteLine($"Stats: OptedIn {processed.Where(kv => kv.Value == false).Count()} Droped {processed.Where(kv => kv.Value == true).Count()} Total {processed.Count} ");
            foreach (var item in processed.Where(kv => kv.Value == false))
            {
                // drop
                await DropAsync(algod, sendFrom, new Address(item.Key), config.Airdrop.Asset, param);
            }

            var round = param.LastRound;
            while (true)
            {
                try
                {
                    var block = await algod.WaitForBlockAsync(round++);

                    var newTxs = await GetTxsFromIndexerAsync(indexerS, config, round - 10);
                    ProcessTxs(newTxs);
                    Console.WriteLine($"{round} Stats: OptedIn {processed.Where(kv => kv.Value == false).Count()} Droped {processed.Where(kv => kv.Value == true).Count()}");
                    foreach (var item in processed.Where(kv => kv.Value == false))
                    {
                        // drop
                        await DropAsync(algod, sendFrom, new Address(item.Key), config.Airdrop.Asset, param);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    await Task.Delay(10000);
                }
            }

        }
        static readonly Dictionary<string, bool> processed = new();
        static void ProcessTxs(List<Algorand.Indexer.Model.Transaction> txs)
        {
            foreach (var tx in txs)
            {
                if (tx.AssetTransferTransaction == null) continue;
                if (tx.AssetTransferTransaction.Amount > 0)
                {
                    // 1 tx amount = already droped
                    processed[tx.AssetTransferTransaction.Receiver] = true;
                }
                else
                {
                    if (processed.ContainsKey(tx.AssetTransferTransaction.Receiver) && processed[tx.AssetTransferTransaction.Receiver]) continue;
                    // 0 tx amount = opt in
                    processed[tx.AssetTransferTransaction.Receiver] = false;
                }
            }
        }

        static async Task<List<Algorand.Indexer.Model.Transaction>> GetTxsFromIndexerAsync(SearchApi indexerS, Model.Configuration config, ulong minRound)
        {
            var allTxs = new List<Algorand.Indexer.Model.Transaction>();
            var txs = await indexerS.searchForTransactionsAsync(assetId: config.Airdrop.Asset, minRound: minRound, limit: 1000);
            allTxs.AddRange(txs.Transactions);
            while (!string.IsNullOrEmpty(txs.NextToken))
            {
                txs = await indexerS.searchForTransactionsAsync(assetId: config.Airdrop.Asset, minRound: minRound, limit: 1000, next: txs.NextToken);
                allTxs.AddRange(txs.Transactions);
            }
            return allTxs;
        }

        static Dictionary<string, int> Errors = new Dictionary<string, int>();
        static async Task DropAsync(DefaultApi algod, Algorand.Algod.Model.Account sendFrom, Address dropTo, ulong asset, Algorand.Algod.Model.TransactionParametersResponse param)
        {
            var adr = dropTo.ToString();
            if (Errors.ContainsKey(adr) && Errors[adr] > 10) { return; }
            try
            {
                var tx = new AssetTransferTransaction()
                {
                    AssetAmount = 1,
                    AssetReceiver = dropTo,
                    Fee = 1000,
                    Sender = sendFrom.Address,
                    FirstValid = param.LastRound,
                    LastValid = param.LastRound + 1000,
                    GenesisID = param.GenesisId,
                    XferAsset = asset,
                    GenesisHash = new Digest(param.GenesisHash)
                };
                var signedTx = tx.Sign(sendFrom);

                var id = await Utils.SubmitTransaction(algod, signedTx);
                //var resp = await Utils.WaitTransactionToComplete(algod, id.Txid);
                Console.WriteLine(id.Txid);
            }
            catch (Algorand.ApiException<Algorand.Algod.Model.ErrorResponse> ex)
            {
                Console.WriteLine($"{dropTo} {ex.Result?.Message} {ex.Result?.Data} {ex.ToString()}");
                await Task.Delay(3000);
                if (!Errors.ContainsKey(adr)) Errors[adr] = 0;
                Errors[adr]++;
            }
        }
    }
}