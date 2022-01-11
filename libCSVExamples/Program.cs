//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using libCSV;
using System.Data;

namespace libCSVExamples {
    internal class Program {
        static void Main(string[] args) {
            Console.WriteLine("Welcome to libCSVExamples. Those examples showcase the use of the library.");

            DataTable simple = ReadSimpleCSV("SampleCSV/TwoColumnCSV.csv");

            PrintDataTable(simple);
        }

        public static DataTable ReadSimpleCSV(string filename) {
            DataTable schema = new DataTable();
            schema.Columns.Add(new DataColumn("ColumnA", typeof(string)));
            schema.Columns.Add(new DataColumn("ColumnB", typeof(string)));

            DataTable result = CSVParser.ReadDefinedCSV(schema, filename);
            return result;
        }


        public static void PrintDataTable(DataTable table) {
            //Print the header
            foreach (DataColumn col in table.Columns) {
                Console.Write($"{col.ColumnName}\t");
            }
            Console.Write("\n");
            //Print the rows
            foreach (DataRow r in table.Rows) {
                foreach (var item in r.ItemArray) {
                    Console.Write($"{item}\t");
                }
                Console.Write("\n");
            }
        }
    }
}
