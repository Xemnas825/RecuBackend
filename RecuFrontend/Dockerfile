FROM nginx:alpine
COPY index.html styles.css sheet-rules.js app.js /usr/share/nginx/html/
COPY docker-entrypoint.sh /docker-entrypoint.sh
RUN sed -i 's/\r$//' /docker-entrypoint.sh && chmod +x /docker-entrypoint.sh
ENV API_BASE_URL=http://localhost:8080
EXPOSE 80
ENTRYPOINT ["/docker-entrypoint.sh"]
