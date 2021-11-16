using System;
using System.Collections.Generic;

namespace RPS
{
    class Program
    {
        /// <summary>
        /// You are allowed to modify this program as much as you like for TRAINING PURPOSES ONLY
        /// we will only be getting the contents of the PlayerOne.cs file
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var pOneInputs = new List<Item>();
            var pTwoINputs = new List<Item>();
            var p1points = 0;
            var p2points = 0;
            var p1 = new PlayerOne();
            var p2 = new PracticeBot();
            Console.WriteLine("Welcome to Rock Paper Scissors");

            do
            {
                var pone = p1.GetItem(pOneInputs,pTwoINputs);
                var ptwo = p2.GetItem(pTwoINputs, pOneInputs);

                pOneInputs.Add(pone);
                pTwoINputs.Add(ptwo);

                Console.WriteLine("Player One throws {0}", pone.ToString());
                Console.WriteLine("Player Two throws {0}", ptwo);

                var winner = Player.None;

                if (pone == ptwo)
                {
                    winner = Player.None;
                }
                else
                {
                    if (pone == Item.Paper)
                    {
                        winner = ptwo == Item.Scissors ? Player.Two : Player.One;
                    }
                    else if (pone == Item.Rock)
                    {
                        winner = ptwo == Item.Paper ? Player.Two : Player.One;

                    }
                    else if (pone == Item.Scissors)
                    {
                        winner = ptwo == Item.Rock ? Player.Two : Player.One;
                    }
                }

                Console.WriteLine("The winner is {0}", winner);

                if (winner == Player.One)
                {
                    p1points++;
                }
                else if (winner == Player.Two)
                {
                    p2points++;
                }
                
                Console.WriteLine("Score  P1 - {0} P2 - {1}", p1points, p2points);

                Console.ReadKey();

                if (p1points == 3 || p2points == 3)
                {
                    break;
                }

            } while (true);

            if (p1points == 3)
            {
                Console.WriteLine("Player One Wins!");
            }
            else
            {
                Console.WriteLine("Player Two Wins!");
            }

            Console.ReadKey();
        }
    }
}
