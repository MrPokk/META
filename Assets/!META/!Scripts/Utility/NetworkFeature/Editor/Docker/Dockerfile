FROM ubuntu:22.04
ARG DEBIAN_FRONTEND=noninteractive
ARG SERVER_BUILD_PATH=Builds/Server
COPY ${SERVER_BUILD_PATH} /root/build/
WORKDIR /root/
RUN chmod +x /root/build/metaServer
RUN apt-get update && \
    apt-get install -y ca-certificates && \
    apt-get clean && \
    update-ca-certificates
CMD [ "/root/build/metaServer", "-batchmode -nographics", "$UNITY_COMMANDLINE_ARGS"]