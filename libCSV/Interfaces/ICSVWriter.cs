using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libCSV.Interfaces {
    public interface ICSVWriter {
        public void WriteCSV(string filename, DataTable data, CSVParseOptions options = null) {
        }
    }
}
