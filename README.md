# auto-highlighter-back-end
a tool for streamers to automatically create highlights from their vods


To run this repo you need an appsettings.json file that follows this format (just replace the connection strings with your own. The one provided is invalid)

```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "AllowedHosts": "*",
  "FileUploadLocation": "AppData",
  "ConnectionStrings": {
    "AzureBlobStorage": "DefaultEndpointsProtocol=https;AccountName=autohighlightervods;AccountKey=vDQgW4DJlIPHzIcIFjofXwf2MOkUN5VJf/uVhEBy3qnYKxj80Fpy8EgGqZ1dcYfGj0eGy36XR8SVv2l2wClJeg==;EndpointSuffix=core.windows.net",
    "AzureServiceBus": "Endpoint=sb://autovodhighlightermq.servicebus.windows.net/;SharedAccessKeyName=consume-produce;SharedAccessKey=675dy2+QsHDYqScMZtA3CVmm7fUX2cQDbiVPlD2L2sI="
  },
  "ThrottleSettings": {

  },
  "HighlightSettings": {
    "HighlightLength": 30000
  },
  "FFmpegSettings": {
    "ExecutableLocation": "ffmpeg"
  },
  "ServiceBus": {
    "Name": "higlight-queue"
  }
}
```
