using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace StockBeta
{
    public class SqlHandler
    {
        SqlConnection myConnection;
        bool _isConnected;

        public SqlHandler(string username, string password, string serverUrl, string dbname)
        {
            myConnection = new SqlConnection("user id=" + username + ";password=" + password
                                             + ";server=" + serverUrl + ";Trusted_Connection=yes;"
                                             + "database=" + dbname + "; connection timeout=30");

            try
            {
                myConnection.Open();
                _isConnected = true;
                Console.WriteLine("Successfully connected to database!");
            }
            catch (Exception e)
            {
                _isConnected = false;
                Console.WriteLine("SQL Connection exception: " + e.Message);
            }
        }

        public bool isConnected()
        {
            return _isConnected;
        }

        public int executeNonQuery(string cmd)
        {
            int rc = 0;
            try {
                SqlCommand myCommand = new SqlCommand(cmd, myConnection);
                rc = myCommand.ExecuteNonQuery();
            }
            catch (Exception ex) {
                Console.WriteLine(ex.Message);
                return -1;
            }
            return rc;
        }

        public List<StockData> getPriceData(string stock, DateTime startDt, DateTime endDt)
        {
            List<StockData> stockprices = new List<StockData>();
            try
            {
                SqlDataReader myReader = null;
                string startDtStr = "'" + startDt.Year.ToString() + "-" + startDt.Month.ToString() + "-" + startDt.Day.ToString() + "'";
                string endDtStr = "'" + endDt.Year.ToString() + "-" + endDt.Month.ToString() + "-" + endDt.Day.ToString() + "'";
                SqlCommand myCommand = new SqlCommand(@"select a.dt, a.price as sp500, b.price as stkprc
                                                        from (select * from stock_tbl where stock = '^GSPC') as a
                                                        left join (select * from stock_tbl where stock = '"
                                                        + stock + 
                                                      @"') as b
                                                        on a.dt = b.dt
                                                        where a.dt >= " + startDtStr + " and a.dt <= " + endDtStr +
                                                        "order by a.dt",
                                                      myConnection);
                myReader = myCommand.ExecuteReader();
                while (myReader.Read()) {
                    StockData s = new StockData();
                    s.set_dt(myReader.GetDateTime(0));  // dt
                    s.set_mktprc(myReader.GetDouble(1)); // mkt i.e. sp500
                    if (myReader[2] != DBNull.Value) {
                        s.set_stkprc(myReader.GetDouble(2));  // stock price
                    }
                    stockprices.Add(s);
                }
                myReader.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return stockprices;
        }
    }
}
