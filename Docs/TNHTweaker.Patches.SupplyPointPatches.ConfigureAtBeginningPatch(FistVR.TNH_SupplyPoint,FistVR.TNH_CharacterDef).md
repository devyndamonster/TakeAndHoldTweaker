### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches').[SupplyPointPatches](TNHTweaker.Patches.SupplyPointPatches.md 'TNHTweaker.Patches.SupplyPointPatches')

## SupplyPointPatches.ConfigureAtBeginningPatch(TNH_SupplyPoint, TNH_CharacterDef) Method

Replaces entire call that configures the player beginning equipment when the game starts <br/><br/>  
Related Features: <br/>  
- [ Have starting equipment use our own EquipmentGroup loot system ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101')<br/>

```csharp
public static bool ConfigureAtBeginningPatch(FistVR.TNH_SupplyPoint __instance, FistVR.TNH_CharacterDef c);
```
#### Parameters

<a name='TNHTweaker.Patches.SupplyPointPatches.ConfigureAtBeginningPatch(FistVR.TNH_SupplyPoint,FistVR.TNH_CharacterDef).__instance'></a>

`__instance` [FistVR.TNH_SupplyPoint](https://docs.microsoft.com/en-us/dotnet/api/FistVR.TNH_SupplyPoint 'FistVR.TNH_SupplyPoint')

<a name='TNHTweaker.Patches.SupplyPointPatches.ConfigureAtBeginningPatch(FistVR.TNH_SupplyPoint,FistVR.TNH_CharacterDef).c'></a>

`c` [FistVR.TNH_CharacterDef](https://docs.microsoft.com/en-us/dotnet/api/FistVR.TNH_CharacterDef 'FistVR.TNH_CharacterDef')

#### Returns
[System.Boolean](https://docs.microsoft.com/en-us/dotnet/api/System.Boolean 'System.Boolean')