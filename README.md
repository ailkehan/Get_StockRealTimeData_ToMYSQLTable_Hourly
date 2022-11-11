# Get_StockRealTimeData_ToMYSQLTable_Hourly
Get any stock data and insert it to MYSQL table every hour 
 
1 - Gets stock data specified in the shareList variable by using the https://financialmodelingprep.com API.Current list contains AAPL, GOOG and MSFT.
2 - It uses C# periodic timer to access the API every hour.
3 - Inserts the data into the corresponding MYSQL table. For example, AAPL data is inserted to nasdaqdb.AAPL table. 
4 - The following SQL Create table command can be used to create the AAPL table. 

CREATE TABLE `aapl` (
  `ticker` char(5) NOT NULL,
  `datetime` datetime NOT NULL,
  `open` decimal(10,2) DEFAULT NULL,
  `price` decimal(10,2) DEFAULT NULL,
  `high` decimal(10,2) DEFAULT NULL,
  `low` decimal(10,2) DEFAULT NULL,
  `prevclose` decimal(10,2) DEFAULT NULL,
  `volume` int DEFAULT NULL,
  PRIMARY KEY (`datetime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
