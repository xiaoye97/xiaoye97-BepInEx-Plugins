using BepInEx;
using HarmonyLib;
using System.Linq;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace BiggerReformSize
{
    [BepInPlugin("me.xiaoye97.plugin.Dsyon.BiggerReformSize", "BiggerReformSize", "1.2")]
    public class BiggerReformSize : BaseUnityPlugin
    {
        private const int size = 20;

        private void Start()
        {
            Harmony.CreateAndPatchAll(typeof(BiggerReformSize));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BuildTool_Reform), MethodType.Constructor)]
        public static void SizePatch(BuildTool_Reform __instance)
        {
            __instance.cursorIndices = new int[size * size];
            __instance.cursorPoints = new UnityEngine.Vector3[size * size];
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(BuildTool_Reform), "ReformAction")]
        public static IEnumerable<CodeInstruction> SizePatch2(IEnumerable<CodeInstruction> instructions)
        {
            UnityEngine.Debug.Log("[BiggerReformSize]Patch BuildTool_Reform.ReformAction");
            var codes = instructions.ToList();
            codes[21].opcode = OpCodes.Ldc_I4_S;
            codes[21].operand = size;
            codes[24].opcode = OpCodes.Ldc_I4_S;
            codes[24].operand = size;
            codes[72].opcode = OpCodes.Ldc_I4_S;
            codes[72].operand = size;
            return codes.AsEnumerable();
        }
    }
}