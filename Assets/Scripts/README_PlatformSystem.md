# Sistema de Gestión de Herramientas por Plataforma

## Descripción General

Este sistema permite gestionar automáticamente la activación/desactivación de componentes según el tipo de plataforma (Desktop, VR, Mobile), proporcionando una solución escalable para manejar herramientas específicas de cada plataforma.

## Componentes del Sistema

### 1. `PlatformComponentGroup`
**Estructura de datos** que asocia un `PlayerCharacterType` con una lista de componentes.

**Características:**
- Lista de componentes a gestionar
- Opción para destruir vs desactivar componentes
- Control de habilitación por grupo
- Validación automática de componentes

### 2. `PlatformToolsManager`
**Manager principal** que controla la activación/desactivación de componentes según la plataforma.

**Funcionalidades:**
- Detección automática de plataforma
- Configuración por grupos de plataforma
- Gestión de componentes en tiempo real
- API programática para control dinámico
- Validación y limpieza automática

### 3. `PlatformToolsManagerEditor`
**Custom Inspector** que proporciona una interfaz visual intuitiva para configurar el sistema.

**Características:**
- Interfaz organizada por secciones
- Botones de acción rápida
- Información de debug en tiempo real
- Validación visual de configuración

### 4. `PlatformSpecificTool`
**Ejemplo de herramienta** que demuestra cómo crear componentes específicos de plataforma.

## Cómo Usar el Sistema

### Paso 1: Configurar el Manager

1. **Añadir PlatformToolsManager** a un GameObject:
```csharp
// El GameObject que contiene las herramientas que quieres gestionar
GameObject toolContainer = new GameObject("Platform Tools");
PlatformToolsManager manager = toolContainer.AddComponent<PlatformToolsManager>();
```

2. **Configurar en el Inspector:**
   - **Auto Detect Platform**: Detecta automáticamente la plataforma actual
   - **Update On Platform Change**: Actualiza automáticamente al cambiar de plataforma
   - **Enable Debug Logs**: Muestra información de debug en consola

### Paso 2: Configurar Grupos de Plataforma

En el Inspector del `PlatformToolsManager`:

1. **Agregar Grupo de Plataforma** → Click "Add Platform Group"
2. **Seleccionar Platform Type** (Desktop, VR, Mobile)
3. **Añadir Componentes**:
   - Manualmente: Arrastra componentes a la lista
   - Quick Add: "Current Object" o "Children" para agregar automáticamente
4. **Configurar Opciones**:
   - **Destroy Instead of Disable**: Si quieres destruir componentes en vez de desactivarlos
   - **Is Enabled**: Para activar/desactivar todo el grupo

### Paso 3: Crear Herramientas Específicas

#### Opción A: Heredar de ToolComponent
```csharp
public class MiHerramientaVR : ToolComponent
{
    protected override void OnToolActivatedInternal()
    {
        // Solo se ejecuta cuando está activa
        Debug.Log("Herramienta VR activada!");
        ConfigurarControladores();
    }
    
    protected override void OnToolDeactivatedInternal()
    {
        // Limpieza al desactivar
        LimpiarControladores();
    }
}
```

#### Opción B: Usar PlatformSpecificTool
```csharp
public class MiHerramientaCustom : PlatformSpecificTool
{
    // Ya incluye lógica de validación por plataforma
    // Solo necesitas configurar qué plataformas soporta
}
```

### Paso 4: Configuración Programática

```csharp
// Obtener el manager
PlatformToolsManager manager = GetComponent<PlatformToolsManager>();

// Agregar componentes dinámicamente
List<Component> vrComponents = new List<Component> 
{ 
    vrController, 
    vrUI, 
    vrTracker 
};
manager.AddPlatformGroup(PlayerCharacterType.VR, vrComponents);

// Cambiar plataforma manualmente
manager.ChangePlatform(PlayerCharacterType.Mobile);

// Refrescar configuración
manager.RefreshConfiguration();
```

## Ejemplos de Uso

### Ejemplo 1: Herramientas de Medición por Plataforma

```
MeasurementTools (GameObject)
├── PlatformToolsManager (Component)
├── Desktop Group:
│   ├── MouseMeasureTool
│   ├── KeyboardShortcuts
│   └── DesktopUI
├── VR Group:
│   ├── ControllerMeasureTool
│   ├── HandTracking
│   └── SpatialUI
└── Mobile Group:
    ├── TouchMeasureTool
    ├── GyroscopeInput
    └── MobileUI
```

### Ejemplo 2: Sistema de Navegación

```csharp
// Configurar navegación específica por plataforma
public class NavigationManager : MonoBehaviour
{
    void Start()
    {
        var platformManager = GetComponent<PlatformToolsManager>();
        
        // Desktop: WASD + Mouse
        var desktopComponents = new List<Component> 
        { 
            GetComponent<WASDMovement>(), 
            GetComponent<MouseLook>() 
        };
        
        // VR: Teleport + Head tracking
        var vrComponents = new List<Component> 
        { 
            GetComponent<TeleportMovement>(), 
            GetComponent<VRTracking>() 
        };
        
        // Mobile: Touch controls
        var mobileComponents = new List<Component> 
        { 
            GetComponent<TouchMovement>(), 
            GetComponent<TouchLook>() 
        };
        
        platformManager.AddPlatformGroup(PlayerCharacterType.Desktop, desktopComponents);
        platformManager.AddPlatformGroup(PlayerCharacterType.VR, vrComponents);
        platformManager.AddPlatformGroup(PlayerCharacterType.Mobile, mobileComponents);
    }
}
```

## Características Avanzadas

### Destruir vs Desactivar
- **Desactivar**: Mejor rendimiento, permite reactivar componentes
- **Destruir**: Libera más memoria, irreversible

### Detección Automática vs Manual
- **Automática**: Usa `PlatformController.Instance.GetPlayerCharacterType()`
- **Manual**: Útil para testing o configuraciones específicas

### Debug y Monitoreo
- Logs detallados de activación/desactivación
- Info de runtime en el Inspector
- Botones de limpieza y actualización

## Integración con ToolMenuController

```csharp
// En tu ToolDefinition, el ScriptsContainer puede tener un PlatformToolsManager
// que automáticamente active solo los componentes correctos para la plataforma actual

ScriptsContainer (Prefab)
├── PlatformToolsManager
├── DesktopTools (activado solo en Desktop)
├── VRTools (activado solo en VR)
└── MobileTools (activado solo en Mobile)
```

## Ventajas del Sistema

1. **Automatización**: Gestión automática según plataforma
2. **Flexibilidad**: Control granular por componente
3. **Performance**: Solo carga componentes necesarios
4. **Mantenibilidad**: Configuración visual clara
5. **Escalabilidad**: Fácil añadir nuevas plataformas
6. **Debug**: Información clara del estado del sistema

Este sistema te permite crear aplicaciones verdaderamente multiplataforma donde cada plataforma tiene sus herramientas optimizadas, sin código duplicado ni configuración manual compleja.