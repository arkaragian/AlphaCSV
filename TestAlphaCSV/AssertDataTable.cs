
//
// Copyright (c) Aris Karagiannidis and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System;
using System.Data;

namespace TestlibCSV {
    public static class AssertDataTable {
        //https://stackoverflow.com/questions/18365915/assert-areequal-method-usage-in-checking-datatable-types

        public static void AreEqual(DataTable DT1, DataTable DT2) {
            if ((DT1 == null) && (DT2 == null)) {
                return;
            } else if ((DT1 != null) && (DT2 != null)) {
                if ((DT1.Columns.Count == DT2.Columns.Count)) {
                    //Check that the column types are the same
                    for (int i = 0; i < DT1.Columns.Count; i++) {
                        if (DT1.Columns[i].DataType != DT2.Columns[i].DataType) {
                            throw new Exception("The type of of columns is not the same in both tables!");
                        }
                    }
                    //Check that the headers are the same
                    for (int i = 0; i < DT1.Columns.Count; i++) {
                        if (DT1.Columns[i].ColumnName != DT2.Columns[i].ColumnName) {
                            throw new Exception("Column names do not match!");
                        }
                    }
                } else {
                    throw new Exception("Tables do not have the same number of columnt");
                }

                //Up to this point we have verified the columns. Now we are going th verify the rows
                if (DT1.Rows.Count == DT2.Rows.Count) {
                    for (int i = 0; i < DT1.Columns.Count; i++) {
                        for (int j = 0; j < DT1.Rows.Count; j++) {
                            if (!DT1.Rows[j][i].Equals(DT2.Rows[j][i])) {
                                throw new Exception("Cells are not equal!");
                            }
                        }
                    }
                    return;
                } else {
                    return;
                }
            } else {
                throw new Exception("One table is null and the other is not!");
            }
        } //End of Compare Data Datables
    } //End of class
} //End of namespace
