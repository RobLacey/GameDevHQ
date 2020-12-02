public interface INodeBase
{
    bool PointerOverNode { get; set; }
    void Start();
    void OnEnable();
    void OnDisable();
    void DeactivateNode();
    void OnEnter(bool isDragEvent);
    void OnExit(bool isDragEvent);
    void OnSelected(bool isDragEvent);
}