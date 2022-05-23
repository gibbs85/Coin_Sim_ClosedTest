using System;
using System.Collections.Generic;
using System.Text;

namespace StockDOGE
{
    public class Stock
    {
        private string name;            // 이름
        private double price;            // 가격
        private double[] recordPrice;    // 가격 기록
        private double[] recordWeight;   // 가중치 기록
        private double gaussMean;    // gaussian dist. 의 평균
        private double gaussStd;     // gaussian dist. 의 표준편차
        private int updateCount;        // 몇 번 update 되었는지 count
        private int recordLengthMAX;       // 가격과 가중치를 몇 개까지 기억하여 (RNN에) 활용할 건지를 명시적으로 지정하는 변수.
        private string description;     // 종목 설명

        /*/////////////////////////////////////////////////////////////////////
         * 
         * public functions are:
         * 
         *  double updateGaussian();      
         *  double updateForced(double weight);  
         *  double updateForced(double mean, double std)
         *
         *  string getName();
         *  double getPrice();
         *  double[] getPriceRecord();
         *  double getMean();
         *  double getStd();
         *  
         *  void setName(string newName);
         *  void setMean(double newMean);
         *  void setStd(double newStd);
         *   
         *  
        *///////////////////////////////////////////////////////////////////////

        /*/////////////////////////////////////////////////////////////////////
         * 2022/05/20
         * NOTE.
         * 
         * 
         * RNN functions are disabled(private).
         * 
         * made some private attributes those don't have getter/setter functions in general
         *  to have them.
         * 
         * changed the "update..." functions to return the updated values.
         * 
         *//////////////////////////////////////////////////////////////////////


        public Stock(string name, double initPrice, int recordLengthMAX)
        {
            this.price = initPrice; //입력받은 초기 가격
            this.recordPrice = new double[recordLengthMAX];    // 입력받은 recordLength만큼 기록을 남기고 RNN에 사용
            this.recordPrice[0] = this.price;
            this.recordWeight = new double[recordLengthMAX];// 입력받은 recordLength만큼 기록을 남기고 RNN에 사용
            this.recordWeight[0] = 1;
            this.updateCount = 1;
            this.recordLengthMAX = recordLengthMAX;

            this.gaussMean = 1;
            this.gaussStd = 0.005;

            this.name = name;
            this.description = "NODESC";
        }

        public Stock(double initPrice, int recordLengthMAX)
        {
            this.price = initPrice; //입력받은 초기 가격
            this.recordPrice = new double[recordLengthMAX];    // 입력받은 recordLength만큼 기록을 남기고 RNN에 사용
            this.recordPrice[0] = this.price;
            this.recordWeight = new double[recordLengthMAX];// 입력받은 recordLength만큼 기록을 남기고 RNN에 사용
            this.recordWeight[0] = 1;
            this.updateCount = 1;
            this.recordLengthMAX = recordLengthMAX;

            this.gaussMean = 1;
            this.gaussStd = 0.005;

            this.name = "NONAME";
            this.description = "NODESC";
        }
        public Stock(string name, double initPrice)
        {
            this.price = initPrice;
            this.recordPrice = new double[1];
            this.recordPrice[0] = this.price;
            this.recordWeight = new double[1];
            this.recordWeight[0] = 1;
            this.updateCount = 1;
            this.recordLengthMAX = 1;

            this.gaussMean = 1;
            this.gaussStd = 0.005;

            this.name = name;
            this.description = "NODESC";
        }

        public Stock(double initPrice)
        {
            this.price = initPrice;
            this.recordPrice = new double[1];
            this.recordPrice[0] = this.price;
            this.recordWeight = new double[1];
            this.recordWeight[0] = 1;
            this.updateCount = 1;
            this.recordLengthMAX = 1;

            this.gaussMean = 1;
            this.gaussStd = 0.005;

            this.name = "NONAME";
            this.description = "NODESC";
        }

        //RNN 없는 버전의 update
        public double updateGaussian()
        {
            /* 
             * Gaussian 랜덤 변수 가중치를 생성하여 다음 가격 업데이트
             * 
             */
            double newWeight = this.gaussianNum();

            this.price = this.price * newWeight;

            //아래는 recordPrice, recordWeight 업데이트. recordPrice의 length만큼의 최신 기록만 남긴다
            updateRecordPrice(this.price);
            updateRecordWeight(newWeight);

            this.updateCount++;

            return this.price;
        }

        public double updateForced(double weight)
        {
            /*
             * 인수로 받은 weight로 다음 가격 업데이트.
             * 
             */

            this.price = this.price * weight;

            updateRecordPrice(this.price);
            updateRecordWeight(weight);

            this.updateCount++;

            return this.price;
        }

