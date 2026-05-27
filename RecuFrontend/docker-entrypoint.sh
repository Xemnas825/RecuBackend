#!/bin/sh
set -e
sed -i "s|__API_BASE__|${API_BASE_URL}|g" /usr/share/nginx/html/app.js
exec nginx -g 'daemon off;'
