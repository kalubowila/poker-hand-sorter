using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Poker_Hand_Sorter
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileName = "poker-hands.txt";
            int playerWin_1 = 0;
            int playerWin_2 = 0;
            //Effieicent with large files
            foreach (var line in File.ReadLines(fileName))
            {

                List<string> player1 = line.Split(' ').Take(5).ToList();
                List<string> player2 = line.Split(' ').Skip(5).Take(5).ToList();

                //Console.WriteLine($"Is Same Suit: {IsSameSuit(player1)}");
                //Console.WriteLine($"Is Same Suit: {IsSameSuit(player2)}");

                List<string> playerSuit_1 = player1.Select(x => { return x[1].ToString(); }).ToList();
                List<string> playerSuit_2 = player2.Select(x => { return x[1].ToString(); }).ToList();

                List<int> playerValues_1 = player1.Select(x => { return CardValue(x[0].ToString()); }).ToList();
                List<int> playerValues_2 = player2.Select(x => { return CardValue(x[0].ToString()); }).ToList();

                int rankPlayer1 = GetRank(playerSuit_1, playerValues_1);
                int rankPlayer2 = GetRank(playerSuit_2, playerValues_2);

                if (rankPlayer1 > rankPlayer2) playerWin_1++;
                else if (rankPlayer2 > rankPlayer1) playerWin_2++;
                else
                {
                    int tieRank = TieBreak(rankPlayer1, playerValues_1, playerValues_2);

                    switch (tieRank)
                    {
                        case 1:
                            playerWin_1++;
                            break;
                        case 2:
                            playerWin_2++;
                            break;
                    }
                }

            }

            Console.WriteLine("Player 1: {0}", playerWin_1);
            Console.WriteLine("Player 2: {0}", playerWin_2);

            Console.ReadLine();
        }

        static int GetRank(List<string> playerSuit, List<int> playerValues)
        {
            var groupedCountedList = playerValues.GroupBy(x => x).Select(o => o.Count()).ToList();

            bool isSameSuit = IsSameSuit(playerSuit);
            bool isConsecutive = IsConsecutive(playerValues);

            if (isSameSuit && isConsecutive && playerValues.Contains(14)) return 10; //Royal Flush
            else if (isSameSuit && isConsecutive) return 9; //Straight flush
            else if (groupedCountedList.Count() == 2 && groupedCountedList.Contains(4)) return 8; //Four of a kind
            else if (groupedCountedList.Count() == 2 && groupedCountedList.Contains(3) && groupedCountedList.Contains(2)) return 7; //Full house
            else if (isSameSuit) return 6; //Flush
            else if (isConsecutive) return 5; //Straight
            else if (groupedCountedList.Count() == 3 && groupedCountedList.Contains(3)) return 4; //Three of a kind
            else if (groupedCountedList.Count() == 3 && groupedCountedList.Contains(2)) return 3; //Two pairs
            else if (groupedCountedList.Count() == 4) return 2; //Pair
            else if (groupedCountedList.Count() == 5) return 1; //High card

            return 0;
        }

        static int TieBreak(int rank, List<int> playerValues_1, List<int> playerValues_2)
        {
            switch (rank)
            {
                case 10:
                    //Royal Flush
                    return 0;
                case 9:
                    //Straight flush
                    return CheckForHighCard(playerValues_1, playerValues_2);
                case 8:
                    //Four of a kind
                    return CheckForPairs(playerValues_1, playerValues_2);
                case 7:
                    //Full house
                    int duplicatePlayer1Val = playerValues_1.GroupBy(x => x).Where(g => g.Count() == 3).FirstOrDefault().FirstOrDefault();
                    int duplicatePlayer2Val = playerValues_2.GroupBy(x => x).Where(g => g.Count() == 3).FirstOrDefault().FirstOrDefault();

                    if (duplicatePlayer1Val > duplicatePlayer2Val) return 1;
                    else if (duplicatePlayer1Val < duplicatePlayer2Val) return 2;
                    else
                    {
                        duplicatePlayer1Val = playerValues_1.GroupBy(x => x).Where(g => g.Count() == 2).FirstOrDefault().FirstOrDefault();
                        duplicatePlayer2Val = playerValues_2.GroupBy(x => x).Where(g => g.Count() == 2).FirstOrDefault().FirstOrDefault();

                        if (duplicatePlayer1Val > duplicatePlayer2Val) return 1;
                        else if (duplicatePlayer1Val < duplicatePlayer2Val) return 2;
                        else return 0;
                    }
                case 6:
                    //Flush
                    return CheckForHighCard(playerValues_1, playerValues_2);
                case 5:
                    //Straight
                    return CheckForHighCard(playerValues_1, playerValues_2);
                case 4:
                    //Three of a kind
                    return CheckForPairs(playerValues_1, playerValues_2);
                case 3:
                    //Two pairs
                    var duplicatePlayer1 = playerValues_1.GroupBy(x => x).SelectMany(grp => grp.Skip(1)).ToList();
                    var duplicatePlayer2 = playerValues_2.GroupBy(x => x).SelectMany(grp => grp.Skip(1)).ToList();

                    if (duplicatePlayer1.All(duplicatePlayer2.Contains))
                    {
                        int remainPlayer1 = playerValues_1.Except(duplicatePlayer1).FirstOrDefault();
                        int remainPlayer2 = playerValues_2.Except(duplicatePlayer2).FirstOrDefault();

                        if (remainPlayer1 > remainPlayer2) return 1;
                        else if (remainPlayer1 < remainPlayer2) return 2;
                        else return 0;
                    }
                    else
                    {
                        return CheckForHighCard(duplicatePlayer1, duplicatePlayer2);
                    }
                case 2:
                    //Pair
                    return CheckForPairs(playerValues_1, playerValues_2);
                case 1:
                    //High card
                    return CheckForHighCard(playerValues_1, playerValues_2);
                default:
                    break;
            }
            return 0;
        }

        static int CheckForPairs(List<int> playerValues_1, List<int> playerValues_2)
        {
            int duplicatePlayer1 = playerValues_1.GroupBy(x => x).SelectMany(grp => grp.Skip(1)).FirstOrDefault();
            int duplicatePlayer2 = playerValues_2.GroupBy(x => x).SelectMany(grp => grp.Skip(1)).FirstOrDefault();

            if (duplicatePlayer1 > duplicatePlayer2) return 1;
            else if (duplicatePlayer1 < duplicatePlayer2) return 2;
            else
            {
                playerValues_1.RemoveAll(s => s == duplicatePlayer1);
                playerValues_2.RemoveAll(s => s == duplicatePlayer2);
                return CheckForHighCard(playerValues_1, playerValues_2);
            }
        }

        static int CheckForHighCard(List<int> playerValues_1, List<int> playerValues_2)
        {
            int len = playerValues_1.Count();
            playerValues_1.Sort();
            playerValues_2.Sort();

            for (int i = len - 1; i >= 0; i--)
            {
                if (playerValues_1[i] > playerValues_2[i]) return 1;
                else if (playerValues_1[i] < playerValues_2[i]) return 2;
                else continue;
            }

            return 0;
        }

        static bool IsConsecutive(List<int> playerValues)
        {
            return (playerValues.Distinct().Count() == 5 && (playerValues.Max() - playerValues.Min() + 1) == 5);
        }

        static bool IsSameSuit(List<string> playerSuit)
        {
            return !playerSuit.Any(o => o != playerSuit[0]);
        }

        static int CardValue(string val)
        {
            switch (val)
            {
                case "T": return 10;
                case "J": return 11;
                case "Q": return 12;
                case "K": return 13;
                case "A": return 14;
                default: return int.Parse(val);
            }
        }
    }

}
