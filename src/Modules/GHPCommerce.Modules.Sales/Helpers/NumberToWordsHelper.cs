using System;

namespace GHPCommerce.Modules.Sales.Helpers
{
    public static class NumberToWordsHelper
    {
        
        public static string NumberToText(decimal amount, bool isUK = false)
        {
            var number = Convert.ToInt32(amount);
            var reste = Convert.ToInt32(amount - number);
            if (number == 0) return "Zero";
            string and = isUK ? "and " : ""; // deals with UK or US numbering
            if (number == -2147483648) return "Minus Two Billion One Hundred " + and +
            "Forty Seven Million Four Hundred " + and + "Eighty Three Thousand " +
            "Six Hundred " + and + "Forty Eight";
            int[] num = new int[4];
            int first = 0;
            int u, h, t;
            System.Text.StringBuilder sb = new System.Text.StringBuilder(); 
            if (number < 0)
            {
                sb.Append("Minus ");
                number = -number;
            }
            string[] words0 = {"", "Un ", "Deux ", "Trois ", "Quatre ", "Cinque ", "Sixe ", "Sept ", "Huit ", "Neuf "};
            string[] words1 = {"Dix ", "Onze ", "Douze ", "Treize ", "Quatorze ", "Quinze ", "Seize ", "Dix-sept ", "Dix-huits ", "Dix-neuf "};
            string[] words2 = {"Vingt ", "Trente ", "Quarante ", "Cinquante ", "Soixante ", "Soixante-dix ", "Quatre-vingt ", "Quatre-vingt-dix "};
            string[] words3 = { "Mille ", "Million ", "Billion " };
            num[0] = number % 1000;           // units
            num[1] = number / 1000;
            num[2] = number / 1000000;
            num[1] = num[1] - 1000 * num[2];  // thousands
            num[3] = number / 1000000000;     // billions
            num[2] = num[2] - 1000 * num[3];  // millions
            for (int i = 3; i > 0; i--)
            {
                if (num[i] != 0)
                {
                    first = i;
                    break;
                }
            }
            for (int i = first; i >= 0; i--)
            {
                if (num[i] == 0) continue;
                u = num[i] % 10;              // ones
                t = num[i] / 10;
                h = num[i] / 100;             // hundreds
                t = t - 10 * h;               // tens
                if (h > 0) sb.Append(words0[h] + "Cent ");
                if (u > 0 || t > 0)
                {
                    if (h > 0 || i < first) sb.Append(and);
                    if (t == 0)
                        sb.Append(words0[u]);
                    else if (t == 1)
                        sb.Append(words1[u]);
                    else
                        sb.Append(words2[t - 2] + words0[u]);
                }
                if (i != 0) sb.Append(words3[i - 1]);
            }

            sb.Append("Dinars");
            if (reste > 0)
            {
                u = reste % 10;              // ones
                t = reste / 10;
                sb.Append(words0[u]);
                sb.Append(words1[t]);
                sb.Append("Centimes");

            }

            if (sb.ToString().StartsWith("Un")) 
                return sb.ToString().Replace("Un Mille", "").TrimStart().TrimEnd();
            return sb.ToString().TrimEnd();
        }
    }
}