using System;
using System.Collections.Generic;
using System.IO;

namespace StockBeta
{
    class Program
    {
        static void calculateAndStoreBetas(SqlHandler sqlHandle, string stock, List<StockData> data, int rollingWindow)
        {
            string insertBetasCmd = "";

            if (data.Count <= rollingWindow) {
                // no history
                return;
            }

            for (int i = rollingWindow + 1; i < data.Count; ++i)
            {
                bool stockPriceExistsAllDates = true;
                List<double> mktReturns = new List<double>();
                List<double> stkReturns = new List<double>();

                // history
                for (int j = i - rollingWindow; j < i; ++j)
                {
                    if (!data[j].get_stkprc().HasValue || !data[j - 1].get_stkprc().HasValue
                        || data[j].get_stkprc().Value == 0 || data[j - 1].get_stkprc().Value == 0)
                    {
                        // cannot calculate returns and hence cannot calculate beta
                        stockPriceExistsAllDates = false;
                        break;
                    }
                    double stkRet = (data[j].get_stkprc().Value - data[j - 1].get_stkprc().Value) / data[j - 1].get_stkprc().Value;
                    double mktRet = (data[j].get_mktprc() - data[j - 1].get_mktprc()) / data[j - 1].get_mktprc();
                    stkReturns.Add(stkRet);
                    mktReturns.Add(mktRet);
                }

                if (stockPriceExistsAllDates)
                {
                    double covar = StatMath.covariance(mktReturns, stkReturns);
                    double beta = covar / StatMath.variance(mktReturns);
                    insertBetasCmd += "INSERT INTO betas_tbl VALUES ('" + stock + "', ";
                    insertBetasCmd += "'" + data[i].get_dt().Year.ToString()
                                      + "-" + data[i].get_dt().Month.ToString() 
                                      + "-" + data[i].get_dt().Day.ToString() + "'";
                    insertBetasCmd += ", " + beta.ToString();
                    insertBetasCmd += ", " + covar.ToString() + ")\n";

                    if (i%10 == 0) {
                        // insert in batches
                        sqlHandle.executeNonQuery(insertBetasCmd);
                        insertBetasCmd = "";
                    }
                }
            }

            if (insertBetasCmd != "") {
                // output last remaining betas
                sqlHandle.executeNonQuery(insertBetasCmd);
            }
        }

        static void Main(string[] args)
        {
            List<string> stocks = new List<string>();

            // instantiate sqlHandle
            SqlHandler sqlHandle = new SqlHandler("radhika Chigurupati", "1212",
                                                  @"DESKTOP-LLFV7OU\SQLEXPRESS", "stockdb");
            if (!sqlHandle.isConnected()) {
                return;
            }

            // stocks file name
            string filename = @"C:\Users\radhi\Desktop\stocks.txt";
            StreamWriter errorfile = new StreamWriter(@"C:\Users\radhi\Desktop\errors.txt");
            
            if (!File.Exists(filename)) {
                Console.WriteLine("Stocks file doesn't exist!");
                return;
            }

            // read stocks
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines) {
                stocks.Add(line);
            }
            Console.WriteLine("Successfully read " + stocks.Count.ToString() + " stocks from file!");

            // start and end dates
            DateTime startDt = new DateTime(2014, 01, 01);
            DateTime endDt = new DateTime(2015, 06, 01);

            // get stock data into database
            WebStockDataReader wdr = new WebStockDataReader();
            int rc = 0;
            Console.WriteLine("Retrieving stocks data from yahoo and storing into database...");
            int cnt = 0;
            foreach (string stock in stocks) {
                rc = wdr.getStockData(sqlHandle, stock, startDt, endDt);
                cnt++;
                if(cnt%100 == 0) {
                    // show progress to user
                    Console.WriteLine("Finished retrieving " + cnt.ToString() + " stocks / " 
                                       + stocks.Count.ToString() + " stocks ...(still in progress)");
                }
                if (rc != 0) {
                    errorfile.WriteLine(stock);
                }
            }
            errorfile.Close();
            Console.WriteLine("Completed retrieving stock data and storing into database!");
            
            // get market data -- sp 500 into database
            wdr.getStockData(sqlHandle, "^GSPC", startDt, endDt);
            Console.WriteLine("Completed retrieving market data and storing into database!");

            DateTime betaStDt = new DateTime(2014, 06, 01);
            DateTime betaEndDt = new DateTime(2014, 12, 31);
            int rollingWindow = 30;
            
            cnt = 0;
            foreach (string stock in stocks) {
                List<StockData> stk_data = sqlHandle.getPriceData(stock, betaStDt.Subtract(TimeSpan.FromDays(30)), betaEndDt);
                calculateAndStoreBetas(sqlHandle, stock, stk_data, rollingWindow);
                cnt++;
                if (cnt%100 == 0) {
                    // show progress
                    Console.WriteLine("Finished calculating and storing betas for " 
                                       + cnt.ToString() + " stocks ... (still in progress)");
                }
            }

            Console.WriteLine("Program finished calculating betas and storing into database .. Press any key to exit");
            Console.ReadKey();
        }
    }
}
