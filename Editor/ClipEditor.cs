
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Dropecho {
  public class ClipEditorWindow : EditorWindow {
    [MenuItem("Dropecho/Layered Sounds/Clip Editor")]
    public static void Open() {
      var title = ObjectNames.NicifyVariableName(typeof(GenerativeAudioClip).Name) + " Editor";
      var window = GetWindow<ClipEditorWindow>(title);
      window.minSize = new Vector2(800, 600);
    }

    public virtual void CreateGUI() {
      rootVisualElement.style.flexGrow = 1;
      rootVisualElement.Add(new ClipEditor());
    }
  }


  public class ClipEditor : VisualElement {
    List<GenerativeAudioClip> clips = new List<GenerativeAudioClip>();

    public ClipEditor() {
      this.style.flexDirection = FlexDirection.Row;
      this.style.flexGrow = 1;

      RefreshAssetList();

      var leftpanel = new Box() {
        style = {
        paddingTop=5,
        flexDirection = FlexDirection.Column,
        justifyContent= Justify.SpaceBetween,
        minWidth = 250,
        borderRightColor = new Color(0.1f,0.1f,0.1f,1f),
        borderRightWidth = 1
      }
      };

      leftpanel.Add(RenderTypeList());
      var bottomButtons = new Box();
      bottomButtons.Add(new Button(() => RefreshAssetList()) { text = "Refresh" });
      bottomButtons.Add(new Button(() => CreateAssetWithSavePrompt()) { text = "Create" });

      leftpanel.Add(bottomButtons);

      this.Add(leftpanel);
      this.Add(new Box() { name = "right-panel", style = { flexGrow = 1, paddingLeft = 5, paddingRight = 5, paddingBottom = 5, paddingTop = 5 } });
    }

    public void RefreshAssetList() {
      clips.Clear();
      clips.AddRange(GetAssetsOfType<GenerativeAudioClip>());
      var list = this.Q<ListView>("type-list");
      if (list != null) {
        list.RefreshItems();
      }
    }

    protected void SetSelected(GenerativeAudioClip obj) {
      if (obj) {
        var list = this.Q<ListView>("type-list");
        if (list != null) {
          var index = list.itemsSource.IndexOf(obj);
          list.SetSelection(index);
        }
      }
    }

    protected virtual ListView RenderTypeList() {
      var list = new ListView {
        name = "type-list",
        makeItem = makeItem,
        bindItem = bindItem,
        itemsSource = clips,
        selectionType = SelectionType.Single
      };

      list.onSelectionChange += (selected) => {
        var rightPanel = this.Q<Box>("right-panel");
        rightPanel.Clear();

        var item = selected.FirstOrDefault() as GenerativeAudioClip;
        if (item == null) {
          return;
        }

        var UIElEditor = Editor.CreateEditor(item).CreateInspectorGUI();
        if (UIElEditor == null) {
          var editor = Editor.CreateEditor(item);
          rightPanel.Add(new IMGUIContainer(() => editor.DrawDefaultInspector()));
        } else {
          rightPanel.Add(UIElEditor);
        }
      };

      return list;
    }

    private void bindItem(VisualElement element, int index) {
      var label = element.Q<Label>();
      var button = element.Q<Button>();
      label.text = clips[index].name;

      element.Remove(button);
      element.Add(new Button(() => {
        if (EditorUtility.DisplayDialog("Delete", "Are you sure you want to delete this?\nIt will be deleted from disk and cannot be undone.", "Delete", "Cancel")) {
          var list = this.Q<ListView>();

          if (list.selectedIndex == index) {
            if (list.selectedIndex > 0) {
              list.SetSelection(list.selectedIndex - 1);
            } else {
              list.ClearSelection();
            }
          }

          AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(clips[index]));
          clips.Remove(clips[index]);
          list.RefreshItems();
        }
      }) { text = "Delete", style = { height = 24 } });
    }

    private VisualElement makeItem() {
      var item = new VisualElement() {
        style = {
          paddingLeft = 10, paddingRight = 10, paddingBottom = 0, paddingTop = 0,
          justifyContent = Justify.SpaceBetween,
          alignItems = Align.Center,
          borderBottomWidth = 1,
          borderBottomColor = Color.black,
          flexDirection = FlexDirection.Row
        }
      };
      item.Add(new Label());
      item.Add(new Button() { style = { height = 28 } });
      return item;
    }

    // Creates a new ScriptableObject via the default Save File panel
    void CreateAssetWithSavePrompt() {
      var type = typeof(GenerativeAudioClip);
      var path = EditorUtility.SaveFilePanelInProject("Save ScriptableObject", type.Name + ".asset", "asset", "Enter a file name for the ScriptableObject.", "Assets");
      if (path == "") return;

      var asset = ScriptableObject.CreateInstance(type);
      AssetDatabase.CreateAsset(asset, path);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();
      AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
      EditorGUIUtility.PingObject(asset);

      RefreshAssetList();
      SetSelected(asset as GenerativeAudioClip);
    }

    private static T[] GetAssetsOfType<T>() where T : UnityEngine.Object {
      var typeName = typeof(T).Name;

      return AssetDatabase.FindAssets("t:" + typeName)
        .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
        .Select(path => AssetDatabase.LoadAssetAtPath<T>(path))
        .ToArray();
    }
  }
}