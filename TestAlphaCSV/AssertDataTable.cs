
//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.Data;

namespace TestAlphaCSV {
    public static class AssertDataTable {
        //https://stackoverflow.com/questions/18365915/assert-areequal-method-usage-in-checking-datatable-types

        public static void AreEqual(DataTable expected, DataTable actual) {
            if ((expected == null) && (actual == null)) {
                return;
            } else if ((expected != null) && (actual != null)) {
                if ((expected.Columns.Count == actual.Columns.Count)) {
                    //Check that the column types are the same
                    for (int i = 0; i < expected.Columns.Count; i++) {
                        if (expected.Columns[i].DataType != actual.Columns[i].DataType) {
                            throw new Exception($"The type of of columns is not the same in both tables! Expected:{expected.Columns[i].DataType} Actual:{actual.Columns[i].DataType} for index {i}");
                        }
                    }
                    //Check that the headers are the same
                    for (int i = 0; i < expected.Columns.Count; i++) {
                        if (expected.Columns[i].ColumnName != actual.Columns[i].ColumnName) {
                            throw new Exception($"Column names do not match! Expected:{expected.Columns[i].ColumnName} Actual:{actual.Columns[i].ColumnName} for index {i}");
                        }
                    }
                } else {
                    throw new Exception("Tables do not have the same number of columns");
                }

                //Up to this point we have verified the columns. Now we are going th verify the rows
                if (expected.Rows.Count == actual.Rows.Count) {
                    for (int i = 0; i < expected.Columns.Count; i++) {
                        for (int j = 0; j < expected.Rows.Count; j++) {
                            if (!expected.Rows[j][i].Equals(actual.Rows[j][i])) {
                                throw new Exception($"Cells are not equal! In Row {j} and Column {i} Expected {expected.Rows[j][i]} Actual {actual.Rows[j][i]}");
                            }
                        }
                    }
                    return;
                } else {
                    return;
                }
            } else {
                if (expected == null) {
                    throw new Exception("Expected table is null!");
                }

                if (actual == null) {
                    throw new Exception("Actual table is null!");
                }
            }
        } //End of Compare Data Datables
    } //End of class
} //End of namespace
