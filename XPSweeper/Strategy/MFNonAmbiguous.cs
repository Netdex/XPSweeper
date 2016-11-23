using System;
using System.Collections.Generic;
using System.Drawing;

namespace XPSweeper.Strategy
{
    class MFNonAmbiguous
    {
        private static readonly int[,] Adjacent =
        {
            {-1, -1 }, {-1,  0 }, {-1,  1 }, { 0, -1 }, { 0,  1 }, { 1, -1 }, { 1,  0 },{ 1,  1 }
        };

        

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
    }
}
