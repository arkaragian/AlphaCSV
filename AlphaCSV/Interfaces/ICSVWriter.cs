using System.Data;

namespace AlphaCSV.Interfaces;

public interface ICSVWriter {
    void WriteCSV(string filename, DataTable data, CSVWriteOptions? options = null);
}