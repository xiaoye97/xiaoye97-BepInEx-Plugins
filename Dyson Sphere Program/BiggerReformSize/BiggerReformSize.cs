using BepInEx;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace BiggerReformSize
{
    [BepInPlugin("me.xiaoye97.plugin.Dsyon.BiggerReformSize", "BiggerReformSize", "1.1")]
    public class BiggerReformSize : BaseUnityPlugin
    {
        private const int size = 20;

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(BiggerReformSize));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlayerAction_Build), MethodType.Constructor)]
        public static void SizePatch(PlayerAction_Build __instance)
        {
            __instance.reformIndices = new int[size * size];
            __instance.reformPoints = new UnityEngine.Vector3[size * size];
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(PlayerAction_Build), "DetermineBuildPreviews")]
        public static IEnumerable<CodeInstruction> SizePatch2(IEnumerable<CodeInstruction> instructions)
        {
            UnityEngine.Debug.Log("[BiggerReformSize]Patch PlayerAction_Build.DetermineBuildPreviews");
            var codes = instructions.ToList();
            codes[4804].opcode = OpCodes.Ldc_I4_S;
            codes[4804].operand = size;
            codes[4807].opcode = OpCodes.Ldc_I4_S;
            codes[4807].operand = size;
            codes[4853].opcode = OpCodes.Ldc_I4_S;
            codes[4853].operand = size;
            return codes.AsEnumerable();
        }
    }
}