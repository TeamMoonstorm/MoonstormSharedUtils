using HG;
using R2API.AddressReferencedAssets;
using RoR2;
using RoR2.Editor;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MSU.Editor.EditorWindows
{
    public class IDADTemplaterPopupWindow : PopupWindowContent
    {
        [Flags]
        private enum BodyKindType
        {
            //Include anything that has a Survivordef, also include Scavenger and Mithrix; useful for item type displays
            SurvivorLike = 1,
            //Include any CharacterMaster's body prefab thats NOT a survivor; useful for elite displays
            Monsters = 2,
            //Include any DroneDef related character.
            Drone = 4
        }

        private enum KeyAssetType
        {
            Item,
            Equipment, 
            EliteEquipment
        }

        [MenuItem("CONTEXT/ItemDisplayAddressedDictionary/Add vanilla IDRS in bulk")]
        private static void Open(MenuCommand menuCommand)
        {
            ItemDisplayAddressedDictionary idad = (ItemDisplayAddressedDictionary)menuCommand.context;

            Event current = Event.current;
            Vector2 mousePos = Vector2.zero;
            if (current != null)
            {
                mousePos = current.mousePosition;
            }
            else
            {
                mousePos = Input.mousePosition;
            }

            PopupWindow.Show(new Rect(mousePos, Vector2.zero), new IDADTemplaterPopupWindow(idad));
        }

        private SerializedObject _targetIDADSerializedObject;
        private ItemDisplayAddressedDictionary _targetIDAD;

        private BodyKindType _bodyKindType = BodyKindType.SurvivorLike;
        private LimbFlags _limbFlags = LimbFlags.None;
        private GameObject _defaultDisplayPrefab = null;
        private GameObject _spikeDisplayPrefab = null;

        private bool _isFirstKeyAssetCheck;
        private KeyAssetType _keyAssetType;

        IEnumerator runningCoroutine;

        public override void OnGUI(Rect rect)
        {
            bool CanRunCoroutine()
            {
                bool noCoroutineRunning = runningCoroutine == null;
                bool defaultDisplayPrefabValid = false;
                bool spikeDisplayPrefabValid = true;

                if(!_defaultDisplayPrefab)
                {
                    EditorGUILayout.LabelField($"There is no default display prefab, cannot add in bulk.", EditorStyles.boldLabel);
                }
                else if(!_defaultDisplayPrefab.TryGetComponent<ItemDisplay>(out _))
                {
                    EditorGUILayout.LabelField($"The default display prefab is not valid, it doesnt have an ItemDisplay component", EditorStyles.boldLabel);
                }
                else
                {
                    defaultDisplayPrefabValid = true;
                }

                if(_spikeDisplayPrefab && !_spikeDisplayPrefab.TryGetComponent<ItemDisplay>(out _))
                {
                    spikeDisplayPrefabValid = false;
                    EditorGUILayout.LabelField($"The spike display prefab is not valid, it doesnt have an ItemDisplay component", EditorStyles.boldLabel);
                }

                return noCoroutineRunning && defaultDisplayPrefabValid && spikeDisplayPrefabValid;
            }

            if (HasValidKeyAsset() == false)
            {
                return;
            }

            _bodyKindType = (BodyKindType)EditorGUILayout.EnumFlagsField("Body Kind Type", _bodyKindType);
            _limbFlags = (LimbFlags)EditorGUILayout.EnumFlagsField("Limb Flags", _limbFlags);
            _defaultDisplayPrefab = (GameObject)EditorGUILayout.ObjectField("Default Display Prefab", _defaultDisplayPrefab, typeof(GameObject), false);
            if(_keyAssetType == KeyAssetType.Item)
            {
                if(((ItemDef)_targetIDAD.keyAsset).tier == ItemTier.AssignedAtRuntime)
                {
                    _spikeDisplayPrefab = (GameObject)EditorGUILayout.ObjectField("Mithrix Spike Display Prefab", _spikeDisplayPrefab, typeof(GameObject), false);
                }
            }
            EditorGUILayout.LabelField(GetInfoString(_targetIDAD.keyAsset), EditorStyles.wordWrappedLabel);

            using(new EditorGUI.DisabledScope(CanRunCoroutine() == false))
            {
                if (GUILayout.Button("Add in Bulk"))
                {
                    runningCoroutine = AddEntriesInBulk();
                    editorWindow.StartCoroutine(runningCoroutine);
                }
            }
        }

        private bool HasValidKeyAsset()
        {
            var keyAsset = _targetIDAD.keyAsset;
            if (!keyAsset)
            {
                EditorGUILayout.LabelField($"This ItemDisplayAddressedDictionary has no Key Asset, please assign a Key Asset.");
                return false;
            }
            bool isItemLike = keyAsset is ItemDef;
            bool isEquipmentLike = keyAsset is EquipmentDef;

            if (!(isItemLike || isEquipmentLike))
            {
                EditorGUILayout.LabelField($"Supplied KeyAsset does not inherit from ItemDef or EquipmentDef.");
                return false;
            }

            if(_isFirstKeyAssetCheck == false)
            {
                _isFirstKeyAssetCheck = true;

                if(isItemLike)
                {
                    _bodyKindType = BodyKindType.SurvivorLike;
                    _keyAssetType = KeyAssetType.Item;
                }
                else if(isEquipmentLike)
                {
                    EquipmentDef equipmentDef = keyAsset as EquipmentDef;
                    if(equipmentDef.passiveBuffDef && equipmentDef.passiveBuffDef.eliteDef)
                    {
                        _bodyKindType = BodyKindType.SurvivorLike | BodyKindType.Monsters;
                        _keyAssetType = KeyAssetType.EliteEquipment;
                    }
                    else
                    {
                        _bodyKindType = BodyKindType.SurvivorLike;
                        _keyAssetType = KeyAssetType.Equipment;
                    }
                }
            }

            return true;
        }

        private string GetInfoString(ScriptableObject keyAsset)
        {
            //0 index is what always happens, 1 index depends on the key asset type.
            var stringBuilder = HG.StringBuilderPool.RentStringBuilder();

            stringBuilder.AppendLine("Adding IDRS that come from:");

            foreach(BodyKindType flag in Enum.GetValues(typeof(BodyKindType)))
            {
                if(flag == BodyKindType.SurvivorLike && _bodyKindType.HasFlag(BodyKindType.SurvivorLike))
                {
                    stringBuilder.Append("* SurvivorDefs (Engi Turrets Included). ");
                    if(_keyAssetType == KeyAssetType.Item || _keyAssetType == KeyAssetType.EliteEquipment)
                    {
                        stringBuilder.AppendLine("(Includes Scavenger and Mithrix)");
                    }
                    else if(_keyAssetType == KeyAssetType.Equipment)
                    {
                        stringBuilder.AppendLine("(Includes Scavenger)");
                    }
                }
                if(flag == BodyKindType.Monsters && _bodyKindType.HasFlag(BodyKindType.Monsters))
                {
                    stringBuilder.AppendLine("* CharacterMaster Bodies that are not Survivors.");
                }
                if(flag == BodyKindType.Drone && _bodyKindType.HasFlag(BodyKindType.Drone))
                {
                    stringBuilder.AppendLine("* DroneDef Bodies.");
                }
            }

            string result = stringBuilder.ToString();
            HG.StringBuilderPool.ReturnStringBuilder(stringBuilder);

            return result;
        }

        private IEnumerator AddEntriesInBulk()
        {
            Undo.RecordObject(_targetIDAD, "Add Entries in Bulk");

            using var progressBar = new DisposableProgressBar("Adding IDRS Entries in Bulk", "Fetching IDRS", 0);
            CoroutineWithResult getItemDisplaysToAddSubroutine = new CoroutineWithResult(GetItemDisplaysToAdd(progressBar));
            while(getItemDisplaysToAddSubroutine.MoveNext())
            {
                yield return null;
            }

            ReadOnlyCollection<string> itemDisplayRuleSetsToAdd = getItemDisplaysToAddSubroutine.boxedResult as ReadOnlyCollection<string>;

            for (int i = 0; i < itemDisplayRuleSetsToAdd.Count; i++)
            {
                progressBar.Update(R2EKMath.Remap(i, 0, itemDisplayRuleSetsToAdd.Count, 0.5f, 1f), null, $"Adding IDRS to IDAD ({i}/{itemDisplayRuleSetsToAdd.Count})");
                yield return null;

                string idrsGuid = itemDisplayRuleSetsToAdd[i];

                //Guard against the "idrsNone"
                if (idrsGuid == "1b03760dbbc74a34289e67486b831f62")
                    continue;

                ItemDisplayAddressedDictionary.DisplayDictionaryEntry entry = new ItemDisplayAddressedDictionary.DisplayDictionaryEntry { targetIDRS = new AssetReferenceT<ItemDisplayRuleSet>(idrsGuid) };

                //We need to figure out the display prefab we want.
                AddressReferencedPrefab addressReferencedPrefab = null;

                //If the key asset type is an item, check if the idrs we're adding is for mithrix, if so, we need to use a spike.
                if(_keyAssetType == KeyAssetType.Item && idrsGuid == "8464d010b0eb5874087422831a1044e2")
                {
                    string displayPrefabGuid = "";
                    bool isAssignedAtRuntime = false;
                    switch(((ItemDef)_targetIDAD.keyAsset).tier)
                    {
                        case ItemTier.NoTier:
                            break;
                        case ItemTier.Tier1:
                            displayPrefabGuid = "a8dc7aa7f61d18e4db92a79611b96f90"; //ItemInfection, White
                            break;
                        case ItemTier.Tier2:
                            displayPrefabGuid = "66d3fe6a84e3e144c952dd3fab421173"; //ItemInfection, Green
                            break;
                        case ItemTier.Tier3:
                            displayPrefabGuid = "b06477748e168314abd4c861c4cbcd76"; //ItemInfection, Red
                            break;
                        case ItemTier.Boss:
                            displayPrefabGuid = "64cc7537dfeb23d4a949c313de5ae697"; //ItemInfection, Boss
                            break;
                        case ItemTier.Lunar:
                            displayPrefabGuid = "de62e89840152434cb622e434bf9bb62"; //ItemInfection, Blue
                            break;
                        case ItemTier.VoidTier1:
                        case ItemTier.VoidTier2:
                        case ItemTier.VoidTier3:
                        case ItemTier.VoidBoss:
                            displayPrefabGuid = "62dfa363e5d9edf40adbc338c710e9c5"; //ItemInfection, Void
                            break;
                        case ItemTier.FoodTier:
                            displayPrefabGuid = "c919d63addcdc72458922b582f9c31c7"; //ItemIndection, Food
                            break;
                        case ItemTier.AssignedAtRuntime:
                            isAssignedAtRuntime = true;
                            break;
                    }

                    //We've gotten a display prefab for mithrix using the vanilla spikes
                    if(!string.IsNullOrWhiteSpace(displayPrefabGuid))
                    {
                        addressReferencedPrefab = new AddressReferencedPrefab(displayPrefabGuid);
                    }
                    else
                    {
                        //If it's assigned at runtime, check if the end user specified a display prefab.
                        if(isAssignedAtRuntime && _spikeDisplayPrefab)
                        {
                            addressReferencedPrefab = new AddressReferencedPrefab(_spikeDisplayPrefab);
                        }
                        else //Otherwise, continue to the next one, no display for mithrix.
                        {
                            continue;
                        }
                    }
                }
                else
                {
                    addressReferencedPrefab = new AddressReferencedPrefab(_defaultDisplayPrefab);
                }

                entry.AddDisplayRule(new ItemDisplayAddressedDictionary.ItemAddressedDisplayRule
                {
                    ruleType = ItemDisplayRuleType.ParentedPrefab,
                    displayPrefab = addressReferencedPrefab
                });

                if(_limbFlags != LimbFlags.None)
                {
                    entry.AddDisplayRule(new ItemDisplayAddressedDictionary.ItemAddressedDisplayRule
                    {
                        ruleType = ItemDisplayRuleType.LimbMask,
                        limbMask = _limbFlags
                    });
                }

                HG.ArrayUtils.ArrayAppend(ref _targetIDAD.displayEntries, entry);
            }
            _targetIDADSerializedObject.Update();
        }

        private IEnumerator<ReadOnlyCollection<string>> GetItemDisplaysToAdd(DisposableProgressBar progressBar)
        {
            void AddSafe(List<string> list, string guid)
            {
                if(!list.Contains(guid))
                {
                    list.Add(guid);
                }
            }

            progressBar.Update(null, null, "Querying IDRS GUIDS");

            //We need a list of IDRS guids, that way we can extrapolate the IDRS's guid via a name check on the path, we'll use a set so we can remove the ones we've found so far.
            var lookup = new AddressablesPathDictionary.EntryLookup()
                .WithLookupType(AddressablesPathDictionary.EntryType.Guid)
                .WithTypeRestriction(typeof(ItemDisplayRuleSet));
            var subroutine = lookup.PerformLookupAsync();
            while(subroutine.MoveNext())
            {
                yield return null;
            }

            progressBar.Update(0.01f, null, "Querying IDRS GUIDS");

            HashSet<string> allIDRSGUIDs = new HashSet<string>(lookup.results);
            //We're going to immediatly get the "survivor like" body prefabs, these kinds of prefabs are ones from SurvivorDefs, but includes Engi's turrets.
            //This is because we dont want to add said survivor's idrs if the only thing the used selected was Monsters, since the Monsters enum option works off CharacterMaster.
            var bodyPrefabCoroutine = new CoroutineWithResult<List<GameObject>>(GetSurvivorLikeBodyPrefabs(progressBar));
            while(bodyPrefabCoroutine.MoveNext())
            {
                yield return null;
            }
            var survivorLikeBodyPrefabs = bodyPrefabCoroutine.result;

            //Same logic as above, we dont want to include drones for monster prefabs.
            bodyPrefabCoroutine = new CoroutineWithResult<List<GameObject>>(GetDroneDefBodyPrefabs(progressBar));
            while(bodyPrefabCoroutine.MoveNext())
            {
                yield return null;
            }
            var droneDefBodyPrefabs = bodyPrefabCoroutine.result;

            List<string> result = new List<string>();
            if((_bodyKindType & BodyKindType.SurvivorLike) != 0)
            {
                //This will get most of the survivor IDRS, we're adding special cases later.
                for (int i = 0; i < survivorLikeBodyPrefabs.Count; i++)
                {
                    progressBar.Update(R2EKMath.Remap(i, 0, survivorLikeBodyPrefabs.Count, 0.05f, 0.2f), null, $"Getting SurivorLike IDRS ({i}/{survivorLikeBodyPrefabs.Count}");
                    yield return null;

                    GameObject bodyObject = survivorLikeBodyPrefabs[i];
                    if (!bodyObject)
                    {
                        continue;
                    }

                    CharacterModel model = bodyObject.GetComponentInChildren<CharacterModel>();
                    if(!model)
                    {
                        continue;
                    }

                    if (!model.itemDisplayRuleSet)
                        continue;

                    string guid = ResolveItemDisplayRuleSetGUID(allIDRSGUIDs, model.itemDisplayRuleSet);
                    if(!string.IsNullOrEmpty(guid))
                    {
                        AddSafe(result, guid);
                    }
                }

                //Regardless of asset kind, add scavenger.
                AddSafe(result, "f5bda7d11b594d74e9e8cffcc5ee521b");//idrsScav

                //If it's an item or elite equipment, add mithrix.
                if(_keyAssetType == KeyAssetType.Item || _keyAssetType == KeyAssetType.EliteEquipment)
                {
                    AddSafe(result, "8464d010b0eb5874087422831a1044e2"); //idrsBrother
                }
            }
            if((_bodyKindType & BodyKindType.Monsters) != 0)
            {
                //Work off CharacterMasters, we'll get their body prefabs, and as long as those bodies are not considered survivor-like bodies, add their IDRS to the set.
                var characterMasterGUIDS = new AddressablesPathDictionary.EntryLookup()
                    .WithLookupType(AddressablesPathDictionary.EntryType.Guid)
                    .WithTypeRestriction(typeof(GameObject))
                    .WithComponentRequirement(typeof(CharacterMaster), false)
                    .PerformLookup();

                for (int i = 0; i < characterMasterGUIDS.Count; i++)
                {
                    progressBar.Update(R2EKMath.Remap(i, 0, characterMasterGUIDS.Count, 0.2f, 0.35f), null, $"Getting Monster IDRS ({i}/{characterMasterGUIDS.Count}");

                    yield return null;

                    string characterMasterGUID = characterMasterGUIDS[i];
                    GameObject prefab = Addressables.LoadAssetAsync<GameObject>(characterMasterGUID).WaitForCompletion();

                    if (!prefab)
                        continue;

                    if(!prefab.TryGetComponent<CharacterMaster>(out var master))
                    {
                        continue;
                    }

                    GameObject bodyPrefab = master.bodyPrefab;
                    if(!bodyPrefab)
                    {
                        continue;
                    }

                    //We don't want to include survivor or drone IDRS for monsters.
                    if(survivorLikeBodyPrefabs.Contains(bodyPrefab))
                    {
                        continue;
                    }
                    if(droneDefBodyPrefabs.Contains(bodyPrefab))
                    {
                        continue;
                    }

                    CharacterModel model = bodyPrefab.GetComponentInChildren<CharacterModel>();
                    if (!model)
                    {
                        continue;
                    }

                    if (!model.itemDisplayRuleSet)
                        continue;

                    string guid = ResolveItemDisplayRuleSetGUID(allIDRSGUIDs, model.itemDisplayRuleSet);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        AddSafe(result, guid);
                    }
                }
            }
            if((_bodyKindType & BodyKindType.Drone) != 0)
            {
                for (int i = 0; i < droneDefBodyPrefabs.Count; i++)
                {
                    progressBar.Update(R2EKMath.Remap(i, 0, droneDefBodyPrefabs.Count, 0.35f, 0.5f), null, $"Getting Drone IDRS ({i}/{droneDefBodyPrefabs.Count}");
                    yield return null;

                    GameObject bodyObject = droneDefBodyPrefabs[i];
                    if (!bodyObject)
                    {
                        continue;
                    }

                    CharacterModel model = bodyObject.GetComponentInChildren<CharacterModel>();
                    if (!model)
                    {
                        continue;
                    }

                    if (!model.itemDisplayRuleSet)
                        continue;

                    string guid = ResolveItemDisplayRuleSetGUID(allIDRSGUIDs, model.itemDisplayRuleSet);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        AddSafe(result, guid);
                    }
                }
            }
            yield return new ReadOnlyCollection<string>(result);
        }

        private IEnumerator<List<GameObject>> GetSurvivorLikeBodyPrefabs(DisposableProgressBar progressBar)
        {
            progressBar.Update(0.01f, null, "Querying SurvivorLike BodyPrefabs");

            var lookup = new AddressablesPathDictionary.EntryLookup()
                    .WithLookupType(AddressablesPathDictionary.EntryType.Guid)
                    .WithTypeRestriction(typeof(SurvivorDef));
            var subroutine = lookup.PerformLookupAsync();
            while(subroutine.MoveNext())
            {
                yield return null;
            }
            var survivorDefGUIDS = lookup.results;

            progressBar.Update(0.02f, null);

            var strictSurvivorLikePrefabs = new List<GameObject>();
            for (int i = 0; i < survivorDefGUIDS.Count; i++)
            {
                progressBar.Update(R2EKMath.Remap(i, 0, survivorDefGUIDS.Count, 0.02f, 0.03f), null, $"Querying SurvivorLike Body Prefabs ({i}/{survivorDefGUIDS.Count})");

                yield return null;

                string survivorDefGUID = survivorDefGUIDS[i];
                SurvivorDef survivorDef = Addressables.LoadAssetAsync<SurvivorDef>(survivorDefGUID).WaitForCompletion();
                if (!survivorDef)
                {
                    continue;
                }

                GameObject bodyObject = survivorDef.bodyPrefab;
                if (!bodyObject)
                {
                    continue;
                }

                strictSurvivorLikePrefabs.Add(bodyObject);
            }
            //Add engi's turrets
            strictSurvivorLikePrefabs.Add(Addressables.LoadAssetAsync<GameObject>("04508f474b420d546b5d55a9a18a9698").WaitForCompletion());
            strictSurvivorLikePrefabs.Add(Addressables.LoadAssetAsync<GameObject>("78e114e549394f945b82672fd4893994").WaitForCompletion());

            yield return strictSurvivorLikePrefabs;
        }

        private IEnumerator<List<GameObject>> GetDroneDefBodyPrefabs(DisposableProgressBar progressBar)
        {
            progressBar.Update(0.03f, null, "Querying DroneDef Body Prefabs");

            var lookup = new AddressablesPathDictionary.EntryLookup()
                .WithLookupType(AddressablesPathDictionary.EntryType.Guid)
                .WithTypeRestriction(typeof(DroneDef));
            var subroutine = lookup.PerformLookupAsync();
            while(subroutine.MoveNext())
            {
                yield return null;
            }
            var droneDefGUIDs = lookup.results;

            progressBar.Update(0.04f, null, null);

            var droneDefBodyPrefabs = new List<GameObject>();
            for (int i = 0; i < droneDefGUIDs.Count; i++)
            {
                progressBar.Update(R2EKMath.Remap(i, 0, droneDefGUIDs.Count, 0.04f, 0.05f), null, $"Querying DroneDef Body Prefabs ({i}/{droneDefGUIDs.Count})");

                yield return null;

                string droneDefGUID = droneDefGUIDs[i];
                DroneDef droneDef = Addressables.LoadAssetAsync<DroneDef>(droneDefGUID).WaitForCompletion();
                if (!droneDef)
                    continue;

                GameObject droneBodyPrefab = droneDef.bodyPrefab;
                if (!droneBodyPrefab)
                    continue;

                droneDefBodyPrefabs.Add(droneBodyPrefab);
            }
            yield return droneDefBodyPrefabs;
        }

        private string ResolveItemDisplayRuleSetGUID(HashSet<string> allIDRSGuids, ItemDisplayRuleSet idrs)
        {
            AddressablesPathDictionary instance = AddressablesPathDictionary.instance;
            string guidMatch = "";
            foreach(string guid in allIDRSGuids)
            {
                //We do a path check, since the path should contain the IDRS's name.
                string path = instance.GetPathFromGUID(guid);

                //No junk IDRS
                if(path.StartsWith("RoR2/Junk"))
                {
                    continue;
                }

                //No InDev IDRS
                if(path.StartsWith("RoR2/InDev"))
                {
                    continue;
                }

                //No Trash
                if(path.StartsWith("RoR2/Trash"))
                {
                    continue;
                }

                if(!path.Contains(idrs.name))
                {
                    continue;
                }

                //Load the asset
                var result = Addressables.LoadAssetAsync<ItemDisplayRuleSet>(guid).WaitForCompletion();

                //If the result exists and it's the idrs itself, we've hit our match
                if (result && result == idrs)
                {
                    guidMatch = guid;
                    break;
                }
            }

            //Remove from the set to speed up further searches.
            allIDRSGuids.Remove(guidMatch);

            return guidMatch;
        }

        public override Vector2 GetWindowSize()
        {
            Vector2 size = base.GetWindowSize();
            size.x = 600;
            return size;
        }

        public IDADTemplaterPopupWindow(ItemDisplayAddressedDictionary targetIDAD)
        {
            _targetIDADSerializedObject = new SerializedObject(targetIDAD);
            _targetIDAD = targetIDAD;
        }
    }
}
