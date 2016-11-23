using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XPSweeper
{
    class MFAlgorithm
    {
        private static readonly int[,] Adjacent =
        {
            {-1, -1 }, {-1,  0 }, {-1,  1 }, { 0, -1 }, { 0,  1 }, { 1, -1 }, { 1,  0 },{ 1,  1 }
        };

        public static void MarkByAssumption(int[,] mfArr)
        {
            int mh = mfArr.GetLength(1);
            int mw = mfArr.GetLength(0);
            for (int y = 0; y < mh; y++)
            {
                for (int x = 0; x < mw; x++)
                {
                    if (mfArr[x, y] > 0)
                    {
                        int mineCount = 0;
                        // count adjacent mines
                        for (int i = 0; i < 8; i++)
                        {
                            int ax = x + Adjacent[i, 0];
                            int ay = y + Adjacent[i, 1];
                            if (ax >= 0 && ax < mw && ay >= 0 && ay < mh)
                                if (mfArr[ax, ay] == -2)
                                    mineCount++;
                        }
                        // if there are mines around that are unflagged
                        if (mineCount < mfArr[x, y])
                        {
                            // find all adjacent cells that could be mines
                            List<Point> unknown = new List<Point>();
                            for (int i = 0; i < 8; i++)
                            {
                                int ax = x + Adjacent[i, 0];
                                int ay = y + Adjacent[i, 1];
                                if (ax >= 0 && ax < mw && ay >= 0 && ay < mh)
                                    if (mfArr[ax, ay] == -1)
                                        unknown.Add(new Point(ax, ay));
                            }
                            // create array of cell permutations
                            int[] midx = new int[unknown.Count];
                            int[] pidx = new int[unknown.Count];

                            for (int i = 0; i < midx.Length; i++)
                                midx[i] = i;

                            bool first = true;
                            bool found = false;
                            
                            while (NextPermutation(midx))
                            {
                                bool possible = true;
                                
                                // for every adjacent cell that could have a mine considered
                                for (int i = 0; i < mfArr[x, y] - mineCount; i++)
                                {
                                    Point p = unknown[i];
                                    // for every adjacent cell to this adjacent cell
                                    for (int j = 0; j < 8; j++)
                                    {
                                        int jx = p.X + Adjacent[j, 0];
                                        int jy = p.Y + Adjacent[j, 1];
                                        if (jx >= 0 && jx < mw && jy >= 0 && jy < mh)
                                        {
                                            if (mfArr[jx, jy] > 0)
                                            {
                                                // count adjacent mines to this cell
                                                int projMineCount = 0;
                                                for (int k = 0; k < 8; k++)
                                                {
                                                    int kx = jx + Adjacent[k, 0];
                                                    int ky = jy + Adjacent[k, 1];
                                                    if (kx >= 0 && kx < mw && ky >= 0 && ky < mh)
                                                    {
                                                        if (mfArr[kx, ky] == -2)
                                                            projMineCount++;
                                                        for(int l = 0; l < mfArr[x,y] - mineCount; l++)
                                                            if (unknown[l].X == kx && unknown[l].Y == ky)
                                                                projMineCount++;
                                                    }
                                                }
                                                if (projMineCount != mfArr[jx, jy])
                                                {
                                                    possible = false;
                                                    break ;
                                                }
                                            }
                                        }
                                    }
                                    if (!possible) break;
                                }
                                if (possible)
                                {
                                    
                                    if (first)
                                    {
                                        Array.Copy(midx, 0, pidx, 0, midx.Length);
                                        first = false;
                                        found = true;
                                    }
                                    else
                                    {
                                        found = false;
                                        break;
                                    }
                                }
                            }
                            if (found)
                            {
                                for (int i = 0; i < mfArr[x, y] - mineCount; i++)
                                {
                                    Point p = unknown[pidx[i]];
                                    mfArr[p.X, p.Y] = -3;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void ClearKnown(int[,] mfArr)
        {
            int mh = mfArr.GetLength(1);
            int mw = mfArr.GetLength(0);

            for (int y = 0; y < mh; y++)
            {
                for (int x = 0; x < mw; x++)
                {
                    if (mfArr[x, y] > 0)
                    {
                        int mineCount = 0;
                        for (int i = 0; i < 8; i++)
                        {
                            int ax = x + Adjacent[i, 0];
                            int ay = y + Adjacent[i, 1];
                            if (ax >= 0 && ax < mw && ay >= 0 && ay < mh)
                                if (mfArr[ax, ay] == -2)
                                    mineCount++;
                        }
                        if (mineCount == mfArr[x, y])
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                int ax = x + Adjacent[i, 0];
                                int ay = y + Adjacent[i, 1];
                                if (ax >= 0 && ax < mw && ay >= 0 && ay < mh)
                                    if (mfArr[ax, ay] == -1)
                                        mfArr[ax, ay] = -3;
                            }
                        }
                    }
                }
            }
        }
        public static bool IdentifyAdjMines(int[,] mfArr)
        {
            int mh = mfArr.GetLength(1);
            int mw = mfArr.GetLength(0);
            
            bool f = false;
            for (int y = 0; y < mh; y++)
            {
                for (int x = 0; x < mw; x++)
                {
                    if (mfArr[x, y] > 0)
                    {
                        int adjCount = 0;

                        for (int i = 0; i < 8; i++)
                        {
                            int ax = x + Adjacent[i, 0];
                            int ay = y + Adjacent[i, 1];
                            if (ax >= 0 && ax < mw && ay >= 0 && ay < mh)
                                if (mfArr[ax, ay] < 0) // not empty
                                    adjCount++;
                        }
                        if (adjCount == mfArr[x, y])
                        {
                            for (int i = 0; i < 8; i++)
                            {
                                int ax = x + Adjacent[i, 0];
                                int ay = y + Adjacent[i, 1];
                                if (ax >= 0 && ax < mw && ay >= 0 && ay < mh)
                                    if (mfArr[ax, ay] < 0)
                                        if (mfArr[ax, ay] != -2)
                                        {
                                            f = true;
                                            mfArr[ax, ay] = -2;
                                        }
                            }
                        }
                    }
                }
            }
            return f;
        }

        private static bool NextPermutation(int[] numList)
        {
            /*
             Knuths
             1. Find the largest index j such that a[j] < a[j + 1]. If no such index exists, the permutation is the last permutation.
             2. Find the largest index l such that a[j] < a[l]. Since j + 1 is such an index, l is well defined and satisfies j < l.
             3. Swap a[j] with a[l].
             4. Reverse the sequence from a[j + 1] up to and including the final element a[n].

             */
            var largestIndex = -1;
            for (var i = numList.Length - 2; i >= 0; i--)
            {
                if (numList[i] < numList[i + 1])
                {
                    largestIndex = i;
                    break;
                }
            }

            if (largestIndex < 0) return false;

            var largestIndex2 = -1;
            for (var i = numList.Length - 1; i >= 0; i--)
            {
                if (numList[largestIndex] < numList[i])
                {
                    largestIndex2 = i;
                    break;
                }
            }

            var tmp = numList[largestIndex];
            numList[largestIndex] = numList[largestIndex2];
            numList[largestIndex2] = tmp;

            for (int i = largestIndex + 1, j = numList.Length - 1; i < j; i++, j--)
            {
                tmp = numList[i];
                numList[i] = numList[j];
                numList[j] = tmp;
            }

            return true;
        }
    }
}
