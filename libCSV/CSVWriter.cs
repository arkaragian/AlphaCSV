using System.Data;
using System.IO.Abstractions;
using System.Text;

namespace libCSV {
    public class CSVWriter {

        readonly IFileSystem FSInterface;

        public CSVWriter(IFileSystem fileSystem) {
            this.FSInterface = fileSystem;
        }

        public CSVWriter() : this(new FileSystem()) {
        }

        public void WriteCSV(string filename, DataTable data, CSVParseOptions options = null) {
            if(options == null) {
                options = new CSVParseOptions();
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < data.Columns.Count; i++) {
                string colName = data.Columns[i].ColumnName;
                bool quoted = false;

                if (colName.Contains(options.Delimeter)) {
                    sb.Append(options.QuoteCharacter);
                    quoted = true;
                }

                foreach (char c in colName) {
                    if (c == options.QuoteCharacter) {
                        //TODO: Decide if I want to escape the character
                        throw new InvalidOperationException($"Field cannot contain the quote character {c}");
                    }
                    sb.Append(c);
                }

                if (quoted) {
                    sb.Append(options.QuoteCharacter);
                    quoted = false;
                }

                if (i != data.Columns.Count - 1) {
                    sb.Append(options.Delimeter);
                }
            }
            sb.Append('\r');
            sb.Append('\n');
            string header = sb.ToString();

            sb.Clear();
            List<string> lines = new List<string>();

            for (int i = 0; i < data.Rows.Count; i++) {
                for (int j = 0; j < data.Rows[i].ItemArray.Length; j++) {
                    string field;
                    if (data.Rows[i].ItemArray[j] == null) {
                        field = "";
                    } else {
                        field = data.Rows[i].ItemArray[j].ToString();
                    }
                    bool quoted = false;

                    if (field.Contains(options.Delimeter)) {
                        sb.Append(options.QuoteCharacter);
                        quoted = true;
                    }

                    foreach (char c in field) {
                        if (c == options.QuoteCharacter) {
                            //TODO: Decide if I want to escape the character
                            throw new InvalidOperationException($"Field cannot contain the quote character {c}");
                        }
                        sb.Append(c);
                    }

                    if (quoted) {
                        sb.Append(options.QuoteCharacter);
                        quoted = false;
                    }


                    if (j != data.Rows[i].ItemArray.Length - 1) {
                        sb.Append(options.Delimeter);
                    }
                }
                sb.Append('\r');
                sb.Append('\n');
                lines.Add(sb.ToString());
                sb.Clear();
            }

            FileStream fileStream = File.OpenWrite(filename);
            StreamWriter writer = new StreamWriter(fileStream);
            writer.Write(header);
            for(int i=0; i<lines.Count; i++) {
                writer.Write(lines[i]);
            }
            writer.Flush();
            writer.Close();
        }
    }
}
