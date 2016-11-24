using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XPSweeper.Strategy
{
    class MFDeductive
    {
        private static readonly int[,] Adjacent =
        {
            {-1, -1 }, {-1,  0 }, {-1,  1 }, { 0, -1 }, { 0,  1 }, { 1, -1 }, { 1,  0 },{ 1,  1 }
        };

        private static readonly int[,] NullKernel =
        {
            {-1, -1, -1},
            {-1, -1, -1},
            {-1, -1, -1}
        };

        public static bool DeduceMineLocation(int[,] mfArr)
        {
            bool f = false;
            for (int y = 0; y < mfArr.GetLength(1); y++)
            {
                for (int x = 0; x < mfArr.GetLength(0); x++)
                {
                    // make sure tile is number tile
                    if (mfArr[x, y] <= 0) continue;
                    int adjMines = AdjacentCount(x, y, mfArr, NullKernel, x, y, -2);
                    // make sure tile isn't already satisfied
                    if (mfArr[x, y] == adjMines) continue;

                    int adjMinesReq = mfArr[x, y] - adjMines;
                    int adjUnknown = AdjacentCount(x, y, mfArr, NullKernel, x, y, -1);

                    // find all adjacent cells that are unknown
                    List<Point> unknownPoints = new List<Point>();
                    for (int i = 0; i < 8; i++)
                    {
                        int ax = x + Adjacent[i, 0];
                        int ay = y + Adjacent[i, 1];
                        if (ax >= 0 && ax < mfArr.GetLength(0) && ay >= 0 && ay < mfArr.GetLength(1) &&
                            mfArr[ax, ay] == -1)
                            unknownPoints.Add(new Point(Adjacent[i, 0], Adjacent[i, 1]));
                    }

                    List<int[,]> validPerms = new List<int[,]>();
                    int[] permIndex = new int[adjMinesReq];
                    for (int i = 0; i < adjMinesReq; i++)
                        permIndex[i] = adjMinesReq - i - 1;
                    // for all permutations of possible mine configurations
                    do
                    {
                        // copy initial kernel configuration from mfArr
                        int[,] kernel = new int[3, 3];
                        for (int dy = 0; dy < 3; dy++)
                            for (int dx = 0; dx < 3; dx++)
                            {
                                int augx = x + dx - 1;
                                int augy = y + dy - 1;
                                if (augx >= 0 && augx < mfArr.GetLength(0) 
                                    && augy >= 0 && augy < mfArr.GetLength(1))
                                    kernel[dx, dy] = mfArr[x + dx - 1, y + dy - 1];
                            }

                        // put mines selected from permutation into kernel
                        for (int i = 0; i < adjMinesReq; i++)
                        {
                            Point up = unknownPoints[permIndex[i]];
                            kernel[up.X + 1, up.Y + 1] = -2;
                        }

                        // set all other spots as kernel empty
                        for (int dy = 0; dy < 3; dy++)
                            for (int dx = 0; dx < 3; dx++)
                                if (kernel[dx, dy] == -1)
                                    kernel[dx, dy] = -3;

                        if (CheckPermutation(kernel, x, y, mfArr))
                            validPerms.Add(kernel);
                    } while (NextUniqueCombination(permIndex, adjUnknown));

                    // combine all possible kernels together
                    int[,] meshKernel = new int[3, 3];
                    for (int dy = 0; dy < 3; dy++)
                        for (int dx = 0; dx < 3; dx++)
                            meshKernel[dx, dy] = -1;
                    foreach (int[,] validk in validPerms)
                    {
                        for (int dy = 0; dy < 3; dy++)
                            for (int dx = 0; dx < 3; dx++)
                            {
                                if (meshKernel[dx, dy] == -1 || meshKernel[dx, dy] == validk[dx, dy])
                                    meshKernel[dx, dy] = validk[dx, dy];
                                else meshKernel[dx, dy] = -9;
                            }
                    }
                    // reflect certain deductions in mfArr
                    for (int dy = 0; dy < 3; dy++)
                        for (int dx = 0; dx < 3; dx++)
                        {
                            int kcx = x + dx - 1;
                            int kcy = y + dy - 1;
                            if (kcx >= 0 && kcx < mfArr.GetLength(0) && kcy >= 0 && kcy < mfArr.GetLength(1))
                            {
                                if (meshKernel[dx, dy] == -2 && mfArr[kcx, kcy] != -2)
                                {
                                    mfArr[kcx, kcy] = -2;
                                    f = true;
                                }
                                else if (meshKernel[dx, dy] == -3 && mfArr[kcx, kcy] != -3)
                                {
                                    mfArr[kcx, kcy] = -3;
                                    f = true;
                                }
                            }
                        }
                }
            }
            return f;
        }

        private static int SubstituteKernel(int x, int y, int[,] mfArr, int[,] kernel, int kx, int ky)
        {
            if (x >= kx - 1 && x < kx + 2 && y >= ky - 1 && y < ky + 2 &&
                kernel[x - kx + 1, y - ky + 1] != -1)
                return kernel[x - kx + 1, y - ky + 1];
            return mfArr[x, y];
        }

        private static bool CheckPermutation(int[,] kernel, int kx, int ky, int[,] mfArr)
        {
            for (int i = 0; i < 8; i++)
            {
                int ax = kx + Adjacent[i, 0];
                int ay = ky + Adjacent[i, 1];
                if (ax >= 0 && ax < mfArr.GetLength(0) && ay >= 0 && ay < mfArr.GetLength(1) &&
                    SubstituteKernel(ax, ay, mfArr, kernel, kx, ky) > 0)
                {
                    int km = AdjacentCount(ax, ay, mfArr, kernel, kx, ky, -2);
                    int ka = AdjacentCount(ax, ay, mfArr, kernel, kx, ky, -1);
                    if (km > mfArr[ax, ay] || km + ka < mfArr[ax, ay])
                        return false;
                }
            }
            return true;
        }

        private static int AdjacentCount(int x, int y, int[,] mfArr, int[,] kernel, int kx, int ky, int t)
        {
            int cnt = 0;
            for (int i = 0; i < 8; i++)
            {
                int ax = x + Adjacent[i, 0];
                int ay = y + Adjacent[i, 1];
                if (ax >= 0 && ax < mfArr.GetLength(0) && ay >= 0 && ay < mfArr.GetLength(1) &&
                    SubstituteKernel(ax, ay, mfArr, kernel, kx, ky) == t)
                    cnt++;
            }
            return cnt;
        }

        private static bool NextUniqueCombination(int[] list, int max)
        {
            do
            {
                if (!NextCombination(list, max)) return false;
            } while (!IsUnique(list));
            return true;
        }

        private static bool NextCombination(int[] list, int max)
        {
            list[0]++;
            int idx = 0;
            while (list[idx] >= max)
            {
                if (idx + 1 >= list.Length)
                    return false;
                list[idx] = list[idx + 1] + 1;
                idx++;

                list[idx]++;
            }
            return true;
        }

        private static bool IsUnique(int[] list)
        {
            for (int i = 0; i < list.Length; i++)
                for (int j = i + 1; j < list.Length; j++)
                    if (list[i] == list[j]) return false;
            return true;
        }
    }
}
