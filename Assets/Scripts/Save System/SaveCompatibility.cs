using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Save_System {
public class SaveCompatibility : MonoBehaviour {
    private static string FixStr(string message, char c) {
        // To remove double quotes (passed as 'c') of the chip name
        var aStr = new StringBuilder(message);
        for (var i = 0; i < aStr.Length; i++)
            if (aStr[i] == c)
                aStr.Remove(i, 1);
        return aStr.ToString();
    }

    public static dynamic FixSaveCompatibility(string chipSaveString) {
        var lol = JsonConvert.DeserializeObject<dynamic>(chipSaveString);

        for (var i = 0; i < lol.savedComponentChips.Count; i++) {
            var newValue = new List<OutputPin>();
            var newValue2 = new List<InputPin>();

            // Replace all 'outputPinNames' : [string] in save with 'outputPins' :
            // [OutputPin]
            for (var j = 0; j < lol.savedComponentChips[i].outputPinNames.Count; j++)
                newValue.Add(
                    new OutputPin { name = lol.savedComponentChips[i].outputPinNames[j],
                                    wireType = 0
                                  });
            lol.savedComponentChips[i].Property("outputPinNames").Remove();
            lol.savedComponentChips[i].outputPins =
                JsonConvert.DeserializeObject<dynamic>(
                    JsonConvert.SerializeObject(newValue));

            // Add to all 'inputPins' dictionary the property 'wireType' with a value
            // of 0 (at version 0.25 buses did not exist so its impossible for the
            // wire to be of other type)
            for (var j = 0; j < lol.savedComponentChips[i].inputPins.Count; j++)
                newValue2.Add(new InputPin {
                name = lol.savedComponentChips[i].inputPins[j].name,
                parentChipIndex =
                lol.savedComponentChips[i].inputPins[j].parentChipIndex,
                parentChipOutputIndex =
                lol.savedComponentChips[i].inputPins[j].parentChipOutputIndex,
                isCyclic = lol.savedComponentChips[i].inputPins[j].isCylic,
                wireType = 0
            });
            lol.savedComponentChips[i].inputPins =
                JsonConvert.DeserializeObject<dynamic>(
                    JsonConvert.SerializeObject(newValue2));
        }

        // Update save file. Delete the old one a create one with the updated
        // version
        string savePath = SaveSystem.GetPathToSaveFile(
                              FixStr(JsonConvert.SerializeObject(lol.name), (char)0x22));
        File.Delete(savePath);
        using (var writer = new StreamWriter(savePath)) {
            writer.Write(JsonConvert.SerializeObject(lol, Formatting.Indented));
            writer.Close();
        }

        return JsonConvert.SerializeObject(lol, Formatting.Indented);
    }

    private class OutputPin {
        [JsonProperty("name")]
        public string name {
            get;
            set;
        }

        [JsonProperty("wireType")] public int wireType {
            get;
            set;
        }
    }

    private class InputPin {
        [JsonProperty("name")]
        public string name {
            get;
            set;
        }

        [JsonProperty("parentChipIndex")] public int parentChipIndex {
            get;
            set;
        }

        [JsonProperty("parentChipOutputIndex")] public int parentChipOutputIndex {
            get;
            set;
        }

        [JsonProperty("isCylic")] public bool isCyclic {
            get;
            set;
        }

        [JsonProperty("wireType")] public int wireType {
            get;
            set;
        }
    }
}
}
