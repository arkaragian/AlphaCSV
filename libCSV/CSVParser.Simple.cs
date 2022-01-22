using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libCSV {
    public partial class CSVParser {


        public DataTable ParseSimpleCSV(string path, CSVParseOptions options = null, List<string> validationPatterns = null) {
            //Before we continue we need to make some assumptions for the file. e.g to know how many fields we need to parse.
            //Thus we read only the first line, deduce information there and try to move forward.

            //File.ReadLines makes use of lazy evaluation and doesn't read the whole file into an array of lines first.
            //https://stackoverflow.com/questions/27345854/read-only-first-line-from-a-text-file/27345927
            string FirstLine = FSInterface.File.ReadLines(path).First();

            if (options == null) {
                options = new CSVParseOptions();
            }

            string[] fields = ParseLine(FirstLine, options);
            DataTable schema = new DataTable();
            foreach (string field in fields) {
                DataColumn column = new DataColumn(field, typeof(string));
                schema.Columns.Add(column);
            }

            return ParseDefinedCSV(schema, path, options, null);
        }
    }
}
