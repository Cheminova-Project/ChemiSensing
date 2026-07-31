using System.IO;
using Paroxe.PdfRenderer;
using UIControllers;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UIElements;
using UnityEngine.Video;
using Button = UnityEngine.UIElements.Button;
using Image = UnityEngine.UIElements.Image;

public class MultimediaManager : MonoBehaviour
{
    /// <summary>
    /// Referencia al contenido multimedia actual.
    /// </summary>
    public object currentMedia;

    // UI Elements
    private string folderButtonName = "folder-button";
    private string itemsContainerName = "doc-container";
    private string docLabelName = "doc-image";
    private string docImageName = "doc-image";
    private string folderTitleName = "folder-title";
    
    // Map things
    public VisualTreeAsset docElement;
    public GameObject canvas;
    public UnityEngine.UI.Button closeButton;
    public PDFViewer pdfViewer;
    public RawImage annexDataImage;
    public AudioSource audioSource;
    public DynamicVideoDisplay dynamicVideoDisplay;
    public GameObject videoController;
    public Sprite pdfSprite;
    public Sprite imageSprite;
    public Sprite audioSprite;
    public Sprite videoSprite;


    private Button returnButton;
    private Button folderButton;

    private UIDocument uIDocument;

    private int annexDataID;
    
    public UnityEvent OnReturnToAnnexData = new UnityEvent();

    private void OnDocsListReceived(AnnexDataData annexDataData, bool success)
    {
        if (success)
        {
            InsertItems(annexDataData);
        }
        else
        {
            Debug.LogWarning("Error when obtaining docs list.");
        }
    }


    /// <summary>
    /// Inicializa el gestor de multimedia con el documento UI y el ID de los datos del anexo.
    /// </summary>
    /// <param name="document">El documento UI asociado.</param>
    /// <param name="annexDataID">El ID de los datos del anexo.</param>
    public void Initialize(UIDocument document, int annexDataID)
    {
        if (uIDocument == null)
        {
            uIDocument = document;
            InitializeUI();
        }

        this.annexDataID = annexDataID;
        CloseMultimedia();
        LoadDocsFromAnnexData();    
    }

    /// <summary>
    /// Desinicializa el gestor de multimedia, cerrando cualquier contenido abierto.
    /// </summary>
    public void Deinitialize()
    {
        CloseMultimedia();
    }

