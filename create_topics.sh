#!/bin/bash
# This script searches recursively for .env files, extracts env variables
# whose names include "_TOPIC_" and creates a Kafka topic for each found variable.

# Base directory to search for .env files (change as needed)
BASE_DIR="."

PARTITIONS=1
REPLICATION_FACTOR=1
BOOTSTRAP_SERVER="localhost:9092"

echo "Searching for .env files under ${BASE_DIR}"
env_files=$(find "$BASE_DIR" -type f -name ".env")

for file in $env_files; do
  echo "Processing ${file}"

  while IFS= read -r line || [[ -n "$line" ]]; do
    [[ -z "$line" || "$line" =~ ^[[:space:]]*# ]] && continue

    if [[ $line =~ ^([^=]*_TOPIC_[^=]*)=(.*)$ ]]; then
      var_name="${BASH_REMATCH[1]}"
      topic_value="${BASH_REMATCH[2]}"

      topic_value=$(echo "$topic_value" | sed -e 's/^"//' -e 's/"$//')

      if [[ -z "$topic_value" ]]; then
        echo "Skipping $var_name because it is empty"
        continue
      fi

      echo "Creating topic for variable '${var_name}' with topic name '${topic_value}'"
      
      docker exec -it ist-kafka-1 kafka-topics.sh \
        --create \
        --bootstrap-server "$BOOTSTRAP_SERVER" \
        --topic "$topic_value" \
        --partitions "$PARTITIONS" \
        --replication-factor "$REPLICATION_FACTOR"

      if [[ $status -eq 0 ]]; then
        echo "Topic '${topic_value}' created successfully."
      else
        if echo "$output" | grep -qi "already exists"; then
          echo "Topic '${topic_value}' already exists. Skipping."
        else
          echo "Failed to create topic '${topic_value}'. Error: $output"
        fi
      fi

    fi
  done < "$file"
done

echo "All applicable topics processed."
