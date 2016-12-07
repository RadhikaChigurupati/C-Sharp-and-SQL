using System;
using System.Net;
using System.Text;
using System.IO;

namespace StockBeta
{
    public class WebStockDataReader
    {
        public int getStockData(SqlHandler handle, string stock, DateTime startDt, DateTime endDt)
        {
            try
            {
                string yahooURL = @"http://ichart.finance.yahoo.com/table.csv?s=" + stock
                                    + "&a=" + (startDt.Month - 1).ToString()
                                    + "&b=" + startDt.Day.ToString()
                                    + "&c=" + startDt.Year.ToString()
                                    + "&d=" + (endDt.Month - 1).ToString()
                                    + "&e=" + endDt.Day.ToString()
                                    + "&f=" + endDt.Year.ToString()
                                    + "&g=d&ignore=.csv";

                HttpWebRequest webreq = (HttpWebRequest)WebRequest.Create(yahooURL);
                HttpWebResponse webresp = (HttpWebResponse)webreq.GetResponse();
                StreamReader strm = new StreamReader(webresp.GetResponseStream(), Encoding.ASCII);

                string stk_data = strm.ReadToEnd();
                char[] newLine = { '\n' };
                char[] comma = { ',' };
                string[] stk_lines = stk_data.Split(newLine, StringSplitOptions.RemoveEmptyEntries);

                string insertCmd = "";
                int i = 0;
                foreach (string line in stk_lines)
                {
                    i++;
                    if (line.Contains("Date")) {
                        continue;
                    }
                    // Console.WriteLine(line);
                    string[] fields = line.Split(comma, StringSplitOptions.RemoveEmptyEntries);
                    insertCmd += "INSERT INTO stock_tbl VALUES ('" + stock + "'" + ",";
                    insertCmd += "'" + fields[0] + "',";
                    insertCmd += fields[6] + ")\n";

                    // Console.WriteLine(cmd);
                    if (i % 10 == 0) {
                        // insert in batches
                        handle.executeNonQuery(insertCmd);
                        insertCmd = "";
                    }
                }
                if (insertCmd != "") {
                    // insert remaining
                    handle.executeNonQuery(insertCmd);
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine(stock + " : " + e.Message);
                return -1;
            }
            return 0;
        }
    }
}
