#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class FileUploader : MonoBehaviour
{
    public void OpenWithFilePicker(Action<string[]> onFilesPicked)
    {
        Debug.Log("Opening with Native File Picker...");

#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Select a file", "", "");
        if (string.IsNullOrEmpty(path))
            onFilesPicked?.Invoke(null);
        else
            onFilesPicked?.Invoke(new string[] { path });
#elif UNITY_SERVER || UNITY_STANDALONE_LINUX
        Debug.LogWarning("File picker not supported on Dedicated Server or Linux.");
        onFilesPicked?.Invoke(null);
#else
        NativeFilePicker.PickMultipleFiles((paths) =>
        {
            StartCoroutine(HandleFilePickerResult(paths, onFilesPicked));
        });
#endif
    }

    private IEnumerator HandleFilePickerResult(string[] paths, Action<string[]> onFilesPicked)
    {
        yield return null;
        yield return null;
        
        onFilesPicked?.Invoke(paths);
    }
    
    public void OpenWithGallery(Action<string[]> onFilesPicked, Action<string[]> onNamesPicked = null)
    {
#if UNITY_EDITOR
        string path = EditorUtility.OpenFilePanel("Select media", "", "png,jpg,jpeg,heic,heif,mp4,mp3");
        if (string.IsNullOrEmpty(path))
        {
            onFilesPicked?.Invoke(null);
            onNamesPicked?.Invoke(null);
            return;
        }

        string finalPath = CopyOrConvertFile(path);
        if (finalPath != null)
        {
            string fileName = Path.GetFileName(finalPath);
            onFilesPicked?.Invoke(new string[] { finalPath });
            onNamesPicked?.Invoke(new string[] { fileName });
        }
        else
        {
            onFilesPicked?.Invoke(null);
            onNamesPicked?.Invoke(null);
        }

#else
        var mixedType = NativeGallery.MediaType.Image | NativeGallery.MediaType.Video | NativeGallery.MediaType.Audio;

        NativeGallery.GetMixedMediasFromGallery((paths) =>
        {
            StartCoroutine(HandleGalleryResult(paths, onFilesPicked, onNamesPicked));
        }, mixedType, "Select media files");
#endif
    }

    private IEnumerator HandleGalleryResult(string[] paths, Action<string[]> onFilesPicked, Action<string[]> onNamesPicked)
    {
        yield return null;
        yield return null;
        
        if (paths == null || paths.Length == 0)
        {
            onFilesPicked?.Invoke(null);
            onNamesPicked?.Invoke(null);
            yield break;
        }

        string[] finalPaths = new string[paths.Length];
        string[] fileNames = new string[paths.Length];

        for (int i = 0; i < paths.Length; i++)
        {
            string finalPath = CopyOrConvertFile(paths[i]);
            if (finalPath == null)
                continue;

            finalPaths[i] = finalPath;
            fileNames[i] = Path.GetFileName(finalPath);
        }
        
        onFilesPicked?.Invoke(finalPaths);
        onNamesPicked?.Invoke(fileNames);
    }
    
    private Texture2D MakeTextureReadable(Texture2D source)
    {
        RenderTexture tmp = RenderTexture.GetTemporary(
            source.width,
            source.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.sRGB);

        Graphics.Blit(source, tmp);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = tmp;

        Texture2D readableTex = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        readableTex.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
        readableTex.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tmp);

        return readableTex;
    }
    
    private string CopyOrConvertFile(string originalPath)
    {
        if (string.IsNullOrEmpty(originalPath) || !File.Exists(originalPath))
            return null;

        string ext = Path.GetExtension(originalPath).ToLowerInvariant();
        bool isHeic = ext == ".heic" || ext == ".heif";
        string newExt = isHeic ? ".jpg" : ext;
        string fileName = Path.GetFileNameWithoutExtension(originalPath) + newExt;
        string newPath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            if (isHeic)
            {
                Texture2D tex = NativeGallery.LoadImageAtPath(originalPath, maxSize: 2048);
                if (tex == null)
                {
                    return null;
                }

                // Convertir a textura readable preservando brillo
                Texture2D readableTex = MakeTextureReadable(tex);

                // Codificar a JPG y guardar
                byte[] jpgData = readableTex.EncodeToJPG(90);
                File.WriteAllBytes(newPath, jpgData);

                Destroy(tex);
                Destroy(readableTex);
            }
            else
            {
                File.Copy(originalPath, newPath, true);
            }

            return newPath;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error when copying or converting {originalPath}: {e}");
            return null;
        }
    }
    
    public string GetFileNameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

        try
        {
            Uri uri = new Uri(url);
            return Path.GetFileName(uri.LocalPath);
        }
        catch (UriFormatException)
        {
            return Path.GetFileName(url);
        }
    }
    
    public void OpenCamera(bool isPhoto, Action<string[]> onFilesPicked, Action<string[]> onNamesPicked = null)
    {
#if UNITY_EDITOR
        Debug.LogWarning("Camera is not supported in the Editor. Please use 'Select media' instead.");
        onFilesPicked?.Invoke(null);
        onNamesPicked?.Invoke(null);
#else
        NativeCamera.CameraCallback callback = (path) =>
        {
            StartCoroutine(HandleCameraResult(path, onFilesPicked, onNamesPicked));
        };

        if (isPhoto)
            NativeCamera.TakePicture(callback, maxSize: 2048);
        else
            NativeCamera.RecordVideo(callback);
#endif
    }

    private IEnumerator HandleCameraResult(string path, Action<string[]> onFilesPicked, Action<string[]> onNamesPicked)
    {
        yield return null;
        yield return null;

        if (string.IsNullOrEmpty(path))
        {
            onFilesPicked?.Invoke(null);
            onNamesPicked?.Invoke(null);
            yield break;
        }

        string finalPath = CopyOrConvertFile(path);
        if (finalPath != null)
        {
            onFilesPicked?.Invoke(new string[] { finalPath });
            onNamesPicked?.Invoke(new string[] { Path.GetFileName(finalPath) });
        }
        else
        {
            onFilesPicked?.Invoke(null);
            onNamesPicked?.Invoke(null);
        }
    }
}