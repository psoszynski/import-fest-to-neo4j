# import-fest-to-neo4j
An application to import data from a schema based xml with information about medicines in Norway to neo4j database.

Fest files that constitute input to the application can be downloaded from: https://legemiddelverket.no/andre-temaer/fest/nedlasting-av-fest

Possible usecases:
dotnet run "C:\Temp\fest25\fest25.xml"
dotnet run "C:\Temp\fest25.xml" "C:\Temp\Neo4jImport"
dotnet run "C:\Temp\fest25.xml" LMM
dotnet run "C:\Temp\fest25.xml" LMV
dotnet run "C:\Temps\fest25.xml" VMS

Neo4j desktop can be downloaded from: https://neo4j.com/download-center/#desktop

Once a neo4j database are created and csv and cypher files are copied to import folder one can import these csv files by executing the following commando in cmd (in root folder of the database):

type import\installfest.cypher | bin\cypher-shell.bat -u <username> -p <password>
