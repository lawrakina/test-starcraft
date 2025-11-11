using Core.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace DroneResourceCollection.UI
{
    /// <summary>
    /// Компонент отображения счетчиков ресурсов по фракциям
    /// </summary>
    public class ResourceCounterDisplay : MonoBehaviour
    {
        [SerializeField] private Text _redFactionText;
        [SerializeField] private Text _blueFactionText;
        
        private int _redResourceCount;
        private int _blueResourceCount;

        public void Initialize()
        {
            UpdateDisplay();
        }

        public void UpdateResourceCount(FactionType faction, int count)
        {
            if (faction == FactionType.Red)
            {
                _redResourceCount = count;
            }
            else if (faction == FactionType.Blue)
            {
                _blueResourceCount = count;
            }
            
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_redFactionText != null)
            {
                _redFactionText.text = $"Red Faction: {_redResourceCount}";
            }
            
            if (_blueFactionText != null)
            {
                _blueFactionText.text = $"Blue Faction: {_blueResourceCount}";
            }
        }
    }
}


