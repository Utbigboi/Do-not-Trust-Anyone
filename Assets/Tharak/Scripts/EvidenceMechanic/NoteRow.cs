using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CaseDesk
{
    // One line in the notepad list: a toggle (for selecting) + a handwritten label.
    public class NoteRow : MonoBehaviour
    {
        public Toggle toggle;
        public TMP_Text label;
        [HideInInspector] public int index;

        public void Set(int i, string text)
        {
            index = i;
            if (label) label.text = text;
            if (toggle) toggle.isOn = false;
        }
    }
}
