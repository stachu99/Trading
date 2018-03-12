#!/bin/bash
project="Trading"
directoryPath="/var/lib/Trading_dev"

com="docker-compose down"
servis1com="docker-compose up -d --build"
servis1="Database"
container1com="bash WaitForContStatHealthy.sh"
container1="Trading_Database_Dev"
servis2com="docker-compose up -d --build"
servis2="TradingCore"
servis3com="docker-compose up -d --build"
servis3="DataScraper"

echo Start $project Project ...
mkdir $directoryPath
mkdir $directoryPath/Database
mkdir $directoryPath/TradingCore
mkdir $directoryPath/DataScraper

echo command: $com
$com
echo command: $servis1com $servis1
$servis1com $servis1
echo command: $container1com $container1
$container1com $container1
echo command: $servis2com $servis2
$servis2com $servis2
echo command: $servis3com $servis3
$servis3com $servis3
echo End of the script.
