﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaCSV.Interfaces {
    public interface ICSVParser {
        DataTable ParseDefinedCSV(DataTable schema, string path, CSVParseOptions options = null, List<string> validationPatterns = null);
        DataTable ParseSimpleCSV(string path, CSVParseOptions options = null, List<string> validationPatterns = null);
    }
}
