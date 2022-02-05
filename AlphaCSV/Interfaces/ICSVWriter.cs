using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaCSV.Interfaces {
    public interface ICSVWriter {
        void WriteCSV(string filename, DataTable data, CSVParseOptions options = null);
    }
}
