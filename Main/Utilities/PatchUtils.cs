using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TNHTweaker.Utilities
{
    public static class PatchUtils
    {

        public static void RemoveStartToEnd(
            ILCursor cursor, 
            Func<Instruction,bool>[] startingPredicates, 
            Func<Instruction, bool>[] endingPredicates)
        {
            cursor.GotoNext(MoveType.Before, startingPredicates);
            int startingIndex = cursor.Index;

            cursor.GotoNext(MoveType.After, endingPredicates);
            int endingIndex = cursor.Index;

            int removalLength = endingIndex - startingIndex;
            Debug.Log("We are about to remove # instructions: " + removalLength);

            cursor.Index = startingIndex;
            cursor.RemoveRange(removalLength);
        }

    }
}
