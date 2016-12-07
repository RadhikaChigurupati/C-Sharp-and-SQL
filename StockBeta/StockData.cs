using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StockBeta
{
    public class StockData
    {
        DateTime dt;
        double? stkprc;  // stock price can be null
        double mktprc;   // market price cannot be null

        public StockData() {
            stkprc = null;
        }

        // Setters below
        public void set_mktprc(double price) {
            mktprc = price;
        }

        public void set_stkprc(double price) {
            stkprc = price;
        }

        public void set_dt(DateTime _dt) {
            dt = _dt;
        }

        // Getters below
        public double? get_stkprc() {
            return stkprc;
        }

        public double get_mktprc() {
            return mktprc;
        }

        public DateTime get_dt() {
            return dt;
        }
    }
}
