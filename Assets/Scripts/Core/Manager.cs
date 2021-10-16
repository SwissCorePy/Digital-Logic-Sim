using System;
using System.Collections.Generic;
using Chip;
using Graphics;
using Save_System;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Core
{
public class Manager : MonoBehaviour
{
    private static Manager _instance;

    public ChipEditor chipEditorPrefab;
    public ChipPackage chipPackagePrefab;
    public Wire wirePrefab;
    public Chip.Chip[] builtinChips;
    public List<Chip.Chip> spawnableChips;
    [FormerlySerializedAs("UIManager")] public UIManager uiManager;

    private ChipEditor _activeChipEditor;
    private int _currentChipCreationIndex;

    public static ChipEditor ActiveChipEditor => _instance._activeChipEditor;

    private void Awake()
    {
        _instance = this;
        _activeChipEditor = FindObjectOfType<ChipEditor>();
        FindObjectOfType<CreateMenu>().ONChipCreatePressed += SaveAndPackageChip;
        FindObjectOfType<UpdateButton>().ONChipUpdatePressed += UpdateChip;
    }

    private void Start()
    {
        spawnableChips = new List<Chip.Chip>();
        SaveSystem.Init();
        SaveSystem.LoadAll(this);
    }

    public event Action<Chip.Chip> CustomChipCreated;
    public event Action<Chip.Chip> CustomChipUpdated;

    public Chip.Chip LoadChip(ChipSaveData loadedChipData)
    {
        _activeChipEditor.LoadFromSaveData(loadedChipData);
        _currentChipCreationIndex = _activeChipEditor.creationIndex;

        var loadedChip = PackageChip();
        LoadNewEditor();
        return loadedChip;
    }

    public void ViewChip(Chip.Chip chip)
    {
        LoadNewEditor();
        uiManager.ChangeState(UIManagerState.Update);

        var chipSaveData =
            ChipLoader.GetChipSaveData(chip, builtinChips, spawnableChips, wirePrefab, _activeChipEditor);
        _activeChipEditor.LoadFromSaveData(chipSaveData);
    }

    private void SaveAndPackageChip()
    {
        ChipSaver.Save(_activeChipEditor);
        PackageChip();
        LoadNewEditor();
    }

    private void UpdateChip()
    {
        var updatedChip = TryPackageAndReplaceChip(_activeChipEditor.chipName);
        ChipSaver.Update(_activeChipEditor, updatedChip);
        LoadNewEditor();
    }

    private Chip.Chip PackageChip()
    {
        var package = Instantiate(chipPackagePrefab, transform);
        package.PackageCustomChip(_activeChipEditor);
        package.gameObject.SetActive(false);

        var customChip = package.GetComponent<Chip.Chip>();
        customChip.canBeEdited = true;
        CustomChipCreated?.Invoke(customChip);
        _currentChipCreationIndex++;
        spawnableChips.Add(customChip);
        return customChip;
    }

    private Chip.Chip TryPackageAndReplaceChip(string original)
    {
        var oldPackage = Array.Find(GetComponentsInChildren<ChipPackage>(true), cp => cp.name == original);
        if (oldPackage != null) Destroy(oldPackage.gameObject);

        var package = Instantiate(chipPackagePrefab, transform);
        package.PackageCustomChip(_activeChipEditor);
        package.gameObject.SetActive(false);

        var customChip = package.GetComponent<Chip.Chip>();
        customChip.canBeEdited = true;
        var index = spawnableChips.FindIndex(c => c.chipName == original);
        if (index >= 0)
        {
            spawnableChips[index] = customChip;
            CustomChipUpdated?.Invoke(customChip);
        }

        return customChip;
    }

    private void LoadNewEditor()
    {
        if (_activeChipEditor)
        {
            Destroy(_activeChipEditor.gameObject);
            uiManager.ChangeState(UIManagerState.Create);
        }

        _activeChipEditor = Instantiate(chipEditorPrefab, Vector3.zero, Quaternion.identity);
        _activeChipEditor.creationIndex = _currentChipCreationIndex;
    }

    public void SpawnChip(Chip.Chip chip)
    {
        if (chip is CustomChip custom)
            custom.ApplyWireModes();

        _activeChipEditor.chipInteraction.SpawnChip(chip);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
}
}