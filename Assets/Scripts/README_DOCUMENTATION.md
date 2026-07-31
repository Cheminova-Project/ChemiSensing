# Documentación de Scripts - ChemiSensing

## Scripts Documentados ✅

Los siguientes archivos han sido completamente documentados con comentarios XML compatibles con **Doxygen**:

### Directorio Raíz (`Assets/Scripts/`)

1. **SessionPrefs.cs** - Singleton para almacenar datos de sesión en memoria
2. **GlobalVariables.cs** - Singleton para variables globales de la aplicación
3. **ToolComponent.cs** - Clase base para todos los componentes de herramientas
4. **CustomDownload.cs** - Implementación personalizada de descarga de archivos
5. **CustomTextureDownload.cs** - Implementación personalizada de descarga de texturas
6. **EmptyDownload.cs** - Implementación vacía para casos de prueba
7. **EmptyTextureDownload.cs** - Generador de texturas placeholder
8. **ToolDefinition.cs** - ScriptableObject que define herramientas del sistema
9. **ToolDatabase.cs** - Base de datos centralizada de herramientas
10. **BuildTargetProfile.cs** - Perfiles de compilación para diferentes plataformas
11. **UserMeeting.cs** - Gestor de interfaz de reunión de usuarios
12. **PlatformToolsManager.cs** - Manager de activación/desactivación según plataforma
13. **PlatformComponentGroup.cs** - Estructura para asociar plataformas con componentes

## Cómo Generar Documentación con Doxygen

### Instalación y Uso (PowerShell)

```powershell
# 1. Instalar Doxygen (solo primera vez)
.\Generate-Docs.ps1 -Install

# 2. Generar documentación HTML
.\Generate-Docs.ps1 -Build

# 3. Abrir en navegador
.\Generate-Docs.ps1 -Serve

# 4. O hacer todo en un paso
.\Generate-Docs.ps1 -All

# 5. Limpiar archivos generados
.\Generate-Docs.ps1 -Clean
```

### Instalación Manual de Doxygen

Si prefieres instalar manualmente:

```powershell
# Con Chocolatey (Windows)
choco install doxygen.install

# O descarga desde: https://www.doxygen.nl/download.html
```

### Generar Documentación Manualmente

```powershell
# Desde el directorio raíz del proyecto
doxygen Doxyfile
```

La documentación se generará en: `Documentation/html/index.html`

## Estructura de Comentarios XML Utilizados

```csharp
/// <summary>
/// Descripción breve de la clase/método
/// </summary>
/// <param name="parametro">Descripción del parámetro</param>
/// <returns>Descripción del valor de retorno</returns>
/// <remarks>Notas adicionales</remarks>
/// <example>Ejemplo de uso</example>
```

## Carpetas Pendientes de Documentación

Las siguientes carpetas contienen scripts adicionales que pueden necesitar documentación:

- `AR/` - Scripts de realidad aumentada
- `Build/` - Helpers y configuración de compilación
- `Character/` - Scripts de personajes
- `ConnectionStates/` - Estados de conexión
- `Core/` - Funcionalidad core
- `Debugging/` - Herramientas de depuración
- `GltfUtilities/` - Utilidades GLTF
- `Managers/` - Managers del sistema
- `Models/` - Modelos de datos
- `Networking/` - Sistema de red
- `Object Manipulation/` - Manipulación de objetos 3D
- `Rooms/` - Sistema de salas
- `UI/` - Componentes de interfaz
- `XR/` - Funcionalidad XR/VR

## Próximos Pasos

1. **Revisar scripts en subcarpetas** y documentar según prioridad
2. **Configurar DocFX** con el archivo `docfx.json` apropiado
3. **Integrar en CI/CD** para generar documentación automáticamente
4. **Publicar documentación** en GitHub Pages o servidor interno

## Estándares de Documentación

### Para Clases
- `<summary>`: Propósito de la clase
- Describir responsabilidades principales
- Mencionar dependencias clave

### Para Métodos
- `<summary>`: Qué hace el método
- `<param>`: Cada parámetro con descripción clara
- `<returns>`: Qué retorna el método
- `<remarks>`: Comportamientos especiales o advertencias

### Para Propiedades
- `<summary>`: Qué representa la propiedad
- Mencionar valores por defecto si aplica

## Herramientas Adicionales

- **Visual Studio IntelliSense**: Los comentarios XML mejoran el autocompletado
- **Rider**: Soporte completo para documentación XML
- **VSCode + C# Extension**: Visualización de documentación en hover

---

**Fecha de última actualización**: Diciembre 2, 2025  
**Responsable**: Sistema de Documentación Automática
