#!/bin/bash
servis3com="docker-compose up -d --build --force-recreate"
servis3="TradingCore"

echo command: $servis3com $servis3
$servis3com $servis3
