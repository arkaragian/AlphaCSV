# [](https://github.com/arkaragian/AlphaCSV/compare/v1.0.0-beta.0...v) (2022-03-15)


### Bug Fixes

* fix issue [#9](https://github.com/arkaragian/AlphaCSV/issues/9) ([888c935](https://github.com/arkaragian/AlphaCSV/commit/888c93553f32e1c7eb7754b92c3280ba452f8c72))



# [1.0.0-beta.0](https://github.com/arkaragian/AlphaCSV/compare/v1.0.0-alpha0...v1.0.0-beta.0) (2022-03-15)


### Bug Fixes

* **`AssertDataTable`:** `AreEqual` now throws exception when row count does not match. ([ddfcf35](https://github.com/arkaragian/AlphaCSV/commit/ddfcf35d3482ef02486750fc1ef945dcd9d99799))


* refactor!: `CSVWriter` now uses `CSVWriteOptions` ([03d035e](https://github.com/arkaragian/AlphaCSV/commit/03d035eb9dc6b31b4262843ac435a90330305e08))
* feat!: accept validators. issue #8 ([f6bda6a](https://github.com/arkaragian/AlphaCSV/commit/f6bda6a74be0a39ec515b17e66f434ddfbc2bf31)), closes [#8](https://github.com/arkaragian/AlphaCSV/issues/8)


### Features

* **`AlphaCSV`:** The project now targets `netstandard2.0` instead of `net6.0`. ([be1cabf](https://github.com/arkaragian/AlphaCSV/commit/be1cabff9330e02beb4f53e2ff691aa27a050b6c))
* **`CSVParser`:** Resolving issue [#6](https://github.com/arkaragian/AlphaCSV/issues/6). ([f439493](https://github.com/arkaragian/AlphaCSV/commit/f439493b0a53ffd546a8f8276d30189d78ac547c))
* **`CSVParserOptions`:** Resolving issue [#6](https://github.com/arkaragian/AlphaCSV/issues/6). ([46d88e7](https://github.com/arkaragian/AlphaCSV/commit/46d88e73eb3332531a643a6b8f943ef7bef7b39f))
* **`CSVWriteOptions`:** add `QuoteFieldsWithoutDelimeter` flag ([f876dcf](https://github.com/arkaragian/AlphaCSV/commit/f876dcf93a2775cc12ac2c553e3c5f745a8bc085))
* **`CSVWriter`:** take into account the `QuoteFieldsWithoutDelimeter` flag ([e112498](https://github.com/arkaragian/AlphaCSV/commit/e112498a7b920dd62a3db4b443ea2d9855142bf5))


### BREAKING CHANGES

* The `CSVParseOptions` can no longer be used for
writing CSV files.
* Regular expressions are no longer beeing accepted.
Any old code that used such expressions should now be wrapped arround
a delegate method.



# 1.0.0-alpha0 (2022-01-30)



