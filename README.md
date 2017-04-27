# DumpElection

> Merges French 2017 election results into a CSV file.

Retrieves all French 2017 election results from many HTML files and merges them
into a single easily usable CSV file.

The data used is provided by [French government](elections.interieur.gouv.fr)

## Pre-Requisites

_Visual Studio_ must be installed.

The _Visual Studio Community_ version can fe freely downloaded on
[Microsoft's website](https://imagine.microsoft.com/en-us/Catalog/Product/530).

## Usage

1. Open the `DumpElection.sln` project under _Visual Studio_
2. Build then run the project, which can be achieved by pressing F5.
3. An informative black console will open to show the progress of department
   processing. Note that the console will remain empty until the first
   department has been processed.
4. At the end of the processing, the console is closed and a `elections.csv`
   file is created in `DumpElection/bin/Debug` or `DumpElection/bin/Release`
   directory depending on you current run configuration.

If an error occurs, the console will automatically close.

## See Also

- A dump of a generated CSV can be found [here](https://linx.li/elections.csv)
