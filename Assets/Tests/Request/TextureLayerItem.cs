using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using UnityEngine.Networking;
using System;
using Unity.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class TextureLayerItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Colors")]
    public Color selectedColor = new Color(0.949f, 0.741f, 0.4f, 1f); // Color to apply when the item is selected
    public Color originalColor = new Color(1f, 1f, 1f, 1f); // Original color of the item
    
    [Header("Texture Layer Item")]
    public TextMeshProUGUI textureLayerName;

    public Image previewImage;

    public Button downloadButton;
    public Sprite visbilityIcon, nonVisbilityIcon, downloadIcon, removeIcon, syncIcon, errorIcon;
    

    [NonSerialized] public TextureManager textureManager;
    [NonSerialized] public static EventHandler<TextureLayerItem> OnTextureLayerItemClicked;
    
    Image imageComponent;
    private int remoteTextureID = -1;
    

    void OnEnable()
    {
        downloadButton.onClick.AddListener(OnDownloadClicked);
        textureManager.onTextureStartedDownload += SetSyncIcon;
    }

    void OnDisable()
    {
        downloadButton.onClick.RemoveListener(OnDownloadClicked);
        textureManager.onTextureStartedDownload -= SetSyncIcon;
    }

    public void OnDownloadClicked()
    {
        if (remoteTextureID == -1)
        {
            Debug.LogError("Remote texture ID is not set.");
            return;
        }
        
        //If the texture is already downloaded, remove it and reset the button icon to downloadIcon
        if (textureManager.IsTextureDownloaded(remoteTextureID))
        {
            textureManager.RemoveTexture(remoteTextureID);
            downloadButton.image.sprite = downloadIcon;
        }
        //If the texture is not downloaded, start the download coroutine
        else
        {
            textureManager.StartDownload(remoteTextureID, OnTextureDownloaded);
        }
    }

    private void OnTextureDownloaded()
    {
        //Debug.Log("Texture downloaded successfully.");
    }
    
    
    public int GetRemoteTextureID()
    {
        return remoteTextureID;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnTextureLayerItemClicked?.Invoke(this, this);
    }

    public void InitializeElement(TextureManager texManager, int id)
    {
        this.remoteTextureID = id;
        this.textureManager = texManager;
        this.textureManager.onRemoteTexturesValueChanged += OnRemoteTexturesValueChanged;
    }

    private void OnRemoteTexturesValueChanged(RemoteTexture remoteTexture)
    {
        if (remoteTexture.id != remoteTextureID)
            return;
        else
        {
            imageComponent.color = remoteTexture.isVisible ? selectedColor : originalColor;
        }
        
    }

    public void InitializeUI(RemoteTexture remoteTexture)
    {
        textureLayerName.text = remoteTexture.name.ToString();
        remoteTextureID = remoteTexture.id;
        imageComponent = GetComponent<Image>();
    }
    
    public void SetSyncIcon(int id)
    {
        if(id != remoteTextureID) return;
        if (downloadButton != null)
        {
            downloadButton.image.sprite = syncIcon;
        }
    }

    public void SetRemoveIcon()
    {
        if (downloadButton != null)
        {
            downloadButton.image.sprite = removeIcon;
        }
    }
}
