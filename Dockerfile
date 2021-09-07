FROM mcr.microsoft.com/dotnet/sdk:5.0 AS buildenv

RUN git clone --recursive https://github.com/archivedc/youtube-dl-discord-connect /build && \
    dotnet build /build/youtube-dl-discord-connect/youtube-dl-discord-connect.csproj -c Release -o /artifact

# ------------------------------------------------------

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS runenv

COPY --from=buildenv /artifact /app

RUN apt-get update && apt-get install -y python3-pip python3 && rm -rf /var/lib/apt/lists/* && ln -s /usr/bin/python3 /usr/bin/python

RUN pip3 install -r /app/livechat_downloader/requirements.txt && pip3 install youtube_dl

WORKDIR /app/

ENTRYPOINT ["dotnet", "youtube-dl-discord-connect.dll"]
