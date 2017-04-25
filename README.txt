# DumpElection

This project uses the data provided by French government on elections.interieur.gouv.fr to make a CSV file.
It merges many HTML files into a single easily usable CSV file.

In order to work, you have to install Visual Studio and open the DumpElection.sln.
There is a free version of Visual Studio named Visual Studio Community that can be freely downloaded on Microsoft's website.

You will then have to build and run the project which can be achieved by pressing F5.

A black console should open and tell you whenever a department has been processed.
The console will remain empty until the first one has been processed.

When done or if an error occurs, the console will automatically close.
If the process was successful a "elections.csv" file is created in the DumpElection/bin/Debug or DumpElection/bin/Release directory depending on you current configuration.

A dump can be found at: https://linx.li/elections.csv
