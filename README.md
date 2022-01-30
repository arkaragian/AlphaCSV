# AlphaCSV
A small .net 6 library that reads and writes CSV files. The goal of this library
is to help anyone who makes small automation or migration projects. It can however
be used in larger projects.

The library imports the CSV into a datatable. The columns of the datatable can have
multiple types which can be described in a datatable schema.

This library can also parse CSV files into lists of classes or recrods provided that:

1) They provide public parameterless constructors
2) Hve public set properties

## Installation
In order to use the library install the relevant nuget package.

## Usage
Just include the following line in the start of your program or in your global usings.
```csharp
using AlphaCSV;
```

### Parsing of a simple file

A simple csv file such as the following:
```
ColumnA,ColumnB
Row1,ValA
Row2,ValB
Row3,ValC
```
Can be parsed as follows:
```csharp
CSVParser parser = new CSVParser();
DataTable result = parser.ParseSimpleCSV(filename);
```
Note: The above method will produce a datatable whose columns will be of `string` type.

### Defining types on the `DataTable` columns.
Sometimes you may need a more complex file to parse. A file where you might need a
different schema. This function can be performed with the `ParseDefinedCSV` method.

For example for a file like this:

```
ColumnA,ColumnB
Row1,1
Row2,3
Row3,5
```

```csharp
DataTable schema = new DataTable();
schema.Columns.Add(new DataColumn("ColumnA", typeof(string)));
schema.Columns.Add(new DataColumn("ColumnB", typeof(int)));

CSVParser parser = new CSVParser();
DataTable result = parser.ParseDefinedCSV(schema, filename);
```

The CSV file is then parsed into a datatable. Note that the schema of the file
has to be defined, both in terms of column names and also in terms of `type`.

### Parsing custom types with the `ParseType` method.
If you have custom types then those could be parsed as the following example:
For a record
```csharp
public record Person {
	public string Name { get; set; }
	public string Surname { get; set; }
}
```
A CSV file could can be parsed as follows:
```csharp
CSVParser parser = new CSVParser();
List<Person> persons = parser.ParseType<Person>(filepath);
```
Of course the CSV file should have the following form
```
Name,Surname
John,Doe
Jane,Doe
```
The column ordering does not matter as long as the correct property names are given. The following file is just as valid:
```
Surname,Name
Doe,John
Doe,Jane
```


## The `CSVParseOptions` Class
This class controls the behavior of the csv parser and may be passed to the parser method as an optional argument.

The options can be set through the public properies of the class. The parameters that can be controlled are the following :
- The `Delimeter`. This is the character that seperates the fields within the file. (Default value of `,`).
- The `CommentCharacter`. Every line that starts with this character is ignored. (Default value of `#`).
- The `QuoteCharacter`. This is the character that escapes a field if that field contains the `Delimerer` character.
- The `CheckHeaders` indicates to the parser if it must validate the file headers against a schema. (Default `false`). This does not apply for the `ParseType`
method as it always checks for the correct names.
- The `ValidateFields` indicates to the parser if it must validate the field values against a provided regular expression (Default `false`).
- The `DateTimeFormat` indicates to the parser the format of any DateTime that may be contained in the file. (Default is `""`).
- The `ContainsHeaders` indicates if the CSV file has a header row. (Default value is `false`) note that this option is not taken into account when reading a defined CSV.
- The `AllowEmptyLastField` Indicates wether the last field of the file lines can be empty. (eg when there is no data on the last field) Default `true`.

