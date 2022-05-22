using System;
using System.Windows.Forms;
using ScottPlot;

namespace StockDOGE
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
            int nTimes = 100;
            double Mean = 1.01;
            double Std = 0.005;

            Stock stock = new Stock("연세전자", 1500, 100);
            stock.setMean(1.01);
            stock.setStd(0.005);

            Console.WriteLine(stock.getPrice());
            Console.WriteLine(stock.updateForced(1.1, 0.2));
            Console.WriteLine(stock.getPrice());

            //Console.WriteLine("\nTEST DOEN\n");

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
            plt.AddSignal(stock.getPriceRecord());
            plt.Title(title);

            plt.SaveFig(fileName);

        }
    }
}

