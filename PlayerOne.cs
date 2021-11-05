using System;
using System.Collections.Generic;
using System.Text;

namespace RPS
{
    public  class PlayerOne : IPlayer
    {
        /// <summary>
        /// NOTE YOU ARE ALLOWED TO MODIFY THIS CLASS 
        /// </summary>
        /// <param name="yourPastItems"></param>
        /// <param name="opponentsPastItems"></param>
        /// <returns></returns>
        public Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            /*
             * Do something with both you and your opponent's past items to determine your current one
             */

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
