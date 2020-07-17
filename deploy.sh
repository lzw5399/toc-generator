# $1代表执行shell时外部传入的参数
buildNumber=$1
serverChartLocation=$2
cd $serverChartLocation
echo -e "\033[36m start deploying, current buildNumber=$buildNumber \033[0m"

# pull newest image
echo -e "\033[36m step1: check whether toc-helm exists  \033[0m"
tochelm=$(helm ls | grep toc-release)
if test -n "$tochelm"; then
  helm install -f values.yaml --set env.buildnumber=$buildNumber toc-release .
else
  helm upgrade -f values.yaml --set env.buildnumber=$buildNumber toc-release .
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
