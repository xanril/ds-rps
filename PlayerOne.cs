using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RPS
{
    public  class PlayerOne : IPlayer
    {
        #region Fields

        private Item _previousItem = Item.Paper;

        #endregion

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

            ChangeWhenLosingPreviousRound(yourPastItems, opponentsPastItems);
            return _previousItem;

            

        }

        #region Algorithms

        /// <summary>
        ///     This is the original algorithm provided by organizers.
        /// </summary>
        /// <returns></returns>
        private Item GenerateRandomItem()
        {
            var a = new Random();
            var s = a.Next(1, 3);

            switch (s)
            {
                case 1:
                    _previousItem = Item.Paper;
                    break;
                    
                case 2:
                    _previousItem = Item.Rock;
                    break;

                case 3:
                    _previousItem = Item.Scissors;
                    break;

                default:
                    _previousItem = Item.Scissors;
                    break;
            }

            return _previousItem;
        }


        /// <summary>
        ///     This algrorithm checks the last round if we won. If we lost, we change our 
        ///     default return item to the item that beats the last item the opponent used.
        /// </summary>
        /// <param name="yourPastItems"></param>
        /// <param name="opponentsPastItems"></param>
        /// <returns></returns>
        private Item ChangeWhenLosingPreviousRound(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            var myItem = yourPastItems?.LastOrDefault();
            var opponentItem = opponentsPastItems?.LastOrDefault();

            // We check if we won the previous round.
            if (!isRoundWon(myItem, opponentItem))
            {
                // if we lost, we change the item that we are consistently returning
                if (opponentItem == Item.Scissors)
                {
                    // since we lost, the opponent may be using scissors.
                    // so we switch to rock so that we can beat them next time.
                    // same with the other conditions below.
                    _previousItem = Item.Rock;
                }
                else if (opponentItem == Item.Rock)
                {
                    _previousItem = Item.Paper;
                }
                else if (opponentItem == Item.Paper)
                {
                    _previousItem = Item.Scissors;
                }
            }

            // if we are still winning, we continue returning the item.
            return _previousItem;
        }

        /// <summary>
        ///     This algrorithm randomly get from opponents past items and return a counter for it.
        /// </summary>        
        private Item? RandomByNumberOfItems(List<Item> opponentsPastItems){
            double totalItem = opponentsPastItems.Count;
            if(totalItem >= 50){
                 var opponentPick = new Item();
                var ourPick = new Item();
                opponentPick = opponentsPastItems.OrderBy(x=> Guid.NewGuid()).First();
                if(opponentPick == Item.Paper){
                    ourPick = Item.Scissors;
                }
                else if(opponentPick == Item.Rock){
                    ourPick = Item.Paper;
                }
                else if(opponentPick == Item.Scissors){
                    ourPick = Item.Rock;
                }
                return ourPick;
            }
            return null;
        }

        #endregion

        #region Helpers

        /// <summary>
        ///     Checks within the rules of RPS if the player item 
        ///     compared to opponent item wins.
        /// </summary>
        /// <param name="myItem"></param>
        /// <param name="opponentItem"></param>
        /// <returns>True if the player won. False, otherwise.</returns>
        private bool isRoundWon(Item? myItem, Item? opponentItem)
        {
            if ((myItem == Item.Paper && opponentItem == Item.Rock)
                || (myItem == Item.Rock && opponentItem == Item.Scissors)
                || (myItem == Item.Scissors && opponentItem == Item.Paper))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
