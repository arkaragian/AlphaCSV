using System;
using System.Collections.Generic;
using System.Data;

namespace AlphaCSV.Interfaces;

public interface ICSVParser {
    DataTable ParseDefinedCSV(DataTable schema, string path, CSVParseOptions? options = null, List<Func<string, bool>>? validationPatterns = null);
    DataTable ParseSimpleCSV(string path, CSVParseOptions? options = null, List<Func<string, bool>>? validationPatterns = null);
    List<T> ParseType<T>(string path, CSVParseOptions? options = null, List<Func<string, bool>>? validationPatterns = null);
}