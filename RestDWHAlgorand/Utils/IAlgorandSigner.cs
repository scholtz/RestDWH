using Algo = Algorand;
using AlgoTransactions = Algorand.Algod.Model.Transactions;

namespace RestDWH.Algorand.Utils
{
    public interface IAlgorandSigner
    {
        /// <summary>
        /// Sign transactions
        /// </summary>
        /// <param name="txs">List of unsigned transactions</param>
        /// <returns></returns>
        public IEnumerable<AlgoTransactions.SignedTransaction> Sign(AlgoTransactions.Transaction[] txs);
        /// <summary>
        /// Returns the signing address
        /// </summary>
        /// <returns></returns>
        public Algo.Address getAddress();
    }
}
