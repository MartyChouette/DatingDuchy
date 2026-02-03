#if UNITY_EDITOR
using UnityEngine;

[AddComponentMenu("Miscellaneous/Editor Notes")]
public class EditorNotes : MonoBehaviour
{
    [TextArea(5, 100)]
    public string Notes = "Add your notes here.";
}
#endif
