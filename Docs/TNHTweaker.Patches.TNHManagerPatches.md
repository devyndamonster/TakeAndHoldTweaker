#### [TakeAndHoldTweaker](index.md 'index')
### [TNHTweaker.Patches](TNHTweaker.Patches.md 'TNHTweaker.Patches')

## TNHManagerPatches Class

```csharp
public static class TNHManagerPatches
```

Inheritance [System.Object](https://docs.microsoft.com/en-us/dotnet/api/System.Object 'System.Object') &#129106; TNHManagerPatches

| Methods | |
| :--- | :--- |
| [GenerateValidPatrolPatch(ILContext, MethodBase)](TNHTweaker.Patches.TNHManagerPatches.GenerateValidPatrolPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).md 'TNHTweaker.Patches.TNHManagerPatches.GenerateValidPatrolPatch(MonoMod.Cil.ILContext, System.Reflection.MethodBase)') | Patches GenerateValidPatrol method to call our own GeneratePatrol method <br/><br/><br/>Related Features: <br/><br/>- [ Allow multiple types of sosigs to spawn in patrols ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/111 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/111')<br/> |
| [InitTablesPatch(TNH_Manager)](TNHTweaker.Patches.TNHManagerPatches.InitTablesPatch(FistVR.TNH_Manager).md 'TNHTweaker.Patches.TNHManagerPatches.InitTablesPatch(FistVR.TNH_Manager)') | Before initializing the base character classes tables, initialize the extended character classes tables <br/><br/><br/>Related Features: <br/><br/>- [ Have starting equipment use our own EquipmentGroup loot system ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/101')<br/><br/>- [ Have object constructor use our own EquipmentGroup loot system ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/105 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/105')<br/> |
| [SetPhaseTakeBoxCountPatch(ILContext, MethodBase)](TNHTweaker.Patches.TNHManagerPatches.SetPhaseTakeBoxCountPatch(MonoMod.Cil.ILContext,System.Reflection.MethodBase).md 'TNHTweaker.Patches.TNHManagerPatches.SetPhaseTakeBoxCountPatch(MonoMod.Cil.ILContext, System.Reflection.MethodBase)') | Patches SetPhase_Take method to use the SupplyChallenges min and max boxes values <br/><br/><br/>Related Features: <br/><br/>- [ Allow you to set min and max boxes spawned at supply points ](https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106 'https://github.com/devyndamonster/TakeAndHoldTweaker/issues/106')<br/> |
