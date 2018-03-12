#!/bin/bash
servis4com="docker-compose up -d --build --force-recreate"
servis4="DataScraper"

echo command: $servis4com $servis4
$servis4com $servis4
