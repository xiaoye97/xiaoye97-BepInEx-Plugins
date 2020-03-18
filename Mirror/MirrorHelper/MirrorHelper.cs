using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using DG.Tweening;

namespace MirrorHelper
{
    [BepInPlugin("me.xiaoye97.plugin.MirrorHelper", "MirrorHelper", "1.0")]
    public class MirrorHelper : BaseUnityPlugin
    {
        void Start()
        {
            new Harmony("me.xiaoye97.plugin.MirrorHelper").PatchAll();
        }

        /// <summary>
        /// 判断两个点是否相同(根据点和偏移量)
        /// </summary>
        private static bool SameP(int r, int c, int rp, int cp)
        {
            if (r < 0 || r > 6) return false;
            if (r + rp < 0 || r + rp > 6) return false;
            if (c < 0 || c > 7) return false;
            if (c + cp < 0 || c + cp > 7) return false;
            return StarBox.Instance.StarTable[r, c].IsSameType(StarBox.Instance.StarTable[r + rp, c + cp]);
        }

        /// <summary>
        /// 判断两个点是否相同(根据两点)
        /// </summary>
        private static bool Same(int r1, int c1, int r2, int c2)
        {
            if (r1 < 0 || r1 > 6) return false;
            if (r2 < 0 || r2 > 6) return false;
            if (c1 < 0 || c1 > 7) return false;
            if (c2 < 0 || c2 > 7) return false;
            return StarBox.Instance.StarTable[r1, c1].IsSameType(StarBox.Instance.StarTable[r2, c2]);
        }

        /// <summary>
        /// 判断多个点是否相同
        /// </summary>
        private static bool MSame(int[] rs, int[] cs)
        {
            for(int i = 1; i < rs.Length; i++)
            {
                if (!Same(rs[0], cs[0], rs[i], cs[i])) return false;
            }
            return true;
        }

        /// <summary>
        /// 检查是否可以5颗连线变成S级宝石
        /// </summary>
        private static bool CheckSGem(int r, int c)
        {
            //竖向
            if (SameP(r - 1, c, -1, 0) && SameP(r + 1, c, 1, 0) && SameP(r - 1, c, 2, 0) && (SameP(r - 1, c, 1, 1) || SameP(r - 1, c, 1, -1)))
            {
                return true;
            }
            //横向
            if (SameP(r, c - 1, 0, -1) && SameP(r, c + 1, 0, 1) && SameP(r, c - 1, 0, 2) && (SameP(r, c - 1, 1, 1) || SameP(r, c - 1, -1, 1)))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查是否可以组成T字
        /// </summary>
        private static bool CheckT(int r, int c)
        {
            //上
            if (MSame(new int[] { r - 1, r - 2, r, r, r + 1 }, new int[] { c, c, c - 1, c + 1, c })) return true;
            //下
            if (MSame(new int[] { r + 1, r + 2, r, r, r - 1 }, new int[] { c, c, c + 1, c - 1, c })) return true;
            //左
            if (MSame(new int[] { r, r, r - 1, r + 1, r }, new int[] { c - 1, c - 2, c, c, c + 1 })) return true;
            //右
            if (MSame(new int[] { r, r, r + 1, r - 1, r }, new int[] { c + 1, c + 2, c, c, c - 1 })) return true;
            return false;
        }

        public static void Check()
        {
            try
            {
                for (int row = 0; row < StarBox.Instance.NumX; row++)
                {
                    for (int col = 0; col < StarBox.Instance.NumY; col++)
                    {
                        //检查是否可以5颗连线变成S级宝石
                        if (CheckSGem(row, col))
                        {
                            Sequence sequence = DOTween.Sequence();
                            sequence.Append(StarBox.Instance.StarTable[row, col].SpriteObj.transform.DOScale(new Vector3(2f, 2f, 2f), 0.1f));
                            sequence.Append(StarBox.Instance.StarTable[row, col].SpriteObj.transform.DOScale(Vector3.one, 0.1f));
                        }
                        else if(CheckT(row, col))
                        {
                            StarBox.Instance.StarTable[row, col].JumpAnimation();
                        }
                    }
                }
            }
            catch(Exception e)
            {

            }
        }

        [HarmonyPatch(typeof(StarBox), "Update")]
        class StarPatch
        {
            public static float coolTime = 1f;
            public static void Postfix()
            {
                coolTime -= Time.deltaTime;
                if (coolTime < 0)
                {
                    if (StarBox.Instance.CanTouch)
                    {
                        Check();
                        coolTime = 1f;
                    }
                }
            }
        }
    }
}
