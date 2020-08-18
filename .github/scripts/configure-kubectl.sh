#!/usr/bin/env bash

set -eo pipefail

touch "$GITHUB_WORKSPACE/.kubeconfig"

echo "LS0tLS1CRUdJTiBDRVJUSUZJQ0FURS0tLS0tCk1JSUJWekNCL3FBREFnRUNBZ0VBTUFvR0NDcUdTTTQ5QkFNQ01DTXhJVEFmQmdOVkJBTU1HR3N6Y3kxelpYSjIKWlhJdFkyRkFNVFU1TlRZd05qUXdOakFlRncweU1EQTNNalF4TmpBd01EWmFGdzB6TURBM01qSXhOakF3TURaYQpNQ014SVRBZkJnTlZCQU1NR0dzemN5MXpaWEoyWlhJdFkyRkFNVFU1TlRZd05qUXdOakJaTUJNR0J5cUdTTTQ5CkFnRUdDQ3FHU000OUF3RUhBMElBQkRRTTJWa3ErN3BOMVduWFkwMkcrVmtSWVE0SFBqSUw5YUF1NTl4bTVPeVEKMnNmZlpYckZGTWE3MEt6cEdIUGkyL09pTEpFR2lhd3FpOEJzYXRKSWJWaWpJekFoTUE0R0ExVWREd0VCL3dRRQpBd0lDcERBUEJnTlZIUk1CQWY4RUJUQURBUUgvTUFvR0NDcUdTTTQ5QkFNQ0EwZ0FNRVVDSVFEVFFSaWJwQkszCnFpZ3JLSEc5UXpnbzVjMXNlUG1ueDJMQWRCM29iQ1BNdEFJZ044aHVoTjdDS1p5VW14U1RrWkNoLzBBbUJGWlUKaC90cHJ4cHhTNVJTaHhFPQotLS0tLUVORCBDRVJUSUZJQ0FURS0tLS0tCg==" \
  | base64 -d > .kube.ca.crt

kubectl --kubeconfig="$GITHUB_WORKSPACE/.kubeconfig" config set-cluster seemy.app \
  --server=https://k3s.seemy.app:6443 \
  --certificate-authority=.kube.ca.crt
kubectl --kubeconfig="$GITHUB_WORKSPACE/.kubeconfig" config set-credentials cluster-admin \
  --username=admin --password="$K3S_PASSWORD"
kubectl --kubeconfig="$GITHUB_WORKSPACE/.kubeconfig" config set-context seemy.app \
  --cluster=seemy.app --user=cluster-admin
kubectl --kubeconfig="$GITHUB_WORKSPACE/.kubeconfig" config use-context seemy.app
