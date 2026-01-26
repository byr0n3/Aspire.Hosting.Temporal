#!/bin/sh -eu

# Wait for the Temporal server to be running.
while ! nc -z localhost 7233; do
  sleep 0.1
done

echo 'Temporal server is available! Creating namespace…'

# Try to get the details of the namespace.
# If this command fails, the namespace doesn't exist and we should try and create it.
temporal operator namespace describe -n $NAMESPACE || temporal operator namespace create -n $NAMESPACE
