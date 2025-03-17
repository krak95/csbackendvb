using System.Net.WebSockets;
using System.Text;
using webAPIreact.Models;

public class WebSocketHandler
{
    public async Task HandleWebSocketAsync(WebSocket webSocket)
    {
        string rtspUrl = "rtsp://localhost:8554/mystream";
        string ffmpegCmd = $"-rtsp_transport tcp -i {rtspUrl} -f mjpeg -";
        string ffmpegPath = @"C:\Users\jamorim\AppData\Local\Microsoft\WinGet\Packages\Gyan.FFmpeg_Microsoft.Winget.Source_8wekyb3d8bbwe\ffmpeg-7.1.1-full_build\bin\ffmpeg.exe";

        using (var process = new System.Diagnostics.Process())
        {
            process.StartInfo.FileName = ffmpegPath;
            process.StartInfo.Arguments = ffmpegCmd;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.Start();

            byte[] buffer = new byte[4096];
            while (!process.StandardOutput.EndOfStream && webSocket.State == WebSocketState.Open)
            {
                int bytesRead = await process.StandardOutput.BaseStream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead > 0)
                {
                    await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, bytesRead),
                        WebSocketMessageType.Binary, true, CancellationToken.None);
                }
            }
        }
    }
}
