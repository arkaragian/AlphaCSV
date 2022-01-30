//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using AlphaCSV;
using System.Data;

namespace libCSVExamples {
    internal class Program {
        static void Main(string[] args) {
            Console.WriteLine("Welcome to AlphaCSV examples. Those examples showcase the use of the library.");

            DataTable simple = ReadSimpleCSV("SampleCSV/TwoColumnCSV.csv");

            PrintDataTable(simple);

            WriteToCSV();
        }

        public static DataTable ReadSimpleCSV(string filename) {
            DataTable schema = new DataTable();
            schema.Columns.Add(new DataColumn("ColumnA", typeof(string)));
            schema.Columns.Add(new DataColumn("ColumnB", typeof(string)));

            CSVParser Parser = new CSVParser();
            DataTable result = Parser.ParseDefinedCSV(schema, filename);
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

        public static void WriteToCSV() {
            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn("Hello",typeof(string)));
            table.Columns.Add(new DataColumn("Value",typeof(int)));

            DataRow r = table.NewRow();
            r[0] = "Aris";
            r[1] = 20;
            
            table.Rows.Add(r);

            r = table.NewRow();
            r[0] = "Test";
            r[1] = 25;

            table.Rows.Add(r);

            CSVWriter writer = new CSVWriter();
            writer.WriteCSV("WriteTest.csv", table);
        }
    }
}