        public double updateForced(double mean, double std)
        {
            /*
             * 인수로 받은 mean과 std로 gaussian 랜덤변수 생성하여 weight로 사용, 다음 가격 업데이트.
             * 
             */
            double weight = this.gaussianNum(mean, std);

            this.price = this.price * weight;

            updateRecordPrice(this.price);
            updateRecordWeight(weight);

            this.updateCount++;

            return this.price;
        }

        private void updateRecordPrice(double newPrice)
        {
            if (this.updateCount >= this.recordLengthMAX)
            {
                this.arrayRotate(ref this.recordPrice, -1);
                this.recordPrice[this.recordLengthMAX - 1] = newPrice;
            }
            else
            {
                this.recordPrice[this.updateCount] = newPrice;
            }
        }

        private void updateRecordWeight(double newWeight)
        {
            if (this.updateCount >= this.recordLengthMAX)
            {
                this.arrayRotate(ref this.recordWeight, -1);
                this.recordWeight[this.recordLengthMAX - 1] = newWeight;
            }
            else
            {
                this.recordWeight[this.updateCount] = newWeight;
            }
        }

        private void updateByRNNonPrice()//아마도 주식 가격에 적합할듯
        {
            /*/////////////////////////////////////////////////////////////////////////
             * 
             * 현재가격 + (이전 가격들을 RNN한 다음 예측 가격 * 새로운 랜덤 가중치)
             * 
             * 또는, RNN 예측치의 비중을 늘려
             * 
             * 이전 가격들을 RNN한 다음 예측 가격 + (현재 가격 * 새로운 랜덤 가중치 /2)
             * 
             *//////////////////////////////////////////////////////////////////////////
        }

        private void updateByRNNonWeight()//아마도 코인 가격에 적합할듯
        {
            /*//////////////////////////////////////////////////////////////////////////////////
             * 
             * 현재가격 + (이전 가중치들을 RNN한 다음 예측 가중치 + 새로운 랜덤 가중치)의 조합(평균) * 현재 가격
             * 
             *//////////////////////////////////////////////////////////////////////////////////

            this.price = this.price + (this.RNN(recordWeight) + this.gaussianNum()) / 2 * this.price;
        }

        private double RNN(double[] data)
        {
            return 0.0;
        }

        private double gaussianNum()
        {
            /*/////////////////////////////////////////////////////////////////////////
             * 해당 객체의 gaussianMean과 gaussianStd를 가지는 gaussian dist. 를 확률밀도함수로 하는 랜덤 수를 리턴
             * 
             * 
             */////////////////////////////////////////////////////////////////////////
            Random rand = new Random();
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            /*
             * 잘못된 mean, std 설정 등으로 인해 weight가 음수가 나오는 경우를 막기 위한 dirty solution
             */
            double result = this.gaussStd * (randStdNormal) + this.gaussMean;
            if (result < 0)
                result = 0;

            return result;


            //return this.gaussStd * (randStdNormal) + this.gaussMean;
        }

        private double gaussianNum(double mean, double std)
        {
            Random rand = new Random();
            double u1 = 1.0 - rand.NextDouble();
            double u2 = 1.0 - rand.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

            double result = std * (randStdNormal) + mean;
            if (result < 0)
                result = 0;

            return result;
        }

        private void arrayRotate<T>(ref T[] array, int shiftCount)
        {
            T[] backupArray = new T[array.Length];
            for (int index = 0; index < array.Length; index++)
            {
                backupArray[(index + array.Length + shiftCount % array.Length) % array.Length] =
                    array[index];
            }
            array = backupArray;
        }

        private double[] arrayMake(int nNumber)
        {
            double[] array = new double[nNumber];
            for (int i = 0; i < nNumber; i++)
            {
                array[i] = i + 1;
            }
            return array;
        }


        /*
         * below are the setter/getter functions.
         */

        public double getPrice()
        {
            return this.price;
        }

        public int getUpdateCount()
        {
            return this.updateCount;
        }
        public string getName()
        {
            return this.name;
        }

        public double[] getPriceRecord()
        {
            int count = this.updateCount;
            if (count > this.recordPrice.Length)
                count = this.recordPrice.Length;

            double[] result = new double[count];
            for (int i=0; i<count; i++)
            {
                result[i] = this.recordPrice[i];
            }
            return result;
        }

        public double getMean()
        {
            return this.gaussMean;
        }

        public double getStd()
        {
            return this.gaussStd;
        }

        public void setMean(double mean)
        {
            this.gaussMean = mean;
        }

        public void setStd(double std)
        {
            this.gaussStd = std;
        }

        public void setName(string newName)
        {
            this.name = newName;
        }

        public void setDesc(string description)
        {
            this.description = description;
        }

    }
}
