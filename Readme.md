Updated: 2018.03.03
Private solution to automatize some actions for a stock trading.
Language: C#, framework: ASP.NET Core 2.0, Docker.

Parts:
TradingCore - main program and database Mysql.
    - Nlog to SQLite database;
    - Docker image FROM microsoft/aspnetcore:latest.


DataScraper - extracts data from websites, eg.: Yahoo finance,
    - Nlog to SQLite database;
    - Docker image FROM microsoft/aspnetcore:latest;
    - Used a Selenium standalone Chrome Docker image for grap a data from websites with javascript.

Docker containers in start order:
    - Trading_Database;
    - Trading_SeleniumChrome;
    - Trading_TradingCore;
    - Trading_DataScraper.

