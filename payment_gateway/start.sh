#!/bin/sh

crond

exec uvicorn main:app --host 0.0.0.0 --port 8089
