#!/bin/bash
project="Trading"
com="docker-compose down"
servis1com="docker-compose up -d --build"
servis1="database"
container1com="bash WaitForContStatHealthy.sh"
container1="Trading_Database_Dev"
servis2com="docker-compose up -d --build"
servis2="tradingcore"
servis3com="docker-compose up -d --build"
servis3="YahooScraper"

echo Start $project Project ...
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
