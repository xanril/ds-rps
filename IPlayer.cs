using System;
using System.Collections.Generic;
using System.Text;

namespace RPS
{
    public interface IPlayer
    {
        public Item GetItem(List<Item> yourPastItems,List<Item> opponentsPastItems);
    }
}
