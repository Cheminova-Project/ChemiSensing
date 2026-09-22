using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Globalization;

public class GeneralInformationUI : ToolComponent
{
    private UIDocument uiDocument;
    private VisualElement rootElement;
    
    // UI Elements - Cabecera
    private string itemIconName = "item-icon";
    private string itemNameName = "item-name-general";
    private string creatorLabelName = "creator-label";
    private string guidLabelName = "guid-label";
    private string instancesCountName = "instances-count";
    private string annotationsCountName = "annotations-count";
    private string documentsCountName = "documents-count";
    private string e3dCountName = "e3ditem-counts";

    // UI Elements - CH Element
    private string chElementDetailsContainerName = "ch-element-details-container";
    private string chDescriptionTextName = "ch-description-text";
    
    // Tabla Izquierda (CH)
    private string creatorValueName = "creator-value";
    private string scaleValueName = "scale-value";
    private string heritageCategoryValueName = "heritage-category-value";
    private string startYearValueName = "start-year-value";
    private string endYearValueName = "end-year-value";
    private string approxDatesValueName = "approx-dates-value";
    private string materialsValueName = "materials-value";

    // Tabla Derecha (CH)
    private string countryValueName = "country-value";
    private string provinceValueName = "province-value";
    private string municipalityValueName = "municipality-value";
    private string latitudeValueName = "latitude-value";
    private string longitudeValueName = "longitude-value";
    
    protected override void OnToolActivatedInternal()
    {
        base.OnToolActivatedInternal();
        uiDocument = GetComponentInParent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogWarning("UIDocument component not found on this GameObject.");
            return;
        }

        rootElement = uiDocument.rootVisualElement;
        
        FillGeneralInformationData(GlobalVariables.Instance.GetSelectedCHElementData());
    }

    protected override void OnToolDeactivatedInternal()
    {
        base.OnToolDeactivatedInternal();
    }

    public void FillGeneralInformationData(CHElementData chElementData)
    {
        if (chElementData != null)
        {
            var itemNameElement = rootElement.Q<Label>(itemNameName);
            if (itemNameElement != null)
            {
                if (!string.IsNullOrEmpty(chElementData.name))
                    itemNameElement.text = chElementData.name;
                else
                    itemNameElement.text = "Unknown Name";
            }

            var creatorLabel = rootElement.Q<Label>(creatorLabelName);
            if (creatorLabel != null)
            {
                string creator = chElementData.username;
                creatorLabel.text = "created by: <b>" + (string.IsNullOrEmpty(creator) ? "Unknown" : creator) + "</b>";
            }

            var guidLabel = rootElement.Q<Label>(guidLabelName);
            if (guidLabel != null)
            {
                string currentGuid = chElementData.GUID;
                guidLabel.text = "GUID: <b>" + (string.IsNullOrEmpty(currentGuid) ? "N/A" : currentGuid) + "</b>";
            }
            
            var e3dCount = rootElement.Q<VisualElement>(e3dCountName);
            if (e3dCount != null) 
                e3dCount.style.display = DisplayStyle.Flex;

            DownloadHeaderIcon(chElementData);
            StartCoroutine(LoadCounters(chElementData));

            var chContainer = rootElement.Q<VisualElement>(chElementDetailsContainerName);
            
            if (chContainer != null)
            {
                chContainer.style.display = DisplayStyle.Flex;

                var desc = rootElement.Q<Label>(chDescriptionTextName);
                if (desc != null)
                    desc.text = !string.IsNullOrEmpty(chElementData.description) ? chElementData.description : "No description available.";

                var creatorVal = rootElement.Q<Label>(creatorValueName);
                if (creatorVal != null) creatorVal.text = !string.IsNullOrEmpty(chElementData.author) ? chElementData.author : "Unknown";

                var scaleVal = rootElement.Q<Label>(scaleValueName);
                if (scaleVal != null) scaleVal.text = !string.IsNullOrEmpty(chElementData.scale) ? chElementData.scale : "Unknown";

                var catVal = rootElement.Q<Label>(heritageCategoryValueName);
                if (catVal != null) catVal.text = !string.IsNullOrEmpty(chElementData.heritage_category) ? chElementData.heritage_category : "Unknown";

                var startVal = rootElement.Q<Label>(startYearValueName);
                if (startVal != null) startVal.text = chElementData.start_year.ToString();

                var endVal = rootElement.Q<Label>(endYearValueName);
                if (endVal != null) endVal.text = chElementData.end_year.ToString();

                var approxVal = rootElement.Q<Label>(approxDatesValueName);
                if (approxVal != null) approxVal.text = chElementData.approximate_date ? "yes" : "no";

                var matVal = rootElement.Q<Label>(materialsValueName);
                if (matVal != null)
                {
                    if (chElementData.materials != null && chElementData.materials.Count > 0)
                    {
                        string mats = string.Join("\n• ", chElementData.materials.Select(m => m.name));
                        matVal.text = "• " + mats;
                    }
                    else
                    {
                        matVal.text = "None specified";
                    }
                }

                var countryVal = rootElement.Q<Label>(countryValueName);
                if (countryVal != null) countryVal.text = chElementData.country != null && !string.IsNullOrEmpty(chElementData.country.name) ? chElementData.country.name : "Unknown";

                var provVal = rootElement.Q<Label>(provinceValueName);
                if (provVal != null) provVal.text = chElementData.province != null && !string.IsNullOrEmpty(chElementData.province.name) ? chElementData.province.name : "Unknown";

                var muniVal = rootElement.Q<Label>(municipalityValueName);
                if (muniVal != null) muniVal.text = chElementData.municipality != null && !string.IsNullOrEmpty(chElementData.municipality.name) ? chElementData.municipality.name : "Unknown";

                var latVal = rootElement.Q<Label>(latitudeValueName);
                if (latVal != null) latVal.text = chElementData.latitude.HasValue ? chElementData.latitude.Value.ToString() : "N/A";

                var lonVal = rootElement.Q<Label>(longitudeValueName);
                if (lonVal != null) lonVal.text = chElementData.longitude.HasValue ? chElementData.longitude.Value.ToString() : "N/A";
            }
        }
        else
        {
            Debug.LogWarning("CHElementData is null. General Information cannot be loaded.");
        }
    }

    private void DownloadHeaderIcon(CHElementData chElementData)
    {
        var iconElement = rootElement.Q<VisualElement>(itemIconName);
        
        if (iconElement == null)
            return;
        
        StartCoroutine(CHElementDB.GetCHElementIcon(chElementData?.icon, (tex, ok) => {
            if (ok && tex != null)
                iconElement.style.backgroundImage = tex;
        }));
    }

    private IEnumerator LoadCounters(CHElementData chElementData)
    {
        var lblAnnotations = rootElement.Q<Label>(annotationsCountName);
        var lblThirdCounter = rootElement.Q<Label>(documentsCountName);

        int? annotations = 0, thirdCounterValue = 0;
        
        yield return CHElementDB.GetAnnotationListFromCHElement((r, ok) => { if (ok && r != null) annotations = r.total; }, chElementData.id, perPage: 1);
        yield return CHElementDB.GetAnnexDataListFromCHElement((r, ok) => { if (ok && r != null) thirdCounterValue = r.total; }, chElementData.id, perPage: 1);
        
        if (lblAnnotations != null) 
            lblAnnotations.text = annotations + " annotations";
        
        if (lblThirdCounter != null) 
            lblThirdCounter.text = thirdCounterValue + " documents";
    }
}