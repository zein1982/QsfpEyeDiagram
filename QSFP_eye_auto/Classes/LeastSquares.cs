using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QSFP_eye_auto.Classes
{
    public static class LeastSquares
    {
        public static void FindCoefs(double[] xArray, double[] yArray, ref double a, ref double b)
        {
            double xSum = 0, ySum = 0; //Суммы значений x и y
            double X, Y; //Средние значения x и y
            double xySum = 0; //Сумма перемноженных значений x и y
            double x2Sum = 0; //Сумма значений x^2

            //Вычисление всех перечисленных выше значений
            for (int i = 0; i < xArray.Length; i++)
            {
                xSum += xArray[i];
                ySum += yArray[i];
                xySum += xArray[i] * yArray[i];
                x2Sum += xArray[i] * xArray[i];
            }
            X = xSum / xArray.Length;
            Y = ySum / yArray.Length;

            b = (xySum - xArray.Length * X * Y) / (x2Sum - xArray.Length * X * X); // K
            a = Y - b * X; // B

        }


        public static int QuadCalculate(int[] moduleval, double[] oscval)
        {
            var x = 0.0;
            var x2 = 0.0;
            var x3 = 0.0;
            var x4 = 0.0;
            var y = 0.0;
            var xy = 0.0;
            var x2y = 0.0;
            var len = moduleval.Length;

            // суммируем все значения

            for (int i = 0; i < len; i++)
            {
                x2 += Math.Pow(moduleval[i], 2);
                x3 += Math.Pow(moduleval[i], 3);
                x4 += Math.Pow(moduleval[i], 4);
                x += moduleval[i];
                y += oscval[i];
                xy += moduleval[i] * oscval[i];
                x2y += Math.Pow(moduleval[i], 2) * oscval[i];
            }

            // матрицы для рассчета определителей
            double[,] delta = {
                { x2, x,  len},
                { x3, x2, x },
                {x4, x3, x2 }
            };

            double[,] deltaA = {
                { y, x,  len},
                { xy, x2, x },
                {x2y, x3, x2 }
            };

            double[,] deltaB = {
                { x2, y,  len},
                { x3, xy, x },
                {x4, x2y, x2 }
            };

            double[,] deltaC = {
                { x2, x,  y},
                { x3, x2, xy },
                {x4, x3, x2y }
            };

            var deltaDet = Determinant(delta);

            var detA = Determinant(deltaA) / deltaDet;

            var detB = Determinant(deltaB) / deltaDet;

            var detC = Determinant(deltaC) / deltaDet;

            // находим экстремум
            var extr = (int)((-detB) / (detA * 2));

            var dataWrite = detA * Math.Pow(extr, 2) + detB * extr + detC;

            return extr;


        }

        public static double Determinant(double[,] matrix)
        {
            var result =
                  matrix[0, 0] * matrix[1, 1] * matrix[2, 2]
                + matrix[0, 1] * matrix[1, 2] * matrix[2, 0]
                + matrix[0, 2] * matrix[1, 0] * matrix[2, 1]
                - matrix[0, 0] * matrix[1, 2] * matrix[2, 1]
                - matrix[0, 1] * matrix[1, 0] * matrix[2, 2]
                - matrix[0, 2] * matrix[1, 1] * matrix[2, 0];
            return result;
        }

    }
}