    void OnDestroy()
    {
        if (folderButton != null)
        {
            folderButton.clicked -= ReturnToAnnexDataList;
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseMultimedia);
        }
    }


    private void InitializeUI()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        folderButton = uIDocument.rootVisualElement.Q<Button>(folderButtonName);
        if (folderButton != null)
        {
            folderButton.clicked += ReturnToAnnexDataList;
        }
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseMultimedia);
        }
    }
    
    private void InsertItems(AnnexDataData annexDataData)
    { 
        var itemsContainer = uIDocument.rootVisualElement.Q<ScrollView>(itemsContainerName);
        itemsContainer.Clear();
        var folderNameLabel = uIDocument.rootVisualElement.Q<Label>(folderTitleName);
        if (folderNameLabel != null)
            folderNameLabel.text = annexDataData.title;
        
        foreach (var doc in annexDataData.docs)
        {
            if (doc.content_type.Contains("pdf") || doc.content_type.Contains("image") || doc.content_type.Contains("audio") || doc.content_type.Contains("video"))
            {
                var element = docElement.CloneTree();
                itemsContainer.Add(element);
                // Asignar el nombre y la descripción al elemento
                var nameLabel = element.Q<Label>(docLabelName);
                if (nameLabel != null)
                    nameLabel.text = Path.GetFileNameWithoutExtension(doc.name);
                var image = element.Q<VisualElement>(docImageName);
                if (image != null)
                {
                    var texture = new Texture2D(1, 1);
                    if (doc.content_type.Contains("pdf"))
                        texture = pdfSprite.texture;
                    else if (doc.content_type.Contains("image"))
                        texture = imageSprite.texture;
                    else if (doc.content_type.Contains("audio"))
                        texture = audioSprite.texture;
                    else if (doc.content_type.Contains("video"))
                        texture = videoSprite.texture;
                    
                    image.style.backgroundImage = new StyleBackground(texture);
                }
                else
                {
                    Debug.LogWarning("ERROR: " + docImageName + " not found in UI Document.");
                }
                
                element.RegisterCallback<ClickEvent>(ev =>
                {
                    if (ev.button == 0)
                    {
                        //Debo desactivar todos los elementos multimedia antes de activar uno nuevo
                        CloseMultimedia();
                        if (doc.content_type.Contains("pdf"))
                        {
                            canvas.SetActive(true);
                            pdfViewer.gameObject.SetActive(true);
                            DownloadAndFillAnnexDataPdf(pdfViewer, doc.id);
                        }
                        else if (doc.content_type.Contains("image"))
                        {
                            canvas.SetActive(true);
                            annexDataImage.gameObject.SetActive(true);
                            DownloadAndFillAnnexDataImage(annexDataImage, doc.id);
                        }
                        else if (doc.content_type.Contains("audio"))
                        {
                            canvas.SetActive(true);
                            audioSource.gameObject.SetActive(true);
                            DownloadAndPlayAnnexDataAudio(audioSource, doc.name, doc.id);
                        }
                        else if (doc.content_type.Contains("video"))
                        {
                            canvas.SetActive(true);
                            dynamicVideoDisplay.gameObject.SetActive(true);
                            videoController.gameObject.SetActive(true);
                            DownloadAndFillAnnexDataVideo(doc.id);
                        }
                        closeButton.gameObject.SetActive(true);
                    }
                });
            }
        }
    }
    
    private void DownloadAndFillAnnexDataImage(RawImage image, int? annexDataImage)
    {
        StartCoroutine(AnnexDataDB.GetAnnexDataImage(annexDataImage, FillImage(image)));
    }

    private UnityAction<Texture2D, bool> FillImage(RawImage imageToFill)
    {
        return (texture, success) =>
        {
            if (success && texture != null)
            {
                AdjustRawImageAspect(imageToFill, (float)texture.width/texture.height);
                imageToFill.texture = texture;
            }
            else
            {
                Debug.LogWarning("Couldn't load doc image.");
            }
        };
    }
    
    private void DownloadAndPlayAnnexDataAudio(AudioSource source, string audioName, int? annexDataAudio)
    {
        StartCoroutine(AnnexDataDB.GetAnnexDataAudio(annexDataAudio, audioName, FillAudio(source)));
    }

    private UnityAction<AudioClip, bool> FillAudio(AudioSource audioSource)
    {
        return (audioClip, success) =>
        {
            if (success && audioSource != null)
            {
                audioSource.clip = audioClip;
                audioSource.Play();
            }
            else
            {
                Debug.LogWarning("Couldn't load doc audio.");
            }
        };
    }
    
    private void DownloadAndFillAnnexDataPdf(PDFViewer pdfViewer, int? annexDataPdf)
    {
        StartCoroutine(AnnexDataDB.GetAnnexDataPdf(annexDataPdf, FillPdf(pdfViewer)));
    }

    private UnityAction<PDFDocument, bool> FillPdf(PDFViewer pdfViewer)
    {
        return (pdfDocument, success) =>
        {
            if (success && pdfViewer != null)
            {
                pdfViewer.LoadDocument(pdfDocument);
            }
            else
            {
                Debug.LogWarning("Couldn't load doc PDF.");
            }
        };
    }
    
    private void DownloadAndFillAnnexDataVideo(int? annexDataVideo)
    {
        StartCoroutine(AnnexDataDB.GetAnnexDataVideo(annexDataVideo, dynamicVideoDisplay.GetVideoPlayer(), FillVideo()));
    }

    private UnityAction<VideoPlayer, bool> FillVideo()
    {
        return (videoPlayerObject, success) =>
        {
            if (success && videoPlayerObject != null)
            {
                dynamicVideoDisplay.PrepareVideoPlayer();
                //dynamicVideoDisplay.LoadVideoURL(videoPlayerObject.url);
                //dynamicVideoDisplay.LoadVideoClip(videoPlayerObject.clip);
            }
            else
            {
                Debug.LogWarning("Couldn't load doc video.");
            }
        };
    }


    private void LoadDocsFromAnnexData()
    {
        StartCoroutine(AnnexDataDB.GetAnnexDataByID(OnDocsListReceived, annexDataID));
    }
    
    
    
    /// <summary>
    /// Cierra cualquier contenido multimedia abierto y restablece el estado del reproductor multimedia.
    /// </summary>
    public void CloseMultimedia()
    {
        if (audioSource != null && audioSource.isPlaying)
            audioSource.Stop();
        
        dynamicVideoDisplay.CloseVideoPlayer();
        audioSource.gameObject.SetActive(false);
        ClearRawImage(annexDataImage, true);
        annexDataImage.gameObject.SetActive(false);
        pdfViewer.gameObject.SetActive(false);
        dynamicVideoDisplay.ResetVideoPlayer();
        dynamicVideoDisplay.gameObject.SetActive(false);
        videoController.gameObject.SetActive(false);
        canvas.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }
    
    public void ClearRawImage(RawImage image, bool destroyTexture = false)
    {
        if (image == null)
            return;

        if (destroyTexture && image.texture != null)
        {
            if (image.texture is RenderTexture rt)
            {
                rt.Release();
                DestroyImmediate(rt);
            }
            else
            {
                DestroyImmediate(image.texture);
            }
        }

        image.texture = null;
    }

    void AdjustRawImageAspect(RawImage image, float aspectRatio)
    {
        image.gameObject.transform.GetComponent<AspectRatioFitter>().aspectRatio = aspectRatio;
    }
    
    private void ReturnToAnnexDataList()
    {
        OnReturnToAnnexData.Invoke();
    }
}