#!/bin/bash
set -o errexit

# $1 and $2 represent externally passed parameters when executing the shell
buildNumber=$1
serverChartLocation=$2
cd $serverChartLocation
echo -e "\033[36m start deploying \033[0m"
echo -e "\033[32m log: current buildNumber=$buildNumber \033[0m"
echo -e "\033[32m log: current serverChartLocation=$serverChartLocation \033[0m"

# install or upgrade helm release
echo -e "\033[36m step1: check whether toc-helm exists  \033[0m"
if test -z "$(helm ls | grep toc-release)"; then
  echo -e "\033[32m log: current doesn't exist toc-release, will install one\033[0m"
  helm install -f values.yaml --set env.buildnumber=$buildNumber --set image.tag=$buildNumber toc-release .
else
  echo -e "\033[32m log: current already exist toc-release, will upgrade it \033[0m"
  helm upgrade -f values.yaml --set env.buildnumber=$buildNumber --set image.tag=$buildNumber toc-release .
fi

# remove dangling images
echo -e "\033[36m step2: remove dangling images \033[0m"
danglings=$(sudo docker images -f "dangling=true" -q)
if test -n "$danglings"; then
  sudo docker rmi $(sudo docker images -f "dangling=true" -q) >>/dev/null 2>&1
  if [[ $? != 0 ]]; then
    echo 'failed to remove danglings container...'
    exit $?
  fi
fi

echo -e "\033[36m done! \033[0m"
exit 0
