# AlphaCSV
A small netstandard2.0 library that reads and writes CSV files. The goal of this
library is to help anyone who makes small automation or migration projects. It can
however be used in larger projects.

In general the library is designed to import the CSV into a datatable from there the
one can use the datatable however he likse. Each column of the datatable can have different 
types which can be described in a datatable schema.

This library can also parse CSV files into `List<T>` where `T` is user defined classes or
records provided that those types:

1) Provide public parameterless constructors
2) Have public set properties for each of the column defined in a CSV file.

## Before Proceeding
This library is released as is and is still in <b>ALPHA</b> state. I am not reponsible
for any issues or possible data loss that may occur with the use of this library.

## Installation
In order to use the library just install the nuget package from nuget.org.

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

### Performing field validation with validation delegates
When field validation must be performed during parsing the user may define delegate functions that receive a string input and return a boolean
indicating if the validation is successful. For example define validator methods.
```csharp
        bool StringValidator(string input) {
            return true;
        }

        bool IntValidator(string input) {
            return input.Contains('1');
        }

        bool DateValidator(string input) {
            return input.StartsWith("15");
        }
```
Then create a list of delegates and pass them as an argument to Parsing method:

```csharp
            Func<string, bool> Validatora = StringValidator;
            Func<string, bool> Validatorb = IntValidator;
            Func<string, bool> Validatorc = DateValidator;
            List<Func<string, bool>> validators = new List<Func<string, bool>>() {
                Validatora,
                Validatorb,
                Validatorc
            };
            CSVParser parser = new CSVParser();
            CSVParseOptions options = new CSVParseOptions();
            options.ValidateFields = true;
            DataTable result = parser.ParseDefinedCSV(mySchema,"test.csv",options,validators);
            AssertDataTable.AreEqual(expectedData,result);
```
Note that the validation needs to be enabled in the options.



### Parsing custom types with the `ParseType<T>` method.
If you have custom types then those could be parsed as the following example:
For a record
```csharp
public record Person {
	public string Name { get; set; }
	public string Surname { get; set; }
}
```
A CSV file containing the type could be parsed as follows:
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
This class controls the behavior of the `CSVParser` class and may be passed to the parser's methods as an optional argument.

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
- The `DecimalSeperator` Indicates which character will be used a decimal number seperator. Default value is '.'

## The `CSVWriteOptions` Class
This class controls the behavior of the `CSVWriter` and may be passed to the writer's methods as an optional agument.

The options can be set through the public properies of the class. The parameters that can be controlled are the following :
- The `Delimeter`. This is the character that seperates the fields within the file. (Default value of `,`).
- The `QuoteCharacter`. This is the character that escapes a field if that field contains the `Delimerer` character.
- The `WriteHeaders`. This option directs the `CSVWriter` to indicate if the datatable names should be written as headers in the CSV file (Default value of `true`).
- The `DateTimeFormat` indicates to the `CSVWriter` the format of the DateTime that will be writtern in the file. (Default is `"dd-MMM-yyyy"`).
- The `DecimalSeperator` Indicates which character will be used a decimal number seperator. Default value is '.'

## Setting up a Development environment
The project uses commit linting thus:

you need to install node js and npm and follow the instructions bellow

```sh
# Install commitlint cli and conventional config
npm install --save-dev @commitlint/{config-conventional,cli}
# For Windows:
npm install --save-dev @commitlint/config-conventional @commitlint/cli

# Configure commitlint to use conventional config
echo "module.exports = {extends: ['@commitlint/config-conventional']}" > commitlint.config.js
```

To lint commits before they are created you can use Husky's `commit-msg` hook:

```sh
# Install Husky v6
npm install husky --save-dev
# or
yarn add husky --dev

# Activate hooks
npx husky install
# or
yarn husky install

# Add hook
npx husky add .husky/commit-msg 'npx --no -- commitlint --edit "$1"'
# Sometimes above command doesn't work in some command interpreters
# You can try other commands below to write npx --no -- commitlint --edit $1
# in the commit-msg file.
npx husky add .husky/commit-msg \"npx --no -- commitlint --edit '$1'\"
# or
npx husky add .husky/commit-msg "npx --no -- commitlint --edit $1"

# or
yarn husky add .husky/commit-msg 'yarn commitlint --edit $1'
```

Check the [husky documentation](https://typicode.github.io/husky/#/?id=manual) on how you can automatically have Git hooks enabled after install for different `yarn` versions.

**Detailed Setup instructions**

- [Local setup](https://conventional-changelog.github.io/commitlint/#/guides-local-setup) - Lint messages on commit with husky
- [CI setup](https://conventional-changelog.github.io/commitlint/#/guides-ci-setup) - Lint messages during CI builds
