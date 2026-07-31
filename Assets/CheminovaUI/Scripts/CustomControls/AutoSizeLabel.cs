using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Label de UI Toolkit que ajusta automáticamente su tamaño de fuente
/// para que el texto quepa dentro de sus dimensiones asignadas.
/// </summary>
[UxmlElement("AutoSizeLabel")]
public partial class AutoSizeLabel : Label
{
    /// <summary>
    /// Nombre del grupo al que pertenece este label para ajuste grupal de fuente.
    /// </summary>
    [UxmlAttribute]
    public string group { get; set; }

    /// <summary>
    /// Tamaño mínimo de fuente permitido.
    /// </summary>
    [UxmlAttribute]
    public float minFontSize { get; set; } = 4f;

    /// <summary>
    /// Tamaño máximo de fuente permitido.
    /// </summary>
    [UxmlAttribute]
    public float maxFontSize { get; set; } = 100f;

    /// <summary>
    /// Constructor por defecto. Registra los callbacks necesarios para el ajuste automático.
    /// </summary>
    public AutoSizeLabel()
    {
        RegisterCallback<AttachToPanelEvent>(evt =>
        {
            // Registrar callback para cuando el elemento sea visible y tenga dimensiones
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        });

        RegisterCallback<DetachFromPanelEvent>(evt =>
        {
            // Limpiar callback al desconectarse del panel
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        });
        
        // Recalcular al principio
        RegisterCallback<AttachToPanelEvent>(evt => ScheduleFontRecalculation());
    }

    /// <summary>
    /// Callback que se ejecuta cuando cambian las dimensiones del elemento.
    /// </summary>
    /// <param name="evt">Evento de cambio de geometría.</param>
    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        if (Mathf.Abs(evt.oldRect.width - evt.newRect.width) < 0.5f && 
            Mathf.Abs(evt.oldRect.height - evt.newRect.height) < 0.5f)
        {
            return; 
        }
        
        // Solo calcular si el elemento tiene dimensiones válidas y está visible
        if (contentRect.width > 0 && contentRect.height > 0 && style.display.value != DisplayStyle.None)
        {
            ScheduleFontRecalculation();
        }
    }
    
    /// <summary>
    /// Programa la recalculación de fuente en AutoSizeLabelGroups o individualmente.
    /// </summary>
    public void ScheduleFontRecalculation()
    {
        if (panel == null) return;
        
        if (string.IsNullOrEmpty(group))
        {
            // Sin grupo - calcular individualmente
            float size = ComputeBestFitFontSize();
            ApplyFontSize(size);
        }
        else
        {
            // Con grupo - registrar y programar recálculo grupal
            AutoSizeLabelGroups.Register(this, group);
            // Usar un delay más pequeño y asegurar que solo se ejecute una vez por grupo
            this.schedule.Execute(() => AutoSizeLabelGroups.Recalculate(group)).ExecuteLater(10);
        }
    }

    /// <summary>
    /// Calcula el mejor tamaño de fuente para que el texto quepa en el área disponible.
    /// </summary>
    /// <returns>Tamaño de fuente recomendado.</returns>
    public float ComputeBestFitFontSize()
    {
        if (string.IsNullOrEmpty(text))
        {
            return minFontSize;
        }

        // Usar resolvedStyle en lugar de contentRect para obtener las dimensiones reales
        var availableWidth = resolvedStyle.width;
        var availableHeight = resolvedStyle.height;

        if (availableWidth <= 0 || availableHeight <= 0)
        {
            return minFontSize;
        }
        // Método inspirado en LabelAutoFit: medir el texto en su tamaño natural y calcular el multiplicador
        var currentTextSize = MeasureTextSize(text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);
        
        if (currentTextSize.x <= 0 || currentTextSize.y <= 0)
        {
            return minFontSize;
        }

        // Calcular multiplicadores para ajustar tanto ancho como alto
        var widthMultiplier = availableWidth / currentTextSize.x;
        var heightMultiplier = availableHeight / currentTextSize.y;
        
        // Usar el multiplicador más restrictivo
        var multiplier = Mathf.Min(widthMultiplier, heightMultiplier);

        // Calcular el nuevo tamaño de fuente basado en el tamaño actual y el multiplicador
        var currentFontSize = resolvedStyle.fontSize;
        var newFontSize = currentFontSize * multiplier;

        // Aplicar límites y redondear
        newFontSize = Mathf.Clamp(newFontSize, minFontSize, maxFontSize);

        return newFontSize;
    }

    /// <summary>
    /// Aplica el tamaño de fuente calculado al label.
    /// </summary>
    /// <param name="size">Tamaño de fuente a aplicar.</param>
    public void ApplyFontSize(float size)
    {
        float finalSize = Mathf.Max(size, minFontSize);
        if (Mathf.Abs(resolvedStyle.fontSize - finalSize) > 0.5f)
        {
            style.fontSize = finalSize;
        }
    }
    
    /// <summary>
    /// Fuerza la recalculación del tamaño de fuente.
    /// Útil cuando el elemento cambia de visibilidad o contenido.
    /// </summary>
    public void ForceRecalculate()
    {
        if (panel != null && style.display.value != DisplayStyle.None)
        {
            ScheduleFontRecalculation();
        }
    }
}

/// <summary>
/// Utilidad estática para gestionar grupos de AutoSizeLabel y recalcular tamaños de fuente uniformes.
/// </summary>
public static class AutoSizeLabelGroups
{
    private static Dictionary<string, List<AutoSizeLabel>> _groups = new();
    private static readonly HashSet<string> _pendingGroups = new();
  
    /// <summary>
    /// Registra un label en un grupo para ajuste grupal de fuente.
    /// </summary>
    /// <param name="label">Label a registrar.</param>
    /// <param name="group">Nombre del grupo.</param>
    public static void Register(AutoSizeLabel label, string group)
    {
        if (!_groups.ContainsKey(group))
            _groups[group] = new List<AutoSizeLabel>();

        if (!_groups[group].Contains(label))
            _groups[group].Add(label);
    }
    
    /// <summary>
    /// Recalcula y aplica el tamaño de fuente óptimo para todos los labels de un grupo.
    /// </summary>
    /// <param name="group">Nombre del grupo a recalcular.</param>
    public static void Recalculate(string group)
    {
        if (!_groups.ContainsKey(group) || _pendingGroups.Contains(group)) return;

        _pendingGroups.Add(group);

        var labels = _groups[group];
        float minSize = float.MaxValue;
        bool foundValidLabel = false;

        // Limpiar labels que ya no están en el panel
        labels.RemoveAll(label => label.panel == null);

        // Calcular mejor font-size de cada label
        foreach (var label in labels)
        {
            if (label.panel == null || label.style.display.value == DisplayStyle.None)
            {
                continue; // ignorar los que no están listos
            }
            
            float size = label.ComputeBestFitFontSize();
            if (size < minSize)
            {
                minSize = size;
                foundValidLabel = true;
            }
        }

        // Solo aplicar si encontramos al menos un label válido
        if (foundValidLabel && minSize != float.MaxValue)
        {
            // Aplicar tamaño uniforme
            foreach (var label in labels)
            {
                if (label.panel != null && label.style.display.value != DisplayStyle.None)
                {
                    label.ApplyFontSize(minSize);
                }
            }
        }

        _pendingGroups.Remove(group);
    }
}
