#!/bin/bash
SLEEP_LENGHT=2

function getContainerHealth {
  docker inspect --format "{{json .State.Health.Status }}" $1
}

function waitContainer {
echo Checking health status for container: $1 until status become \"healthy\". Interval: $SLEEP_LENGHT s.
  while STATUS=$(getContainerHealth $1); [ $STATUS != "\"healthy\"" ]; do
    if [ $STATUS == "\"unhealthy\"" ]; then
      echo Failed! Container: $1, status: $STATUS.
      exit -1
    fi
    echo Container: $1, status: $STATUS.
    sleep $SLEEP_LENGHT
  done
  echo Container: $1, status: $STATUS.
}

for var in "$@"
do
  container=${var}
  waitContainer $container
done
