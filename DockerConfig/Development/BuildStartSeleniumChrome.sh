#!/bin/bash
servis2com="docker-compose up -d --build --force-recreate"
servis2="SeleniumChrome"

echo command: $servis2com $servis2
$servis2com $servis2
