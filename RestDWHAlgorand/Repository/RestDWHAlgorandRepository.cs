using Algo = Algorand;
using Algorand.Algod;
using Algorand.Algod.Model;
using Algorand.Algod.Model.Transactions;
using Algorand.Utils;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestDWH.Algorand.Model.Config;
using RestDWH.Algorand.Utils;
using RestDWH.Base.Model;
using RestDWH.Base.Repository;
using System.Security.Claims;
using System.Text;

namespace RestDWH.Algorand.Repository
{
    public class RestDWHAlgorandRepository<TEnt> : IDWHRepository<TEnt>
        where TEnt : class
    {
        private readonly DefaultApi algodClient;
        private readonly IAlgorandSigner signer;
        private readonly IOptionsMonitor<RestDWHAlgorandConfig> config;
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="algodClient">DI IDefaultApi</param>
        /// <param name="signer">DI IAlgorandSigner</param>
        /// <param name="config">DI IOptionsMonitor<RestDWHAlgorandConfig></param>
        public RestDWHAlgorandRepository(IDefaultApi algodClient, IAlgorandSigner signer, IOptionsMonitor<RestDWHAlgorandConfig> config)
        {

            this.algodClient = algodClient as DefaultApi ?? throw new Exception("DefaultApi not configured");
            this.signer = signer;
            this.config = config;
        }

        public async Task<DBBase<TEnt>> DeleteAsync(string id, ClaimsPrincipal? user = null)
        {
            var txParams = await algodClient.TransactionParamsAsync();
            //Algorand.Algod.Model.Transactions.ApplicationCallTransaction()

            var box = await algodClient.GetApplicationBoxByNameAsync(config.CurrentValue.ApplicationId, id);
            if (box?.Value == null)
            {
                throw new Exception("Box not found");
            }
            var ret = JsonConvert.DeserializeObject<DBBase<TEnt>>(Encoding.UTF8.GetString(box.Value));
            if (ret == null)
            {
                throw new Exception("Unable to deserialize data at the box");
            }
            var appCall = new ApplicationNoopTransaction()
            {
                FirstValid = txParams.LastRound,
                LastValid = txParams.LastRound + 5,
                Fee = 1000,
                GenesisHash = new Algo.Digest(txParams.GenesisHash),
                GenesisId = txParams.GenesisId,
                Sender = signer.getAddress(),

                ApplicationId = config.CurrentValue.ApplicationId,
                Boxes = new List<BoxRef>() { new BoxRef() { App = config.CurrentValue.ApplicationId, Name = System.Text.Encoding.UTF8.GetBytes(id) } },
                ApplicationArgs = new List<byte[]>()
                {
                    Convert.FromHexString("1234")
                }
            };

            var signed = signer.Sign(new Transaction[1] { appCall });
            var sent = await algodClient.TransactionsAsync(signed.ToList());
            var tx = await Algo.Utils.Utils.WaitTransactionToComplete(algodClient, sent.Txid, 6);
            if (tx == null) throw new Exception($"Error while deleting data. Tx {sent.Txid} has been submitted to the chain but not found in block");

            return ret;
        }

        public Task<DBListBase<TEnt, DBBase<TEnt>>> GetAsync(int from = 0, int size = 10, string query = "*", string sort = "", ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<DBBase<TEnt>?> GetByIdAsync(string id, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, object>> GetByIdWithFieldsAsync(string id, string fields = "id", ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<List<object>?> GetPropertiesAsync(string attribute, int offset = 0, int limit = 10, string query = "*", string sortType = "CountDescending", ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<FieldsListBase> GetWithFieldsAsync(string fields = "id", int from = 0, int size = 10, string query = "*", string sort = "", ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<DBBase<TEnt>> PatchAsync(string id, JsonPatchDocument<TEnt> data, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<DBBase<TEnt>> PostAsync(TEnt data, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<DBBase<TEnt>> PutAsync(string id, TEnt data, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }

        public Task<DBBase<TEnt>> UpsertAsync(string id, TEnt data, ClaimsPrincipal? user = null)
        {
            throw new NotImplementedException();
        }
    }
}
