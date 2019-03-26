using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace com.blargs.raft.raftattack.Patches
{
    [HarmonyPatch(typeof(Shark))]
    [HarmonyPatch("ChangeState")]
    [HarmonyPatch(new Type[] { typeof(SharkState) })]
    class ChangeStatePatch
    {

        static void Postfix(Shark __instance, SharkState newState)
        {
            switch(newState)
            {
                case SharkState.AttackRaft:
#if DEBUG
                    RConsole.Log(newState.ToString());
#endif
                    RaftAttack.SetActive(__instance);
                    break;
                default:
                    break;
            }
        }
    }
}
