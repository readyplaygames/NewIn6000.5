using UnityEngine;

namespace Common
{
    public class InstructionPanel : MonoBehaviour
    {
        public GameObject panel;

        public void SetPanelActive(bool value)
        {
            if (panel != null)
            {
                panel.SetActive(value);
            }
        }
    }
}