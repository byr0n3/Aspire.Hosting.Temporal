#!/bin/sh -eu

echo 'Creating Temporal database and tables…'

# Create database and setup tables.
temporal-sql-tool --plugin postgres12 --ep $POSTGRES_SEEDS -p $DB_PORT -u $POSTGRES_USER -pw $POSTGRES_PWD --db temporal create
temporal-sql-tool --plugin postgres12 --ep $POSTGRES_SEEDS -p $DB_PORT -u $POSTGRES_USER -pw $POSTGRES_PWD --db temporal setup-schema -v 0.0
temporal-sql-tool --plugin postgres12 --ep $POSTGRES_SEEDS -p $DB_PORT -u $POSTGRES_USER -pw $POSTGRES_PWD --db temporal update-schema -d /etc/temporal/schema/postgresql/v12/temporal/versioned 

echo 'Creating visibility database and tables…'

# Create visibility database and setup tables.
temporal-sql-tool --plugin postgres12 --ep $POSTGRES_SEEDS -p $DB_PORT -u $POSTGRES_USER -pw $POSTGRES_PWD --db temporal_visibility create
temporal-sql-tool --plugin postgres12 --ep $POSTGRES_SEEDS -p $DB_PORT -u $POSTGRES_USER -pw $POSTGRES_PWD --db temporal_visibility setup-schema -v 0.0
temporal-sql-tool --plugin postgres12 --ep $POSTGRES_SEEDS -p $DB_PORT -u $POSTGRES_USER -pw $POSTGRES_PWD --db temporal_visibility update-schema -d /etc/temporal/schema/postgresql/v12/visibility/versioned

echo 'Creating config file…'

# Setup the config file using the passed Docker arguments.
dockerize -template /etc/temporal/config/config_template.yaml:/etc/temporal/config/docker.yaml

echo 'Starting namespace creation script…'

# Start the creation of the Temporal namespace in the background.
exec /etc/create-namespace.sh &

echo 'Starting Temporal…'

# Run the Temporal server.
temporal-server --env docker start
