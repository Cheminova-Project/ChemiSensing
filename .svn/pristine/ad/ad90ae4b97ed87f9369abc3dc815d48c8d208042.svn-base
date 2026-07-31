using GLTFast;
using GLTFast.Logging;
using UnityEngine;
using GLTFast.Materials;
using GLTFast.Schema;
using UnityEngine.Rendering;
using Material = UnityEngine.Material;

/// <summary>
/// Generador personalizado de materiales para modelos GLTF.
/// Permite crear y asignar materiales específicos durante la importación de modelos GLTF.
/// </summary>
public class CustomMaterialGenerator : IMaterialGenerator
{
    // Material base que se usará para todos los modelos
    public UnityEngine.Material baseMaterial;
    
    // Constructor que recibe el material base a utilizar
    public CustomMaterialGenerator(UnityEngine.Material customMaterial)
    {
        baseMaterial = customMaterial;
    }
    
    public UnityEngine.Material GenerateMaterial(MaterialBase gltfMaterial, IGltfReadable gltf, bool pointsSupport = false)
    {
        // Crea una nueva instancia del material base
        UnityEngine.Material material = new UnityEngine.Material(baseMaterial);
        // Opcional: Aplicar propiedades específicas del material GLTF si es necesario
        if (gltfMaterial is GLTFast.Schema.Material gltfMat)
        {
            // Puedes mapear propiedades específicas aquí si lo deseas
            if (gltfMat.pbrMetallicRoughness != null)
            {
                var pbr = gltfMat.pbrMetallicRoughness;
                
                if (pbr.baseColorFactor != null && pbr.baseColorFactor.Length >= 4)
                {
                    Color baseColor = new Color(
                        pbr.baseColorFactor[0],
                        pbr.baseColorFactor[1],
                        pbr.baseColorFactor[2],
                        pbr.baseColorFactor[3]
                    );
                    
                    material.SetColor("_BaseColor", baseColor);
                }
                
                // Ejemplo: Aplicar textura difusa si existe
                if (pbr.baseColorTexture != null)
                {
                    var texture = gltf.GetTexture(pbr.baseColorTexture.index);
                    if (texture != null)
                    {
                        material.SetTexture("_BaseMap", texture);
                    }
                }
            }
            // Normal Map
            if (gltfMat.normalTexture != null)
            {
                var normalTex = gltf.GetTexture(gltfMat.normalTexture.index);
                if (normalTex != null)
                {
                    material.SetTexture("_BumpMap", normalTex);
                    material.EnableKeyword("_NORMALMAP");
                }
            }
            // Ambient Occlusion (AO)
            if (gltfMat.occlusionTexture != null)
            {
                var aoTex = gltf.GetTexture(gltfMat.occlusionTexture.index);
                if (aoTex != null)
                {
                    material.SetTexture("_OcclusionMap", aoTex);
                    material.SetFloat("_OcclusionStrength", gltfMat.occlusionTexture.strength);
                    material.EnableKeyword("_OCCLUSIONMAP");
                }
            }
        }
        
        return material;
    }
    
    public void SetLogger(ICodeLogger logger)
    {
        // Implementar el logger si es necesario
        
    }

    public UnityEngine.Material GetDefaultMaterial(bool pointsSupport = false)
    {
        // Devuelve una instancia del material base como material por defecto
        return new UnityEngine.Material(baseMaterial);
    }
    
    
    public UnityEngine.Material GetPbrMetallicRoughnessMaterial(bool doubleSided = false)
    {
        var mat = new UnityEngine.Material(baseMaterial);
        if (doubleSided)
        {
            mat.SetInt("_Cull", (int)CullMode.Off);
        }
        return mat;
    }

    public UnityEngine.Material GetPbrSpecularGlossinessMaterial(bool doubleSided = false)
    {
        var mat = new UnityEngine.Material(baseMaterial);
        if (doubleSided)
        {
            mat.SetInt("_Cull", (int)CullMode.Off);
        }
        return mat;
    }

    public UnityEngine.Material GetUnlitMaterial(bool doubleSided = false)
    {
        var mat = new UnityEngine.Material(baseMaterial);
        if (doubleSided)
        {
            mat.SetInt("_Cull", (int)CullMode.Off);
        }
        return mat;
    }

    public UnityEngine.Material GetKhrMaterialsUnlitMaterial(bool doubleSided = false)
    {
        var mat = new UnityEngine.Material(baseMaterial);
        if (doubleSided)
        {
            mat.SetInt("_Cull", (int)CullMode.Off);
        }
        return mat;
    }

    public UnityEngine.Material GetClearCoatMaterial(bool doubleSided = false)
    {
        var mat = new UnityEngine.Material(baseMaterial);
        if (doubleSided)
        {
            mat.SetInt("_Cull", (int)CullMode.Off);
        }
        return mat;
    }

    public UnityEngine.Material GetTransmissionMaterial(bool doubleSided = false)
    {
        var mat = new UnityEngine.Material(baseMaterial);
        if (doubleSided)
        {
            mat.SetInt("_Cull", (int)CullMode.Off);
        }
        return mat;
    }

    public UnityEngine.Material GetSheenMaterial(bool doubleSided = false)
    {
        var mat = new UnityEngine.Material(baseMaterial);
        if (doubleSided)
        {
            mat.SetInt("_Cull", (int)CullMode.Off);
        }
        return mat;
    }
    /*
    /// <summary>
    /// Crea un material personalizado basado en parámetros de importación GLTF.
    /// </summary>
    /// <param name="parameters">Parámetros de importación.</param>
    /// <returns>Material generado.</returns>
    public Material GenerateMaterial(object parameters)
    {
        // Lógica para crear y configurar el material
        return new Material(Shader.Find("Standard"));
    }
    */
}
