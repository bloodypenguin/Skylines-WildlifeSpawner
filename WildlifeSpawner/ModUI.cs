using ColossalFramework.UI;
using UnityEngine;
using System.Linq;

namespace WildlifeSpawner
{
    public static class ModUI
    {
        private static UIDropDown _animalDropDown;
        private static UIPanel _panel;
        private static UIButton _plopButton;

        public static int SelectedIndex => _animalDropDown.selectedIndex;

        public static void Initialize()
        {
            Dispose();


            var uiView = UIView.GetAView();
            _panel = uiView.AddUIComponent(typeof(UIPanel)) as UIPanel;
            _panel.name = "WildlifeSpawnerPanel";
            _panel.backgroundSprite = "MenuPanel2";
            _panel.size = new Vector2(100 + 200 + 12, 72);
            _panel.isVisible = false;
            _panel.relativePosition = new Vector3(0, 874);
            UIUtil.SetupTitle("Wildlife Spawn Point", _panel);

            var collideLabel = _panel.AddUIComponent<UILabel>();
            collideLabel.text = "Animal";
            collideLabel.width = 100;
            collideLabel.height = 24;
            collideLabel.textScale = 0.8f;
            collideLabel.relativePosition = new Vector2(4, 44);

            _animalDropDown = UIUtil.CreateDropDown(_panel);
            _animalDropDown.width = 200;
            _animalDropDown.height = 24;
            _animalDropDown.listWidth = 200;
            _animalDropDown.tooltip = "Select animal";
            _animalDropDown.relativePosition = new Vector2(8 + 100, 44);
            _animalDropDown.items = new[] { "Random" }.Concat(WildlifeSpawnManager.instance.Animals.m_buffer.Select(index =>
              {
                  var prefab = PrefabCollection<CitizenInfo>.GetPrefab(index);
                  return prefab != null ? prefab.name : "";
              }).Where(s => !string.IsNullOrEmpty(s))).ToArray();
            _animalDropDown.listPosition = UIDropDown.PopupListPosition.Above;

            Reset();

            _plopButton = UIView.GetAView().FindUIComponent<UITabstrip>("MainToolstrip").AddUIComponent<UIButton>(); //main button on in game tool strip.
            _plopButton.size = new Vector2(43, 49);
            _plopButton.eventClick += (component, param) =>
            {
                var tool = ToolsModifierControl.SetTool<BuildingTool>();
                tool.m_prefab = PrefabCollection<BuildingInfo>.FindLoaded("Wildlife Spawn Point");
            };
            _plopButton.normalBgSprite = "ToolbarIconGroup6Normal";
            _plopButton.normalFgSprite = "IconPolicyBigBusiness";
            _plopButton.focusedBgSprite = "ToolbarIconGroup6Focused";
            _plopButton.hoveredBgSprite = "ToolbarIconGroup6Hovered";
            _plopButton.pressedBgSprite = "ToolbarIconGroup6Pressed";
            _plopButton.disabledBgSprite = "ToolbarIconGroup6Disabled";
            _plopButton.name = "WildlifeSpawnPointPlopButton";
            _plopButton.tooltip = "Place animal spawn point";
        }

        public static void Reset()
        {
            Hide();
            if (_animalDropDown != null)
            {
                _animalDropDown.selectedIndex = 0;
            }
        }

        public static void Dispose()
        {
            Reset();
            if (_panel != null)
            {
                Object.Destroy(_panel.gameObject);
            }
            if (_plopButton != null)
            {
                Object.Destroy(_plopButton.gameObject);
            }
            _plopButton = null;
            _animalDropDown = null;
            _panel = null;
        }

        public static void Hide()
        {
            _panel?.Hide();
        }

        public static void Show()
        {
            _panel?.Show();
        }
    }
}