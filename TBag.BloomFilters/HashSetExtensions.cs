﻿

namespace TBag.BloomFilters
{
    using System.Collections.Generic;
    using System.Linq;
  
    internal static class HashSetExtensions
    {
        /// <summary>
        /// Move modified keys to the modified list.
        /// </summary>
        /// <typeparam name="TId">Type of the key</typeparam>
        /// <param name="modifiedEntities">The modified entities</param>
        /// <param name="listA">Identifiers only in the first set</param>
        /// <param name="listB">Identifiers only in the second set</param>
        internal static void MoveModified<TId>(this HashSet<TId> modifiedEntities, HashSet<TId> listA, HashSet<TId> listB)
        {
            if (listA == modifiedEntities || listB == modifiedEntities) return;
            foreach (var modItem in listA.Where(itm => listB.Contains(itm)).ToArray())
            {
                modifiedEntities.Add(modItem);
            }
            foreach (var modItem in modifiedEntities)
            {
                listA.Remove(modItem);
                listB.Remove(modItem);
            }
        }
    }
}