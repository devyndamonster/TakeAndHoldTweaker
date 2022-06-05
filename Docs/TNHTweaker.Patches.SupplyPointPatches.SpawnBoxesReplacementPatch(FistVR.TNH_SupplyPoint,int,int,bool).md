#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches').[SupplyPointPatches](TNHTweaker.Patches.SupplyPointPatches.md 'TNHTweaker.Patches.SupplyPointPatches')

## SupplyPointPatches.SpawnBoxesReplacementPatch(TNH_SupplyPoint, int, int, bool) Method

Replaces entire call that spawns supply point boxes <br/><br/>  
Related Features: <br/>  
- [ Allow you to set min and max boxes spawned at supply points ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106')<br/>  
- [ Allow you to set min and max tokens per supply point ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/107 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/107')<br/>  
- [ Allow you to set min and max health drops per supply point ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/108 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/108')<br/>

```csharp
public static bool SpawnBoxesReplacementPatch(FistVR.TNH_SupplyPoint __instance, int min, int max, bool SpawnToken);
```
#### Parameters

<a name='TNHTweaker.Patches.SupplyPointPatches.SpawnBoxesReplacementPatch(FistVR.TNH_SupplyPoint,int,int,bool).__instance'></a>

`__instance` [FistVR.TNH_SupplyPoint](https://docs.microsoft.com/en-us/dotnet/api/FistVR.TNH_SupplyPoint 'FistVR.TNH_SupplyPoint')

<a name='TNHTweaker.Patches.SupplyPointPatches.SpawnBoxesReplacementPatch(FistVR.TNH_SupplyPoint,int,int,bool).min'></a>

`min` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

<a name='TNHTweaker.Patches.SupplyPointPatches.SpawnBoxesReplacementPatch(FistVR.TNH_SupplyPoint,int,int,bool).max'></a>

`max` [System.Int32](https://docs.microsoft.com/en-us/dotnet/api/System.Int32 'System.Int32')

<a name='TNHTweaker.Patches.SupplyPointPatches.SpawnBoxesReplacementPatch(FistVR.TNH_SupplyPoint,int,int,bool).SpawnToken'></a>

`SpawnToken` [System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')