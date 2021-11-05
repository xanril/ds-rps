using System;
using System.Collections.Generic;
using System.Text;

namespace RPS
{
    public class PracticeBot : IPlayer
    {
        public Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)

        {
            var a = new Random();
            var s = a.Next(1, 3);

            switch (s)
            {
                case 1:
                    return Item.Paper;
                case 2:
                    return Item.Rock;
                case 3:
                    return Item.Scissors;
                default:
                    return Item.Scissors;
            }
        }
    }
}
