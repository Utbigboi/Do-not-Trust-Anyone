using UnityEngine;

namespace CaseDesk
{
    // The leak. Click to open the notepad in "feed the Gull" mode.
    public class FaxMachine : MonoBehaviour, IClickable
    {
        public void OnClick()
        {
            if (CallController.I == null || CallController.I.IsOpen) return;
            CallController.I.OpenLeak();
        }
    }
}
