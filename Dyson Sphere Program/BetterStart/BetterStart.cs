using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace BetterStart
{
    [BepInPlugin("me.xiaoye97.plugin.Dyson.BetterStart", "BetterStart", "1.0")]
    public class BetterStart : BaseUnityPlugin
    {
        public static int StarCount = 10;
        public static bool EnableBirthTidalLocked = true;

        void Start()
        {
            Harmony.CreateAndPatchAll(typeof(BetterStart));
        }

        /// <summary>
        /// 自定义初始星球
        /// </summary>
        /// <param name="planetData"></param>
        public static void CustomBirthPlanet(PlanetData planetData)
        {
            //潮汐锁定
            if (EnableBirthTidalLocked)
            {
                planetData.obliquity = 0;
                planetData.orbitAround = 0;
                planetData.orbitInclination = 0;
                planetData.rotationPeriod = planetData.orbitalPeriod;
                planetData.singularity |= EPlanetSingularity.TidalLocked;
            }
            planetData.overrideName = "起始星";
        }

        #region 星系数量上限
        [HarmonyPostfix, HarmonyPatch(typeof(UIGalaxySelect), "_OnOpen")]
        public static void GalaxySelectPatch(UIGalaxySelect __instance)
        {
            var slider = Traverse.Create(__instance).Field("starCountSlider").GetValue<Slider>();
            slider.maxValue = 255;
        }

        [HarmonyTranspiler, HarmonyPatch(typeof(UIGalaxySelect), "OnStarCountSliderValueChange")]
        public static IEnumerable<CodeInstruction> GalaxySelectPatch2(IEnumerable<CodeInstruction> instructions)
        {
            UnityEngine.Debug.Log("[UniverseEx]Patch UIGalaxySelect.OnStarCountSliderValueChange");
            var codes = instructions.ToList();
            codes[14].opcode = OpCodes.Ldc_I4;
            codes[14].operand = 255;
            codes[16].opcode = OpCodes.Ldc_I4;
            codes[16].operand = 255;
            return codes.AsEnumerable();
        }
        #endregion

        #region 初始星系随机
        [HarmonyPrefix, HarmonyPatch(typeof(UniverseGen), "CreateGalaxy")]
        public static bool UniverseGenPatch(ref GalaxyData __result, GameDesc gameDesc)
        {
            __result = CreateGalaxy(gameDesc);
            return false;
        }

        public static GalaxyData CreateGalaxy(GameDesc gameDesc)
        {
            int galaxyAlgo = gameDesc.galaxyAlgo;
            int galaxySeed = gameDesc.galaxySeed;
            int num = gameDesc.starCount;
            if (galaxyAlgo < 20200101 || galaxyAlgo > 20591231)
            {
                throw new Exception("Wrong version of unigen algorithm!");
            }
            System.Random random = new System.Random(galaxySeed);
            int seed = random.Next();
            num = Traverse.Create(typeof(UniverseGen)).Method("GenerateTempPoses", seed, num, 4, 2.0, 2.3, 3.5, 0.18).GetValue<int>();
            GalaxyData galaxyData = new GalaxyData();
            galaxyData.seed = galaxySeed;
            galaxyData.starCount = num;
            galaxyData.stars = new StarData[num];
            Assert.Positive(num);
            if (num <= 0)
            {
                return galaxyData;
            }
            float num2 = (float)random.NextDouble();
            float num3 = (float)random.NextDouble();
            float num4 = (float)random.NextDouble();
            float num5 = (float)random.NextDouble();
            int num6 = Mathf.CeilToInt(0.01f * (float)num + num2 * 0.3f);
            int num7 = Mathf.CeilToInt(0.01f * (float)num + num3 * 0.3f);
            int num8 = Mathf.CeilToInt(0.016f * (float)num + num4 * 0.4f);
            int num9 = Mathf.CeilToInt(0.013f * (float)num + num5 * 1.4f);
            int num10 = num - num6;
            int num11 = num10 - num7;
            int num12 = num11 - num8;
            int num13 = (num12 - 1) / num9;
            int num14 = num13 / 2;
            for (int i = 0; i < num; i++)
            {
                int seed2 = random.Next();
                ESpectrType needSpectr = ESpectrType.X;
                if (i == 3)
                {
                    needSpectr = ESpectrType.M;
                }
                else if (i == num12 - 1)
                {
                    needSpectr = ESpectrType.O;
                }
                EStarType needtype = EStarType.MainSeqStar;
                if (i % num13 == num14)
                {
                    needtype = EStarType.GiantStar;
                }
                if (i >= num10)
                {
                    needtype = EStarType.BlackHole;
                }
                else if (i >= num11)
                {
                    needtype = EStarType.NeutronStar;
                }
                else if (i >= num12)
                {
                    needtype = EStarType.WhiteDwarf;
                }
                galaxyData.stars[i] = StarGen.CreateStar(galaxyData, Traverse.Create(typeof(UniverseGen)).Field("tmp_poses").GetValue<List<VectorLF3>>()[i], i + 1, seed2, needtype, needSpectr);
            }
            AstroPose[] astroPoses = galaxyData.astroPoses;
            StarData[] stars = galaxyData.stars;
            for (int j = 0; j < galaxyData.astroPoses.Length; j++)
            {
                astroPoses[j].uRot.w = 1f;
                astroPoses[j].uRotNext.w = 1f;
            }
            for (int k = 0; k < num; k++)
            {
                StarGen.CreateStarPlanets(galaxyData, stars[k], gameDesc);
                astroPoses[stars[k].id * 100].uPos = (astroPoses[stars[k].id * 100].uPosNext = stars[k].uPosition);
                astroPoses[stars[k].id * 100].uRot = (astroPoses[stars[k].id * 100].uRotNext = Quaternion.identity);
                astroPoses[stars[k].id * 100].uRadius = stars[k].physicsRadius;
            }
            galaxyData.UpdatePoses(0.0);
            galaxyData.birthPlanetId = 0;
            if (num > 0)
            {
                StarData starData = stars[0];
                for (int l = 0; l < starData.planetCount; l++)
                {
                    PlanetData planetData = starData.planets[l];
                    ThemeProto themeProto = LDB.themes.Select(planetData.theme);
                    if (themeProto != null && themeProto.Distribute == EThemeDistribute.Birth)
                    {
                        galaxyData.birthPlanetId = planetData.id;
                        galaxyData.birthStarId = starData.id;
                        CustomBirthPlanet(planetData);
                        break;
                    }
                }
            }
            Assert.Positive(galaxyData.birthPlanetId);
            for (int m = 0; m < num; m++)
            {
                StarData starData2 = galaxyData.stars[m];
                for (int n = 0; n < starData2.planetCount; n++)
                {
                    PlanetData planet = starData2.planets[n];
                    PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(planet);
                    planetAlgorithm.GenerateVeins(true);
                }
            }
            UniverseGen.CreateGalaxyStarGraph(galaxyData);
            return galaxyData;
        }
        #endregion

        #region 初始星系星数
        [HarmonyPrefix, HarmonyPatch(typeof(StarGen), "CreateStarPlanets")]
        public static bool CreateStarPlanetsPatch(GalaxyData galaxy, StarData star, GameDesc gameDesc)
        {
            CreateStarPlanets(galaxy, star, gameDesc);
            return false;
        }

        public static void SetPGas(int index, double value)
        {
            var pGas = Traverse.Create(typeof(StarGen)).Field("pGas").GetValue<double[]>();
            pGas[index] = value;
            Traverse.Create(typeof(StarGen)).Field("pGas").SetValue(pGas);
        }

        public static void CreateStarPlanets(GalaxyData galaxy, StarData star, GameDesc gameDesc)
        {
            Traverse.Create(typeof(StarGen)).Field("pGas").SetValue(new double[100]);
            System.Random random = new System.Random(star.seed);
            random.Next();
            random.Next();
            random.Next();
            int seed = random.Next();
            System.Random random2 = new System.Random(seed);
            double num = random2.NextDouble();
            double num2 = random2.NextDouble();
            double num3 = random2.NextDouble();
            double num4 = random2.NextDouble();
            double num5 = random2.NextDouble();
            double num6 = random2.NextDouble() * 0.2 + 0.9;
            double num7 = random2.NextDouble() * 0.2 + 0.9;
            if (star.type == EStarType.BlackHole)
            {
                star.planetCount = 1;
                star.planets = new PlanetData[star.planetCount];
                int info_seed = random2.Next();
                int gen_seed = random2.Next();
                star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, false, info_seed, gen_seed);
            }
            else if (star.type == EStarType.NeutronStar)
            {
                star.planetCount = 1;
                star.planets = new PlanetData[star.planetCount];
                int info_seed2 = random2.Next();
                int gen_seed2 = random2.Next();
                star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, false, info_seed2, gen_seed2);
            }
            else if (star.type == EStarType.WhiteDwarf)
            {
                if (num < 0.699999988079071)
                {
                    star.planetCount = 1;
                    star.planets = new PlanetData[star.planetCount];
                    int info_seed3 = random2.Next();
                    int gen_seed3 = random2.Next();
                    star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, false, info_seed3, gen_seed3);
                }
                else
                {
                    star.planetCount = 2;
                    star.planets = new PlanetData[star.planetCount];
                    if (num2 < 0.30000001192092896)
                    {
                        int info_seed4 = random2.Next();
                        int gen_seed4 = random2.Next();
                        star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, false, info_seed4, gen_seed4);
                        info_seed4 = random2.Next();
                        gen_seed4 = random2.Next();
                        star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, 4, 2, false, info_seed4, gen_seed4);
                    }
                    else
                    {
                        int info_seed4 = random2.Next();
                        int gen_seed4 = random2.Next();
                        star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 4, 1, true, info_seed4, gen_seed4);
                        info_seed4 = random2.Next();
                        gen_seed4 = random2.Next();
                        star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 1, 1, 1, 1, false, info_seed4, gen_seed4);
                    }
                }
            }
            else if (star.type == EStarType.GiantStar)
            {
                if (num < 0.30000001192092896)
                {
                    star.planetCount = 1;
                    star.planets = new PlanetData[star.planetCount];
                    int info_seed5 = random2.Next();
                    int gen_seed5 = random2.Next();
                    star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (num3 <= 0.5) ? 2 : 3, 1, false, info_seed5, gen_seed5);
                }
                else if (num < 0.800000011920929)
                {
                    star.planetCount = 2;
                    star.planets = new PlanetData[star.planetCount];
                    if (num2 < 0.25)
                    {
                        int info_seed6 = random2.Next();
                        int gen_seed6 = random2.Next();
                        star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (num3 <= 0.5) ? 2 : 3, 1, false, info_seed6, gen_seed6);
                        info_seed6 = random2.Next();
                        gen_seed6 = random2.Next();
                        star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, (num3 <= 0.5) ? 3 : 4, 2, false, info_seed6, gen_seed6);
                    }
                    else
                    {
                        int info_seed6 = random2.Next();
                        int gen_seed6 = random2.Next();
                        star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, 3, 1, true, info_seed6, gen_seed6);
                        info_seed6 = random2.Next();
                        gen_seed6 = random2.Next();
                        star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 1, 1, 1, 1, false, info_seed6, gen_seed6);
                    }
                }
                else
                {
                    star.planetCount = 3;
                    star.planets = new PlanetData[star.planetCount];
                    if (num2 < 0.15000000596046448)
                    {
                        int info_seed7 = random2.Next();
                        int gen_seed7 = random2.Next();
                        star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (num3 <= 0.5) ? 2 : 3, 1, false, info_seed7, gen_seed7);
                        info_seed7 = random2.Next();
                        gen_seed7 = random2.Next();
                        star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, (num3 <= 0.5) ? 3 : 4, 2, false, info_seed7, gen_seed7);
                        info_seed7 = random2.Next();
                        gen_seed7 = random2.Next();
                        star.planets[2] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 2, 0, (num3 <= 0.5) ? 4 : 5, 3, false, info_seed7, gen_seed7);
                    }
                    else if (num2 < 0.75)
                    {
                        int info_seed7 = random2.Next();
                        int gen_seed7 = random2.Next();
                        star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (num3 <= 0.5) ? 2 : 3, 1, false, info_seed7, gen_seed7);
                        info_seed7 = random2.Next();
                        gen_seed7 = random2.Next();
                        star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 1, 0, 4, 2, true, info_seed7, gen_seed7);
                        info_seed7 = random2.Next();
                        gen_seed7 = random2.Next();
                        star.planets[2] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 2, 2, 1, 1, false, info_seed7, gen_seed7);
                    }
                    else
                    {
                        int info_seed7 = random2.Next();
                        int gen_seed7 = random2.Next();
                        star.planets[0] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 0, 0, (num3 <= 0.5) ? 3 : 4, 1, true, info_seed7, gen_seed7);
                        info_seed7 = random2.Next();
                        gen_seed7 = random2.Next();
                        star.planets[1] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 1, 1, 1, 1, false, info_seed7, gen_seed7);
                        info_seed7 = random2.Next();
                        gen_seed7 = random2.Next();
                        star.planets[2] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, 2, 1, 2, 2, false, info_seed7, gen_seed7);
                    }
                }
            }
            else
            {
                if (star.index == 0)
                {
                    star.planetCount = StarCount;
                    for (int i = 0; i < star.planetCount; i++)
                    {
                        SetPGas(i, random.NextDouble() % 0.9);
                    }
                }
                else if (star.spectr == ESpectrType.M)
                {
                    if (num < 0.1)
                    {
                        star.planetCount = 1;
                    }
                    else if (num < 0.3)
                    {
                        star.planetCount = 2;
                    }
                    else if (num < 0.8)
                    {
                        star.planetCount = 3;
                    }
                    else
                    {
                        star.planetCount = 4;
                    }
                    if (star.planetCount <= 3)
                    {
                        SetPGas(0, 0.2);
                        SetPGas(1, 0.2);
                    }
                    else
                    {
                        SetPGas(0, 0);
                        SetPGas(1, 0.2);
                        SetPGas(2, 0.3);
                    }
                }
                else if (star.spectr == ESpectrType.K)
                {
                    if (num < 0.1)
                    {
                        star.planetCount = 1;
                    }
                    else if (num < 0.2)
                    {
                        star.planetCount = 2;
                    }
                    else if (num < 0.7)
                    {
                        star.planetCount = 3;
                    }
                    else if (num < 0.95)
                    {
                        star.planetCount = 4;
                    }
                    else
                    {
                        star.planetCount = 5;
                    }
                    if (star.planetCount <= 3)
                    {
                        SetPGas(0, 0.18);
                        SetPGas(1, 0.18);
                    }
                    else
                    {
                        SetPGas(0, 0);
                        SetPGas(1, 0.18);
                        SetPGas(2, 0.28);
                        SetPGas(3, 0.28);
                    }
                }
                else if (star.spectr == ESpectrType.G)
                {
                    if (num < 0.4)
                    {
                        star.planetCount = 3;
                    }
                    else if (num < 0.9)
                    {
                        star.planetCount = 4;
                    }
                    else
                    {
                        star.planetCount = 5;
                    }
                    if (star.planetCount <= 3)
                    {
                        SetPGas(0, 0.18);
                        SetPGas(1, 0.18);
                    }
                    else
                    {
                        SetPGas(0, 0);
                        SetPGas(1, 0.2);
                        SetPGas(2, 0.3);
                        SetPGas(3, 0.3);
                    }
                }
                else if (star.spectr == ESpectrType.F)
                {
                    if (num < 0.35)
                    {
                        star.planetCount = 3;
                    }
                    else if (num < 0.8)
                    {
                        star.planetCount = 4;
                    }
                    else
                    {
                        star.planetCount = 5;
                    }
                    if (star.planetCount <= 3)
                    {
                        SetPGas(0, 0.2);
                        SetPGas(1, 0.2);
                    }
                    else
                    {
                        SetPGas(0, 0);
                        SetPGas(1, 0.22);
                        SetPGas(2, 0.31);
                        SetPGas(3, 0.31);
                    }
                }
                else if (star.spectr == ESpectrType.A)
                {
                    if (num < 0.3)
                    {
                        star.planetCount = 3;
                    }
                    else if (num < 0.75)
                    {
                        star.planetCount = 4;
                    }
                    else
                    {
                        star.planetCount = 5;
                    }
                    if (star.planetCount <= 3)
                    {
                        SetPGas(0, 0.2);
                        SetPGas(1, 0.2);
                    }
                    else
                    {
                        SetPGas(0, 0.1);
                        SetPGas(1, 0.28);
                        SetPGas(2, 0.3);
                        SetPGas(3, 0.35);
                    }
                }
                else if (star.spectr == ESpectrType.B)
                {
                    if (num < 0.3)
                    {
                        star.planetCount = 4;
                    }
                    else if (num < 0.75)
                    {
                        star.planetCount = 5;
                    }
                    else
                    {
                        star.planetCount = 6;
                    }
                    if (star.planetCount <= 3)
                    {
                        SetPGas(0, 0.2);
                        SetPGas(0, 0.2);
                    }
                    else
                    {
                        SetPGas(0, 0.1);
                        SetPGas(1, 0.22);
                        SetPGas(2, 0.28);
                        SetPGas(3, 0.35);
                        SetPGas(4, 0.35);
                    }
                }
                else if (star.spectr == ESpectrType.O)
                {
                    if (num < 0.5)
                    {
                        star.planetCount = 5;
                    }
                    else
                    {
                        star.planetCount = 6;
                    }
                    SetPGas(0, 0.1);
                    SetPGas(1, 0.2);
                    SetPGas(2, 0.25);
                    SetPGas(3, 0.3);
                    SetPGas(4, 0.32);
                    SetPGas(5, 0.35);
                }
                else
                {
                    star.planetCount = 1;
                }
                star.planets = new PlanetData[star.planetCount];
                int num8 = 0;
                int num9 = 0;
                int num10 = 0;
                int num11 = 1;
                for (int i = 0; i < star.planetCount; i++)
                {
                    int info_seed8 = random2.Next();
                    int gen_seed8 = random2.Next();
                    double num12 = random2.NextDouble();
                    double num13 = random2.NextDouble();
                    bool flag = false;
                    if (num10 == 0)
                    {
                        num8++;
                        if (i < star.planetCount - 1 && num12 < Traverse.Create(typeof(StarGen)).Field("pGas").GetValue<double[]>()[i])
                        {
                            flag = true;
                            if (num11 < 3)
                            {
                                num11 = 3;
                            }
                        }
                        while (star.index != 0 || num11 != 3)
                        {
                            int num14 = star.planetCount - i;
                            int num15 = 9 - num11;
                            if (num15 > num14)
                            {
                                float num16 = (float)num14 / (float)num15;
                                if (num11 > 3)
                                {
                                    num16 = Mathf.Lerp(num16, 1f, 0.45f) + 0.01f;
                                }
                                else
                                {
                                    num16 = Mathf.Lerp(num16, 1f, 0.15f) + 0.01f;
                                }
                                double num17 = random2.NextDouble();
                                if (num17 >= (double)num16)
                                {
                                    num11++;
                                    continue;
                                }
                            }
                            goto IL_D1A;
                        }
                        flag = true;
                    }
                    else
                    {
                        num9++;
                        flag = false;
                    }
                IL_D1A:
                    star.planets[i] = PlanetGen.CreatePlanet(galaxy, star, gameDesc, i, num10, (num10 != 0) ? num9 : num11, (num10 != 0) ? num9 : num8, flag, info_seed8, gen_seed8);
                    num11++;
                    if (flag)
                    {
                        num10 = num8;
                        num9 = 0;
                    }
                    if (num9 >= 1 && num13 < 0.8)
                    {
                        num10 = 0;
                        num9 = 0;
                    }
                }
            }
            int num18 = 0;
            int num19 = 0;
            int num20 = 0;
            for (int j = 0; j < star.planetCount; j++)
            {
                if (star.planets[j].type == EPlanetType.Gas)
                {
                    num18 = star.planets[j].orbitIndex;
                    break;
                }
            }
            for (int k = 0; k < star.planetCount; k++)
            {
                if (star.planets[k].orbitAround == 0)
                {
                    num19 = star.planets[k].orbitIndex;
                }
            }
            if (num18 > 0)
            {
                int num21 = num18 - 1;
                bool flag2 = true;
                for (int l = 0; l < star.planetCount; l++)
                {
                    if (star.planets[l].orbitAround == 0 && star.planets[l].orbitIndex == num18 - 1)
                    {
                        flag2 = false;
                        break;
                    }
                }
                if (flag2 && num4 < 0.2 + (double)num21 * 0.2)
                {
                    num20 = num21;
                }
            }
            int num22;
            if (num5 < 0.2)
            {
                num22 = num19 + 3;
            }
            else if (num5 < 0.4)
            {
                num22 = num19 + 2;
            }
            else if (num5 < 0.8)
            {
                num22 = num19 + 1;
            }
            else
            {
                num22 = 0;
            }
            if (num22 != 0 && num22 < 5)
            {
                num22 = 5;
            }
            star.asterBelt1OrbitIndex = (float)num20;
            star.asterBelt2OrbitIndex = (float)num22;
            if (num20 > 0)
            {
                star.asterBelt1Radius = StarGen.orbitRadius[num20] * (float)num6 * star.orbitScaler;
            }
            if (num22 > 0)
            {
                star.asterBelt2Radius = StarGen.orbitRadius[num22] * (float)num7 * star.orbitScaler;
            }
        }
        #endregion
    }
}
