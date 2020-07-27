# build stage
FROM golang:1.14 as builder

ENV GO111MODULE=on \
    GOPROXY=https://goproxy.cn,direct

WORKDIR /app

COPY . .

RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build .

RUN mkdir publish && cp toc-generator publish && \
    cp -r views publish && cp -r assets publish

# final stage
FROM scratch

WORKDIR /app

COPY --from=builder /app/publish .

ENV GIN_MODE=release \
    PORT=80

EXPOSE 80

ENTRYPOINT ["./toc-generator"]