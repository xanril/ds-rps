using System.Collections.Generic;
using System.Linq;

namespace RPS
{
    public class PlayerOne : IPlayer
    {
        #region Fields

        private readonly AlgorithmOrchestrator _algorithmOrchestrator;

        #endregion

        #region Methods

        #region Constructors

        public PlayerOne()
        {
            _algorithmOrchestrator = new AlgorithmOrchestrator();
        }

        #endregion

        /// <summary>
        ///     NOTE YOU ARE ALLOWED TO MODIFY THIS CLASS
        /// </summary>
        /// <param name="yourPastItems"></param>
        /// <param name="opponentsPastItems"></param>
        /// <returns></returns>
        public Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            /*
             * Do something with both you and your opponent's past items to determine your current one
             */

            return _algorithmOrchestrator.GetItem(yourPastItems, opponentsPastItems);
        }

        #endregion
    }


    #region Orchestrator Classes =======================================================================

    /// <summary>
    ///     Orchestrates between various algorithms.
    ///     Finds the algorithm with the highest success rate.
    /// </summary>
    public class AlgorithmOrchestrator : IPlayer
    {
        #region Constants

        private const int MAX_MEMORY = 10;

        #endregion

        #region Fields

        private readonly IDictionary<IPlayerAlgorithm, int> _algorithmStats;
        private readonly IList<IPlayerAlgorithm> _pendingForDeletion;

        #endregion

        #region Methods

        #region Constructors

        public AlgorithmOrchestrator()
        {
            _algorithmStats = new Dictionary<IPlayerAlgorithm, int>
            {
                {new ChangeWhenLosingPreviousRoundAlgorithm(), 0},
                {new ChangeWhenLosingPreviousRoundCounterAlgorithm(), 0}
            };

            _pendingForDeletion = new List<IPlayerAlgorithm>(MAX_MEMORY);
        }

        #endregion

        public Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            var roundElapsed = yourPastItems.Count;

            // Update the stats first.
            if (roundElapsed > 0)
            {
                updateStats(opponentsPastItems.LastOrDefault());
            }

            // Forget win rates when the max memory has been reached.
            if (roundElapsed > MAX_MEMORY)
            {
                var key = _pendingForDeletion.FirstOrDefault();

                if (key != null)
                {
                    _algorithmStats[key]--;
                }

                _pendingForDeletion.RemoveAt(0);
            }

            // Get the item for all algorithms.
            foreach (var key in _algorithmStats.Keys)
            {
                key.GetItem(yourPastItems, opponentsPastItems);
            }

            // Get the algorithm with the highest win rate.
            var selectedAlgorithm = _algorithmStats.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
            _pendingForDeletion.Add(selectedAlgorithm);
            return selectedAlgorithm.PreviousItem;
        }

        private void updateStats(Item opponentsLastItem)
        {
            foreach (var key in _algorithmStats.Keys.ToList())
            {
                if (!RpsHelper.IsRoundWon(key.PreviousItem, opponentsLastItem))
                {
                    _algorithmStats[key]--;
                    continue;
                }

                _algorithmStats[key]++;
            }
        }

        #endregion
    }

    #endregion


    #region Algorithms =======================================================================

    /// <summary>
    ///     Interface for player algorithm.
    /// </summary>
    public interface IPlayerAlgorithm : IPlayer
    {
        #region Properties

        Item PreviousItem { get; set; }

        #endregion
    }

    /// <summary>
    ///     Algorithm to counter <see cref="ChangeWhenLosingPreviousRoundAlgorithm" />.
    /// </summary>
    public class ChangeWhenLosingPreviousRoundCounterAlgorithm : IPlayerAlgorithm
    {
        #region Fields

        private Item _previousItem = Item.Paper;

        #endregion

        #region Properties

        public Item PreviousItem { get; set; }

        #endregion

        #region Methods

        public Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            PreviousItem = getMove(yourPastItems, opponentsPastItems);
            return PreviousItem;
        }

        private Item getMove(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            var myItem = yourPastItems?.LastOrDefault();
            var opponentItem = opponentsPastItems?.LastOrDefault();

            // We check if our opponent won the previous round.
            if (!RpsHelper.IsRoundWon(opponentItem, myItem))
            {
                // if they lost, they may change the item that they are consistently returning
                if (myItem == Item.Scissors)
                {
                    // So we'll assume they'll counter our previous turn using a rock
                    // We'll use paper.
                    // same conditions below.
                    _previousItem = Item.Paper;
                }
                else if (myItem == Item.Rock)
                {
                    _previousItem = Item.Scissors;
                }
                else if (myItem == Item.Paper)
                {
                    _previousItem = Item.Rock;
                }
            }

            // if we are still winning, we continue returning the item.
            return _previousItem;
        }

        #endregion
    }

    /// <summary>
    ///     This algorithm checks the last round if we won. If we lost, we change our
    ///     default return item to the item that beats the last item the opponent used.
    /// </summary>
    public class ChangeWhenLosingPreviousRoundAlgorithm : IPlayerAlgorithm
    {
        #region Fields

        private Item _previousItem = Item.Paper;

        #endregion

        #region Properties

        public Item PreviousItem { get; set; }

        #endregion

        #region Methods

        public Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            PreviousItem = getMove(yourPastItems, opponentsPastItems);
            return PreviousItem;
        }

        private Item getMove(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            var myItem = yourPastItems?.LastOrDefault();
            var opponentItem = opponentsPastItems?.LastOrDefault();

            // We check if we won the previous round.
            if (!RpsHelper.IsRoundWon(myItem, opponentItem))
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

        #endregion
    }

    #endregion


    #region Helper Classes =======================================================================

    public static class RpsHelper
    {
        #region Methods

        /// <summary>
        ///     Checks within the rules of RPS if the player item
        ///     compared to opponent item wins.
        /// </summary>
        /// <param name="myItem"></param>
        /// <param name="opponentItem"></param>
        /// <returns>True if the player won. False, otherwise.</returns>
        public static bool IsRoundWon(Item? myItem, Item? opponentItem)
        {
            return myItem == Item.Paper && opponentItem == Item.Rock
                   || (myItem == Item.Rock && opponentItem == Item.Scissors)
                   || (myItem == Item.Scissors && opponentItem == Item.Paper);
        }

        #endregion
    }

    #endregion
}