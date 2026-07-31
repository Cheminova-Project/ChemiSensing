using GLTFast;
using GLTFast.Loading;
using UnityEngine.Networking;
using System.Threading.Tasks;
using System;
using UnityEngine;

public class AuthorizedDownloadProvider : IDownloadProvider
{
    private string token;
    private bool allowTextures;

    public AuthorizedDownloadProvider(string bearerToken, bool allowTextures = true)
    {
        token = bearerToken;
        this.allowTextures = allowTextures;
    }

    public async Task<IDownload> Request(Uri url)
    {
        string lowerUrl = url.ToString().ToLower();

        if (!allowTextures && (
            lowerUrl.EndsWith(".png") ||
            lowerUrl.EndsWith(".jpg") ||
            lowerUrl.EndsWith(".jpeg")
        ))
        {
            Debug.Log($"[AuthorizedDownloadProvider] Bloqueando imagen desde Request(): {url}");
            return new EmptyDownload(); // igual que EmptyTextureDownload pero para IDownload
        }

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        var download = new CustomDownload(request);
        await download.Run();
        request.Dispose();
        return download;
    }

    // Optional: override for texture or buffer downloads if needed
    public async Task<ITextureDownload> RequestTexture(Uri url, bool nonReadable)
    {
        if (!allowTextures)
        {
            Debug.Log($"[AuthorizedDownloadProvider] Bloqueando textura: {url}");
            // 1x1 transparente (no descarga, rápido, y compatible con glTFast)
            return new EmptyTextureDownload(1, 1, Color.clear, nonReadable);
        }

        var request = UnityWebRequestTexture.GetTexture(url.ToString(), nonReadable);
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        var download = new CustomTextureDownload(request);
        await download.Run();
        return download;

    }
}
