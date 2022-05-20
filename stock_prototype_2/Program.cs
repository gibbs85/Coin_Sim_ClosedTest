using System;
using System.Windows.Forms;
using ScottPlot;

namespace stock_prototype_2
{
    class Program
    {
        static void Main(string[] args)
        {

            /* ////////////////////////////////
             * 괜찮아보이는 값들 mean, std
             * 1.0002,      0.005
             * 1.00001,     0.005       steady. good upto 50,000 times. works for upto 1,000,000 times.
             * 1.000011,    0.005       steady. but, occasionally skies up in less then 10,000 times.
             */////////////////////////////////

            double price = 1500;
            int nTimes = 200;
            double Mean = 1.00001;
            double Std = 0.005;

            Stock stock = new Stock(price, nTimes);
            stock.setMean(Mean);
            stock.setStd(Std);

            for (int i = 0; i < nTimes; i++)
            {

                if (i == 20)
                {
                    stock.setMean(1.1);
                    stock.setStd(0.005);
                }
                if (i == 21)
                {
                    stock.setMean(0.995);
                    stock.setStd(0.005);
                }
                if (i == 40)
                {
                    stock.setMean(1.1);
                    stock.setStd(0.005);
                }
                if (i == 41)
                {
                    stock.setMean(0.995);
                    stock.setStd(0.005);
                }
                if (i == 60)
                {
                    stock.setMean(1.1);
                    stock.setStd(0.005);
                }
                if (i == 61)
                {
                    stock.setMean(0.995);
                    stock.setStd(0.005);
                }
                if (i == 80)
                {
                    stock.setMean(1.1);
                    stock.setStd(0.005);
                }
                if (i == 81)
                {
                    stock.setMean(0.995);
                    stock.setStd(0.005);
                }
                if (i == 99)
                {
                    stock.setMean(1);
                    stock.setStd(0.005);
                }

                stock.updateGaussian();
            }

            Console.WriteLine("\nTEST DOEN\n");

            /*///////////////////////////////////////////////////////////////////////
             * 
             * Plotting과 png 파일 저장
             * 
             * 
             * 테스트 시 png 파일 경로 : ...\bin\Debug\netcoreapp3.1\
             * 
            /*///////////////////////////////////////////////////////////////////////

            //string title = (nTimes).ToString() + " updates, m = " + (Mean).ToString() + ", s = " + (Std).ToString() + " ";
            string title = (nTimes).ToString() + " updates, m = " + (Mean).ToString() + ", s = " + (Std).ToString() + " with control";
            string fileName = title + ".png";

            var plt = new ScottPlot.Plot(600, 400);
            plt.XLabel("updates");
            plt.YLabel("price");
            plt.AddSignal(stock.recordPrice);
            plt.Title(title);

            plt.SaveFig(fileName);

        }
    }
}

