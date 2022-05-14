using System;

namespace stock_prototype_2
{
    class Program
    {
        static void Main(string[] args)
        {

            /* ////////////////////////////////
             * 괜찮아보이는 값들 mean, std
             * 0.0002,      0.005
             * 0.00001,     0.005       steady. good upto 50,000 times. works for upto 1,000,000 times.
             * 0.000011,    0.005       steady. but, occasionally skies up in less then 10,000 times.
             */////////////////////////////////

            Stock stock = new Stock(1500, 8);
            stock.setMean(0.00001);
            stock.setStd(0.005);

            for (int i = 0; i < 50; i++)
            {
                
                if (i == 5)
                    stock.setMean(0.1);

                if (i == 15)
                    stock.setMean(-0.2);

                if (i == 20)
                {
                    stock.setMean(0.0);
                    stock.setStd(0.0);
                }

                if ( i == 25)
                {
                    stock.setMean(-0.5);
                    stock.setStd(0.0);
                }

                if (i == 30)
                {
                    stock.setMean(1);
                    stock.setStd(0.0);
                }

                if (i == 35)
                {
                    stock.setMean(0.00001);
                    stock.setStd(0.005);
                }

                stock.updateGaussian();
                Console.WriteLine(stock.getPrice());
            }

            Console.WriteLine("TEST DOEN");

        }
    }
}

class Stock
{
    private double price;            // 가격
    private double[] recordPrice;    // 가격 기록
    private double[] recordWeight;   // 가중치(생성했던 gaussianNumber weight 기록)
    private double gaussianMean;    // gaussian dist. 의 평균
    private double gaussianStd;     // gaussian dist. 의 표준편차
    private int updateCount;        // 몇 번 update 되었는지 count
    private int recordLength;       // 가격과 가중치를 몇 개까지 기억하여 (RNN에) 활용할 건지를 명시적으로 지정하는 변수.

    /*/////////////////////////////////////////////////////////////////////
        void updateGaussian();      // Gaussian 랜덤 가중치만 사용한 업데이트
        void updateByRNNonPrice();      // 가격 레코드를 RNN 하여 만든 예상치 * Gaussian 랜덤 가중치 사용하여 업데이트
        void updateByRNNonWeight();      // (가중치 레코드를 RNN 하여 만든 예상치 + Gaussian 랜덤 가중치)/2 사용하여 업데이트
        void update(double percent);  // percent 만큼 변동. ex). update(0.1) 은 가격의 10% 만큼 더 한다.
    *///////////////////////////////////////////////////////////////////////

    public Stock(double initPrice, int recordLength)
    {
        this.price = initPrice; //입력받은 초기 가격
        this.recordPrice = new double[recordLength];    // 입력받은 recordLength만큼 기록을 남기고 RNN에 사용
        this.recordPrice[0] = this.price;
        this.recordWeight = new double[recordLength];// 입력받은 recordLength만큼 기록을 남기고 RNN에 사용
        this.recordWeight[0] = 0;
        this.updateCount = 1;
        this.recordLength = recordLength;

        //standard gaussian dist.
        this.gaussianMean = 0;
        this.gaussianStd = 1;
    }


    //RNN 없는 버전의 update
    public void updateGaussian()
    {
        double newWeight = this.gaussianNum();

        this.price = this.price + this.price * newWeight;

        //아래는 recordPrice 업데이트. recordPrice의 length만큼의 최신 기록만 남긴다
        if (this.updateCount == this.recordLength)
        {
            this.arrayRotate(ref this.recordPrice, -1);
            this.recordPrice[this.recordLength - 1] = this.price;
        }
        else
        {
            this.recordPrice[this.updateCount] = this.price;
        }

        //아래는 recordWeight 업데이트.
        if (this.updateCount == this.recordLength)
        {
            this.arrayRotate(ref this.recordWeight, -1);
            this.recordWeight[this.recordLength - 1] = newWeight;
        }
        else
        {
            this.recordWeight[this.updateCount] = newWeight;
        }

        //updateCount는 recordLength를 최댓값으로 갖는다.
        if(this.updateCount < this.recordLength)
            this.updateCount++;
    }

    public void update(double weight)
    {

        this.price = this.price + this.price * weight;

        //아래는 recordPrice 업데이트. recordPrice의 length만큼의 최신 기록만 남긴다
        if (this.updateCount >= this.recordLength)
        {
            this.arrayRotate(ref this.recordPrice, -1);
            this.recordPrice[this.recordLength - 1] = this.price;
        }
        else
        {
            this.recordPrice[this.updateCount] = this.price;
        }

        //아래는 recordWeight 업데이트.
        if (this.updateCount >= this.recordLength)
        {
            this.arrayRotate(ref this.recordWeight, -1);
            this.recordWeight[this.recordLength - 1] = weight;
        }
        else
        {
            this.recordWeight[this.updateCount] = weight;
        }

        //updateCount는 recordLength를 최댓값으로 갖는다.
        if (this.updateCount < this.recordLength)
            this.updateCount++;
    }

    public void updateByRNNonPrice()//아마도 주식 가격에 적합할듯
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

    public void updateByRNNonWeight()//아마도 코인 가격에 적합할듯
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
         */////////////////////////////////////////////////////////////////////////
        Random rand = new Random();
        double u1 = 1.0 - rand.NextDouble();
        double u2 = 1.0 - rand.NextDouble();
        double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);

        return this.gaussianStd * (randStdNormal) + this.gaussianMean;
    }

    private void arrayRotate <T> (ref T[] array, int shiftCount)
    {
        T[] backupArray = new T[array.Length];
        for (int index =0; index < array.Length; index++)
        {
            backupArray[(index + array.Length + shiftCount % array.Length) % array.Length] =
                array[index];
        }
        array = backupArray;
    }



    public double getPrice()
    {
        return this.price;
    }
    public void setMean(double mean)
    {
        this.gaussianMean = mean;
    }

    public void setStd(double std)
    {
        this.gaussianStd = std;
    }
}