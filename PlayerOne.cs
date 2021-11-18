using System;
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
            _algorithmOrchestrator = new AlgorithmOrchestrator(new List<IPlayerAlgorithm>
            {
                new ChangeWhenLosingPreviousRoundAlgorithm(Item.Paper),
                new ChangeWhenLosingPreviousRoundAlgorithm(Item.Rock),
                new ChangeWhenLosingPreviousRoundAlgorithm(Item.Scissors),
                new ChangeWhenLosingPreviousRoundCounterAlgorithm(Item.Paper),
                new ChangeWhenLosingPreviousRoundCounterAlgorithm(Item.Rock),
                new ChangeWhenLosingPreviousRoundCounterAlgorithm(Item.Scissors),
                new RandomByNumberOfItemsAlgorithm(),
            });
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

        private const int MAX_MEMORY = 20;

        #endregion

        #region Fields

        private readonly IList<IPlayerAlgorithm> _algoList;
        private readonly IList<bool[]> _pendingForDeletionResult;

        #endregion

        #region Methods

        #region Constructors

        public AlgorithmOrchestrator(IList<IPlayerAlgorithm> algoList)
        {
            _algoList = algoList;
            
            _pendingForDeletionResult = new List<bool[]>(MAX_MEMORY);
        }

        #endregion

        public Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            var roundElapsed = yourPastItems.Count;
            RpsHelper.Log($"[ROUND {roundElapsed + 1}]");

            // Update the stats first.
            if (roundElapsed > 0)
            {
                updateStats(opponentsPastItems.LastOrDefault(), roundElapsed);
            }

            // Forget win rates when the max memory has been reached.
            applyMemory(roundElapsed);

            // Just showing some debug information.
            foreach (var algoItem in _algoList)
            {
                RpsHelper.Log($"WIN={algoItem.WinCount} | CHANCE={algoItem.GetChanceRate(roundElapsed):n2} | {algoItem.GetType()}");
            }

            // Performs the move for all algorithm.
            foreach (var algoItem in _algoList)
            {
                algoItem.GetItem(yourPastItems, opponentsPastItems);
            }

            // Get the algorithm with the highest win rate.
            return pickAlgorithm(roundElapsed).PreviousItem;
        }

        /// <summary>
        ///     Selects the algorithm with the highest chance rate.
        ///     The algorithm itself defines its chance rate.
        /// </summary>
        /// <param name="roundElapsed">The number of rounds elapsed.</param>
        /// <returns>The optimal algorithm to used.</returns>
        private IPlayerAlgorithm pickAlgorithm(int roundElapsed)
        {
            IPlayerAlgorithm selectedAlgorithm = null;
            var selectedAlgorithmChanceRate = default(float);
            foreach (var algoItem in _algoList)
            {
                var chanceRate = algoItem.GetChanceRate(roundElapsed);

                if (selectedAlgorithm != null &&
                    selectedAlgorithmChanceRate > chanceRate)
                {
                    continue;
                }

                selectedAlgorithm = algoItem;
                selectedAlgorithmChanceRate = chanceRate;
            }

            RpsHelper.Log($"Using {selectedAlgorithm?.GetType()}");
            return selectedAlgorithm;
        }

        /// <summary>
        ///     Forgets the result of a turn when max memory has been reached.
        /// </summary>
        /// <param name="roundElapsed">The number of rounds elapsed.</param>
        private void applyMemory(int roundElapsed)
        {
            if (roundElapsed <= MAX_MEMORY)
            {
                return;
            }

            var memoryResultList = _pendingForDeletionResult.FirstOrDefault();

            for (var i = 0; i < memoryResultList.Length; i++)
            {
                var algoItem = _algoList[i];

                // Skip those algorithms who are statistics-based
                // and won't use memory.
                if (!algoItem.UseMemory)
                {
                    continue;
                }

                algoItem.UpdateWinCount(!memoryResultList[i], roundElapsed);
            }

            RpsHelper.Log($"Forgetting result {string.Join(",", memoryResultList)}");
            _pendingForDeletionResult.RemoveAt(0);
        }

        private void updateStats(Item opponentsLastItem, int roundElapsed)
        {
            var resultList = new bool[_algoList.Count];
            for (var i = 0; i < _algoList.Count; i++)
            {
                var algoItem = _algoList[i];
                var isRoundWon = RpsHelper.IsRoundWon(algoItem.PreviousItem, opponentsLastItem);
                algoItem.UpdateWinCount(isRoundWon, roundElapsed);
                resultList[i] = isRoundWon;
            }

            _pendingForDeletionResult.Add(resultList);
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

        bool UseMemory { get; set; }

        int WinCount { get; }

        Item PreviousItem { get; set; }

        #endregion

        #region Methods

        float GetChanceRate(int roundElapsed);

        void UpdateWinCount(bool hasWon, int roundElapsed);

        #endregion
    }

    public abstract class PlayerAlgorithm : IPlayerAlgorithm
    {
        #region Properties

        public bool UseMemory { get; set; } = true;

        public int WinCount { get; protected set; }

        public Item PreviousItem { get; set; }

        #endregion

        #region Methods

        public abstract Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems);

        public virtual float GetChanceRate(int roundElapsed)
        {
            if (roundElapsed == 0)
            {
                return 1f;
            }

            return (float)WinCount / roundElapsed;
        }

        public virtual void UpdateWinCount(bool hasWon, int roundElapsed)
        {
            if (hasWon)
            {
                WinCount++;
            }
            else
            {
                WinCount--;
            }
        }

        #endregion
    }

    /// <summary>
    ///     Algorithm to counter <see cref="ChangeWhenLosingPreviousRoundAlgorithm" />.
    /// </summary>
    public class ChangeWhenLosingPreviousRoundCounterAlgorithm : PlayerAlgorithm
    {
        #region Fields

        private Item _previousItem;

        #endregion

        #region Methods

        #region Constructors

        public ChangeWhenLosingPreviousRoundCounterAlgorithm(Item initialItem)
        {
            _previousItem = initialItem;
        }

        #endregion

        public override Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            PreviousItem = getMove(yourPastItems, opponentsPastItems);
            return PreviousItem;
        }

        private Item getMove(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            if (yourPastItems.Count == 0)
            {
                return _previousItem;
            }

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
    public class ChangeWhenLosingPreviousRoundAlgorithm : PlayerAlgorithm
    {
        #region Fields

        private Item _previousItem;

        #endregion

        #region Methods

        #region Constructors

        public ChangeWhenLosingPreviousRoundAlgorithm(Item initialItem)
        {
            _previousItem = initialItem;
        }

        #endregion

        public override Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            PreviousItem = getMove(yourPastItems, opponentsPastItems);
            return PreviousItem;
        }

        private Item getMove(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            if (yourPastItems.Count == 0)
            {
                return _previousItem;
            }

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

    /// <summary>
    ///     This algorithm randomly get from opponents past items and return a counter for it.
    /// </summary>   
    public class RandomByNumberOfItemsAlgorithm : PlayerAlgorithm
    {
        #region Constants

        private const int ACTIVATION_START_TURN = 100;

        #endregion

        #region Methods

        #region Constructors

        public RandomByNumberOfItemsAlgorithm()
        {
            UseMemory = false;
        }

        #endregion

        public override Item GetItem(List<Item> yourPastItems, List<Item> opponentsPastItems)
        {
            PreviousItem = getMove(opponentsPastItems);
            return PreviousItem;
        }
        
        /// <summary>
        ///     This algorithm randomly get from opponents past items and return a counter for it.
        /// </summary>        
        private Item getMove(List<Item> opponentsPastItems)
        {
            double totalItem = opponentsPastItems.Count;
            if (totalItem >= ACTIVATION_START_TURN)
            {
                var ourPick = new Item();

                // Query the most picked item.
                var itemQuery = opponentsPastItems.GroupBy(x => x)
                    .Select(group => new { ItemType = group.Key, Count = group.Count() })
                    .OrderByDescending(x => x.Count);

                // Get the top most picked item
                var mostPickedItem = itemQuery.First();

                if (mostPickedItem.ItemType == Item.Paper)
                {
                    ourPick = Item.Scissors;
                }
                else if (mostPickedItem.ItemType == Item.Rock)
                {
                    ourPick = Item.Paper;
                }
                else if (mostPickedItem.ItemType == Item.Scissors)
                {
                    ourPick = Item.Rock;
                }
                return ourPick;
            }

            return Item.Paper;
        }

        public override float GetChanceRate(int roundElapsed)
        {
            // Only use this algorithm for turn 50 above.
            if (roundElapsed < ACTIVATION_START_TURN)
            {
                return -100f;
            }

            if (roundElapsed == ACTIVATION_START_TURN)
            {
                return 1f;
            }

            return (float)WinCount / (roundElapsed - ACTIVATION_START_TURN);
        }

        public override void UpdateWinCount(bool hasWon, int roundElapsed)
        {
            // Don't update the state yet until it reaches the target turn no.
            if (roundElapsed < ACTIVATION_START_TURN+1)
            {
                return;
            }

            if (hasWon)
            {
                WinCount++;
            }
            else
            {
                WinCount--;
            }
        }

        #endregion
    }


    #endregion


    #region Helper Classes =======================================================================

    public static class RpsHelper
    {
        #region Constants

        private const bool ENABLE_DEBUG = false;

        #endregion

        #region Methods

        public static void Log(string message)
        {
            if (!ENABLE_DEBUG)
            {
                return;
            }

            Console.WriteLine($"\t\t[DEBUG] {message}");
        }

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