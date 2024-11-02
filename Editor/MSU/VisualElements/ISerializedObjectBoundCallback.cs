using UnityEditor;

namespace MSU.Editor.UIElements
{
    public interface ISerializedObjectBoundCallback
    {
        public void OnBoundSerializedObjectChange(SerializedObject so);
    }
}