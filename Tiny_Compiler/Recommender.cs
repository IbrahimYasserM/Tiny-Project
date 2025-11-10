using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Recommender
{
    public class ErrorRecommend
    {
        public ErrorRecommend() { }
        public static string Recommend(string lex, Dictionary<string, Token_Class> ReservedWords)
        {
            int minCost = lex.Length;
            string minMatch = lex;
            foreach (var x in ReservedWords) {
                int cost = Lca(lex, x.Key);
                if (cost < minCost)
                {
                    minCost = cost;
                    minMatch = x.Key;
                }
            }
            if(minCost*2 < lex.Length)
                return minMatch;
            return lex;
        }
        private static int Lca(string lex, string word)
        {
            int []dp = new int[word.Length+1];
            for (int i = 0; i <= word.Length; ++i)
                dp[i] = i;
            foreach(char c in lex)
            {
                int min = dp[0]++;
                for (int i = 1; i<=word.Length; ++i)
                {
                    int cur = dp[i];
                    dp[i] = Math.Min(dp[i] + 1, min + (c == word[i - 1] ? 0 : 1));
                    min = Math.Min(min + 1, cur);
                }
            }
            int ans = int.MaxValue;
            for (int i = 0; i <= word.Length; ++i)
                ans = Math.Min(ans, dp[i] + word.Length - i);
            return ans;
        }

    }
}
