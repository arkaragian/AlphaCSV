# AlphaCSV

## Installation
In order to use the library install the relevant nuget package.

## Usage
Just include the following line in the start of your program or in your global usings.
```csharp
using AlphaCSV;
```

### Parsing of a simple file

This library handles the parsing of CSV files. For example a simple csv file such as the following:
```
ColumnA,ColumnB
Row1,ValA
Row2,ValB
Row3,ValC
```
Can be parsed as follows:
```csharp
DataTable schema = new DataTable();
schema.Columns.Add(new DataColumn("ColumnA", typeof(string)));
schema.Columns.Add(new DataColumn("ColumnB", typeof(string)));

CSVParser parser = new CSVParser();

DataTable result = parser.ParseDefinedCSV(schema, filename);
```
The CSV file is then parsed into a datatable. Note that the schema of the file
has to be defined, both in terms of column names and also in terms of `type`.

## The `CSVParseOptions` Class
This class controls the behavior of the csv parser and may be passed to the parser method as an optional argument.

The options can be set through the public properies of the class. The parameters that can be controlled are the following :
- The `Delimeter`. This is the character that seperates the fields within the file. (Default value of `,`).
- The `CommentCharacter`. Every line that starts with this character is ignored. (Default value of `#`).
- The `QuoteCharacter`. This is the character that escapes a field if that field contains the `Delimerer` character.
- The `CheckHeaders` indicates to the parser if it must validate the file headers against a schema. (Default `false`).
- The `ValidateFields` indicates to the parser if it must validate the field values against a provided regular expression (Default `false`).
- The `DateTimeFormat` indicates to the parser the format of any DateTime that may be contained in the file. (Default is `""`).
- `ContainsHeaders` indicates if the CSV file has a header row. (Default value is `false`) note that this option is not taken into account when reading a defined CSV.
- The `AllowEmptyLastField` Indicates wether the last field of the file lines can be empty. (eg when there is no data on the last field) Default `true`.